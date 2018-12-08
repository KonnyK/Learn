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
        Debug.Log("ColDetect Init", Mesh);
    }

    void Update()
    {
                Debug.Log("Movement: " + Game_Manager.UpdateAllowed + " " + myPlayer.isAlive());
        if (Game_Manager.UpdateAllowed)
        {
            RaycastHit Hit;
            if (myPlayer.isAlive())
            {
                if (Physics.Raycast(Mesh.position, Vector3.down, out Hit))
                {
                    //handles Invincibility if Alive and something is below
                    if (Hit.transform.gameObject.tag == "CP")
                    {
                        Debug.Log("CollisionRay hit CP", this);
                        myPlayer.ChangeStatus(2);
                        //Loading next Level if its  the last Checkpoint
                        if (GameManager.getLevelManager().IsInGoal(Mesh.position))
                        {
                            Debug.Log("Final Checkpoint!", this);
                            myPlayer.ChangeStatus(0);
                            GameManager.PrepareNextLvl();
                        }
                    }
                    else if (Hit.transform.gameObject.tag == "PF")
                    {
                        myPlayer.ChangeStatus(1);
                        Debug.Log("CollisionRay hit PF", this);
                    }
                }
                else //=> nothing below
                {
                    Debug.Log("CollisionRay hit nothing",this);
                    PlayerManager.KillPlayer();
                    PlayerManager.RespawnInvoke(3);
                }
            }
        }
    }


    private void OnTriggerEnter(Collider Other)
    {
        if (Game_Manager.UpdateAllowed)
        {
            if (Other.transform.parent.tag == "Enemy" && !myPlayer.isInvincible() && myPlayer.isAlive())
            {
                Debug.Log("collision with Enemy", this);
                PlayerManager.KillPlayer();
                PlayerManager.RespawnInvoke(10);
            }
        }
    }

}
