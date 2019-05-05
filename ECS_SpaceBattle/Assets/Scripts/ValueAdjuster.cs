using System.Collections;
using System.Collections.Generic;
using Unity.Transforms;
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
    [Range(0, 1)] public float boidDamping = 1;
    [Range(0, 100)] public float boidMaxSpeed = 1;
    [Range(1, 100)] public float boidBanking = 1;
    [Range(1, 100)] public float boidWeight = 1;
    [Range(1, 100)] public  float moveSpeed = 1;
    [Range(1, 100)] public  float slowingDistance = 1;
    [Range(1, 100)] public  float stopRange = 1;
    [Range(1, 100)] public float maxForce = 1;
    [Range(1, 100)] public float maxNeighbourDist = 10;
    [Range(1, 100)] public float constrainWeight = 1;
    public Transform target;
    public Vector3 shipPos;
    private void Awake()
    {
        //BoidECS.targetPos = new Translation {Value = target.position};
        shipPos = ShipController.posArray[0];
        BoidECS.targetPos = new Translation { Value = shipPos };
        ShipController.targetPos = new Translation { Value = target.position };
    }
    void Update()
    {
        //return;
        BoidECS.AllignmentWeight = AllignmentWeight;
        BoidECS.ArriveWeight = ArriveWeight;
        BoidECS.CohesionWeight = CohesionWeight;
        BoidECS.SeperationWeight = SeperationWeight;
        BoidECS.fleeWeight = fleeWeight;
        
        BoidECS.cellSize = cellSize;
        BoidECS.gridSize = gridSize;
        BoidECS.boidMass = boidMass;
        BoidECS.boidDamping = boidDamping;
        BoidECS.boidMaxSpeed = boidMaxSpeed;
        BoidECS.boidWeight = boidWeight;
        BoidECS.boidBanking = boidBanking;
        BoidECS.moveSpeed = moveSpeed;
        BoidECS.slowingDistance = slowingDistance;
        BoidECS.stopRange = stopRange;
        BoidECS.maxForce = maxForce;
        BoidECS.maxNeighbourDist = maxNeighbourDist;
        BoidECS.constrainWeight = constrainWeight;

        shipPos = ShipController.posArray[0];
        BoidECS.targetPos = new Translation { Value = shipPos };
        ShipController.targetPos = new Translation { Value = target.position };

    }
}
