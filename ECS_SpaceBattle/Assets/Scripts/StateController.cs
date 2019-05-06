using System.Collections;
using Unity.Transforms;
using UnityEngine;

public class StateController : MonoBehaviour
{
    public Vector3 cameraTarget;
    public Vector3 boidTargetVector;
    public Vector3 shipTargetVector;
    public Vector3 shipPos;
    public Transform startingShipTarget;
    public Transform boidStartingTarget;
    public Transform planet;

    WaitForSeconds waitForSeconds;
    bool stage1 = false;
    bool stage2 = false;
    bool stage3 = false;

    int shotIndex = 0;
    bool canChangeShot = false;
    public CameraController cameraController;
    private void Awake()
    {
        waitForSeconds = new WaitForSeconds(0.1f);

        shipPos = ShipController.posArray[0];
        BoidECS.targetPos = new Translation { Value = boidTargetVector };
        ShipController.targetPos = new Translation { Value = shipTargetVector };
        //cameraTarget = shipPos;

        //cameraController.targetPos = cameraTarget;
        //cameraController.tempFollowDistance = cameraController.followDistance;
        StartCoroutine(bla());
    }

    private void Update()
    {
        shipPos = ShipController.posArray[0];
        BoidECS.targetPos = new Translation { Value = boidTargetVector };
        ShipController.targetPos = new Translation { Value = shipTargetVector };

        //cameraTarget = shipPos;
        
        
        return;
        //cameraController.targetPos = shipPos;

        //if (!cameraController.IsAtTarget)
        //{
        //    //cameraController.targetPos = shipPos;
        //    //cameraController.tempFollowDistance = cameraController.followDistance;
        //}
        //else
        //{
        //    Debug.Log("Camera has arrived");
        //    if(ShipController.checkPointArray[0] == false)
        //    {

        //    }
        //}
    }
    

        //Ship goes to vantage point and camera follows 
        //When camera reaches ship camera zooms to view the swarm 
        //When camera reaches swarm viewpoint the swarm targets the ship
        //Then camera watches the ship going to its target

        
    private IEnumerator bla()
    {
        
        while (true)
        {
            
            //cameraController.targetPos = cameraTarget;


            switch (shotIndex)
            {
                case 0:
                    cameraTarget = shipPos;
                    shipTargetVector = startingShipTarget.position;
                    boidTargetVector = boidStartingTarget.position;
                    CameraController.targetPos = cameraTarget;
                    break;
                case 1:
                    cameraTarget = boidStartingTarget.position;
                    cameraController.tempFollowDistance = cameraController.followDistance * 8;
                    CameraController.targetPos = cameraTarget;
                    //TODO increase stopping range
                    break;
                case 2:
                    cameraTarget = shipPos;
                    boidTargetVector = shipPos;
                    cameraController.tempFollowDistance = cameraController.followDistance;
                    CameraController.targetPos = cameraTarget;
                    break;
                case 3:
                    cameraTarget = shipPos;
                    boidTargetVector = shipPos;
                    shipTargetVector = planet.position;
                    CameraController.targetPos = cameraTarget;
                    break;
                case 4:
                    cameraTarget = shipPos;
                    boidTargetVector = shipPos;
                    shipTargetVector = planet.position;
                    cameraController.tempFollowDistance = cameraController.followDistance * 2;
                    CameraController.targetPos = cameraTarget;
                    break;
                default:
                    //Console.WriteLine("Default case");
                    break;
            }

            yield return new WaitForSeconds(2.0f);
            if (cameraController.CheckAtTarget(cameraTarget))
            {
                //if (ShipController.checkPointArray[0])
                
                    
                //cameraController.IsAtTarget = false;

                
                Debug.Log("Camera has arrived");
                shotIndex++;
            }
            yield return waitForSeconds;
        }
    }
    
}
