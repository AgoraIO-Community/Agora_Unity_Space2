using UnityEngine;
using System;
using UnityEngine.Serialization;

namespace Agora.Spaces
{
    [CreateAssetMenu(menuName = "Agora/AppInfoInput", fileName = "AppInfoInput", order = 1)]
    [Serializable]
    public class AppInfoInput : ScriptableObject
    {
        [FormerlySerializedAs("APP_ID")]
        [SerializeField]
        public string appID = "";

        [FormerlySerializedAs("RTC_TOKEN")]
        [SerializeField]
        public string rtcToken = "";

        [FormerlySerializedAs("RTM_TOKEN")]
        [SerializeField]
        public string rtmToken = "";
    }
}
