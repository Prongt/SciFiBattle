using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;

public class BoidECS : JobComponentSystem
{
    [Inject] private Enemy enemies;
    [Inject] private ComponentDataFromEntity<EnemyData> enemyData;
    [Inject] private ComponentDataFromEntity<Translation> position;

    [Inject] private Target targets;

    protected override JobHandle OnUpdate(JobHandle handle)
    {
        return new BoidJob
        {
            deltaTime = Time.deltaTime,
            enemyData = enemyData,
            position = position,
            targetsArray = targets.targets,
            enemiesArray = enemies.enemies
        }.Schedule(handle);
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

    private struct Target
    {
        [ReadOnly] public ComponentDataArray<TargetData> targetData;
        [ReadOnly] public EntityArray targets;
    }

    private struct Enemy
    {
        [ReadOnly] public ComponentDataArray<EnemyData> enemyData;
        [ReadOnly] public EntityArray enemies;
    }


    [BurstCompile]
    private struct BoidJob : IJob
    {
        public float deltaTime;
        public ComponentDataFromEntity<EnemyData> enemyData;
        public ComponentDataFromEntity<Translation> position;
        [ReadOnly] public EntityArray targetsArray;
        [ReadOnly] public EntityArray enemiesArray;

        private float moveSpeed;
        private float slowingDist;

        public void Execute()
        {
            moveSpeed = enemyData[enemiesArray[0]].movementSpeed;
            slowingDist = enemyData[enemiesArray[0]].slowingDistance;
            for (var i = 0; i < enemiesArray.Length; i++)
            for (var j = 0; j < targetsArray.Length; j++)
                Arrive(i, j);
        }

        private void Arrive(int enemyIndex, int targetIndex)
        {
            var dir = position[targetsArray[targetIndex]].Value - position[enemiesArray[enemyIndex]].Value;

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

                var enemyPos = position[enemiesArray[enemyIndex]];
                enemyPos.Value += desired * deltaTime;
                position[enemiesArray[enemyIndex]] = enemyPos;
            }
        }

        private void Allignment(int enemyIndex)
        {
            Vector3 pos;
            var neighbourCount = 0;

            for (var i = 0; i < enemiesArray.Length; i++)
                if (i == enemyIndex)
                {
                }
                else
                {
                    if (Vector3.Distance(position[enemiesArray[i]].Value, position[enemiesArray[enemyIndex]].Value) <
                        enemyData[enemiesArray[enemyIndex]].minNeighbourDist) neighbourCount++;
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