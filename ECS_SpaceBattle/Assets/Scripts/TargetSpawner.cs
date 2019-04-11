using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public class TargetSpawner : JobComponentSystem
{
    public EndSimulationEntityCommandBufferSystem entityCommandBuffer;

    protected override void OnCreateManager()
    {
        entityCommandBuffer = World.Active.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
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

    private struct SpawnJob : IJobForEachWithEntity<TargetSpawnData, LocalToWorld, TargetData>
    {
        public EntityCommandBuffer CommandBuffer;

        public void Execute(Entity entity, int index, ref TargetSpawnData targetSpawnData, ref LocalToWorld location,
            ref TargetData targetData)
        {
            var instance = CommandBuffer.Instantiate(targetSpawnData.prefab);
            var position = math.transform(location.Value,
                targetSpawnData.spawnPos);

            CommandBuffer.SetComponent(instance, new Translation {Value = position});

            CommandBuffer.AddComponent(instance, targetData);

            CommandBuffer.DestroyEntity(entity);
        }
    }
}