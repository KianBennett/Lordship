﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum CharacterAge { Youthful, MiddleAged, Elderly }
public enum CharacterWealth { Rich, GettingBy, Poor }
public enum CharacterOccupation { Monk, Knight, Craftsman, Homemaker, Farmer, Politician }
public enum DispositionType { Positive, Neutral, Negative }

public class NPC : Character 
{
    [SerializeField] private GameObject rumourIndicator;

    [Header("Attributes")]
    public System.Tuple<string, string> charName;
    public CharacterAge age;
    public CharacterOccupation occupation;
    public CharacterWealth wealth;
    public int disposition; // 0 to 100

    [Header("Movement")]
    public bool wander;
    private float pauseTimer;

    // When the player chooses to flatter, threaten or bribe that option gets locked off until the next day
    private Dictionary<DialogueType, bool> usedDialogueOptions;

    public string DisplayName { get { 
        if(charName != null) return charName.Item1 + " " + charName.Item2; 
            else return "";
    } }

    public DispositionType DispositionType { get { 
        if(disposition < 35) return DispositionType.Negative;
            else if(disposition > 70) return DispositionType.Positive;
            else return DispositionType.Neutral;
    } }

    protected override void Awake() 
    {
        base.Awake();

        // Give random initial delay so loads of paths don't get calculated all at once
        pauseTimer = Random.Range(0f, 3f);
        usedDialogueOptions = new Dictionary<DialogueType, bool>() 
        {
            { DialogueType.Flatter, false},  
            { DialogueType.Threaten, false}, 
            { DialogueType.Bribe, false}
        };
    }

    protected override void Update() 
    {
        base.Update();

        if(wander && !Movement.HasTarget && !Movement.IsSpeaking && pauseTimer <= 0) 
        {
            // Set a max distance the NPC can move to to avoid slowing down too much
            GridPoint[] roadGridPoints = TownGenerator.instance.RoadGridPoints.Where(o => Vector3.Distance(TownGenerator.instance.GridPointToWorldPos(o), transform.position) < 30).ToArray();
            GridPoint target = roadGridPoints[Random.Range(0, roadGridPoints.Length)];
            Movement.MoveToPoint(TownGenerator.instance.GridPointToWorldPos(target), false, false);
            bool pause = Random.value > 0.75f; // 75% chance to pause after reaching target
            pauseTimer = pause ? Random.Range(1f, 5f) : 0;
        }
        if(pauseTimer > 0) pauseTimer -= Time.deltaTime;
    }

