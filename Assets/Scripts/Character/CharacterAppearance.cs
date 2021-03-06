using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Playables;
using UnityEngine.Animations;

// Handles setting meshes and materials for each body part of the character

public enum BodyPart { Hair, Body, Hands, Hat }

public class CharacterAppearance : MonoBehaviour 
{

    [SerializeField] private Animator modelAnim;
    [SerializeField] private Character character;
    [SerializeField] private Transform armature;
    [SerializeField] private Transform headBone;
    [SerializeField] private SkinnedMeshRenderer headMesh, hairMesh, bodyMesh, handLMesh, handRMesh;
    [SerializeField] private MeshFilter hatMeshFilter;
    [SerializeField] private MeshRenderer hatMeshRenderer;
    [SerializeField] private Transform selectionCone;
    [SerializeField] private bool randomiseOnAwake;

    [HideInInspector] public int hair, hairColour1, hairColour2;
    [HideInInspector] public int body, bodyColour1, bodyColour2;
    [HideInInspector] public int hands, handsColour1, handsColour2;
    [HideInInspector] public int hat, hatColour1, hatColour2;
    [HideInInspector] public int skinColour;

    private float headBoneHeightInit;
    private Renderer[] renderers;
    private float selectionConeHeight;

    void Awake() 
    {
        if (randomiseOnAwake) Randomise(true, true);
            else ApplyAll();

        headBoneHeightInit = headBone.transform.position.y;
        renderers = GetComponentsInChildren<Renderer>();
        if(selectionCone) selectionConeHeight = selectionCone.localPosition.y;
    }

    void Update() 
    {
        // Lerp towards the desired animation speed
        if (modelAnim && character && character.Movement) {
            float speed = Mathf.Lerp(modelAnim.GetFloat("Speed"), character.Movement.AnimSpeed, Time.deltaTime * 10);
            modelAnim.SetFloat("Speed", speed);
        }

        // Bob the cone up and down
        if(selectionCone != null) {
            selectionCone.localPosition = Vector3.up * (selectionConeHeight + 0.08f * Mathf.Sin(Time.time * 2.5f));
        }

        // Debug correct hair mesh position
        // Debug.DrawRay(transform.position + Vector3.up * hairMesh.sharedMesh.bounds.max.y, Vector3.up);
        // Debug.DrawLine(transform.position + Vector3.up * hairMesh.sharedMesh.bounds.min.y * modelAnim.transform.localScale.y, transform.position + Vector3.up * hairMesh.sharedMesh.bounds.max.y * modelAnim.transform.localScale.y, Color.white);
    }

    private void applyHair() 
    {
        MeshMaterialSet meshSet = AssetManager.instance.hairMeshes[hair];
        updateMeshRenderer(hairMesh, meshSet.mesh);
        updateMeshMaterials(hairMesh, meshSet, hairColour1, hairColour2);
        // if(hairMesh.sharedMesh) hairMesh.sharedMesh.RecalculateBounds();
        updateHatPos();
    }

    private void applyBody() 
    {
        MeshMaterialSet meshSet = AssetManager.instance.bodyMeshes[body];
        updateMeshRenderer(bodyMesh, meshSet.mesh);
        updateMeshMaterials(bodyMesh, meshSet, bodyColour1, bodyColour2);
    }

    private void applyHands() 
    {
        MeshPairMaterialSet meshSet = AssetManager.instance.handMeshes[hands];
        updateMeshRenderer(handLMesh, meshSet.left);
        updateMeshRenderer(handRMesh, meshSet.right);
        updateMeshMaterials(handLMesh, meshSet, handsColour1, handsColour2);
        updateMeshMaterials(handRMesh, meshSet, handsColour1, handsColour2);
    }

    private void applyHat() 
    {
        MeshMaterialSet meshSet = AssetManager.instance.hatMeshes[hat];
        updateMeshFilter(hatMeshFilter, meshSet.mesh);
        updateMeshMaterials(hatMeshRenderer, meshSet, hatColour1, hatColour2);
        updateHatPos();
    }

    // Set hat position to top of hair (or head if there is no hair or the hair is below the top of the head, like the monk)
    private void updateHatPos() 
    {
        float headHeight = headMesh.sharedMesh.bounds.max.y;
        float hairHeight = hairMesh.sharedMesh != null ? hairMesh.sharedMesh.bounds.max.y : 0;
        float hatHeight = Mathf.Max(hairHeight, headHeight) * modelAnim.transform.localScale.y;
        // If called in the editor headBoneHeightInit won't be set so check if it's 0 or not
        float heightFromHeadBone = hatHeight - (headBoneHeightInit != 0 ? headBoneHeightInit : headBone.transform.position.y) + transform.position.y;
        // Need to do along the angle of the head bone incase hat is switched while animating
        hatMeshFilter.transform.position = headBone.transform.position + headBone.transform.up * heightFromHeadBone;
    }

    private void applySkin() 
    {
        setMeshMaterialColour(headMesh, 0, getSkinColor());
        applyBody();
        applyHands();
    }

    public void ApplyAll() 
    {
        applyHair();
        applySkin(); // applyBody and applyHands are both called from applySkin
        applyHat();
    }

    public void Randomise(bool includeColours, bool apply) 
    {
        // Random seed from system time
        Random.InitState((int) System.DateTime.UtcNow.Ticks);

        hair = AssetManager.instance.hairWeightMap.GetRandomIndex();
        body = AssetManager.instance.bodyWeightMap.GetRandomIndex();
        hands = AssetManager.instance.handsWeightMap.GetRandomIndex();
        hat = AssetManager.instance.hatWeightMap.GetRandomIndex();

        if(includeColours) RandomiseColours();
        if(apply) ApplyAll();
    }

