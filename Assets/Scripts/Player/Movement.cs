using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour {

    [SerializeField] private float Acceleration = 300;
    [SerializeField] private float MaxSpeed = 20;
    private Vector3 TargetPos;
    private Transform TargetMarker = null;
    [SerializeField] public GameObject TargetMarkerObject;
    [SerializeField] private Player myPlayer; //set in Editor
    [SerializeField] private Rigidbody RB;
    [SerializeField] private Camera PlayerCam; //set in, you guessed it, Editor
    [SerializeField] private bool TargetCancelled = false; //used to prevent Target from being used again instatntly eventhough it was cancelled (holding both mouse buttons)
    private bool CollidedWithTarget = false;

    public void ClearTarget()
    {
        TargetMarker.gameObject.SetActive(false);
        TargetMarker.position = transform.position;
        TargetPos = transform.position;
    }

    private void FixedUpdate()
    {
        if (TargetMarker == null)
        {
            TargetMarker = Instantiate(TargetMarkerObject, transform.parent).transform;
            TargetMarker.name = transform.name + "_TargetMarker";
        }
        //Debug.Log("PLayerAlive:" + myPlayer.isAlive(), myPlayer);
        if (Game_Manager.UpdateAllowed && myPlayer.isAlive())
        {
            //if (myPlayer.isServer) myPlayer.GetComponent<Game_Manager>().RpcDebug("Movement started on Server");
            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonUp(0)) TargetCancelled = false; //needed because somtimes MouseButtonUp is not recognized
                                                                      //Managing TargetMarker
            if (Input.GetMouseButton(0) && !TargetCancelled)
            {
                RaycastHit[] Hits = Physics.RaycastAll(PlayerCam.ScreenPointToRay(Input.mousePosition));
                bool Target_Set = false;
                foreach (RaycastHit Hit in Hits)
                {
                    if ((Hit.transform.tag == "CP" || Hit.transform.tag == "PF") && (!Target_Set))
                    {
                        TargetMarker.gameObject.SetActive(true);
                        TargetMarker.position = TargetPos = Hit.point; //TargetPos used for movetowards when TargetMarker active
                        Target_Set = true;
                    }
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
                NewDirection = Vector3.Normalize(Vector3.ProjectOnPlane(TargetPos - transform.position, Vector3.up));
                //Debug.Log("Moving with Mouse");
            }

            //Moving with WASD
            Vector2 Dir = myPlayer.getControls().getDir();
            if (Dir != Vector2.zero)
            {
                NewDirection = Vector3.Normalize(NewDirection + Vector3.Normalize(new Vector3(Dir.x, 0, Dir.y)));
            }

            if (RB.velocity.magnitude >= 0.1f && Vector3.Angle(-RB.velocity, Game_Manager.LevelManager.transform.up) >= 5)
            { //Turn Player towards Movement and check for illegal moves
                transform.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(RB.velocity, Game_Manager.LevelManager.transform.up));
                RaycastHit[] Hits = Physics.RaycastAll(transform.position + 2 * NewDirection + RB.velocity * Time.deltaTime + Vector3.up * 5, Vector3.down, 50);
                bool GroundHit = false;
                ///Debug.DrawRay(transform.position, 2 * NewDirection + RB.velocity * Time.deltaTime, Color.blue, 100);
                foreach (RaycastHit Hit in Hits) if (Hit.collider.tag == "PF" || Hit.collider.tag == "CP") GroundHit = true;
                if (!GroundHit)
                {
                    NewDirection = GetComponent<CollisionDetect>().RedirectIllegalMove(NewDirection);
                    Debug.Log("Illegal Move Detected!", this);
                }
            }
            RB.AddForce(Acceleration * NewDirection, ForceMode.Impulse);

            //Stop Movement on Stop Key
            if (Input.GetKey(myPlayer.getControls().getKey("Stop")))
            {
                ClearTarget();
                //Debug.Log("Stopped Moving", this);
                RB.velocity = Vector3.zero;
            }



            //Player tries to move away from activeTarget with WASD
            if (TargetMarker.gameObject.activeSelf && Dir != Vector2.zero && Mathf.Abs(Vector3.Angle(new Vector3(Dir.x, 0, Dir.y), TargetPos - transform.position)) >= 145)
            {
                //Debug.Log("Player moves away", this);
                ClearTarget();
                TargetCancelled = true;
            }

            //check wether Player is somehow too fast
            if (RB.velocity.magnitude > MaxSpeed) RB.velocity = Vector3.ClampMagnitude(RB.velocity, MaxSpeed);

            myPlayer.getControls().DecreaseMovement(false); //slowly decreases the Axis in Controls script
            
        }

        else //when Player is dead or Update is not allowed
        {
            //Debug.Log("Player dead or Game paused, not moving", this);
            myPlayer.getControls().DecreaseMovement(true); //instantly resets Axis to 0;
            ClearTarget(); //clear Target if dead
        }
        if (CollidedWithTarget)
        {
            ClearTarget();
            RB.velocity = Vector3.zero;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "MovementTarget") CollidedWithTarget = true;
     
    }
}
