using System.Collections;
using System.Collections.Generic;
using Unity.Transforms;
using UnityEngine;

public class StateController : MonoBehaviour
{
    public static Vector3 cameraTarget;
    public static Vector3 boidTarget;
    //public static Vector3 shipTarget;
    public Vector3 shipPos;
    public Transform target;

    WaitForSeconds waitForSeconds;
    bool stage1 = false;

    public CameraController cameraController;
    private void Awake()
    {
        waitForSeconds = new WaitForSeconds(0.1f);

        shipPos = ShipController.posArray[0];
        BoidECS.targetPos = new Translation { Value = shipPos };
        ShipController.targetPos = new Translation { Value = target.position };
        //cameraTarget = shipPos;
        //StartCoroutine(StateControllerCoroutine());

        cameraController.targetPos = shipPos;
        //cameraController.tempFollowDistance = cameraController.followDistance;
        StartCoroutine(bla());
    }

    private void Update()
    {
        shipPos = ShipController.posArray[0];
        BoidECS.targetPos = new Translation { Value = shipPos };
        ShipController.targetPos = new Translation { Value = target.position };

        //cameraTarget = shipPos;

        return;
        cameraController.targetPos = shipPos;

        if (!cameraController.IsAtTarget)
        {
            //cameraController.targetPos = shipPos;
            //cameraController.tempFollowDistance = cameraController.followDistance;
        }
        else
        {
            Debug.Log("Camera has arrived");
            if(ShipController.checkPointArray[0] == false)
            {

            }
        }
    }
    

        //Ship goes to vantage point and camera follows 
        //When camera reaches ship camera zooms to view the swarm 
        //When camera reaches swarm viewpoint the swarm targets the ship
        //Then camera watches the ship going to its target

        
    private IEnumerator bla()
    {
        
        while (true)
        {
            yield return waitForSeconds;
            cameraController.targetPos = shipPos;

            if (!cameraController.IsAtTarget)
            {
                //cameraController.targetPos = shipPos;
                //cameraController.tempFollowDistance = cameraController.followDistance;
            }
            else
            {
                Debug.Log("Camera has arrived");
                if (ShipController.checkPointArray[0] == false)
                {

                }
            }
            
        }
    }
    
}
