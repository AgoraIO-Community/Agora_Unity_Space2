using System.Collections.Generic;
using UnityEngine;
using Agora.Demo.Meta.Model;

namespace Agora.Spaces.Controller
{
    /// <summary>
    ///   The PlayerSyncManager keeps track of the user transform data, publish 
    /// local positions, and update remote positions.
    /// </summary>
    public class PlayerSyncManager
    {
        // A dictionary to quickly get the transform of a user's avatar
        Dictionary<string, Transform> AvatarMap = new Dictionary<string, Transform>();

        // The Encoder/Decoder of the TransformData type
        DataCodec<TransformData> TdataCodec = new DataCodec<TransformData>();

        // External caller to add the player transform to the dictionary
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

        // Check if  the userId exists
        public bool HasPlayer(string userId)
        {
            return AvatarMap.ContainsKey(userId);
        }

        // Remove the player from the dictionary
        public void RemovePlayer(string userId)
        {
            if (AvatarMap.ContainsKey(userId))
            {
                GameObject.Destroy(AvatarMap[userId].gameObject);
                AvatarMap.Remove(userId);
            }
        }

        // Clear the dictionary
        public void ClearPlayers()
        {
            AvatarMap.Clear();
        }

        // also used as a callback
        public void UpdateTransform(string userID, Transform me)
        {
            TransformData transformData = new TransformData(me) { UserId = userID };
            MetaRTMController.Instance.PublishTransformSync(transformData.ToJSON());

            // The alternate way is to send the data as binary data, not neccessary more efficient than JSON
            // Keeping here for reference.
            // byte[] bytes = TdataCodec.Encode(transformData);
            // MetaRTMController.Instance.PublishTransformSync(bytes);
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
