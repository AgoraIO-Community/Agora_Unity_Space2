using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Agora.Demo.Meta.Model;

namespace Agora.Spaces.Controller
{
    /// <summary>
    ///   The MetaGameController takes care of life cycles of the players as 
    /// objects in the scene.
    /// </summary>
    public class MetaGameController : MonoBehaviour
    {
        [SerializeField]
        GameObject AvatarPrefab;

        [SerializeField]
        Transform UsersRoot;

        internal PlayerSyncManager SyncManager { get; private set; }

        internal MetaRTMController _rtmController;
        internal MetaRTCController _rtcController;

        private float SyncFrequence = 0.5f; // half second

        const float YPos = 1f;

        Transform _myAvatar;
        bool _exitSelfStateSync = false;
        Vector3 _myInitPosition = Vector3.zero;

        HashSet<string> SubscribeGroup = new HashSet<string>();

        private void Awake()
        {
            SyncManager = new PlayerSyncManager();
            _rtmController = GetComponent<MetaRTMController>();
            _rtcController = GetComponent<MetaRTCController>();
        }

        private void Start()
        {
            _rtmController.InitClient(this);
            _rtmController.OnLoginComplete += () =>
            {
                _exitSelfStateSync = false;
                SpawnAvatar(GameApplication.Instance.UserName, true);
                StartCoroutine(nameof(CoSyncLocalStateTick));
            };
            _rtmController.OnJoinStreamChannel += () =>
            {
                SendTransformData();
                _rtcController.JoinChannel("", GameApplication.Instance.UserName, _myInitPosition);
            };
            _rtmController.OnLeaveStreamChannel += () =>
            {
                _exitSelfStateSync = true;
                SyncManager.ClearPlayers();
            };
            _rtmController.OnUserLeftStreamChannel += (player) =>
            {
                SyncManager.RemovePlayer(player);
                SubscribeGroup.Remove(player);
            };
        }


        internal string GetLogName()
        {
            return "rtm_" + GameApplication.Instance.UserName + ".log";
        }

        Vector3 GetSpawnPosition()
        {
            // return SpawnLocations[num % SpawnLocations.Length];
            Vector2 randXY = UnityEngine.Random.insideUnitCircle;
            return new Vector3(randXY.x * 5, YPos, randXY.y);
        }

        void EnableCameraFollow(bool enable, Transform trans)
        {
            FollowCharacter follow = Camera.main.GetComponent<FollowCharacter>();
            if (follow == null) return;
            if (enable)
            {
                follow.character = trans;
            }
            else
            {
                follow.character = null;
            }
        }


        public bool HasPlayer(string name)
        {
            return SyncManager.HasPlayer(name);
        }

        /// <summary>
        ///    Spawn avatar to a 3d location 
        /// </summary>
        /// <param name="name">User name</param>
        /// <param name="num">The number of people in game</param>
        public void SpawnAvatar(string name, bool owned = false, string json = null)
        {
            Debug.Log("Spawning for -------------" + name);
            Vector3 pos = GetSpawnPosition();
            GameObject ava = Instantiate(AvatarPrefab, pos, Quaternion.identity, UsersRoot);
            ava.name = name;
            if (owned)
            {
                _myInitPosition = pos;
                var sync = ava.AddComponent<TransformSynchronizer>();
                ava.AddComponent<PlayerController>();
                sync.UserID = name;
                _myAvatar = ava.transform;
                EnableCameraFollow(true, _myAvatar);
            }
            else
            {
                TransformData tdata = JsonUtility.FromJson<TransformData>(json);
                ava.transform.localPosition = tdata.LocalPosition;
                ava.transform.localScale = tdata.LocalScale;
                ava.transform.forward = tdata.Forward;
                ava.transform.localRotation = Quaternion.Euler(tdata.EulerAngles);
                ValidateScriptions(name, tdata);
            }

            // label the name
            Transform label = ava.transform.Find("Canvas/NameLabel");
            if (label == null)
            {
                Debug.LogError("Could not find NameLabel on prefab");
                return;
            }

            Text text = label.GetComponent<Text>();
            if (text)
            {
                text.text = name;
            }
            SyncManager.AddPlayerTransform(name, ava.transform, owned);
        }

        IEnumerator CoSyncLocalStateTick()
        {
            while (!_exitSelfStateSync)
            {
                SendTransformData();
                yield return new WaitForSeconds(SyncFrequence);
            }
        }

        internal void SyncSubscriptions(string[] players)
        {
            SubscribeGroup.Clear();
            SubscribeGroup = new HashSet<string>(players);
        }

        internal void SendTransformData()
        {
            if (_myAvatar != null)
            {
                TransformData transformData = new TransformData(_myAvatar)
                {
                    UserId = GameApplication.Instance.UserName
                };
                string json = transformData.ToJSON();
                //Debug.Log("CoSyncStateTick:" + json);
                MetaRTMController.Instance.PublishTransformState(json);
            }
        }

        /// <summary>
        ///    Validate if this player is within the high frequency update range. If so,
        ///  make sure it is subscribed in the topic. Else, only update for the State change
        ///  event.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="transformInfo"></param>
        internal void ValidateScriptions(string player, string transformInfo)
        {
            TransformData tdata = JsonUtility.FromJson<TransformData>(transformInfo);
            ValidateScriptions(player, tdata);
        }

        internal void ValidateScriptions(string player, TransformData tdata)
        {
            if (InRange(tdata.LocalPosition))
            {
                if (!SubscribeGroup.Contains(player))
                {
                    SubscribeGroup.Add(player);
                    string[] p = (new List<string>(SubscribeGroup)).ToArray();
                    MetaRTMController.Instance.SubscribeTopic(MetaRTMController.Instance.TransformTopic, p);
                }
            }
            else
            {
                if (SubscribeGroup.Contains(player))
                {
                    SubscribeGroup.Remove(player);
                }
            }
        }

        bool InRange(Vector3 position)
        {
            return true;
        }

        #region TESTCODE
#if TESTING
        void OnGUI()
        {
            GUILayout.Space(8);
            if (GUILayout.Button("Print SubscribeGroup"))
            {
                List<string> ls = new List<string>(SubscribeGroup);
                string str = string.Join(',', ls);
                Debug.Log("SubscribeGroup:" + str);
            }

            GUILayout.Space(8);
            if (GUILayout.Button($"Up Push Interval {SyncFrequence} X2"))
            {
                SyncFrequence *= 2;
            }

            GUILayout.Space(8);
            if (GUILayout.Button("Resubscribe Topic"))
            {
                //MetaRTMController.Instance.LeaveTopic(MetaRTMController.Instance.TransformTopic);
                string[] p = (new List<string>(SubscribeGroup)).ToArray();
                MetaRTMController.Instance.SubscribeTopic(MetaRTMController.Instance.TransformTopic, p);
            }
        }

        TransformData GetSampleTransformData()
        {
            GameObject game = GameObject.Find("GameController");
            TransformData data = new TransformData(game.transform) { UserId = "Sample" };
            return data;
        }
#endif
        #endregion
    }
}
