using System.Collections;
using System.Collections.Generic;
using ComponentData;
using Unity.Entities;
using UnityEngine;

public class SpawnerBootstrap : MonoBehaviour, IDeclareReferencedPrefabs, IConvertGameObjectToEntity
{

    public GameObject prefab;
    public int spawnCount;
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
            spawnCount = spawnCount
        };
        dstManager.AddComponentData(entity, spawnerData);
        
        dstManager.AddComponentData(entity, enemyData);
        dstManager.SetComponentData(entity, enemyData);

    }
}
