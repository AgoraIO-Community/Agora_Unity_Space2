using System;
using System.Collections;
using UnityEngine;
using Agora.Rtc;
using Agora.Spaces.Controller;

namespace Agora.Spaces.UI
{
    public class MediaTV : MonoBehaviour
    {
        internal IRtcEngineEx RtcEngine = null;
        internal IMediaPlayer MediaPlayer = null;

        [SerializeField]
        bool IsLoop = true;

        [SerializeField]
        string MEDIA_URL = "https://agoracdn.s3.us-west-1.amazonaws.com/videos/Agora.io-Interactions.mp4";

        [SerializeField]
        VideoSurface videoSurface;

        [SerializeField]
        internal uint UidUseInEx;
        [SerializeField]
        internal uint UidUseInMPK = 67890;

        [SerializeField]
        Material meshMaterial;

        string ResidentScene { get; set; }
        RtcConnection _rtcConnection;

        private void Start()
        {

            ResidentScene = gameObject.scene.name;
            StartCoroutine(InitMediaPlayer());
        }

        IEnumerator InitMediaPlayer()
        {
            yield return new WaitUntil(() => MetaRTCController.Instance.RtcEngine != null && MetaRTCController.Instance.JoinedChannel);
            RtcEngine = MetaRTCController.Instance.RtcEngine;
            UidUseInEx = MetaRTCController.Instance.LocalUID;

            MediaPlayer = RtcEngine.CreateMediaPlayer();
            if (MediaPlayer == null)
            {
                Debug.LogError("CreateMediaPlayer failed!");
                yield break;
            }

            MpkEventHandler handler = new MpkEventHandler(this);
            MediaPlayer.InitEventHandler(handler);


        }

        public void Play()
        {
            if (MediaPlayer == null) return;
            //We use the mpk to simulate the voice of remote users.
            JoinChannelExWithMPK(GameApplication.Instance.AppInfoInput.appID, UidUseInMPK, MediaPlayer.GetId());

            var ret = MediaPlayer.Open(MEDIA_URL, 0);
            Debug.Log("Media Open returns: " + ret);

            // Don't listen to this locally
            MediaPlayer.AdjustPlayoutVolume(0);

            if (this.IsLoop)
            {
                MediaPlayer.SetLoopCount(-1);
            }
            else
            {
                MediaPlayer.SetLoopCount(0);
            }
        }

        void StartMediaPlayback()
        {
            uint uid = (uint)MediaPlayer.GetId();
            DisplayStreamFromUser(uid, "", VIDEO_SOURCE_TYPE.VIDEO_SOURCE_MEDIA_PLAYER);


            SpatialAudioStart();
            var ret = MediaPlayer.Play();
            Debug.Log("Media Play returns " + ret);
        }

        public void Stop()
        {
            MediaPlayer?.Stop();
            StopStreamDisplay();

            RtcEngine.LeaveChannelEx(_rtcConnection);
        }

        // use this as a controller to fill video surface only
        public void DisplayStreamFromUser(uint uid, string channelId, VIDEO_SOURCE_TYPE sourceType = VIDEO_SOURCE_TYPE.VIDEO_SOURCE_CAMERA_PRIMARY)
        {
            if (videoSurface)
            {
                Destroy(videoSurface);
            }

            videoSurface = gameObject.AddComponent<VideoSurface>();
            videoSurface.enabled = true;
            videoSurface.SetForUser(uid, channelId, sourceType);
            videoSurface.SetEnable(true);
            ResetTransform(true);
        }

        public void StopStreamDisplay()
        {
            if (videoSurface != null)
            {
                videoSurface.SetEnable(false);
                videoSurface.enabled = false;
                Destroy(videoSurface);
                videoSurface = null;
            }
            ResetMesh();
            ResetTransform(false);
        }

        private void OnDisable()
        {
            Stop();
        }

        private void OnDestroy()
        {
            if (RtcEngine == null) return;

            if (MediaPlayer != null)
                RtcEngine.DestroyMediaPlayer(MediaPlayer);
            RtcEngine = null;
        }

