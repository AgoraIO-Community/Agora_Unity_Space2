using UnityEngine;
using UnityEditor;

namespace Agora.Demo
{
    public class SpaceMenuCommands : MonoBehaviour
    {

        [MenuItem("Agora Tools/ClearPrefs")]
        public static void ClearPrefs()
        {
            Debug.LogWarning("Clearing all PlayerPrefs values");
            PlayerPrefs.DeleteAll();
        }
    }
}
