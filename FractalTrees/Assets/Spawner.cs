//using System.Collections;
//using System.Collections.Generic;
//using JetBrains.Annotations;
//using Unity.Burst;
//using Unity.Collections;
//using Unity.Entities;
//using Unity.Jobs;
//using Unity.Transforms;
//using ComponentData;
//using UnityEngine;

//public class Spawner : JobComponentSystem
//{
//    public static EntityCommandBuffer entityCommandBuffer;
//    protected override void OnCreateManager()
//    {
//        entityCommandBuffer = new EntityCommandBuffer();

//        //entityCommandBuffer.Instantiate(SetData.enemyData.prefab);
//    }

//    protected override JobHandle OnUpdate(JobHandle inputDeps)
//    {
//        return new SpawnJob()
//        {

//        }.Schedule(inputDeps);
//    }
//    private struct SpawnJob : IJob
//    {
//        public void Execute()
//        {
//            throw new System.NotImplementedException();
//        }
//    }
//}
