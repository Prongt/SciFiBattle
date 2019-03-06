using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class SpawnerBootstrap : MonoBehaviour, IDeclareReferencedPrefabs, IConvertGameObjectToEntity
{

    public GameObject prefab;
    public int countX;
    public int countY;
    [SerializeField] public EnemyData enemyData;

    public void DeclareReferencedPrefabs(List<GameObject> gameObjects)
    {
        gameObjects.Add(prefab);
    }


    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        var spawnerData = new EnemySpawnData
        {
            prefab = conversionSystem.GetPrimaryEntity(prefab),
            countX = countX,
            countY = countY
        };
        dstManager.AddComponentData(entity, spawnerData);


        //dstManager.AddComponent(entity, new ComponentType(typeof(EnemyData)));
        var eData = new EnemyData
        {
            movementSpeed = enemyData.movementSpeed,
            slowingDistance = enemyData.slowingDistance,
            minNeighbourDist = enemyData.minNeighbourDist,
            force = enemyData.force,
            acceleration = enemyData.acceleration,
            velocity = enemyData.velocity,
            mass = enemyData.mass
        };

        dstManager.AddComponentData(entity, eData);

    }
}
