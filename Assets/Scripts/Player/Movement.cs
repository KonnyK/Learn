using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Movement : MonoBehaviour {

    [SerializeField] private float Acceleration = 300;
    [SerializeField] private float MaxSpeed = 20;
    private Vector3 TargetPos;
    [SerializeField] private Transform TargetMarker; //set in Editor
    [SerializeField] private Player myPlayer; //set in Editor
    [SerializeField] private Rigidbody RB; //set in Editor
    [SerializeField] private Camera PlayerCam; //set in, you guessed it, Editor
    private bool TargetCancelled = false; //used to prevent Target from being used again instatntly eventhough it was cancelled

    public void ClearTarget()
    {
        TargetMarker.gameObject.SetActive(false);
        TargetMarker.position = transform.position;
        TargetPos = transform.position;
    }

    private void FixedUpdate()
    {
        if (Game_Manager.UpdateAllowed && myPlayer.isAlive())
        {
            if (Input.GetMouseButtonDown(0)) TargetCancelled = false; //needed because somtimes MouseButtonUp is not recognized
                                                                      //Managing TargetMarker
            if (Input.GetMouseButton(0) && !TargetCancelled)
            {
                RaycastHit Hit;
                if (Physics.Raycast(PlayerCam.ScreenPointToRay(Input.mousePosition), out Hit) && (Hit.transform.tag == "CP" | Hit.transform.tag == "PF"))
                {
                    TargetPos = Hit.point; //TargetPos used for movetowards when TargetMarker active
                    TargetMarker.gameObject.SetActive(true);
                    TargetMarker.position = TargetPos;
                }

            }
            //Clear Target on right click or make it changeable by releasing left button
            if (Input.GetMouseButtonUp(0)) TargetCancelled = false;
            if (Input.GetMouseButton(1))
            {
                ClearTarget();
                TargetCancelled = false;
            }


            Vector3 NewDirection = Vector3.zero;
            //Moving with Mouse
            if (TargetMarker.gameObject.activeSelf) if (Vector3.Magnitude(TargetPos - transform.position) > TargetMarker.lossyScale.x)
                {
                    NewDirection = Vector3.Normalize(Vector3.ProjectOnPlane(TargetPos - transform.position, Vector3.up));
                }
                else
                {
                    ClearTarget();
                    RB.velocity = Vector3.zero;
                }
            TargetMarker.position = TargetPos; //stopping TargetMarker from moving with its parent

            //Moving with WASD
            Vector2 Dir = myPlayer.getControls().getDir();
            NewDirection = Vector3.Normalize(NewDirection + Vector3.Normalize(new Vector3(Dir.x, 0, Dir.y)));

            RB.AddForce(Acceleration * NewDirection, ForceMode.Impulse);

            //Stop Movement on Stop Key
            if (Input.GetKey(myPlayer.getControls().getKey("Stop")))
            {
                ClearTarget();
                RB.velocity = Vector3.zero;
            }



            //Player tries to move away from activeTarget with WASD
            if (TargetMarker.gameObject.activeSelf & Mathf.Abs(Vector3.Angle(new Vector3(Dir.x, 0, Dir.y), TargetPos - transform.position)) >= 145)
            {
                ClearTarget();
                TargetCancelled = true;
            }

            //check wether Player is somehow too fast
            if (RB.velocity.magnitude > MaxSpeed) RB.velocity = Vector3.ClampMagnitude(RB.velocity, MaxSpeed);

            //Rotate Mesh
            myPlayer.OrientateMesh(new Vector3(RB.velocity.x, 0, RB.velocity.z), Vector3.up);

            myPlayer.getControls().DecreaseMovement(false); //slowly decreases the Axis in Controls script
        }

        else //when Player is dead or Update is not allowed
        {
            myPlayer.getControls().DecreaseMovement(true); //resets Axis to 0;
            ClearTarget(); //clear Target if dead
        }
    }

}
