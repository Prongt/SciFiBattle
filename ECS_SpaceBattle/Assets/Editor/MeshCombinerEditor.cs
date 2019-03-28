using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MeshCombiner))]
public class MeshCombinerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var myScript = (MeshCombiner) target;
        if (GUILayout.Button("Combine Mesh")) myScript.CreateObject();
    }
}