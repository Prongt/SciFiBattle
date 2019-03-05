using System;
using Unity.Entities;
using UnityEngine;

namespace ComponentData
{
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
        public Vector3 force;
        public Vector3 acceleration;
        public Vector3 velocity;
        public float mass;
    }

    public struct EnemySpawnData : IComponentData
    {
        public Entity prefab;
        public int spawnCount;
    }
}
