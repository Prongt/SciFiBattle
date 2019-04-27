using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using Unity.Physics;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Transforms;
using Collider = Unity.Physics.Collider;
using BoxCollider = Unity.Physics.BoxCollider;
using Random = UnityEngine.Random;
using Unity.Rendering;

public class Bootstrap : MonoBehaviour
{
    private static EntityArchetype targetArchetype;
    private static EntityArchetype enemyArchetype;
    public static EntityCommandBuffer entityCommandBuffer;
    public int enemyCount = 1;

    [SerializeField] public EnemyData enemyData;
    public RenderMesh enemyMesh;
    public Vector3 enemyStartPos;

    public EntityManager entityManager;
    public RenderMesh targetMesh;
    public Vector3 targetStartPos;

    private void Start()
    {
        entityManager = World.Active.EntityManager;
        //entityManager = World.Active.GetOrCreateSystem<EntityManager>;
        entityCommandBuffer = new EntityCommandBuffer();
        CreateArchetypes(entityManager);
        CreateEntities(entityManager);
    }


    public void CreateEntities(EntityManager entityManager)
    {
        var target = entityManager.CreateEntity(targetArchetype);
        entityManager.SetComponentData(target, new Translation {Value = targetStartPos});
        entityManager.SetComponentData(target, new Rotation {Value = quaternion.identity});



        //entityManager.SetComponentData(target, new Scale() { Value = new float3(6.0f, 6.0f, 6.0f) });
        entityManager.SetSharedComponentData(target, targetMesh);


        for (var i = 0; i < enemyCount; i++)
        {
            var enemy = entityManager.CreateEntity(enemyArchetype);
            entityManager.SetComponentData(enemy, new Translation
            {
                Value = new float3(Random.Range(-100.0f, 100.0f),
                    Random.Range(-100.0f, 100.0f), Random.Range(-100.0f, 100.0f))
            });
            entityManager.SetComponentData(enemy, new Rotation {Value = quaternion.identity});
            //entityManager.SetComponentData(enemy, new Scale() { Value = new float3(1.0f, 1.0f, 1.0f) });
            entityManager.SetSharedComponentData(enemy, enemyMesh);
            entityManager.SetComponentData(enemy, enemyData);
        }
    }

    public void CreateArchetypes(EntityManager entityManager)
    {
        targetArchetype = entityManager.CreateArchetype(
            typeof(Rotation),
            //typeof(Scale),
            typeof(Translation),
            typeof(RenderMesh),
            typeof(TargetData)
        );
        enemyArchetype = entityManager.CreateArchetype(
            typeof(Rotation),
            //typeof(Scale),
            typeof(Translation),
            typeof(RenderMesh),
            typeof(EnemyData)
        );
    }
}