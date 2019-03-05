using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using System;
using Random = UnityEngine.Random; 
public class Bootstrap : MonoBehaviour
{
    public MeshInstanceRenderer targetMesh;
    public MeshInstanceRenderer enemyMesh;
    public Vector3 targetStartPos;
    public Vector3 enemyStartPos;
    public int enemyCount = 1;

    [SerializeField] public EnemyData enemyData;

    private static EntityArchetype targetArchetype;
    private static EntityArchetype enemyArchetype;

    public EntityManager entityManager;
    public static EntityCommandBuffer entityCommandBuffer;
    void Start()
    {
        entityManager = World.Active.GetOrCreateManager<EntityManager>();
        entityCommandBuffer = new EntityCommandBuffer();
        CreateArchetypes(entityManager);
        CreateEntities(entityManager);
    }

    

    public void CreateEntities(EntityManager entityManager)
    {
        Entity target = entityManager.CreateEntity(targetArchetype);
        entityManager.SetComponentData(target, new Position { Value = targetStartPos });
        entityManager.SetComponentData(target, new Rotation() { Value = quaternion.identity });
        entityManager.SetComponentData(target, new Scale() { Value = new float3(6.0f, 6.0f, 6.0f) });
        entityManager.SetSharedComponentData(target, targetMesh);


        for (int i = 0; i < enemyCount; i++)
        {
            Entity enemy = entityManager.CreateEntity(enemyArchetype);
            entityManager.SetComponentData(enemy, new Position
            {
                Value = new float3(Random.Range(-100.0f, 100.0f),
                    Random.Range(-100.0f, 100.0f), Random.Range(-100.0f, 100.0f))
            });
            entityManager.SetComponentData(enemy, new Rotation() { Value = quaternion.identity });
            entityManager.SetComponentData(enemy, new Scale() { Value = new float3(1.0f, 1.0f, 1.0f) });
            entityManager.SetSharedComponentData(enemy, enemyMesh);
            entityManager.SetComponentData(enemy, enemyData);
        }
    }

    public void CreateArchetypes(EntityManager entityManager)
    {
        targetArchetype = entityManager.CreateArchetype(
            typeof(Rotation),
            typeof(Scale),
            typeof(Position),
            typeof(MeshInstanceRenderer),
            typeof(TargetData)
        );
        enemyArchetype = entityManager.CreateArchetype(
            typeof(Rotation),
            typeof(Scale),
            typeof(Position),
            typeof(MeshInstanceRenderer),
            typeof(EnemyData)
        );
    }
}
[Serializable]
public struct TargetData : IComponentData
{
    public float movementSpeed;
    public float slowingDistance;
}

[Serializable]
public struct EnemyData : IComponentData
{
    public float movementSpeed;
    public float slowingDistance;
    public float minNeighbourDist;
    public Vector3 force;
    public Vector3 acceleration;
    public Vector3 velocity;
    public float mass;
}


