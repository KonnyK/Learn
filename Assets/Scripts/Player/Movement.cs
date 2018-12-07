using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Movement : MonoBehaviour {

    [SerializeField] private float Acceleration = 300;
    [SerializeField] private float MaxSpeed = 20;
    private Vector3 TargetPos;
    [SerializeField] private Transform TargetMarker; 
    private Player myPlayer; //set in Editor
    private Rigidbody RB;
    private bool VelChanged = true;
    [SerializeField] private Camera PlayerCam; //set in, you guessed it, Editor
    [SerializeField] private Controls currentControls;
    [SerializeField] private bool TargetCancelled = false; //used to prevent Target from being used again instatntly eventhough it was cancelled (holding both mouse buttons)

    public void Initialize(Player P)
    {
        myPlayer = P;
        currentControls = myPlayer.getControls();
        RB = GetComponent<Rigidbody>();
    }

    public void ClearTarget()
    {
        TargetMarker.gameObject.SetActive(false);
        TargetMarker.position = transform.position;
        TargetPos = transform.position;
    }

    private void FixedUpdate()
    {
        //Debug.Log("PLayerAlive:" + myPlayer.isAlive(), myPlayer);
        if (Game_Manager.UpdateAllowed && myPlayer.isAlive())
        {
            //if (myPlayer.isServer) myPlayer.GetComponent<Game_Manager>().RpcDebug("Movement started on Server");
            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonUp(0)) TargetCancelled = false; //needed because somtimes MouseButtonUp is not recognized
                                                                      //Managing TargetMarker
            if (Input.GetMouseButton(0) && !TargetCancelled)
            {
                RaycastHit Hit;
                if (Physics.Raycast(PlayerCam.ScreenPointToRay(Input.mousePosition), out Hit) && (Hit.transform.tag == "CP" | Hit.transform.tag == "PF"))
                {
                    TargetMarker.gameObject.SetActive(true);
                    TargetMarker.position = TargetPos = Hit.point; //TargetPos used for movetowards when TargetMarker active
                }

            }
            //Clear Target on right click or make it changeable by releasing left button
            if (Input.GetMouseButton(1))
            {
                ClearTarget();
                TargetCancelled = false;
            }
            

            Vector3 NewDirection = Vector3.zero;
            //Moving with Mouse
            if (TargetMarker.gameObject.activeSelf)
            {
                VelChanged = true;
                NewDirection = Vector3.Normalize(Vector3.ProjectOnPlane(TargetPos - transform.position, Vector3.up));
                Debug.Log("Moving with Mouse");
            }

            //Moving with WASD
            Vector2 Dir = currentControls.getDir();
            if (Dir != Vector2.zero)
            {
                VelChanged = true;
                Debug.Log("Moving with Keys");
                NewDirection = Vector3.Normalize(NewDirection + Vector3.Normalize(new Vector3(Dir.x, 0, Dir.y)));
            }

            RB.AddForce(Acceleration * NewDirection, ForceMode.Impulse);

            //Stop Movement on Stop Key
            if (Input.GetKey(currentControls.getKey("Stop")))
            {
                ClearTarget();
                Debug.Log("Stopped Moving", this);
                RB.velocity = Vector3.zero;
                VelChanged = true;
            }



            //Player tries to move away from activeTarget with WASD
            if (TargetMarker.gameObject.activeSelf && Dir != Vector2.zero && Mathf.Abs(Vector3.Angle(new Vector3(Dir.x, 0, Dir.y), TargetPos - transform.position)) >= 145)
            {
                Debug.Log("Player moves away", this);
                ClearTarget();
                TargetCancelled = true;
            }

            //check wether Player is somehow too fast
            if (RB.velocity.magnitude > MaxSpeed) RB.velocity = Vector3.ClampMagnitude(RB.velocity, MaxSpeed);

            currentControls.DecreaseMovement(false); //slowly decreases the Axis in Controls script

            if (VelChanged)
            {
                transform.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(RB.velocity, Vector3.up), Vector3.up);
                VelChanged = false;
            }
        }

        else //when Player is dead or Update is not allowed
        {
            Debug.Log("Player dead or Game paused, not moving", this);
            currentControls.DecreaseMovement(true); //instantly resets Axis to 0;
            ClearTarget(); //clear Target if dead
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "MovementTarget")
        {
            ClearTarget();
            RB.velocity = Vector3.zero;
        }
    }
}
