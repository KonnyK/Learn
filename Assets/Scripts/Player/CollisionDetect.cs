using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionDetect : MonoBehaviour {

    private Player myPlayer;
    private Transform Mesh;
    private Player_Manager PlayerManager;
    private Game_Manager GameManager;

    public void Initialize(Transform T)
    {
        GameManager = T.GetComponent<Game_Manager>();
        PlayerManager = T.GetComponent<Player_Manager>();
        myPlayer = T.GetComponent<Player>();
        Mesh = myPlayer.getRB().transform;
    }

    void Update()
    {
        if (Game_Manager.UpdateAllowed)
        {
            RaycastHit Hit;
            if (myPlayer.isAlive()) if (Physics.Raycast(Mesh.position, Vector3.down, out Hit))
                {
                    //handles Invincibility if Alive and something is below
                    if (Hit.transform.gameObject.tag == "CP")
                    {
                        myPlayer.ChangeStatus(2);
                        //Loading next Level if its  the last Checkpoint
                        if (GameManager.getLevelManager().IsInGoal(Mesh.position))
                        {
                            Debug.Log("Final Checkpoint!");
                            GameManager.PrepareNextLvl();
                        }
                    }
                    else if (Hit.transform.gameObject.tag == "PF") myPlayer.ChangeStatus(1);
                }
                else //=> nothing below
                {
                    Debug.Log("Player killed for floating.");
                    PlayerManager.KillPlayer();
                    PlayerManager.RespawnInvoke(3);
                }
        }
    }


    private void OnTriggerEnter(Collider Other)
    {
        if (Game_Manager.UpdateAllowed)
        {
            if (Other.transform.parent.tag == "Enemy" & !myPlayer.isInvincible() & myPlayer.isAlive())
            {
                
                PlayerManager.KillPlayer();
                PlayerManager.RespawnInvoke(3);
            }
        }
    }

}
