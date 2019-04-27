using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using Unity.Physics;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Transforms;
using Collider = Unity.Physics.Collider;
using BoxCollider = Unity.Physics.BoxCollider;

[Serializable]
public struct TargetData : IComponentData
{
    public float movementSpeed;
    public float slowingDistance;
}

[Serializable]
public struct EnemyData : IComponentData
{
    public float movementSpeed;
    public float slowingDistance;
    public float minNeighbourDist;
    public float attackRange;
    public float fleeDistance;
    public float maxSpeed;
    public float damping;
    public float3 force;
    public float3 acceleration;
    public float3 velocity;
    public float mass;
    public Quaternion rotation;
    public bool shouldDestroy;
    public bool inRange;
    public bool fleeing;
}

public struct EnemySpawnData : IComponentData
{
    public Entity prefab;
    public int countX;
    public int countY;
}

//public struct ColliderData : IComponentData
//{
//    public BoxCollider col;
//}

public struct TargetSpawnData : IComponentData
{
    public Entity prefab;
    public float3 spawnPos;
}

