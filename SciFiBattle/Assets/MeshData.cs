using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshData : MonoBehaviour
{
    public Mesh shipMesh;
    public Renderer rend;
    public int numPoints;
    public float gizmoRadius;
    public bool drawGizmos;

    private List<Vector3> pointsInShip;
    void Start()
    {
        pointsInShip = GeneratePointsInMesh(shipMesh, numPoints);
        Debug.Log(pointsInShip.Count);
    }

    public List<Vector3> GeneratePointsInMesh(Mesh mesh, int numPoints)
    {
        List<Vector3> meshPoints = new List<Vector3>();

        Bounds bounds = rend.bounds;

        //for (int x = 0; x < bounds.size.x; x++)
        //{
        //    for (int y = 0; y < bounds.size.y; y++)
        //    {
        //        for (int z = 0; z < bounds.size.z; z++)
        //        {
        //            Vector3 possiblePoint = new Vector3(x, y, z);

        //            if (bounds.Contains(possiblePoint))
        //            {
        //                meshPoints.Add(possiblePoint);
        //            }
        //        } 
        //    }
        //}
        for (int i = 0; i < numPoints; i++)
        {
            
            meshPoints.Add(new Vector3(
                Random.Range(bounds.min.x, bounds.max.x),
                Random.Range(bounds.min.y, bounds.max.y), 
                Random.Range(bounds.min.z, bounds.max.z)
                ));
        }
        
        

        return meshPoints;
    }

    void OnDrawGizmos()
    {
        if (!drawGizmos || !Application.isPlaying)
        {
            return;
        }
        Gizmos.color = Color.red;

        foreach (Vector3 p in pointsInShip)
        {
            Gizmos.DrawSphere(p, gizmoRadius);
        }
    }
}
