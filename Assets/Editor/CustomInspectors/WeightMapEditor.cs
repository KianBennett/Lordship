using UnityEngine;
using UnityEditor;
using System.Linq;

// Custom inspector for WeightMap that adds sliders for each value

public class WeightMapEditor<T> : Editor 
{
    protected WeightMap<T> weightMap;
    protected string[] names;

    void OnEnable() 
    {
        weightMap = (WeightMap<T>) target;
        //Debug.Log(weightMap.WeightMap.Keys.Count + ", " + weightMap.WeightMap.Values.Count);

        setValues();
        if (names == null) names = weightMap.Map.Keys.Select(o => o.ToString()).ToArray();
    }

    protected virtual void setValues() 
    {
        // Overriden in parent classes, calls initValues()
    }

    protected void initValues(T[] list, string[] names) 
    {
        if (list == null || names == null) return;
        if (weightMap.Map == null || weightMap.Map.Keys.Count != list.Length) {
            // If the map size has changed then create a copy of the map and if the new map contains the same keys, use the old map's values
            SerializableDictionary<T, float> oldMap = new SerializableDictionary<T, float>(weightMap.Map);
            weightMap.Map.Clear();
            foreach (T t in list) 
            {
                if(oldMap.ContainsKey(t)) 
                {
                    weightMap.Map.Add(t, oldMap.GetValue(t));
                } 
                else
                {
                    weightMap.Map.Add(t, 1);
                }
            }
        }
        this.names = names;
    }

    public override void OnInspectorGUI() 
    {
        EditorGUI.BeginChangeCheck();

        DrawDefaultInspector();

        // A bit of a hack, but if any of the non-dictionary values have changed (e.g. palette type) then reset the dictionary
        if (EditorGUI.EndChangeCheck()) 
        {
            setValues();
        }

        for (int i = 0; i < weightMap.Map.Keys.Count; i++) 
        {
            T t = weightMap.Map.Keys.ElementAt(i);

            GUILayout.BeginHorizontal();

            GUI.enabled = false;
            EditorGUILayout.Popup("", i, names, EditorStyles.popup);
            GUI.enabled = true;

            weightMap.Map.SetValue(t, EditorGUILayout.Slider(weightMap.Map.GetValue(t), 0, 1, GUILayout.MaxWidth(250)));

            GUILayout.EndHorizontal();
        }

        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(target);
    }
}

[CustomEditor(typeof(MeshWeightMap))]
public class MeshWeightMapEditor : WeightMapEditor<MeshMaterialSet> 
{
    protected override void setValues() 
    {
        BodyPart bodyPart = ((MeshWeightMap) target).bodyPart;
        MeshMaterialSet[] meshes = AssetManager.instance.GetMeshesFromBodyPart(bodyPart);
        initValues(meshes, meshes.Select(o => o.name).ToArray());
    }
}

[CustomEditor(typeof(ColourWeightMap))]
public class ColourWeightMapEditor : WeightMapEditor<PaletteColour> 
{
    protected override void setValues() 
    {
        ColourPalette palette = ((ColourWeightMap) target).ColourPalette;
        if (palette != null) initValues(palette.colours, palette.colours.Select(o => o.name).ToArray());
    }
}