    // Assign random attributes
    public void Randomise() 
    {
        Appearance.Randomise(false, false);
        Movement.LookDir = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));

        charName = AssetManager.instance.GetUniqueNpcName();
        age = (CharacterAge) Random.Range(0, System.Enum.GetNames(typeof(CharacterAge)).Length);
        occupation = (CharacterOccupation) Random.Range(0, System.Enum.GetNames(typeof(CharacterOccupation)).Length - 1); // -1 to not include politician
        wealth = (CharacterWealth) Random.Range(0, System.Enum.GetNames(typeof(CharacterWealth)).Length);
        disposition = Random.Range(40, 60);

        // If poor then high chance of wearing rags and fingerless gloves
        if(wealth == CharacterWealth.Poor && Random.value > 0.2f) 
        {
            Appearance.hands = 2;
            Appearance.body = 3;
        }
        if(occupation == CharacterOccupation.Monk) 
        {
            // Monk hairstyle
            Appearance.hair = 2;
            Appearance.hat = 0;
        }
        if(occupation == CharacterOccupation.Knight) 
        {
            // Helmet and armour
            Appearance.hat = 5;
            Appearance.body = 2;
            Appearance.hair = 0; // Bald head so hair doesn't clip through helmet
        }

        // Randomise colours AFTER setting body parts so it picks colours from the new palettes
        Appearance.RandomiseColours();
        Appearance.ApplyAll();
    }

    public override void SetHovered(bool hovered) 
    {
        base.SetHovered(hovered);

        if(hovered) HUD.instance.tooltip.Show(DisplayName);
            else HUD.instance.tooltip.Hide();

        Appearance.SetHighlighted(hovered);
    }

    public override void OnRightClick() 
    {
        PlayerController.instance.TalkToNpc(this);
    }

    public void ChangeDisposition(int change) 
    {
        // Clamp disposition to 0-100
        disposition = Mathf.Clamp(disposition + change, 0, 100);
        HUD.instance.dialogueMenu.UpdateDispositionBar(disposition);
        if(change > 0) 
        {
            AudioManager.instance.sfxDispositionRaise.PlayAsSFX();
        } 
        else if(change < 0) 
        {
            AudioManager.instance.sfxDispositionLower.PlayAsSFX();
        }
    }

    public bool RespondToFlattery(bool success) 
    {
        usedDialogueOptions[DialogueType.Flatter] = true;

        if(success) 
        {
            int disposition = Random.Range(14, 20);
            if(occupation == CharacterOccupation.Knight) disposition += 10; // Knights particularly enjoy flattery
            ChangeDisposition(disposition);
        }
        else 
        {
            ChangeDisposition(-Random.Range(14, 20));
        }
        return success;
    }

    public bool RespondToThreaten(bool success) 
    {
        usedDialogueOptions[DialogueType.Threaten] = true;

        if(occupation == CharacterOccupation.Knight) success = false; // Knights can't be threatened

        if(success) ChangeDisposition(Random.Range(14, 20));
            else ChangeDisposition(-Random.Range(14, 20));

        return success;
    }

    // bribeAmount = 0 (low), 1 (mid), 2 (height), returns success
    public bool ReceiveBribe(int bribeAmount) 
    {
        usedDialogueOptions[DialogueType.Bribe] = true;

        // Monks and Politicians hate being bribed (by a rival polition at least)
        if(occupation == CharacterOccupation.Monk || occupation == CharacterOccupation.Politician) 
        {
            ChangeDisposition(-40);
            return false;
        }
        // Wealthy citizens need high bribes
        if(wealth == CharacterWealth.Rich) 
        {
            if(bribeAmount == 2) 
            {
                ChangeDisposition(20);
                return true;
            }
            else 
            {
                ChangeDisposition(-30);
                return false;
            }
        }
        // Young and poor people will always respond well to bribes
        if(age == CharacterAge.Youthful || wealth == CharacterWealth.Poor) 
        {
            ChangeDisposition(10 + bribeAmount * 10);
            return true;
        }

        if(bribeAmount > 0) 
        {
            ChangeDisposition(bribeAmount * 10);
            return true;
        } 
        else 
        {
            ChangeDisposition(-8);
            return false;
        }
    }

    public bool HasUsedDialogueType(DialogueType type) 
    {
        if(usedDialogueOptions.ContainsKey(type)) return usedDialogueOptions[type];
        return false;
    }

    public void ResetUsedDialogueTypes() 
    {
        usedDialogueOptions[DialogueType.Flatter] = false;
        usedDialogueOptions[DialogueType.Threaten] = false;
        usedDialogueOptions[DialogueType.Bribe] = false;
    }

    public bool CanGiveRumour() 
    {
        return disposition > 60;
    }

    public void CompleteRumour() 
    {
        ChangeDisposition(Random.Range(40, 50));
        ShowRumourIndicator(false);
    }

    // Most enums can be represented by a string from a direct cast, but some need changes (spaces, hyphons etc)
    public string GetAgeString() 
    {
        if(age == CharacterAge.MiddleAged) return "Middle-aged";
            else return age.ToString();
    }

    public string GetWealthString() 
    {
        if(wealth == CharacterWealth.GettingBy) return "Getting by";
            else return wealth.ToString();
    }

    public string GetOccupationString() 
    {
        return occupation.ToString();
    }

    public void ShowRumourIndicator(bool show) 
    {
        rumourIndicator.SetActive(show);
    }
}
