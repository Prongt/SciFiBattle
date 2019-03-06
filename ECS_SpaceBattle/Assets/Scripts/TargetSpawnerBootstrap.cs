using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class TargetSpawnerBootstrap : MonoBehaviour, IDeclareReferencedPrefabs, IConvertGameObjectToEntity
{
    [SerializeField] public TargetSpawnDataLocal targetSpawnDataLocal;
    [SerializeField] public TargetData targetData;

    public void DeclareReferencedPrefabs(List<GameObject> gameObjects)
    {
        gameObjects.Add(targetSpawnDataLocal.prefab);
    }


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
            slowingDistance = targetData.slowingDistance,
        };

        dstManager.AddComponentData(entity, tData);
    }

    [Serializable]
    public struct TargetSpawnDataLocal
    {
        public GameObject prefab;
        public float3 spawnPos;
    }
}
