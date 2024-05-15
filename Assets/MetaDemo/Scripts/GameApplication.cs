using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Agora.Spaces
{
    // Singleton for controlling loading the application level with extra
    // capability to launch with user input for username and spacename
    // This is a singleton script that the hosting script should be one instance
    // as well.
    public class GameApplication : MonoBehaviour
    {
        [SerializeField]
        internal AppInfoInput AppInfoInput;

        [SerializeField]
        LoginPrompt loginPrompt;

        [SerializeField]
        string GameLevel;

        // Singleton
        public static GameApplication Instance { get; private set; }

        // user name, will be seen as a label on the object
        public string UserName { get; private set; }

        /// <summary>
        ///   Channel Name: 
        ///   Conceptually there is one channel name. However, RTC and RTM are
        /// dingtinct SDKs that uses the same backend.  Therefore, internally
        /// we should make a different channel for each SDK.
        /// </summary>
        string ChannelName { get; set; }

        /// <summary>
        /// Use this channel name for joining a RTC channel
        /// </summary>
        public string RTCChannelName
        {
            get
            {
                return ChannelName + "#RTC";
            }
        }

        /// <summary>
        ///   Use this channel name for joining a RTM channel
        /// </summary>
        public string RTMChannelName
        {
            get
            {
                return ChannelName + "#RTM";
            }
        }

        /// <summary>
        ///   Determine by user if camera should be capturing
        /// </summary>
        public bool EnableVideo { get; private set; } = true;

        // For launching the game from CLI environment
        bool _autoMode = false; // indicates the app is launched from commandline
        // For launching the game from CLI environment
        string _envNameExtension = "";

        void Start()
        {
            if (Instance != null)
            {
                Destroy(Instance.gameObject);
            }
            Instance = this;

            loginPrompt.Play += LoadScene;
            SetupBatchEnvironment();
            DontDestroyOnLoad(this);
        }

        /// <summary>
        ///   PlayButton trigger to load the next scene
        /// </summary>
        /// <param name="info"></param>
        void LoadScene(IEntryInfo info)
        {
            UserName = info.UserName;
            ChannelName = info.SpaceName;
            SceneManager.LoadScene(GameLevel);
        }

        /// <summary>
        ///   Leave the game play scene
        /// </summary>
        public void StopGame()
        {
            // by leaving the game scene, the system will destroy the
            // MetaGameController and invoke the deinit functions as
            // consequence.
            SceneManager.LoadSceneAsync(0);  // 0 is the starting scene
        }

        #region -- batch lanuching --
        // Set up variable using the environment; this is for batch launching
        // instances of app for testing.
        void SetupBatchEnvironment()
        {
            _autoMode = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SPACEAUTOMODE"));
            _envNameExtension = Environment.GetEnvironmentVariable("DEMONUM");
            Debug.Log("EnvNameExtension = " + _envNameExtension);
            if (_autoMode)
            {
                UserName = GanerateUserName();
                ChannelName = Environment.GetEnvironmentVariable("SPACENAME");
                SceneManager.LoadScene(GameLevel);
            }
        }

        string GanerateUserName()
        {
            return Application.platform.ToString() + _envNameExtension;
        }
        #endregion
    }
}
