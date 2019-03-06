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
using Random = UnityEngine.Random;

public class Spawner : JobComponentSystem
{
    public EndSimulationEntityCommandBufferSystem entityCommandBuffer;
    public static EntityManager entityManager;
    protected override void OnCreateManager()
    {
        entityCommandBuffer = World.Active.GetOrCreateManager<EndSimulationEntityCommandBufferSystem>();

        entityManager = World.Active.GetOrCreateManager<EntityManager>();

        //entityCommandBuffer.Instantiate(SetData.enemyData.prefab);
    }
    
    private struct SpawnJob : IJobProcessComponentDataWithEntity<EnemySpawnData, LocalToWorld, EnemyData>
    {
        public EntityCommandBuffer CommandBuffer;

        public void Execute(Entity entity, int index, ref EnemySpawnData spawnData, ref LocalToWorld location, ref EnemyData enemyData)
        {
            for (int x = 0; x < spawnData.countX; x++)
            {
                for (int y = 0; y < spawnData.countY; y++)
                {
                    var instance = CommandBuffer.Instantiate(spawnData.prefab);
                    var position = math.transform(location.Value, 
                        new float3(x * 1.3F, noise.cnoise(new float2(x, y) * 0.21F) * 2, y * 1.3F));

                    CommandBuffer.SetComponent(instance, new Translation {Value = position});

                    //entityManager.AddComponent(instance, new ComponentType(typeof(EnemyData)));


                    //CommandBuffer.SetComponent(instance, enemyData);

                    CommandBuffer.AddComponent(instance, enemyData);
                }

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
