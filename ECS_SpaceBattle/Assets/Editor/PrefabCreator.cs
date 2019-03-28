using UnityEditor;
using UnityEngine;

public class PrefabCreator : MonoBehaviour
{
    [MenuItem("CustomEditor/Create Prefab")]
    private static void CreatePrefab()
    {
        var objectArray = Selection.gameObjects;

        foreach (var gameObject in objectArray)
        {
            var localPath = "Assets/Prefabs/" + gameObject.name + ".prefab";

            if (AssetDatabase.LoadAssetAtPath(localPath, typeof(GameObject)))
            {
                if (EditorUtility.DisplayDialog("Are you sure?",
                    "The Prefab already exists. Do you want to overwrite it?",
                    "Yes",
                    "No"))
                    CreateNew(gameObject, localPath);
            }
            else
            {
                Debug.Log(gameObject.name + " is not a Prefab, will convert");
                CreateNew(gameObject, localPath);
            }

            //DestroyImmediate(gameObject);
        }
    }

    [MenuItem("Examples/Create Prefab", true)]
    private static bool ValidateCreatePrefab()
    {
        return Selection.activeGameObject != null;
    }

    private static void CreateNew(GameObject obj, string localPath)
    {
        Object prefab = PrefabUtility.CreatePrefab(localPath, obj);
        PrefabUtility.ReplacePrefab(obj, prefab, ReplacePrefabOptions.ConnectToPrefab);
    }
}