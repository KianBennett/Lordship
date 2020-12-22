﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(AssetManager))]
public class AssetManagerEditor : Editor {

    public override void OnInspectorGUI() {
        AssetManager assetManager = (AssetManager) target;
        int matCount = assetManager.GetColouredMaterialCount();

        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Coloured material copies: " + matCount);
        if (GUILayout.Button("Clear")) assetManager.ClearMaterialDictionary();
        GUILayout.EndHorizontal();

        EditorGUILayout.Space();

        DrawDefaultInspector();
    }
}