using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ValueAdjuster : MonoBehaviour
{
    [Range(1, 20)] public float ArriveWeight = 1;
    [Range(1, 20)] public float AllignmentWeight = 1;
    [Range(1, 20)] public float SeperationWeight = 1;
    [Range(1, 20)] public float CohesionWeight = 1;

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
        BoidECS.targetPos = target.position;
    }
}
