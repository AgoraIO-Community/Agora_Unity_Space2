using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Agora.Spaces
{
    // Singleton for controlling loading the application level with extra
    // capability to launch with user input for username and spacename
    public class GameApplication : MonoBehaviour
    {
        [SerializeField]
        internal AppInfoInput AppInfoInput;

        [SerializeField]
        LoginPrompt loginPrompt;

        [SerializeField]
        string GameLevel;

        public static GameApplication Instance { get; private set; }

        public string UserName { get; private set; }
        public string ChannelName { get; private set; }

        bool _autoMode = false; // indicates the app is launched from commandline
        string _envNameExtension = "";

        void Start()
        {
            if (Instance != null)
            {
                Destroy(Instance);
            }
            Instance = this;

            loginPrompt.Play += LoadScene;
            SetupBatchEnvironment();
            DontDestroyOnLoad(this);
        }

        // PlayButton trigger to load the next scene
        void LoadScene(IEntryInfo info)
        {
            UserName = info.UserName;
            ChannelName = info.SpaceName;
            SceneManager.LoadScene(GameLevel);
        }

        public void StopGame()
        {

        }

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
    }
}
