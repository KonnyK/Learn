using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class CollisionDetect : NetworkBehaviour {

    [SerializeField] private Player player; //set in Editor
    private Game_Manager GameManager;

    [Client]
    public void Initialize()
    {
        GameManager = transform.GetComponent<Game_Manager>();
    }

    void Update()
    {
        if (Game_Manager.UpdateAllowed && hasAuthority)
        {
            RaycastHit Hit;
            if (player.isAlive()) if (Physics.Raycast(transform.position, Vector3.down, out Hit))
                {
                    //handles Invincibility if Alive and something is below
                    if (Hit.transform.gameObject.tag == "CP")
                    {
                        player.CmdChangeStatus(2);
                        //Loading next Level if its  the last Checkpoint
                        if (GameManager.getLevelManager().IsInGoal(transform.position))
                        {
                            Debug.Log("Final Checkpoint!");
                            GameManager.CmdPrepareNextLvl();
                        }
                    }
                    else if (Hit.transform.gameObject.tag == "PF") player.CmdChangeStatus(1);
                }
                else //=> nothing below
                {
                    Debug.Log("Player killed for floating.");
                    player.CmdKill();
                    player.CmdRequestRespawn();
                }
        }
    }


    private void OnTriggerEnter(Collider Other)
    {
        if (Game_Manager.UpdateAllowed)
        {
            if (Other.transform.parent.tag == "Enemy" & !player.isInvincible() & player.isAlive())
            {
                player.CmdKill();
                player.CmdRequestRespawn();
            }
        }
    }

}
