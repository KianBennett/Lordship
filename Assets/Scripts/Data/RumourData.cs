﻿using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class Rumour 
{
    private string startText, middleText, endText;
    public NPC targetNpc;

    public string StartText { get { return startText.Replace("{name}", targetNpc.DisplayName); } }
    public string MiddleText { get { return middleText; } }
    public string EndText { get { return endText; } }

    public Rumour(NPC npc, string start, string middle, string end) 
    {
        targetNpc = npc;
        startText = start;
        middleText = middle;
        endText = end;
    }
}

[Serializable]
public class RumourData : ScriptableObject
{
    // Each rumour has a start point (NPC giving you the rumour), mid point (special choice unlocked
    // for other NPC) and end points (other NPC response)
    [SerializeField] private List<string> _startPoints;
    [SerializeField] private List<string> _midPoints;
    [SerializeField] private List<string> _endPoints;

    public int RumourCount { get { return _startPoints.Count; } }

    public string GetRumourStart(int index) 
    {
        return _startPoints[index];
    }

    public string GetRumourMid(int index) 
    {
        return _midPoints[index];
    }

    public string GetRumourEnd(int index) 
    {
        return _endPoints[index];
    }

#if UNITY_EDITOR
    public const string PathToAsset = "Assets/Data/Rumours.asset";

    public static RumourData LoadData()
    {
        RumourData data = AssetDatabase.LoadAssetAtPath<RumourData>(PathToAsset);
        if (data == null)
        {
            data = CreateInstance<RumourData>();
            AssetDatabase.CreateAsset(data, PathToAsset);
        }

        return data;
    }
#endif
}

