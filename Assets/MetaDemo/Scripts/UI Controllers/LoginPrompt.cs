using UnityEngine;
using UnityEngine.UI;

namespace Agora.Spaces
{
    public class LoginPrompt : MonoBehaviour, IEntryInfo
    {
        [SerializeField]
        InputField UserNameInput;

        [SerializeField]
        InputField GameSpaceNameInput;


        public string UserName
        {
            get { return UserNameInput.text; }
            set { UserNameInput.text = value; }
        }


        public string SpaceName
        {
            get { return GameSpaceNameInput.text; }
            set { GameSpaceNameInput.text = value; }
        }

        public event System.Action<IEntryInfo> Play;

        private void Awake()
        {
            var SpaceName = PlayerPrefs.GetString("SpaceName", "");
            if (SpaceName != "")
            {
                GameSpaceNameInput.text = SpaceName;
            }

            var username = PlayerPrefs.GetString("UserName", "");
            if (username != "")
            {
                UserNameInput.text = username;
            }

        }

        public void OnPlayButton()
        {
            PlayerPrefs.SetString("UserName", UserNameInput.text);
            PlayerPrefs.SetString("SpaceName", GameSpaceNameInput.text);
            PlayerPrefs.Save();
            if (string.IsNullOrEmpty(SpaceName))
            {
                Debug.LogWarning("SpaceName can not be empty!");
                return;
            }
            if (string.IsNullOrEmpty(UserName))
            {
                Debug.LogWarning("UserName can not be empty!");
                return;
            }
            Play?.Invoke(this);
        }
    }
}