        void OnSceneChanged(string sceneName)
        {
            if (sceneName != ResidentScene)
            {
                Debug.Log("Scene changed to " + sceneName + $" but I am in {ResidentScene}, stop play");
                Stop();
            }
        }

        void ResetMesh()
        {
            if (this != null && this.gameObject != null)
            {
                var render = GetComponent<MeshRenderer>();
                Destroy(render.material);
                render.material = meshMaterial;
            }
        }

        void ResetTransform(bool isVideo)
        {
            Vector3 scale = transform.localScale;
            float z = scale.z;
            if (z > 0 && isVideo)
            {
                z = -z;
            }
            else if (z < 0 && !isVideo)
            {
                z = -z;
            }
            transform.localScale = new Vector3(scale.x, scale.y, z);
        }

        void SpatialAudioStart()
        {
            Debug.Log($"Setting up media at position = {transform.position} forward = {transform.forward}");
            // Set the coordinates in the world coordinate system.
            // This parameter is an array of length 3
            // The three values represent the front, right, and top coordinates
            float[] position = new float[] { transform.position.x, transform.position.y, transform.position.z };
            // Set the unit vector of the x axis in the coordinate system.
            // This parameter is an array of length 3,
            // The three values represent the front, right, and top coordinates
            float[] forward = new float[] { transform.forward.x, transform.forward.y, transform.position.z };
            // Update the spatial position of the specified remote user
            RemoteVoicePositionInfo remotePosInfo = new RemoteVoicePositionInfo(position, forward);
            var rc = MetaRTCController.Instance.SpatialAudioController.UpdateRemotePositionEx(UidUseInMPK, remotePosInfo,
            new RtcConnection(GameApplication.Instance.RTCChannelName, UidUseInEx));
        }

        private void JoinChannelExWithMPK(string channelName, uint uid, int playerId)
        {
            _rtcConnection = new RtcConnection(channelId: channelName, localUid: uid);
            ChannelMediaOptions options = new ChannelMediaOptions();
            options.autoSubscribeAudio.SetValue(false);
            options.autoSubscribeVideo.SetValue(true);
            options.publishCameraTrack.SetValue(false);
            options.publishMediaPlayerAudioTrack.SetValue(true);
            options.publishMediaPlayerVideoTrack.SetValue(true);
            options.publishMediaPlayerId.SetValue(playerId);
            options.enableAudioRecordingOrPlayout.SetValue(false);
            options.clientRoleType.SetValue(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
            var ret = RtcEngine.JoinChannelEx("", _rtcConnection, options);
            RtcEngine.UpdateChannelMediaOptionsEx(options, _rtcConnection);
            Debug.Log("RtcEngineController JoinChannelEx_MPK returns: " + ret);
        }

        internal class MpkEventHandler : IMediaPlayerSourceObserver
        {
            private readonly MediaTV _mediaTV;

            internal MpkEventHandler(MediaTV sample)
            {
                _mediaTV = sample;
            }

            public override void OnPlayerSourceStateChanged(MEDIA_PLAYER_STATE state, MEDIA_PLAYER_ERROR ec)
            {
                Debug.Log("OnPlayerSourceStateChanged = " + state);
                if (state == MEDIA_PLAYER_STATE.PLAYER_STATE_OPEN_COMPLETED)
                {
                    _mediaTV.StartMediaPlayback();
                }
                else if (state == MEDIA_PLAYER_STATE.PLAYER_STATE_STOPPED)
                {
                }
            }

            public override void OnPlayerEvent(MEDIA_PLAYER_EVENT @event, Int64 elapsedTime, string message)
            {
                Debug.Log(string.Format("OnPlayerEvent state: {0}", @event));
            }

            public override void OnPreloadEvent(string src, PLAYER_PRELOAD_EVENT @event)
            {
                Debug.Log(string.Format("OnPreloadEvent src: {0}, @event: {1}", src, @event));
            }

        }
    }
}
