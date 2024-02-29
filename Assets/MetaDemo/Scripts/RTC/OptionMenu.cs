using System.Collections;
using UnityEngine;
using UnityEngine.UI;

using Agora.Util;
using Agora.Spaces.Controller;

namespace Agora.Spaces.UI
{
    public class OptionMenu : MonoBehaviour
    {
        [SerializeField] Button MenuButton;
        [SerializeField] Button ExitButton;
        [SerializeField] ToggleStateButton MuteMicButton;
        [SerializeField] ToggleStateButton MuteCamButton;
        [SerializeField] ToggleStateButton PlayButton;
        [SerializeField] ToggleStateButton SttButton;
        // [SerializeField] Button WebButton;
        [SerializeField] GameObject MenuDialog;
        //[SerializeField] GameObject STTView;
        //[SerializeField] Text STTSubtitle;

        [SerializeField] Dropdown LanguageDropdown;

        [SerializeField] string WebURL;

        // Start is called before the first frame update
        IEnumerator Start()
        {
            MenuDialog.SetActive(false);
            // STTView.SetActive(false);
            MuteCamButton.gameObject.SetActive(false);
            MuteMicButton.gameObject.SetActive(false);
            MenuButton.onClick.AddListener(HandleMenuTap);
            ExitButton.onClick.AddListener(HandleExit);
            yield return new WaitUntil(() => MetaRTCController.Instance.RtcEngine != null);
            SetupButtons();
        }


        void SetupButtons()
        {
            MuteMicButton.Setup(initOnOff: false,
                onStateText: "Mute Mic", offStateText: "Unmute Mic",
                callOnAction: () =>
                {
                    MetaRTCController.Instance.MuteMic(true);
                },
                callOffAction: () =>
                {
                    MetaRTCController.Instance.MuteMic(false);
                }
            );
            MuteCamButton.Setup(initOnOff: false,
                onStateText: "Disable WebCam", offStateText: "Enable WebCam",
                callOnAction: () =>
                {
                    Debug.Log("Local Camera muted");
                    MetaRTCController.Instance.MuteCamera(true);

                },
                callOffAction: () =>
                {
                    Debug.Log("Local Camera enabled");
                    MetaRTCController.Instance.MuteCamera(false);
                }
            );

            PlayButton.Setup(initOnOff: false,
                onStateText: "Play Media", offStateText: "Stop Media",
                callOnAction: () =>
                {
                    Debug.Log("Play media");
                    PlayMedia(true);
                    MenuDialog.SetActive(false);
                },
                callOffAction: () =>
                {
                    Debug.Log("Stop media");
                    PlayMedia(false);
                    MenuDialog.SetActive(false);
                }
            );
            MuteCamButton.gameObject.SetActive(true);
            MuteMicButton.gameObject.SetActive(true);
            // PlayButton.gameObject.SetActive(true);
            // PlayButton.GetComponent<Button>().interactable = GameApplication.Instance.IsHost;

            //WebButton.onClick.AddListener(() => Application.OpenURL(WebURL));
        }

        void HandleMenuTap()
        {
            MenuDialog.SetActive(!MenuDialog.activeInHierarchy);
        }

        void HandleExit()
        {
            GameApplication.Instance.StopGame();
        }

        void PlayMedia(bool play)
        {
            var mediaTV = MetaRTCController.Instance.GetMediaTV();
            if (mediaTV != null)
            {
                if (play) { mediaTV.Play(); }
                else { mediaTV.Stop(); }
            }
        }
    }
}
