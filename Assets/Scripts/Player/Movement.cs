using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Movement : MonoBehaviour {

    [SerializeField] private float Acceleration = 300;
    [SerializeField] private float MaxSpeed = 20;
    private Vector3 TargetPos;
    [SerializeField] private Transform TargetMarker; //set in Editor
    [SerializeField] private Player myPlayer; //set in Editor
    private Rigidbody RB;
    private Transform Mesh;
    [SerializeField] private Camera PlayerCam; //set in, you guessed it, Editor
    [SerializeField] private Controls currentControls;
    private bool TargetCancelled = false; //used to prevent Target from being used again instatntly eventhough it was cancelled (holding both mouse buttons)

    public void Initialize()
    {
        RB = myPlayer.getRB();
        Mesh = myPlayer.Mesh;
        PlayerCam = myPlayer.Mesh.parent.GetComponentInChildren<PlayerCamControl>().GetComponent<Camera>();
        currentControls = myPlayer.getControls();
    }

    public void ClearTarget()
    {
        TargetMarker.gameObject.SetActive(false);
        TargetMarker.position = Mesh.position;
        TargetPos = Mesh.position;
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
            if (TargetMarker.gameObject.activeSelf) if (Vector3.Magnitude(TargetPos - Mesh.position) > TargetMarker.lossyScale.x)
                {
                    NewDirection = Vector3.Normalize(Vector3.ProjectOnPlane(TargetPos - Mesh.position, Vector3.up));
                }
                else
                {
                    ClearTarget();
                    RB.velocity = Vector3.zero;
                }
            TargetMarker.position = TargetPos; //stopping TargetMarker from moving with its parent

            //Moving with WASD
            Vector2 Dir = currentControls.getDir();
            NewDirection = Vector3.Normalize(NewDirection + Vector3.Normalize(new Vector3(Dir.x, 0, Dir.y)));

            RB.AddForce(Acceleration * NewDirection, ForceMode.Impulse);

            //Stop Movement on Stop Key
            if (Input.GetKey(currentControls.getKey("Stop")))
            {
                ClearTarget();
                RB.velocity = Vector3.zero;
            }



            //Player tries to move away from activeTarget with WASD
            if (TargetMarker.gameObject.activeSelf & Mathf.Abs(Vector3.Angle(new Vector3(Dir.x, 0, Dir.y), TargetPos - Mesh.position)) >= 145)
            {
                ClearTarget();
                TargetCancelled = true;
            }

            //check wether Player is somehow too fast
            if (RB.velocity.magnitude > MaxSpeed) RB.velocity = Vector3.ClampMagnitude(RB.velocity, MaxSpeed);

            currentControls.DecreaseMovement(false); //slowly decreases the Axis in Controls script
            myPlayer.CmdSetNewCourse(Mesh.position, RB.velocity);
        }

        else //when Player is dead or Update is not allowed
        {
            currentControls.DecreaseMovement(true); //resets Axis to 0;
            ClearTarget(); //clear Target if dead
        }
    }

}
