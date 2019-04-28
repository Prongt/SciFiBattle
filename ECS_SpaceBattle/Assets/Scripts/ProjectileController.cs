using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;
using Unity.Rendering;
using Unity.Mathematics;

//public class ProjectileController : JobComponentSystem
//{
//    protected override JobHandle OnUpdate(JobHandle inputDeps)
//    {
//        var orientateJob = new OrientateJob
//        {
//            deltaTime = Time.deltaTime
//        }.Schedule(this, inputDeps);
//        orientateJob.Complete();

//        return new MoveJob
//        {
//            deltaTime = Time.deltaTime
//        }.Schedule(this, orientateJob);
//    }

//    [BurstCompile]
//    private struct MoveJob : IJobForEachWithEntity<ProjectileData, Translation, Rotation>
//    {
//        public float deltaTime;
//        public void Execute(Entity entity, int index, ref ProjectileData data, ref Translation trans, ref Rotation rot)
//        {
//            //var force = (float3)Vector3.forward * data.speed * deltaTime;
//            //trans.Value += force;
//        }
//    }

//    [BurstCompile]
//    private struct OrientateJob : IJobForEachWithEntity<ProjectileData, Translation, Rotation>
//    {
//        public float deltaTime;
//        public void Execute(Entity entity, int index, ref ProjectileData data, ref Translation trans, ref Rotation rot)
//        {
//            //var targetPos = data.target;
            
//            //var dir = (targetPos - (Vector3)trans.Value).normalized;
//            //var delta = data.speed * deltaTime;

//            //var heading = Vector3.RotateTowards(trans.Value, dir, delta, 0.0f);
            
//            //rot.Value = Quaternion.LookRotation(heading);

//            rot.Value = Quaternion.LookRotation(data.target);
//        }
//    }
//}
