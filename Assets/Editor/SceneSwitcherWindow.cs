using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class SceneSwitcherWindow : EditorWindow
{
    private Vector2 scroll;

    [MenuItem("Tools/Scene Switcher")]
    public static void ShowWindow()
    {
        GetWindow<SceneSwitcherWindow>("Scene Switcher");
    }

    private void OnGUI()
    {
        GUILayout.Space(5);
        GUILayout.Label("Scenes In Build Settings", EditorStyles.boldLabel);

        scroll = EditorGUILayout.BeginScrollView(scroll);

        var scenes = EditorBuildSettings.scenes;

        for (int i = 0; i < scenes.Length; i++)
        {
            var scene = scenes[i];
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scene.path);

            GUI.backgroundColor = GetSceneColor(scene.path);

            EditorGUILayout.BeginHorizontal("box");

            GUI.enabled = scene.enabled;

            if (GUILayout.Button(sceneName, GUILayout.Height(30)))
            {
                OpenScene(scene.path);
            }

            GUI.enabled = true;

            GUILayout.Label(scene.enabled ? "Enabled" : "Disabled", GUILayout.Width(70));

            EditorGUILayout.EndHorizontal();

            GUI.backgroundColor = Color.white;
        }

        EditorGUILayout.EndScrollView();
    }

    private Color GetSceneColor(string path)
    {
        if (SceneManager.GetActiveScene().path == path)
        {
            return new Color(0.5f, 1f, 0.5f); // current scene highlight
        }

        return Color.white;
    }

    private void OpenScene(string path)
    {
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            EditorSceneManager.OpenScene(path);
        }
    }
}
