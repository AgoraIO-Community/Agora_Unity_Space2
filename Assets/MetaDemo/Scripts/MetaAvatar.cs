using UnityEngine;
using Agora.Rtc;

namespace Agora.Spaces
{

    public class MetaAvatar : MonoBehaviour
    {
        // view of RTC stream
        [SerializeField]
        GameObject ViewEntity;

        public event System.Action OnPlayerPositionChanged;

        private uint _uid;

        private IRTCController _rtc;

        // Start is called before the first frame update
        void Start()
        {
            ViewEntity.SetActive(false);
        }

        // Update is called once per frame
        public void Init(uint uid, IRTCController rtc, bool enableVideo = true)
        {
            _uid = uid;
            rtc.OnOfflineNotify += HandleOffline;
            _rtc = rtc;
            if (enableVideo)
            {
                ViewEntity.SetActive(true);
                EnableVideoDisplay();
            }
        }

        void EnableVideoDisplay()
        {
            var videosurface = ViewEntity.GetComponent<VideoSurface>();
            if (videosurface == null)
            {
                videosurface = ViewEntity.AddComponent<VideoSurface>();
            }
            if (_uid == 0) // local player
            {
                videosurface.SetForUser(0, GameApplication.Instance.RTCChannelName);
            }
            else
            {
                videosurface.SetForUser(_uid, GameApplication.Instance.RTCChannelName, VIDEO_SOURCE_TYPE.VIDEO_SOURCE_REMOTE);

            }
            videosurface.SetEnable(true);
        }

        void HandleOffline(uint offline_uid)
        {
            if (_rtc != null)
            {
                _rtc.OnOfflineNotify -= HandleOffline;
            }
            if (this != null && _uid != 0 && _uid == offline_uid)
            {
                Destroy(gameObject);
            }
        }

    }
}
