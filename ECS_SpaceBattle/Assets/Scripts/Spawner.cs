using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public class Spawner : JobComponentSystem
{
    public static EndSimulationEntityCommandBufferSystem entityCommandBuffer;
    public static int Count;
    protected override void OnCreateManager()
    {
        //Creates the command buffer
        entityCommandBuffer = World.Active.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    //Calls the spawn job once on startup
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var job = new SpawnJob
        {
            CommandBuffer = entityCommandBuffer.CreateCommandBuffer()
        }.ScheduleSingle(this, inputDeps);

        entityCommandBuffer.AddJobHandleForProducer(job);

        return job;
    }


    private struct SpawnJob : IJobForEachWithEntity<EnemySpawnData, LocalToWorld, EnemyData>
    {
        public EntityCommandBuffer CommandBuffer;

        public void Execute(Entity entity, int index, ref EnemySpawnData spawnData, ref LocalToWorld location,
            ref EnemyData enemyData)
        {
            Count = spawnData.countX * spawnData.countY;
            for (var x = 0; x < spawnData.countX; x++)
            for (var y = 0; y < spawnData.countY; y++)
            {
                var instance = CommandBuffer.Instantiate(spawnData.prefab);
                var position = math.transform(location.Value,
                    new float3(x * 1.3F, noise.cnoise(new float2(x, y) * 0.21F) * 2, y * 1.3F));

                CommandBuffer.SetComponent(instance, new Translation {Value = position});

                CommandBuffer.AddComponent(instance, enemyData);

                CommandBuffer.DestroyEntity(entity);
            }

            CommandBuffer.DestroyEntity(entity);
        }
    }
}