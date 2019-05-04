using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ValueAdjuster : MonoBehaviour
{
    [Range(1, 100)] public float ArriveWeight = 1;
    [Range(1, 100)] public float AllignmentWeight = 1;
    [Range(1, 100)] public float SeperationWeight = 1;
    [Range(1, 100)] public float CohesionWeight = 1;
    [Range(1, 100)] public float fleeWeight = 1;
    [Range(1, 100)] public float cellSize = 10;
    [Range(1, 10000)] public float gridSize = 2000;
    [Range(1, 100)] public float boidMass = 1;
    [Range(1, 100)] public float boidDamping = 1;
    [Range(0, 100)] public float boidMaxSpeed = 1;
    public Transform target;

    private void Awake()
    {
        BoidECS.targetPos = target.position;
    }
    void Update()
    {
        BoidECS.AllignmentWeight = AllignmentWeight;
        BoidECS.ArriveWeight = ArriveWeight;
        BoidECS.CohesionWeight = CohesionWeight;
        BoidECS.SeperationWeight = SeperationWeight;
        BoidECS.fleeWeight = fleeWeight;
        BoidECS.targetPos = target.position;
        BoidECS.cellSize = cellSize;
        BoidECS.gridSize = gridSize;
        BoidECS.boidMass = boidMass;
        BoidECS.boidDamping = boidDamping;
        BoidECS.boidMaxSpeed = boidMaxSpeed;
    }
}
