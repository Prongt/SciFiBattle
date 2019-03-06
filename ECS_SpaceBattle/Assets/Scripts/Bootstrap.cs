using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;

using Unity.Rendering;
using UnityEngine.Rendering;
using System;
using Unity.Mathematics;
using Random = UnityEngine.Random; 
public class Bootstrap : MonoBehaviour
{
    public RenderMesh targetMesh;
    public RenderMesh enemyMesh;
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
        entityManager.SetComponentData(target, new Translation { Value = targetStartPos });
        entityManager.SetComponentData(target, new Rotation() { Value = quaternion.identity });
        //entityManager.SetComponentData(target, new Scale() { Value = new float3(6.0f, 6.0f, 6.0f) });
        entityManager.SetSharedComponentData(target, targetMesh);


        for (int i = 0; i < enemyCount; i++)
        {
            Entity enemy = entityManager.CreateEntity(enemyArchetype);
            entityManager.SetComponentData(enemy, new Translation
            {
                Value = new float3(Random.Range(-100.0f, 100.0f),
                    Random.Range(-100.0f, 100.0f), Random.Range(-100.0f, 100.0f))
            });
            entityManager.SetComponentData(enemy, new Rotation() { Value = quaternion.identity });
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