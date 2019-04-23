//using System.Collections.Generic;
//using UnityEngine;

//public class BoidData : MonoBehaviour
//{
//    public static ComputeShader computeShader;
//    public static Transform targetPos;
//    public static Dictionary<int, eData> entityList;
//    //public static List<data> entityData;
//    public ComputeShader shader;
//    public GameObject target;
//    public static float deltaTime;
//    private void OnEnable()
//    {
//        entityList = new Dictionary<int, eData>();
//        computeShader = shader;
//        targetPos = target.transform;
//    }

//    public void Update()
//    {

//        deltaTime = Time.deltaTime;
//        //eData[] d = new eData[entityList.Count];
//        //int index = 0;
//        //foreach (var item in entityList)
//        //{
//        //    if (index == entityList.Count)
//        //    {
//        //        continue;
//        //    }

//        //    d[index] = new eData
//        //    {
//        //        //id = item.Key,
//        //        pos = item.Value,
//        //        //moveSpeed = SpawnerBootstrap._enemyData.movementSpeed,
//        //        //slowingDist = SpawnerBootstrap._enemyData.slowingDistance,
//        //        //deltaTime = Time.deltaTime,
//        //        data = new Vector3(SpawnerBootstrap._enemyData.movementSpeed, 
//        //            SpawnerBootstrap._enemyData.slowingDistance, Time.deltaTime),
//        //        targetPos = target.transform.position
//        //    };
//        //    index++;
//        //}

//        eData[] d = new eData[entityList.Count];
//        int index = 0;
//        foreach (var item in entityList)
//        {
//            if (index == entityList.Count)
//            {
//                continue;
//            }
//            d[index] = item.Value;
//            index++;
//        }

//        // 76 is the size of the VecMatPair struct in bytes 
//        // Vector3 is 3 and Matrix4x4 is 16
//        // 76 = (3(Vector3) + 16(Matrix4x4)) * 4(float)
//        var buffer = new ComputeBuffer(entityList.Count, 36);
//        buffer.SetData(d);
//        var kernel = computeShader.FindKernel("Boid");
//        computeShader.SetBuffer(kernel, "dataBuffer", buffer);
//        computeShader.Dispatch(kernel, d.Length, 1, 1);

//        eData[] b = new eData[entityList.Count];
//        buffer.GetData(b);

//        buffer.Release();

//        index = 0;
//        foreach (var item in entityList)
//        {
//            if (index == entityList.Count)
//            {
//                continue;
//            }
//            var key = item.Key;
//            entityList[key] = b[index];
//            index++;
//        }
//    }
//}

//public struct data
//{
//    public Vector3 pos;
//    public float id;
//}

//public struct eData
//{
//    public Vector3 pos;
//    //public float id;
//    public Vector3 data;
//    //public float moveSpeed;
//    //public float slowingDist;
//    //public float deltaTime;
//    public Vector3 targetPos;
//}
