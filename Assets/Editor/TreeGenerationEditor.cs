using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TreeGeneration))]
public class TreeGenerationEditor : Editor
{
    public override void OnInspectorGUI()
    {
        TreeGeneration tg = (TreeGeneration)target;

        if (DrawDefaultInspector())
        {
            if (tg.autoUpdateInEditor)
            {
                tg.DrawPerlinEditor();
            }
            if (tg.autoUpdateTreeVisibility)
            {
                tg.TreeHiderEditor();
            }
        }

        if(GUILayout.Button("Generate Perlin"))
        {
            tg.DrawPerlinEditor();
        }
        if(GUILayout.Button("Generate Trees"))
        {
            tg.GenTreesEditor();
        }
        if(GUILayout.Button("Re Compute Hidden Trees"))
        {
            tg.TreeHiderEditor();
        }
        if(GUILayout.Button("Clear Editor Memory"))
        {
            tg.ClearEditorOnlyVariables();
        }
    }
}
