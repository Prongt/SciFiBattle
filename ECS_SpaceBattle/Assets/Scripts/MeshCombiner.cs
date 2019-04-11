using System.Collections;
using System.Collections.Generic;
using Unity.Jobs;
using UnityEditor;
using UnityEngine;

public class MeshCombiner : MonoBehaviour
{
    public GameObject meshesToCombine;

    public void CreateObject()
    {
        CombineMeshes();

        var ship = Instantiate(meshesToCombine, Vector3.zero, Quaternion.identity);
        var child = ship.transform.GetChild(0);
        ship.transform.SetParent(null);
        DestroyImmediate(child.gameObject);

        var path = "Assets/Meshes/" + meshesToCombine.name + ".asset";
        Mesh mesh = ship.GetComponent<MeshFilter>().sharedMesh;
        //mesh.SetTriangles(mesh.triangles, 0);
        //mesh.Optimize();
        AssetDatabase.CreateAsset(mesh, path);
    }

    private void CombineMeshes()
    {
        var materials = new ArrayList();
        var combineInstanceArrays = new ArrayList();
        var meshFilters = meshesToCombine.GetComponentsInChildren<MeshFilter>();

        foreach (var meshFilter in meshFilters)
        {
            var meshRenderer = meshFilter.GetComponent<MeshRenderer>();

            if (!meshRenderer ||
                !meshFilter.sharedMesh ||
                meshRenderer.sharedMaterials.Length != meshFilter.sharedMesh.subMeshCount)
                continue;

            for (var s = 0; s < meshFilter.sharedMesh.subMeshCount; s++)
            {
                var materialArrayIndex = Contains(materials, meshRenderer.sharedMaterials[s].name);
                if (materialArrayIndex == -1)
                {
                    materials.Add(meshRenderer.sharedMaterials[s]);
                    materialArrayIndex = materials.Count - 1;
                }

                combineInstanceArrays.Add(new ArrayList());

                var combineInstance = new CombineInstance();
                combineInstance.transform = meshRenderer.transform.localToWorldMatrix;
                combineInstance.subMeshIndex = s;
                combineInstance.mesh = meshFilter.sharedMesh;
                (combineInstanceArrays[materialArrayIndex] as ArrayList).Add(combineInstance);
            }
        }

        var meshFilterCombine = meshesToCombine.GetComponent<MeshFilter>();
        if (meshFilterCombine == null) meshFilterCombine = meshesToCombine.AddComponent<MeshFilter>();
        var meshRendererCombine = meshesToCombine.GetComponent<MeshRenderer>();
        if (meshRendererCombine == null) meshRendererCombine = meshesToCombine.AddComponent<MeshRenderer>();

        var meshes = new Mesh[materials.Count];
        var combineInstances = new CombineInstance[materials.Count];

        for (var m = 0; m < materials.Count; m++)
        {
            var combineInstanceArray =
                (combineInstanceArrays[m] as ArrayList).ToArray(typeof(CombineInstance)) as CombineInstance[];
            meshes[m] = new Mesh();
            meshes[m].CombineMeshes(combineInstanceArray, true, true);

            combineInstances[m] = new CombineInstance();
            combineInstances[m].mesh = meshes[m];
            combineInstances[m].subMeshIndex = 0;
        }

        meshFilterCombine.sharedMesh = new Mesh();
        meshFilterCombine.sharedMesh.CombineMeshes(combineInstances, false, false);

        foreach (var oldMesh in meshes)
        {
            oldMesh.Clear();
            DestroyImmediate(oldMesh);
        }

        var materialsArray = materials.ToArray(typeof(Material)) as Material[];
        meshRendererCombine.materials = materialsArray;

        //foreach (var meshFilter in meshFilters) DestroyImmediate(meshFilter.gameObject);
    }

    private int Contains(ArrayList searchList, string searchName)
    {
        for (var i = 0; i < searchList.Count; i++)
            if (((Material) searchList[i]).name == searchName)
                return i;
        return -1;
    }
}

public struct CombineMeshJob : IJob
{
    public GameObject meshesToCombine;
    public Mesh combinedMesh;

    public void Execute()
    {
        var childList = new List<CombineInstance>();

        var meshFilters = meshesToCombine.GetComponentsInChildren<MeshFilter>();

        for (var i = 0; i < meshFilters.Length; i++)
        {
            var mf = meshFilters[i];

            var ci = new CombineInstance();

            ci.mesh = mf.mesh;
            ci.transform = mf.transform.localToWorldMatrix;
            childList.Add(ci);
        }

        combinedMesh.CombineMeshes(childList.ToArray());
    }
}