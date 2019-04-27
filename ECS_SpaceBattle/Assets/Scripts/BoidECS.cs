using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;

public class BoidECS : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle handle)
    {

        var arriveJob = new ArriveJob
        {
            deltaTime = Time.deltaTime,
            targetPos = new Translation(),
            targetRot = new Rotation()
        }.Schedule(this, handle);

        arriveJob.Complete();

        var boidJob = new BoidJob()
        {
            cmdBuffer = World.Active.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>().CreateCommandBuffer()
        }.Schedule(this, arriveJob);

        boidJob.Complete();

        return new DestroyJob()
        {
        }.Schedule(boidJob);
    }

    [BurstCompile]
    private struct BoidJob : IJobForEachWithEntity<EnemyData, Translation, Rotation>
    {
        [ReadOnly] public EntityCommandBuffer cmdBuffer;
        public void Execute(Entity entity, int index, ref EnemyData enemyData, ref Translation trans, ref Rotation rot)
        {
            if (enemyData.shouldDestroy && entity != null)
            {
                cmdBuffer.DestroyEntity(entity);
            }
            trans.Value += enemyData.force;
            rot.Value = Quaternion.LookRotation(enemyData.force);

            enemyData.force = Vector3.zero;
            //enemyData.rotation = Quaternion.identity;
        }
    }

    [BurstCompile]
    private struct ArriveJob : IJobForEachWithEntity<EnemyData, Translation, Rotation>
    {
        public float deltaTime;
        public Translation targetPos;
        public Rotation targetRot;
        public void Execute(Entity entity, int index, ref EnemyData enemyData, ref Translation trans, ref Rotation rot)
        {
            var moveSpeed = enemyData.movementSpeed;
            var slowingDist = enemyData.slowingDistance;
            var dir = targetPos.Value - trans.Value;
            var distance = new Vector3(dir.x, dir.y, dir.z).magnitude;

            if (distance < 2f)
            {
                //enemyData.shouldDestroy = true;
                //In Range of target
            }
            else
            {
                var ramped = moveSpeed * (distance / slowingDist);
                var clamped = Mathf.Min(ramped, moveSpeed);
                var desired = clamped * (dir / distance);

                enemyData.force += desired * deltaTime;
            }
        }
    }

    [BurstCompile]
    private struct DestroyJob : IJob
    {
        //public EndSimulationEntityCommandBufferSystem cmdBuffer;
        //public NativeList<Entity> entitysToDestroy;

        public void Execute()
        {
            //for (int i = 0; i < entitiesToDestroy.Length; i++)
            //{
            //    if (entitiesToDestroy[i] != null)
            //        cmdBuffer.PostUpdateCommands.DestroyEntity(entitiesToDestroy[i]);
            //}
            //entitiesToDestroy.Clear();
            //entitiesToDestroy.Dispose();
        }
    }
}