using UnityEngine;
using Agora.Rtc;

namespace Agora.Spaces
{

    public class MetaAvatar : MonoBehaviour
    {
        // view of RTC stream
        [SerializeField]
        GameObject ViewEntity;

        // UID of the user asssociated with this avatar
        private uint _uid;

        // the controller interface
        private IRTCController _rtc;

        // Start is called before the first frame update
        void Start()
        {
            ViewEntity.SetActive(false);
        }

        /// <summary>
        ///   Initialize the MetaAvatar instance with offline handler
        /// </summary>
        /// <param name="uid">local user's uid</param>
        /// <param name="rtc">rtc controller instance</param>
        /// <param name="enableVideo">using webcam video or not</param>
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

        /// <summary>
        ///  Enable the video stream display on the video surface component 
        /// </summary>
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

        /// <summary>
        ///  Clean up if the user goes offline
        /// </summary>
        /// <param name="offline_uid"></param>
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
