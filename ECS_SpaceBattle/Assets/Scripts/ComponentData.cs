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
using Unity.Rendering;
using Mesh = UnityEngine.Mesh;
using Material = UnityEngine.Material;

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
    public float maxNeighbourDist;
    public float attackRange;
    public float fleeDistance;
    public float maxSpeed;
    public float damping;
    public float3 force;
    public float3 acceleration;
    public float3 velocity;
    public float mass;
    public Quaternion rotation;
    public bool shouldDestroy;
    public bool inRange;
    public bool fleeing;
    public int index;
    public int cell;
}

public struct EnemySpawnData : IComponentData
{
    public Entity prefab;
    public int countX;
    public int countY;
}

public struct NeighbourData
{
    public int boidIndex;
    public Vector3 pos;
    public Quaternion rot;
}

public struct PosRot
{
    public Vector3 pos;
    public Quaternion rot;
}


public struct TargetSpawnData : IComponentData
{
    public Entity prefab;
    public float3 spawnPos;
}

public struct ProjectileData : IComponentData
{
    public float speed;
    public Vector3 target;
    public Vector3 startingPos;
}

[Serializable]
public struct ProjectileSpawnData : IComponentData
{
    public Entity prefab;
    //public RenderMesh mesh;
}

public class ComponentData : MonoBehaviour
{
    public static EntityArchetype projectileArchtype;
    public static ProjectileSpawnData projectileSpawnData;
    [SerializeField] public ProjectileSpawnData spawnData;
    public RenderMesh mesh;
    public static RenderMesh renderMesh;
    public GameObject target;

    //public static List<ProjectileData> projectiles;

    private void Update()
    {
        BoidECS.targetPos = target.transform.position;
    }
    private void Awake()
    {
        BoidECS.targetPos = target.transform.position;

        //renderMesh = mesh;
        //projectileSpawnData = spawnData;
        //projectileArchtype = World.Active.EntityManager.CreateArchetype(
        //    typeof(Rotation),
        //    typeof(Translation),
        //    typeof(RenderMesh),
        //    typeof(ProjectileData)
        //    );

        //cmdBuffer = World.Active.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>().CreateCommandBuffer();

    }
    public static EntityCommandBuffer cmdBuffer;
    public static void Spawn(float3 pos, Quaternion rot)
    {
        var bullet = cmdBuffer.CreateEntity(projectileArchtype);
        cmdBuffer.SetComponent(bullet, new Translation { Value = pos });
        cmdBuffer.SetComponent(bullet, new Rotation { Value = rot });
        cmdBuffer.AddComponent(bullet, new ProjectileData
        {
            speed = 1.0f,
            //startingPos = data[i].startingPos,
            target = new Translation().Value
        });

        cmdBuffer.AddSharedComponent(bullet, renderMesh);
    }
}

