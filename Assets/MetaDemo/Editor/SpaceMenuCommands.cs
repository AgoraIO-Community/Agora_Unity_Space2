using UnityEngine;
using UnityEditor;

public class SpaceMenuCommands : MonoBehaviour
{

    [MenuItem("Agora Tools/ClearPrefs")]
    public static void ClearPrefs()
    {
        Debug.LogWarning("Clearing all PlayerPrefs values");
        PlayerPrefs.DeleteAll();
    }

    [MenuItem("Agora Tools/InstantiateMe")]
    public static void InstantiateMe()
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);

        // set up transform
        go.transform.Rotate(-90.0f, 0.0f, 0.0f);
        go.name = "Cube_" + System.DateTime.Now.ToShortTimeString();

    }
}
