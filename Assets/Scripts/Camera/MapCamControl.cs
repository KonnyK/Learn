using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapCamControl : MonoBehaviour //parent should be PlayerClient
{

    private KeyCode MapActivatorKey = KeyCode.M;

    public void FindValues(Player P) //called by Levelmanager
    {
        MapActivatorKey = P.getControls().getKey("Show Map");
        transform.GetComponent<Camera>().enabled = false;
    }

    void Update()
    {
        if (Game_Manager.UpdateAllowed)
        {
            if (Input.GetKeyDown(MapActivatorKey))
            {
                transform.position = Game_Manager.LevelManager.transform.up * 1000;
                transform.rotation = Quaternion.LookRotation(-Game_Manager.LevelManager.transform.up, Game_Manager.LevelManager.transform.forward);
                transform.GetComponent<Camera>().enabled = true;
                transform.GetComponent<Camera>().orthographicSize = Game_Manager.LevelManager.getLevelRadius();
            }
            if (Input.GetKeyUp(MapActivatorKey))
            {
                transform.GetComponent<Camera>().enabled = false;
            }
        }
    }

}