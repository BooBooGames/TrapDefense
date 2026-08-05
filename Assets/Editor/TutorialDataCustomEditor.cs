using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TutorialData))]
public class TutorialDataCustomEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(10);

        if (GUILayout.Button("Auto Assign Steps"))
        {
            AutoAssignSteps((TutorialData)target);
        }
    }

    private void AutoAssignSteps(TutorialData tutorialData)
    {
        string assetPath = AssetDatabase.GetAssetPath(tutorialData);
        string parentFolder = Path.GetDirectoryName(assetPath);

        var stepFolders = Directory.GetDirectories(parentFolder)
            .Where(dir => Path.GetFileName(dir).StartsWith("Step"))
            .OrderBy(dir => ExtractStepNumber(dir))
            .ToList();

        tutorialData.TutorialSteps = new List<TutorialStepData>();

        foreach (var folder in stepFolders)
        {
            string relativePath = folder.Replace("\\", "/");

            var stepData = new TutorialStepData();

            string[] guids = AssetDatabase.FindAssets("", new[] { relativePath });

            var startActions = new List<TutorialAction>();
            var constraints = new List<TutorialConstraint>();
            var endActions = new List<TutorialEndAction>();

            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var obj = AssetDatabase.LoadAssetAtPath<Object>(path);

                if (obj is TutorialAction action)
                    startActions.Add(action);
                else if (obj is TutorialConstraint constraint)
                    constraints.Add(constraint);
                else if (obj is TutorialEndAction endAction)
                    endActions.Add(endAction);
            }

            stepData.OnStepStartActions = startActions.ToArray();
            stepData.EndConstraints = constraints.ToArray();
            stepData.OnStepEndActions = endActions.ToArray();

            tutorialData.TutorialSteps.Add(stepData);
        }

        EditorUtility.SetDirty(tutorialData);
        AssetDatabase.SaveAssets();

        Debug.Log($"[TutorialData] Assigned {tutorialData.TutorialSteps.Count} steps.");
    }

    private int ExtractStepNumber(string path)
    {
        string name = Path.GetFileName(path);
        string numberPart = name.Replace("Step", "").Trim();

        return int.TryParse(numberPart, out int number) ? number : 0;
    }
}