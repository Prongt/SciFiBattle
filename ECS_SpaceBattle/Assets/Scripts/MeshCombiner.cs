using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Unity.Jobs;

public class MeshCombiner : MonoBehaviour
{
    public static Mesh CombineMesh(GameObject meshesToCombine)
    {

        List<CombineInstance> childList = new List<CombineInstance>();

        //Gets all meshes from the object
        MeshFilter[] meshFilters = meshesToCombine.GetComponentsInChildren<MeshFilter>();
        MeshRenderer[] renderers = meshesToCombine.GetComponentsInChildren<MeshRenderer>();
        List<Material> materials = new List<Material>();
        foreach (MeshRenderer renderer in renderers)
        {
            Material[] localMats = renderer.sharedMaterials;
            foreach (Material localMat in localMats)
                if (!materials.Contains(localMat))
                    materials.Add(localMat);
        }

        for (int i = 0; i < meshFilters.Length; i++) //Adds the mesh instance of each child to the list
        {
            MeshFilter mf = meshFilters[i];

            CombineInstance ci = new CombineInstance();

            ci.mesh = mf.mesh;
            ci.transform = mf.transform.localToWorldMatrix;
            childList.Add(ci);
        }

        Mesh combinedMesh = new Mesh();
        combinedMesh.CombineMeshes(childList.ToArray()); // Combines all the mesh instances into one mesh




        meshesToCombine.SetActive(false);
        Destroy(meshesToCombine, 3f);

        return combinedMesh;

        //        var job = new CombineMeshJob
        //        {
        //            meshesToCombine = meshesToCombine,
        //            combinedMesh = combinedMesh
        //        };
        //
        //        var handle = job.Schedule();
        //        handle.Complete();
        //
        //        combinedMesh = job.combinedMesh;
    }
}

public struct CombineMeshJob : IJob
{
    public GameObject meshesToCombine;
    public Mesh combinedMesh;

    public void Execute()
    {
        List<CombineInstance> childList = new List<CombineInstance>();

        MeshFilter[] meshFilters = meshesToCombine.GetComponentsInChildren<MeshFilter>();

        for (int i = 0; i < meshFilters.Length; i++)
        {
            MeshFilter mf = meshFilters[i];

            CombineInstance ci = new CombineInstance();

            ci.mesh = mf.mesh;
            ci.transform = mf.transform.localToWorldMatrix;
            childList.Add(ci);
        }


        combinedMesh.CombineMeshes(childList.ToArray());
    }

    //public void AdvancedMerge()
    //{
    //    // All our children (and us)
    //    MeshFilter[] filters = GetComponentsInChildren(false);

    //    // All the meshes in our children (just a big list)
    //    List materials = new List();
    //    MeshRenderer[] renderers = GetComponentsInChildren(false); // <-- you can optimize this
    //    foreach (MeshRenderer renderer in renderers)
    //    {
    //        if (renderer.transform == transform)
    //            continue;
    //        Material[] localMats = renderer.sharedMaterials;
    //        foreach (Material localMat in localMats)
    //            if (!materials.Contains(localMat))
    //                materials.Add(localMat);
    //    }

    //    // Each material will have a mesh for it.
    //    List submeshes = new List();
    //    foreach (Material material in materials)
    //    {
    //        // Make a combiner for each (sub)mesh that is mapped to the right material.
    //        List combiners = new List();
    //        foreach (MeshFilter filter in filters)
    //        {
    //            if (filter.transform == transform) continue;
    //            // The filter doesn't know what materials are involved, get the renderer.
    //            MeshRenderer renderer = filter.GetComponent();  // <-- (Easy optimization is possible here, give it a try!)
    //            if (renderer == null)
    //            {
    //                Debug.LogError(filter.name + " has no MeshRenderer");
    //                continue;
    //            }

    //            // Let's see if their materials are the one we want right now.
    //            Material[] localMaterials = renderer.sharedMaterials;
    //            for (int materialIndex = 0; materialIndex < localMaterials.Length; materialIndex++)
    //            {
    //                if (localMaterials[materialIndex] != material)
    //                    continue;
    //                // This submesh is the material we're looking for right now.
    //                CombineInstance ci = new CombineInstance();
    //                ci.mesh = filter.sharedMesh;
    //                ci.subMeshIndex = materialIndex;
    //                ci.transform = Matrix4x4.identity;
    //                combiners.Add(ci);
    //            }
    //        }
    //        // Flatten into a single mesh.
    //        Mesh mesh = new Mesh();
    //        mesh.CombineMeshes(combiners.ToArray(), true);
    //        submeshes.Add(mesh);
    //    }

    //    // The final mesh: combine all the material-specific meshes as independent submeshes.
    //    List finalCombiners = new List();
    //    foreach (Mesh mesh in submeshes)
    //    {
    //        CombineInstance ci = new CombineInstance();
    //        ci.mesh = mesh;
    //        ci.subMeshIndex = 0;
    //        ci.transform = Matrix4x4.identity;
    //        finalCombiners.Add(ci);
    //    }
    //    Mesh finalMesh = new Mesh();
    //    finalMesh.CombineMeshes(finalCombiners.ToArray(), false);
    //    myMeshFilter.sharedMesh = finalMesh;
    //    Debug.Log("Final mesh has " + submeshes.Count + " materials.");
    //}
}