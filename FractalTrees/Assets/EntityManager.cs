//using System;
//using UnityEngine;
//using Unity.Entities;
//using Unity.Jobs;
//using Unity.Rendering;
//using Unity.Transforms;

//public class EntityManager : JobComponentSystem
//{
//    //public static EntityManager entityManager;
//    public EntityArchetype targetArchetype;
//    public EntityArchetype enemyArchetype;

//    protected override void OnCreateManager()
//    {
//        var entityManager = World.Active.GetOrCreateManager<EntityManager>();
//        //targetArchetype = new EntityArchetype();
//        //targetArchetype.
//        //targetArchetype = new EntityArchetype(
//        //    typeof(Rotation),
//        //    typeof(Scale),
//        //    typeof(Position),
//        //    typeof(MeshInstanceRenderer),
//        //    typeof(TargetData)
//        //);

//        //enemyArchetype = entityManager.CreateArchetype(
//        //    typeof(Rotation),
//        //    typeof(Scale),
//        //    typeof(Position),
//        //    typeof(MeshInstanceRenderer),
//        //    typeof(EnemyData)
//        //);

//        entityManager.CreateArchetypes(entityManager);


//    }

//    //protected override JobHandle OnUpdate(JobHandle inputDeps)
//    //{
//    //    return new CreateArchtypesJob()
//    //    {

//    //    }.ScheduleSingle(this, inputDeps);
//    //}

//    public void CreateArchetypes(EntityManager entityManager)
//    {
//        targetArchetype = entityManager.CreateArchetype(
//            typeof(Rotation),
//            typeof(Scale),
//            typeof(Position),
//            typeof(MeshInstanceRenderer),
//            typeof(TargetData)
//        );
//        enemyArchetype = entityManager.CreateArchetype(
//            typeof(Rotation),
//            typeof(Scale),
//            typeof(Position),
//            typeof(MeshInstanceRenderer),
//            typeof(EnemyData)
//        );
//    }

//    //private struct CreateArchtypesJob : IJobProcessComponentData<>
//    //{
//    //    public void Execute()
//    //    {
//    //        throw new System.NotImplementedException();
//    //    }
//    //}
//}


