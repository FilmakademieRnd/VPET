using UnityEngine;

public class Utilities : MonoBehaviour
{
    private static string logPattern = "<color=#{1}><{2}> {0}</color>";

    public static void CustomLog(string content, string name = "VIVE Integration")
    {
        CustomLog(content, Color.blue, name);
    }

    public static void CustomLog(string content, Color color, string name = "VIVE Integration")
    {
        Debug.LogFormat(logPattern, content, ColorUtility.ToHtmlStringRGBA(color), name);
    }
}