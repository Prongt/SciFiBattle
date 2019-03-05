using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using ComponentData;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class Spawner : JobComponentSystem
{
    public static EndSimulationEntityCommandBufferSystem entityCommandBuffer;
    protected override void OnCreateManager()
    {
        entityCommandBuffer = World.Active.GetOrCreateManager<EndSimulationEntityCommandBufferSystem>();

        //entityCommandBuffer.Instantiate(SetData.enemyData.prefab);
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var job = new SpawnJob
        {
            CommandBuffer = entityCommandBuffer.CreateCommandBuffer()
        }.ScheduleSingle(this, inputDeps);

        entityCommandBuffer.AddJobHandleForProducer(job);

        return job;
    }
    private struct SpawnJob : IJobProcessComponentDataWithEntity<EnemySpawnData, LocalToWorld>
    {
        public EntityCommandBuffer CommandBuffer;

        public void Execute(Entity entity, int index, ref EnemySpawnData spawnData, ref LocalToWorld location)
        {
            for (int x = 0; x < spawnData.spawnCount; x++)
            {
                    var instance = CommandBuffer.Instantiate(spawnData.prefab);

                    var position = math.transform(location.Value,
                        new float3(Random.Range(-100.0f, 100.0f),
                            Random.Range(-100.0f, 100.0f), Random.Range(-100.0f, 100.0f)));
                    CommandBuffer.SetComponent(instance, new Translation { Value = position });
                    //CommandBuffer.AddComponent(instance, EnemyData);
            }
            CommandBuffer.DestroyEntity(entity);
        }
    }
}
