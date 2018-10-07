using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapCamControl : MonoBehaviour
{

    private Controls currentControls;

    public void Initialize() //called by Levelmanager
    {
        transform.position = 100*Vector3.up;
        currentControls = Game_Manager.Players().getLocalPlayer().getControls();
    }

    void Update()
    {
        if (Game_Manager.CanUpdate())
        {
            if (Input.GetKeyDown(currentControls.getKey("Show Map")))
            {
                transform.GetComponent<Camera>().enabled = true;
                transform.GetComponent<Camera>().orthographicSize = Game_Manager.Levels().getLevelRadius();
            }
            if (Input.GetKeyUp(currentControls.getKey("Show Map")))
            {
                transform.GetComponent<Camera>().enabled = false;
            }
        }
    }

}