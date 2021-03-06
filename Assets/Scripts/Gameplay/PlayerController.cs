using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

// Interface between input and player actions

public class PlayerController : Singleton<PlayerController> 
{
    [SerializeField] private Character playerPrefab;

    [ReadOnly] public Character characterHovered;
    [ReadOnly] public NPC npcFollowing, npcSpeaking;
    [ReadOnly] public Character playerCharacter;

    public bool IsInDialogue { get { return npcSpeaking; } }

    protected override void Awake() 
    {
        base.Awake();

        playerCharacter = Instantiate(playerPrefab, transform.position, Quaternion.identity);
        playerCharacter.Appearance.Randomise(true, true);
    }

    void Update() 
    {
        // If following an npc and they get within a certain range, start dialogue
        if(npcFollowing != null && playerCharacter.Movement.FollowedCharacterDistance() < 3) 
        {
            startDialogue();
        }
    }

    // Set player position to a random road grid point
    public void ResetPlayerPosition() 
    {
        Vector2 townSize = new Vector2(TownGenerator.instance.Width, TownGenerator.instance.Height);
        // Make sure the player doesn't start at a point too close to the edge of town
        GridPoint[] possibleStartingPoints = TownGenerator.instance.RoadGridPoints.Where(
            o => o.x > townSize.x * 0.25f && o.x < townSize.x * 0.75f && o.y > townSize.y * 0.25f && o.y < townSize.y * 0.75f).ToArray();
        GridPoint startingPoint = possibleStartingPoints[Random.Range(0, possibleStartingPoints.Length)];

        playerCharacter.transform.position = TownGenerator.instance.GridPointToWorldPos(startingPoint);
        playerCharacter.transform.rotation = Quaternion.Euler(0, 225, 0);
        playerCharacter.Movement.LookDir = playerCharacter.transform.forward;
        CameraController.instance.SetPositionImmediate(playerCharacter.transform.position);
        CameraController.instance.transform.rotation = Quaternion.Euler(0, 15, 0);
        CameraController.instance.ResetCameraDist();
    }

    // Move a character to a target position
    public void MoveCharacter(Vector3 target, bool showMarker) 
    {
        playerCharacter.Movement.MoveToPoint(target, true, showMarker);
        CancelFollowing();
    }

    public void TalkToNpc(NPC npc) 
    {
        npcFollowing = npc;
        playerCharacter.Movement.FollowCharacter(npc, true);
    }

    public void CancelFollowing() 
    {
        npcFollowing = null;
        playerCharacter.Movement.CancelFollowing();
    }

    private void startDialogue() 
    {
        npcSpeaking = npcFollowing;
        CancelFollowing();
        playerCharacter.Movement.SetSpeaking(npcSpeaking);
        npcSpeaking.Movement.SetSpeaking(playerCharacter);
        CameraController.instance.SetInDialogue(npcSpeaking, playerCharacter);    
        DialogueSystem.instance.DisplayBeat(1, true);
    }

    public void ExitDialogue() 
    {
        if(npcSpeaking == null) return;
        NPC speaking = npcSpeaking;
        npcSpeaking = null;
        DialogueSystem.instance.ExitDialogue();
        speaking.Movement.SetSpeaking(null);
        playerCharacter.Movement.SetSpeaking(null);
        CameraController.instance.CancelDialogue();
    }
}
