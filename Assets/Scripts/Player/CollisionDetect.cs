using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionDetect : MonoBehaviour {

    [SerializeField] private Player player; //set in Editor
    [SerializeField] private Level_Manager LevelManager;

    private void Initialize()
    {
        LevelManager = GameObject.Find("Server").GetComponent<Level_Manager>();
    }

    void Update()
    {
        if (Game_Manager.CanUpdate())
        {
            RaycastHit Hit;
            if (player.isAlive()) if (Physics.Raycast(transform.position, Vector3.down, out Hit))
                {
                    //handles Invincibility if Alive and something is below
                    if (Hit.transform.gameObject.tag == "CP")
                    {
                        player.ChangeStatus(2);
                        //Loading next Level if its  the last Checkpoint
                        if (LevelManager.IsInGoal(transform.position))
                        {
                            Debug.Log("Final Checkpoint!");
                            GameObject.Find("GameManager").GetComponent<Game_Manager>().PrepareNextLvl();
                        }
                    }
                    else if (Hit.transform.gameObject.tag == "PF") player.ChangeStatus(1);
                }
                else //=> nothing below
                {
                    Debug.Log("Player killed for floating.");
                    player.Kill();
                    player.RequestRespawn();
                }
        }
    }


    private void OnTriggerEnter(Collider Other)
    {
        if (Game_Manager.CanUpdate())
        {
            if (Other.transform.parent.tag == "Enemy" & !player.isInvincible() & player.isAlive())
            {
                player.Kill();
                player.RequestRespawn();
            }
        }
    }

}
