using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;


namespace Agora.Demo
{
    /// <summary>
    /// First Scene Auto Loader
    /// </summary>
    /// <description>
    /// Based on an idea on this thread:
    /// http://forum.unity3d.com/threads/157502-Executing-first-scene-in-build-settings-when-pressing-play-button-in-editor
    /// </description>
    [InitializeOnLoad]
    public static class FirstSceneAutoLoader
    {

        private const string NAME = "Agora Tools/Run First Scene Always";

        static bool isEnabled
        {
            get
            {
                return EditorPrefs.GetBool(NAME, false);
            }
            set
            {
                EditorPrefs.SetBool(NAME, value);
                SceneListChanged();
            }
        }

        [MenuItem(NAME, false, 0)]
        static void LoadFirstScene()
        {
            isEnabled = !isEnabled;
        }

        [MenuItem(NAME, true, 0)]
        static bool ValidateLoadFirstScene()
        {
            Menu.SetChecked(NAME, isEnabled);
            return true;
        }

        static FirstSceneAutoLoader()
        {
            SceneListChanged();

            EditorBuildSettings.sceneListChanged += SceneListChanged;
        }

        static void SceneListChanged()
        {
            if (!isEnabled)
            {
                EditorSceneManager.playModeStartScene = default;
                return;
            }
            //Ensure at least one build scene exist.
            if (EditorBuildSettings.scenes.Length == 0)
            {
                Debug.Log("No Scenes in Build Settings");
                isEnabled = false;
                return;
            }
            //Reference the first scene
            SceneAsset theScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(EditorBuildSettings.scenes[0].path);
            // Set Play Mode scene to first scene defined in build settings.
            EditorSceneManager.playModeStartScene = theScene;
        }
    }
}