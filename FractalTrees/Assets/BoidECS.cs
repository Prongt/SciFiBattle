using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;

public class BoidECS : JobComponentSystem
{
    public float movementSpeed = 1.0f;
    public float slowingDistance = 0.5f;

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

    [Inject] private Target targets;
    [Inject] private Enemy enemies;
    [Inject] private ComponentDataFromEntity<Position> position;

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        return new ArriveJob()
        {
            deltaTime = Time.deltaTime,
            moveSpeed = movementSpeed,
            slowingDist = slowingDistance,
            position = position,
            targetsArray = targets.targets,
            enemiesArray = enemies.enemies,

        }.Schedule(inputDeps);
    }


    [BurstCompile]
    private struct ArriveJob : IJob
    {
        public float deltaTime;
        public float moveSpeed;
        public float slowingDist;
        public ComponentDataFromEntity<Position> position;
        [ReadOnly] public EntityArray targetsArray;
        [ReadOnly] public EntityArray enemiesArray;


        public void Execute()
        {
            for (int i = 0; i < enemiesArray.Length; i++)
            {
                for (int j = 0; j < targetsArray.Length; j++)
                {
                    var dir = position[targetsArray[j]].Value - position[enemiesArray[i]].Value;

                    float distance = new Vector3(dir.x, dir.y, dir.z).magnitude;
                    if (distance < 0.1f)
                    {

                    }
                    else
                    {
                        float ramped = moveSpeed * (distance / slowingDist);

                        float clamped = Mathf.Min(ramped, moveSpeed);
                        var desired = clamped * (dir / distance);

                        var enemyPos = position[enemiesArray[i]];
                        enemyPos.Value += desired * deltaTime;
                        position[enemiesArray[i]] = enemyPos;

                    }

                    //var enemyPos = position[enemiesArray[i]];
                    //enemyPos.Value += dir * moveSpeed * deltaTime;
                    //position[enemiesArray[i]] = enemyPos;

                }
            }
        }
    }
}
