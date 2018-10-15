using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapCamControl : MonoBehaviour
{

    private Controls currentControls;
    private Level_Manager LevelManager;


    public void Initialize() //called by Levelmanager
    {
        transform.position = 100*Vector3.up;
        LevelManager = GameObject.Find("Server").GetComponent<Level_Manager>();
        currentControls = GameObject.Find("Server").GetComponent<Player_Manager>().getLocalPlayer().getControls();
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