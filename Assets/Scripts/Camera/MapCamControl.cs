using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapCamControl : MonoBehaviour //parent should be PlayerClient
{

    private Controls currentControls;
    private Level_Manager LevelManager;


    public void Initialize(Transform ServerClient) //called by Levelmanager
    {
        transform.position = 100*Vector3.up;
        LevelManager = transform.parent.GetComponent<Level_Manager>();
        currentControls = transform.parent.GetComponent<Player_Manager>().getLocalPlayer().getControls();
    }

    void Update()
    {
        if (Game_Manager.CanUpdate())
        {
            if (Input.GetKeyDown(currentControls.getKey("Show Map")))
            {
                transform.GetComponent<Camera>().enabled = true;
                transform.GetComponent<Camera>().orthographicSize = LevelManager.getLevelRadius();
            }
            if (Input.GetKeyUp(currentControls.getKey("Show Map")))
            {
                transform.GetComponent<Camera>().enabled = false;
            }
        }
    }

}