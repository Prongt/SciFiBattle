//using Unity.Entities;
//using Unity.Jobs;
//using System;
//using Unity.Physics;
//using Unity.Mathematics;
//using Unity.Collections;
//using UnityEngine;
//using Unity.Transforms;
//using System.Collections.Generic;

//public class ProjectileSpawner : MonoBehaviour
//{
//    public static NativeArray<ProjectileData> projectilesList = new NativeArray<ProjectileData>();
//    //public List<ProjectileData> projectiles = new List<ProjectileData>();
//    //protected override JobHandle OnUpdate(JobHandle inputDeps)
//    //{
//    //    var spawn = new ProjectileSpawnJob
//    //    {
//    //        CommandBuffer = World.Active.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>().CreateCommandBuffer(),
//    //        data = new NativeArray<ProjectileData>(ComponentData.projectiles.ToArray(), Allocator.TempJob)
//    //    }.Schedule();
//    //    spawn.Complete();

//    //    return spawn;
//    //}

//    public EntityCommandBuffer CommandBuffer;

//    private void Awake()
//    {
//        CommandBuffer = World.Active.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>().CreateCommandBuffer();
//        //projectilesList = new NativeList<ProjectileData>();
//    }
//    private void Update()
//    {
//        var projectiles = projectilesList.ToArray();
//        for (int i = 0; i < projectiles.Length; i++)
//        {
//            var entity = CommandBuffer.CreateEntity(ComponentData.projectileArchtype);
//            CommandBuffer.SetComponent(entity, new Translation { Value = projectiles[i].startingPos });
//            CommandBuffer.SetComponent(entity, new Rotation { Value = quaternion.identity });
//            CommandBuffer.SetComponent(entity, new ProjectileData
//            {
//                speed = projectiles[i].speed,
//                startingPos = projectiles[i].startingPos,
//                target = projectiles[i].target
//            });

//            if (i == 0)
//            {
//                CommandBuffer.AddSharedComponent(entity, ComponentData.projectileSpawnData.mesh);
//            }

//        }

//        //projectilesList.Clear();
//    }
//}

//public struct ProjectileSpawnJob : IJob
//{
//    [ReadOnly] public EntityCommandBuffer CommandBuffer;
//    //[ReadOnly] public NativeList<ProjectileData> projectilesLocal;
//    public NativeArray<ProjectileData> data;
//    public void Execute()
//    {
//        for (int i = 0; i < data.Length; i++)
//        {
//            var entity = CommandBuffer.CreateEntity(ComponentData.projectileArchtype);
//            CommandBuffer.SetComponent(entity, new Translation { Value = data[i].startingPos });
//            CommandBuffer.SetComponent(entity, new Rotation { Value = quaternion.identity });
//            CommandBuffer.SetComponent(entity, new ProjectileData
//            {
//                speed = data[i].speed,
//                startingPos = data[i].startingPos,
//                target = data[i].target
//            });

//            if (i == 0)
//            {
//                CommandBuffer.AddSharedComponent(entity, ComponentData.projectileSpawnData.mesh);
//            }
            
//        }

//        //ComponentData.projectiles.Clear();
//    }
//}
