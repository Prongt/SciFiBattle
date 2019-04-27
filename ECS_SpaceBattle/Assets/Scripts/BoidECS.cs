using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;
using Unity.Rendering;

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

        var fleeJob = new FleeJob
        {
            targetPos = new Translation(),
            targetRot = new Rotation()
        }.Schedule(this, arriveJob);
        fleeJob.Complete();

        var boidJob = new BoidJob()
        {
            cmdBuffer = World.Active.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>().CreateCommandBuffer(),
            deltaTime = Time.deltaTime,
            //projectileArchtype = World.Active.EntityManager.CreateArchetype(
            //typeof(Rotation),
            //typeof(Translation),
            //typeof(RenderMesh),
            //typeof(ProjectileData)
            //),
            //renderMesh = ComponentData.renderMesh
    }.Schedule(this, fleeJob);

        boidJob.Complete();

        var destroyJob = new DestroyJob()
        {
            //cmdBuffer = World.Active.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>().CreateCommandBuffer()
        }.Schedule(this, boidJob);
        destroyJob.Complete();

        return destroyJob;
    }

    [BurstCompile]
    private struct BoidJob : IJobForEachWithEntity<EnemyData, Translation, Rotation>
    {
        //public EntityArchetype projectileArchtype;
        //public RenderMesh renderMesh;
        [ReadOnly] public EntityCommandBuffer cmdBuffer;
        public float deltaTime;

        //public Unity.Mathematics.Random random;
        public void Execute(Entity entity, int index, ref EnemyData enemyData, ref Translation trans, ref Rotation rot)
        {
            if (!enemyData.inRange)
            {
                trans.Value += enemyData.force;
                rot.Value = Quaternion.LookRotation(enemyData.force);
                enemyData.force = Vector3.zero;
            }
            else
            {
                //Spawn projectiles every few seconds 
                //Send projectiles towards target
                //When in range create explosion effect and damage target

                //ProjectileSpawner.projectilesList.Add(new ProjectileData
                //{
                //    speed = 1.0f,
                //    startingPos = trans.Value,
                //    target = new Translation().Value
                //});

                //var bullet = cmdBuffer.CreateEntity(projectileArchtype);
                //cmdBuffer.SetComponent(entity, new Translation { Value = trans.Value });
                //cmdBuffer.SetComponent(entity, new Rotation { Value = rot.Value });
                //cmdBuffer.AddComponent(entity, new ProjectileData
                //{
                //    speed = 1.0f,
                //    //startingPos = data[i].startingPos,
                //    target = new Translation().Value
                //});
                
                //cmdBuffer.AddSharedComponent(entity, renderMesh);
                ComponentData.Spawn(trans.Value, rot.Value);
            }

            //var temp = enemyData.force / enemyData.mass;
            //enemyData.acceleration = Vector3.Lerp(enemyData.acceleration, temp, deltaTime);
            //enemyData.velocity += enemyData.acceleration * deltaTime;

            //enemyData.velocity = Vector3.ClampMagnitude(enemyData.velocity, enemyData.maxSpeed);
            //var speed = ((Vector3)enemyData.velocity).magnitude;

            //if (speed > 0)
            //{
            //    enemyData.velocity *= (1.0f - (enemyData.damping * deltaTime));
            //}

            

            if (enemyData.shouldDestroy && entity != null)
            {
                cmdBuffer.DestroyEntity(entity);
            }
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

            if (distance <= enemyData.attackRange)
            {
                enemyData.inRange = true;
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
    private struct FleeJob : IJobForEachWithEntity<EnemyData, Translation, Rotation>
    {
        public Translation targetPos;
        public Rotation targetRot;
        public void Execute(Entity entity, int index, ref EnemyData enemyData, ref Translation trans, ref Rotation rot)
        {
            return;

            var desired = targetPos.Value - trans.Value;
            if (((Vector3)desired).magnitude <= enemyData.fleeDistance)
            {
                ((Vector3)desired).Normalize();
                desired *= enemyData.maxSpeed;
                enemyData.force += enemyData.velocity - desired;
                enemyData.fleeing = true;
            }
            else
            {
                enemyData.fleeing = false;
            }
        }
    }


    [BurstCompile]
    private struct DestroyJob : IJobForEachWithEntity<EnemyData>
    {
        //[ReadOnly] public EntityCommandBuffer cmdBuffer;
        public void Execute(Entity entity, int index, ref EnemyData enemyData)
        {
            //if (enemyData.shouldDestroy && entity != null)
            //{
            //    cmdBuffer.DestroyEntity(entity);
            //}
        }
    }
}