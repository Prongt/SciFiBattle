using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class SpawnerBootstrap : MonoBehaviour, IDeclareReferencedPrefabs, IConvertGameObjectToEntity
{
    [SerializeField] public EnemySpawnDataLocal enemySpawnData;
    [SerializeField] public EnemyData enemyData;
    public static EnemyData _enemyData;

    public void DeclareReferencedPrefabs(List<GameObject> gameObjects)
    {
        gameObjects.Add(enemySpawnData.prefab);
    }

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        _enemyData = enemyData;
        var spawnerData = new EnemySpawnData
        {
            prefab = conversionSystem.GetPrimaryEntity(enemySpawnData.prefab),
            countX = enemySpawnData.countX,
            countY = enemySpawnData.countY
        };
        dstManager.AddComponentData(entity, spawnerData);

        var eData = new EnemyData
        {
            movementSpeed = enemyData.movementSpeed,
            slowingDistance = enemyData.slowingDistance,
            minNeighbourDist = enemyData.minNeighbourDist,
            attackRange = enemyData.attackRange,
            fleeDistance = enemyData.fleeDistance,
            maxSpeed = enemyData.maxSpeed,
            force = enemyData.force,
            acceleration = enemyData.acceleration,
            velocity = enemyData.velocity,
            mass = enemyData.mass,
            shouldDestroy = false,
            rotation = enemyData.rotation,
            inRange = false,
            fleeing = false

        };
        dstManager.AddComponentData(entity, eData);
    }

    [Serializable]
    public struct EnemySpawnDataLocal
    {
        public GameObject prefab;
        public int countX;
        public int countY;
    }
}