    public void RandomiseColours() 
    {
        ColourPalette palette;
        if (palette = AssetManager.instance.hairMeshes[hair].materials.colourPalette1) hairColour1 = palette.RandomColourIndex();
        if (palette = AssetManager.instance.hairMeshes[hair].materials.colourPalette2) hairColour2 = palette.RandomColourIndex();
        if (palette = AssetManager.instance.bodyMeshes[body].materials.colourPalette1) bodyColour1 = palette.RandomColourIndex();
        if (palette = AssetManager.instance.bodyMeshes[body].materials.colourPalette2) bodyColour2 = palette.RandomColourIndex();
        if (palette = AssetManager.instance.handMeshes[hands].materials.colourPalette1) handsColour1 = palette.RandomColourIndex();
        if (palette = AssetManager.instance.handMeshes[hands].materials.colourPalette2) handsColour2 = palette.RandomColourIndex();
        if (palette = AssetManager.instance.hatMeshes[hat].materials.colourPalette1) hatColour1 = palette.RandomColourIndex();
        if (palette = AssetManager.instance.hatMeshes[hat].materials.colourPalette2) hatColour2 = palette.RandomColourIndex();
        skinColour = AssetManager.instance.skinColours.weightMap.GetRandomIndex();
    }

    private Color getSkinColor() 
    {
        if (skinColour < AssetManager.instance.skinColours.colours.Length) return AssetManager.instance.skinColours.colours[skinColour].colour;
        return Color.white;
    }

    private void updateMeshMaterials(Renderer renderer, MeshMaterialSet materialSet, int colour1, int colour2) 
    {
        List<Material> materials = new List<Material>();

        if (materialSet.materials.hasColour1) 
        {
            materials.Add(AssetManager.GetColouredMaterial(materialSet.materials.colourPalette1.colours[colour1].colour));
        }
        if (materialSet.materials.hasColour2) 
        {
            materials.Add(AssetManager.GetColouredMaterial(materialSet.materials.colourPalette2.colours[colour2].colour));
        }
        if (materialSet.materials.hasSkin) 
        {
            materials.Add(AssetManager.GetColouredMaterial(getSkinColor()));
        }
        foreach (Material material in materialSet.materials.additionalMaterials) {
            materials.Add(material);
        }

        renderer.sharedMaterials = materials.ToArray();
    }

    private void setMeshMaterialColour(Renderer renderer, int materialIndex, Color color) 
    {
        // Get a material from color dictionary or create one if doesn't exist
        Material newMaterial = AssetManager.GetColouredMaterial(color);

        // Copy of the mesh's material array
        Material[] materials = renderer.sharedMaterials;

        // If the array is too small, resize it
        if (materialIndex >= renderer.sharedMaterials.Length) {
            System.Array.Resize(ref materials, materialIndex + 1);
        }

        // Assign the new materials
        materials[materialIndex] = newMaterial;
        renderer.sharedMaterials = materials;

        return;
    }

    private void updateMeshRenderer(SkinnedMeshRenderer renderer, Mesh newMesh) 
    {
        renderer.sharedMesh = newMesh;

        if (renderer.sharedMesh != null) 
        {
            Bounds bounds = renderer.localBounds;
            bounds.center = renderer.sharedMesh.bounds.center;
            bounds.extents = renderer.sharedMesh.bounds.extents;
            renderer.localBounds = bounds;
        }
    }

    // For the hat mesh which uses a MeshFilter + MeshRenderer instead of SkinnedMeshRenderer
    private void updateMeshFilter(MeshFilter filter, Mesh newMesh) 
    {
        filter.sharedMesh = newMesh;
    }

    public void SetHighlighted(bool highlighted) 
    {
        foreach(Renderer renderer in renderers) 
        {
            if(highlighted) 
            {
                Material mat = renderer.material;
                mat.SetColor("_EmissionColor", new Color(0.15f, 0.15f, 0.15f));
                renderer.material = mat;
                
            } 
            else 
            {
                // Set back to original material, since it won't have an emission it should be stored in AssetManager
                Color materialColour = renderer.sharedMaterial.color;
                Material mat = AssetManager.GetColouredMaterial(materialColour);
                renderer.sharedMaterial = mat;
            }
        }
    }

    public void SkipAnimation(string stateName) 
    {
        if (modelAnim.GetCurrentAnimatorStateInfo(0).IsName(stateName)) 
        {
            modelAnim.Play(0, 0, 1);
        }
    }

    public bool IsPlayingAnimation(string name) 
    {
        return modelAnim.GetCurrentAnimatorStateInfo(0).IsName(name);
    }

    public float CurrentAnimationTime() 
    {
        return modelAnim.GetCurrentAnimatorStateInfo(0).normalizedTime;
    }

    // Copy over all values from another character (like with heads on results screen)
    public void CopyAppearanceFromOther(CharacterAppearance other) 
    {
        hat = other.hat;
        hatColour1 = other.hatColour1;
        hatColour2 = other.hatColour2;
        hair = other.hair;
        hairColour1 = other.hairColour1;
        hairColour2 = other.hairColour2;
        body = other.body;
        bodyColour1 = other.bodyColour1;
        bodyColour2 = other.bodyColour2;
        hands = other.hands;
        handsColour1 = other.handsColour1;
        handsColour2 = other.handsColour2;
        skinColour = other.skinColour;
    }
}
