using System;
using UnityEngine;
using Agora.Rtc;

namespace Agora.Spaces
{
    /// <summary>
    ///   This interface defines the abstraction of RTC Controller
    /// </summary>
    public interface IRTCController
    {
        bool JoinedChannel { get; }
        uint LocalUID { get; }

        event Action<uint> OnOfflineNotify;

        void JoinChannel(string channelName, string userName, Vector3 position);
        void LeaveChannel();
        void MuteCamera(bool mute);
        void MuteMic(bool mute);
        int UpdateRemotePosition(uint uid, RemoteVoicePositionInfo posInfo);
        int UpdateSelfPosition(float[] position, float[] axisForward, float[] axisRight, float[] axisUp);
        void UpdateSpatialAudioPosition(uint remoteUid, float sourceDistance);
    }
}