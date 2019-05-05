using Unity.Entities;
using Unity.Jobs;
using System;
using Unity.Physics;
using Unity.Mathematics;
using Unity.Collections;
using UnityEngine;
using Unity.Transforms;
using System.Collections.Generic;

public class ProjectileSpawner : MonoBehaviour, IDeclareReferencedPrefabs, IConvertGameObjectToEntity
{
    //public static NativeArray<ProjectileData> projectilesList = new NativeArray<ProjectileData>();

    //public EntityCommandBuffer CommandBuffer;

    [SerializeField] public ProjectileSpawnDataLocal spawnData;
    [SerializeField] public ProjectileData data;
    public static ProjectileData _projectileData;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        var spawnerData = new ProjectileSpawnData
        {
            prefab = conversionSystem.GetPrimaryEntity(spawnData.prefab),
        };

        dstManager.AddComponentData(entity, spawnerData);

        _projectileData = data;
        var projectileData = new ProjectileData
        {
            speed = data.speed,
            target = data.target
        };

        dstManager.AddComponentData(entity, projectileData);
    }

    public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
    {
        referencedPrefabs.Add(spawnData.prefab);
    }

    //private void Awake()
    //{
    //    CommandBuffer = World.Active.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>().CreateCommandBuffer();
    //    //projectilesList = new NativeList<ProjectileData>();
    //}
    //private void Update()
    //{
    //    var projectiles = projectilesList.ToArray();
    //    for (int i = 0; i < projectiles.Length; i++)
    //    {
    //        var entity = CommandBuffer.CreateEntity(ComponentData.projectileArchtype);
    //        CommandBuffer.SetComponent(entity, new Translation { Value = projectiles[i].startingPos });
    //        CommandBuffer.SetComponent(entity, new Rotation { Value = quaternion.identity });
    //        CommandBuffer.SetComponent(entity, new ProjectileData
    //        {
    //            speed = projectiles[i].speed,
    //            startingPos = projectiles[i].startingPos,
    //            target = projectiles[i].target
    //        });

    //        if (i == 0)
    //        {
    //            CommandBuffer.AddSharedComponent(entity, ComponentData.projectileSpawnData.mesh);
    //        }

    //    }

    //projectilesList.Clear();
    //}

    [Serializable]
    public struct ProjectileSpawnDataLocal
    {
        public GameObject prefab;
    }
}

