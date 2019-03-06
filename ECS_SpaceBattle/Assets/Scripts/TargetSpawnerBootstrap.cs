using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class TargetSpawnerBootstrap : MonoBehaviour, IDeclareReferencedPrefabs, IConvertGameObjectToEntity
{
    [SerializeField] public TargetData targetData;
    [SerializeField] public TargetSpawnDataLocal targetSpawnDataLocal;


    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        var spawnerData = new TargetSpawnData
        {
            prefab = conversionSystem.GetPrimaryEntity(targetSpawnDataLocal.prefab),
            spawnPos = targetSpawnDataLocal.spawnPos
        };


        dstManager.AddComponentData(entity, spawnerData);


        var tData = new TargetData
        {
            movementSpeed = targetData.movementSpeed,
            slowingDistance = targetData.slowingDistance
        };

        dstManager.AddComponentData(entity, tData);
    }

    public void DeclareReferencedPrefabs(List<GameObject> gameObjects)
    {
        gameObjects.Add(targetSpawnDataLocal.prefab);
    }

    [Serializable]
    public struct TargetSpawnDataLocal
    {
        public GameObject prefab;
        public float3 spawnPos;
    }
}