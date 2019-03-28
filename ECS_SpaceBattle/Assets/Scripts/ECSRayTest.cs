using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics.Systems;
using UnityEngine;

namespace Unity.Physics.Extensions
{
    public class ECSRayTest : MonoBehaviour
    {

        protected NativeList<ColliderCastHit> colliderCastHits;

        public Vector3 direction = new Vector3(1, 0, 0);
        public float distance = 10.0f;
        protected NativeList<DistanceHit> distanceHits;

        protected NativeList<RaycastHit> raycastHits;
        protected RaycastInput raycastInput;


        private void Update()
        {
            ref PhysicsWorld world = ref World.Active.GetExistingManager<BuildPhysicsWorld>().PhysicsWorld;


            float3 origin = transform.position;
            float3 direction = transform.rotation * this.direction * distance;

            raycastHits.Clear();
            colliderCastHits.Clear();
            distanceHits.Clear();

            raycastInput = new RaycastInput
            {
                Ray = new Ray { Origin = origin, Direction = direction },
                Filter = CollisionFilter.Default
            };
            world.CastRay(raycastInput, out RaycastHit hit);

            raycastHits.Add(hit);

            foreach (RaycastHit h in raycastHits.ToArray()) Debug.Log(hit.ColliderKey);
        }
    }
}