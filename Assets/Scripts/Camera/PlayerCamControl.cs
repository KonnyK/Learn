using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamControl: MonoBehaviour {

    [SerializeField] private float Height;
    [SerializeField] private float Angle;
    [SerializeField] private Transform Target;
    private KeyCode MapActivatorKey = KeyCode.None;

    public void SetKeyCode(KeyCode K)
    {
        MapActivatorKey = K;
        Debug.Log("Camera button set " + Target.parent.name, this);
    }

    void Update () {
        //Debug.Log("P_Cam on with " + MapActivatorKey + " " + Game_Manager.UpdateAllowed, this);
        if (Game_Manager.UpdateAllowed && MapActivatorKey != KeyCode.None)
        {
            if (Input.GetKeyDown(MapActivatorKey)) transform.GetComponent<Camera>().enabled = false;
            if (Input.GetKeyUp(MapActivatorKey)) transform.GetComponent<Camera>().enabled = true;


            if (Angle > 90) Angle = 90;
            if (Angle < 0) Angle = 0;
            transform.position = Target.position + Height * Vector3.up - Height / Mathf.Tan(Mathf.PI * Angle / 180f) * Vector3.forward;
            transform.rotation = Quaternion.LookRotation(Target.position - transform.position, Vector3.up);
        }
    }

}
