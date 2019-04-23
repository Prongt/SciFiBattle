using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;

public class BoidECS : JobComponentSystem
{
    //private Enemy enemies;
    //private ComponentDataFromEntity<EnemyData> enemyData;
    //private ComponentDataFromEntity<Translation> position;
    //private ComponentDataFromEntity<Rotation> rotation;

    //private Target targets;
    private Translation t = new Translation();

    protected override JobHandle OnUpdate(JobHandle handle)
    {
        //return new JobHandle();

        //return new BoidJob
        //{
        //    deltaTime = Time.deltaTime,
        //    enemyData = enemyData,
        //    position = position,
        //    rotation = rotation,
        //    targetsArray = targets.targets,
        //    enemiesArray = enemies.enemies
        //}.Schedule(handle);

        return new BoidJob()
        {
            deltaTime = Time.deltaTime,
            targetPos = t
        }.Schedule(this, handle);
    }

    public static void ScheduleDestroyJob(Entity entityToDestroy)
    {
        var job = new DestroyJob
        {
            cmdBuffer = Bootstrap.entityCommandBuffer,
            entityToDestroy = entityToDestroy
        }.Schedule();
        job.Complete();
    }

    //private struct Target
    //{
    //    [ReadOnly] public ComponentDataArray<TargetData> targetData;
    //    [ReadOnly] public EntityArray targets;
    //}

    //private struct Enemy
    //{
    //    [ReadOnly] public ComponentDataArray<EnemyData> enemyData;
    //    [ReadOnly] public EntityArray enemies;
    //}


    [BurstCompile]
    private struct BoidJob : IJobForEachWithEntity<EnemyData, Translation, Rotation>
    {
        public float deltaTime;
        //public ComponentDataFromEntity<EnemyData> enemyData;
        //public ComponentDataFromEntity<Translation> position;
        //public ComponentDataFromEntity<Rotation> rotation;
        //[ReadOnly] public EntityArray targetsArray;
        //[ReadOnly] public EntityArray enemiesArray;

        private float moveSpeed;
        private float slowingDist;
        public Translation targetPos;
        public Rotation targetRot;

        //public void Execute()
        //{
        //    for (var i = 0; i < enemiesArray.Length; i++)
        //    for (var j = 0; j < targetsArray.Length; j++)
        //        Arrive(i, j);
        //}

        private void Arrive(Translation enemyPos, Rotation enemyRot)
        {
            //moveSpeed = enemyData.;
            //slowingDist = enemyData[enemiesArray[enemyIndex]].slowingDistance;
            var dir = targetPos.Value - enemyPos.Value;

            var distance = new Vector3(dir.x, dir.y, dir.z).magnitude;
            if (distance < 0.1f)
            {
                //BoidECS.ScheduleDestroyJob(enemiesArray[i]);
            }
            else
            {
                var ramped = moveSpeed * (distance / slowingDist);
                var clamped = Mathf.Min(ramped, moveSpeed);
                var desired = clamped * (dir / distance);

                //var enemyPos = position[enemiesArray[enemyIndex]];
                enemyPos.Value += desired * deltaTime;
                //position[enemiesArray[enemyIndex]] = enemyPos;


                //var enemyRot = rotation[enemiesArray[enemyIndex]];
                enemyRot.Value = Quaternion.LookRotation(desired);
                //rotation[enemiesArray[enemyIndex]] = enemyRot;
            }
        }

        //private void Allignment(int enemyIndex)
        //{
        //    Vector3 pos;
        //    var neighbourCount = 0;

        //    for (var i = 0; i < enemiesArray.Length; i++)
        //        if (i == enemyIndex)
        //        {
        //        }
        //        else
        //        {
        //            if (Vector3.Distance(position[enemiesArray[i]].Value, position[enemiesArray[enemyIndex]].Value) <
        //                enemyData[enemiesArray[enemyIndex]].minNeighbourDist) neighbourCount++;
        //        }
        //}

        public void Execute(Entity entity, int index, ref EnemyData enemyData, ref Translation trans, ref Rotation rot)
        {
            //for (var i = 0; i < enemiesArray.Length; i++)
            //    for (var j = 0; j < targetsArray.Length; j++)
            //        Arrive(i, j);

            moveSpeed = enemyData.movementSpeed;
            slowingDist = enemyData.slowingDistance;
            //Arrive(trans, rot);

            var dir = targetPos.Value - trans.Value;

            var distance = new Vector3(dir.x, dir.y, dir.z).magnitude;
            if (distance < 0.1f)
            {
                //BoidECS.ScheduleDestroyJob(enemiesArray[i]);
            }
            else
            {
                var ramped = moveSpeed * (distance / slowingDist);
                var clamped = Mathf.Min(ramped, moveSpeed);
                var desired = clamped * (dir / distance);

                //var enemyPos = position[enemiesArray[enemyIndex]];
                trans.Value += desired * deltaTime;
                //position[enemiesArray[enemyIndex]] = enemyPos;


                //var enemyRot = rotation[enemiesArray[enemyIndex]];
                rot.Value = Quaternion.LookRotation(desired);
                //rotation[enemiesArray[enemyIndex]] = enemyRot;
            }
        }
    }


    private struct DestroyJob : IJob
    {
        [WriteOnly] public EntityCommandBuffer cmdBuffer;
        public Entity entityToDestroy;

        public void Execute()
        {
            cmdBuffer.DestroyEntity(entityToDestroy);
        }
    }
}