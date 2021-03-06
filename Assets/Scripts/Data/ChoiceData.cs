﻿using System;
using System.Collections.Generic;
using UnityEngine;

public enum DialogueType { Greeting, Flatter, Threaten, Bribe, Rumours, Goodbye }
public enum ChoiceTextType { Predefined, RandomFlatter, RandomThreaten, BribeAmount, RumourMid }

[Serializable]
public class ChoiceData
{
    [SerializeField] private int _index;
    [SerializeField] private string _text;
    // [SerializeField] private TextList _textList;
    [SerializeField] private ChoiceTextType _textType;
    [SerializeField] private int _beatId;
    [SerializeField] private DialogueType _type;
    [SerializeField] private bool _correctChoice;

    public int Index { get { return _index; } }
    // If the type is set to random then pick a random value from _randomTextList, otherwise use _text
    public string DisplayText 
    { 
        get { return _text; } 
        set { _text = value; }
    }
    public int NextID { get { return _beatId; } }
    public DialogueType Type { get { return _type; }}
    public ChoiceTextType TextType { get { return _textType; } }
    // Used when needing to pick the correct choice
    public bool IsCorrectChoice 
    { 
        get { return _correctChoice; } 
        set { _correctChoice = value; } 
    }

    // Replace "{name}"s with the NPCs first name
    public string FormattedDisplayText(NPC npc) 
    {
        if(npc.charName == null) return DisplayText;
        return DisplayText.Replace("{name}", npc.charName.Item1);
    }
}