using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using System;

public class SpawnerBootstrap : MonoBehaviour, IDeclareReferencedPrefabs, IConvertGameObjectToEntity
{
    [SerializeField] public EnemySpawnDataLocal enemySpawnData;
    [SerializeField] public EnemyData enemyData;

    public void DeclareReferencedPrefabs(List<GameObject> gameObjects)
    {
        gameObjects.Add(enemySpawnData.prefab);
    }

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
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
            force = enemyData.force,
            acceleration = enemyData.acceleration,
            velocity = enemyData.velocity,
            mass = enemyData.mass
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
