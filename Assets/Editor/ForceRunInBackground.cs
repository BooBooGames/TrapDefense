using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class ForceRunInBackground
{
    static ForceRunInBackground()
    {
        EditorApplication.update += Update;
    }

    private static void Update()
    {
        if (Application.runInBackground == false)
        {
            Application.runInBackground = true;
        }
    }
}