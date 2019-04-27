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

public class SpawnerBootstrap : MonoBehaviour, IDeclareReferencedPrefabs, IConvertGameObjectToEntity
{
    [SerializeField] public EnemySpawnDataLocal enemySpawnData;
    [SerializeField] public EnemyData enemyData;
    public static EnemyData _enemyData;
    //[SerializeField] public PhysicsCollider collider;
    public void DeclareReferencedPrefabs(List<GameObject> gameObjects)
    {
        gameObjects.Add(enemySpawnData.prefab);
    }

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        _enemyData = enemyData;
        var spawnerData = new EnemySpawnData
        {
            prefab = conversionSystem.GetPrimaryEntity(enemySpawnData.prefab),
            countX = enemySpawnData.countX,
            countY = enemySpawnData.countY
        };

        dstManager.AddComponentData(entity, spawnerData);

        var eData = new EnemyData
        {
            movementSpeed = enemyData.movementSpeed,
            slowingDistance = enemyData.slowingDistance,
            minNeighbourDist = enemyData.minNeighbourDist,
            attackRange = enemyData.attackRange,
            fleeDistance = enemyData.fleeDistance,
            maxSpeed = enemyData.maxSpeed,
            force = enemyData.force,
            acceleration = enemyData.acceleration,
            velocity = enemyData.velocity,
            mass = enemyData.mass,
            shouldDestroy = false,
            rotation = enemyData.rotation,
            inRange = false,
            fleeing = false

        };
        dstManager.AddComponentData(entity, eData);

        #region PhysicsStuffNotImplemented

        //sourceCollider = dstManager.GetComponentData<PhysicsCollider>(entity).Value;
        //Entity sourceEntity = GameObjectConversionUtility.ConvertGameObjectHierarchy(prefab, World.Active);


        //BlobAssetReference<Collider> sourceCollider = dstManager.GetComponentData<PhysicsCollider>(entity).Value;

        //BlobAssetReference<Unity.Physics.BoxCollider> col;
        //col.Value = new Unity.Physics.BoxCollider
        //{
        //    Center = new Vector3(0.0008711144f, -0.004504155f, -0.5448629f),
        //    Size = new Vector3(0.4545653f, 0.09167009f, 0.5136013f),
        //    ConvexRadius = 0.009167009f,
        //    Filter = CollisionFilter.Default

        //};

        //BoxCollider box = new BoxCollider
        //{
        //    Center = new Vector3(0.0008711144f, -0.004504155f, -0.5448629f),
        //    Size = new Vector3(0.4545653f, 0.09167009f, 0.5136013f),
        //    ConvexRadius = 0.009167009f,
        //    Filter = CollisionFilter.Default
        //};

        //var collider = new PhysicsCollider();
        //collider.Value = ;

        //BoxCollider col = new BoxCollider
        //{
        //    Center = new Vector3(0.0008711144f, -0.004504155f, -0.5448629f),
        //    Size = new Vector3(0.4545653f, 0.09167009f, 0.5136013f),
        //    ConvexRadius = 0.009167009f,
        //    Filter = CollisionFilter.Default
        //};


        //var colData = new ColliderData
        //{
        //    col = col
        //};

        //dstManager.AddComponentData(entity, colData);

        //var bc = new BlobAssetReference<BoxCollider>();
        //bc.Value = col;


        //dstManager.AddComponent(entity, bc);
        //var pc = new PhysicsCollider { Value = new BlobAssetReference<Collider>().Value = c };

        #endregion
    }

    [Serializable]
    public struct EnemySpawnDataLocal
    {
        public GameObject prefab;
        public int countX;
        public int countY;
    }
}
