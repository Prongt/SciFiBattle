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
    public EndSimulationEntityCommandBufferSystem entityCommandBuffer;
    protected override void OnCreateManager()
    {
        entityCommandBuffer = World.Active.GetOrCreateManager<EndSimulationEntityCommandBufferSystem>();

        //entityCommandBuffer.Instantiate(SetData.enemyData.prefab);
    }
    
    private struct SpawnJob : IJobProcessComponentDataWithEntity<EnemySpawnData, LocalToWorld>
    {
        public EntityCommandBuffer CommandBuffer;

        public void Execute(Entity entity, int index, ref EnemySpawnData spawnData, ref LocalToWorld location)
        {
            for (int x = 0; x < spawnData.spawnCount; x++)
            {
                
                    var instance = CommandBuffer.Instantiate(spawnData.prefab);
                    var position = math.transform(location.Value, new float3(10* x, 10 * x, 10 * x));

                    CommandBuffer.SetComponent(instance, new Translation {Value = position});
                    //CommandBuffer.SetComponent(instance, );

                
            }
            CommandBuffer.DestroyEntity(entity);
        }
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
}
