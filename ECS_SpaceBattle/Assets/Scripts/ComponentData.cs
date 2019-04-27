using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;



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
    public float3 force;
    public float3 acceleration;
    public float3 velocity;
    public float mass;
    public Quaternion rotation;
    public bool shouldDestroy;
    public bool inRange;
}

public struct EnemySpawnData : IComponentData
{
    public Entity prefab;
    public int countX;
    public int countY;
}

public struct TargetSpawnData : IComponentData
{
    public Entity prefab;
    public float3 spawnPos;
}

