using System.Collections.Generic;
using UnityEngine;
using Agora.Demo.Meta.Model;

namespace Agora.Demo.Meta.Controller
{

    public class PlayerSyncManager
    {
        Dictionary<string, Transform> AvatarMap = new Dictionary<string, Transform>();
        DataCodec<TransformData> TdataCodec = new DataCodec<TransformData>();


        public void AddPlayerTransform(string userId, Transform player, bool owned)
        {
            Debug.Log($"PlayerSyncManager adding {userId} owned:{owned}");
            AvatarMap[userId] = player;
            if (owned)
            {
                TransformSynchronizer sync = player.GetComponent<TransformSynchronizer>();
                sync.SyncTransform += UpdateTransform;
            }
        }

        public bool HasPlayer(string name)
        {
            return AvatarMap.ContainsKey(name);
        }

        public void RemovePlayer(string userId)
        {
            if (AvatarMap.ContainsKey(userId))
            {
                GameObject.Destroy(AvatarMap[userId].gameObject);
                AvatarMap.Remove(userId);
            }
        }

        public void ClearPlayers()
        {
            AvatarMap.Clear();
        }

        // also used as a callback
        public void UpdateTransform(string userID, Transform me)
        {
            TransformData transformData = new TransformData(me) { UserId = userID };
            // byte[] bytes = TdataCodec.Encode(transformData);
            // send this to the topic
            // MetaRTMController.Instance.PublishTransformSync(bytes);
            MetaRTMController.Instance.PublishTransformSync(transformData.ToJSON());
        }

        // driven by data received from server
        public void OnTransformUpdate(byte[] bytes)
        {
            TransformData transformData = TdataCodec.Decode(bytes);
            if (AvatarMap.ContainsKey(transformData.UserId))
            {
                AvatarMap[transformData.UserId].localPosition = transformData.LocalPosition;
                AvatarMap[transformData.UserId].localScale = transformData.LocalScale;
                // since it is a localRotation being passed, rotate on Self space
                AvatarMap[transformData.UserId].Rotate(transformData.EulerAngles, Space.Self);
            }
        }

        // driven by data received from server
        public void OnTransformUpdate(string json)
        {
            TransformData transformData = JsonUtility.FromJson<TransformData>(json);
            if (AvatarMap.ContainsKey(transformData.UserId))
            {
                AvatarMap[transformData.UserId].localPosition = transformData.LocalPosition;
                AvatarMap[transformData.UserId].localScale = transformData.LocalScale;
                // since it is a localRotation being passed, rotate on Self space
                AvatarMap[transformData.UserId].localEulerAngles = transformData.EulerAngles;
            }
        }
    }
}
