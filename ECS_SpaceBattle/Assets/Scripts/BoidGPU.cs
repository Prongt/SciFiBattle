﻿//using Unity.Burst;
//using Unity.Collections;
//using Unity.Entities;
//using Unity.Jobs;
//using Unity.Transforms;
//using UnityEngine;

//public class BoidGPU : JobComponentSystem
//{
//    protected override JobHandle OnUpdate(JobHandle inputDeps)
//    {
//        return new GetPositions
//        {
//        }.Schedule(this, inputDeps);
//    }
//}


////public struct GPUJob : IJob
////{
////    public Vector3 targetPos;
////    public float deltaTime;
////    public void Execute()
////    {
////        eData[] d = new eData[BoidData.entityList.Count];
////        int index = 0;
////        foreach (var item in BoidData.entityList)
////        {
////            if (index == BoidData.entityList.Count)
////            {
////                continue;
////            }

////            d[index] = new eData
////            {
////                id = item.Key,
////                pos = item.Value,
////                moveSpeed = SpawnerBootstrap._enemyData.movementSpeed,
////                slowingDist = SpawnerBootstrap._enemyData.slowingDistance,
////                deltaTime = deltaTime,
////                targetPos = targetPos
////            };
////            index++;
////        }

////        // 76 is the size of the VecMatPair struct in bytes 
////        // Vector3 is 3 and Matrix4x4 is 16
////        // 76 = (3(Vector3) + 16(Matrix4x4)) * 4(float)
////        var buffer = new ComputeBuffer(BoidData.entityList.Count, 16);
////        buffer.SetData(d);
////        var kernel = BoidData.computeShader.FindKernel("Boid");
////        BoidData.computeShader.SetBuffer(kernel, "dataBuffer", buffer);
////        BoidData.computeShader.Dispatch(kernel, d.Length, 1, 1);

////        data[] b = new data[BoidData.entityList.Count];
////        buffer.GetData(b);

////        buffer.Release();

////        foreach (var item in b)
////        {
////            if (BoidData.entityList.ContainsKey((int) item.id))
////            {
////                BoidData.entityList[(int)item.id] = item.pos;
////            }
////        }
////    }
////}

//[BurstCompile]
//public struct GetPositions : IJobForEachWithEntity<EnemyData, Translation, Rotation>
//{
//    public void Execute(Entity entity, int index, ref EnemyData enemyData, ref Translation pos, ref Rotation rot)
//    {
//        if (BoidData.entityList.ContainsKey(entity.Index))
//        {
//            pos.Value = BoidData.entityList[entity.Index].pos;
//        }
//        else
//        {
//            BoidData.entityList.Add(entity.Index, new eData
//            {
//                pos = pos.Value,
//                data = new Vector3(SpawnerBootstrap._enemyData.movementSpeed,
//                    SpawnerBootstrap._enemyData.slowingDistance, BoidData.deltaTime),
//                targetPos = BoidData.targetPos.position
//            }
//                );
//        }
//    }
//}