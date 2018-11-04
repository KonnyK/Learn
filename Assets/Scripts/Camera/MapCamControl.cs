using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapCamControl : MonoBehaviour //parent should be PlayerClient
{

    private Controls currentControls;
    private Game_Manager GameManager;


    public void Initialize() //called by Levelmanager
    {
        transform.position = 100*Vector3.up;
        GameManager = transform.GetComponentInParent<Game_Manager>();
        currentControls = GameManager.
            getPlayerManager().
            getLocalPlayer().
            getControls();
    }

    void Update()
    {
        if (Game_Manager.UpdateAllowed)
        {
            if (Input.GetKeyDown(currentControls.getKey("Show Map")))
            {
                transform.GetComponent<Camera>().enabled = true;
                transform.GetComponent<Camera>().orthographicSize = GameManager.getLevelManager().getLevelRadius();
            }
            if (Input.GetKeyUp(currentControls.getKey("Show Map")))
            {
                transform.GetComponent<Camera>().enabled = false;
            }
        }
    }

}