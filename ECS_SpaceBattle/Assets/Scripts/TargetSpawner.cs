using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

public class TargetSpawner : JobComponentSystem
{
    public EndSimulationEntityCommandBufferSystem entityCommandBuffer;

    protected override void OnCreateManager()
    {
        entityCommandBuffer = World.Active.GetOrCreateManager<EndSimulationEntityCommandBufferSystem>();
    }

    private struct SpawnJob : IJobProcessComponentDataWithEntity<TargetSpawnData, LocalToWorld, TargetData>
    {
        public EntityCommandBuffer CommandBuffer;

        public void Execute(Entity entity, int index, ref TargetSpawnData targetSpawnData, ref LocalToWorld location, ref TargetData targetData)
        {
                    var instance = CommandBuffer.Instantiate(targetSpawnData.prefab);
                    var position = math.transform(location.Value,
                        targetSpawnData.spawnPos);

                    CommandBuffer.SetComponent(instance, new Translation { Value = position });

                    CommandBuffer.AddComponent(instance, targetData);
  
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
