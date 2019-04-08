using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionDetect : MonoBehaviour {

    [SerializeField] private Player myPlayer;
    private Transform LastHit = null;

    public Vector3 RedirectIllegalMove(Vector3 Dir)
    {
        if (LastHit == null) return Vector3.zero; 
        Vector3 Result;
        switch (LastHit.tag)
        {
            case "PF":
                Result = Vector3.Project(Dir, LastHit.parent.forward).normalized;
                Result += Vector3.Normalize(Result - Dir) * Time.deltaTime;
                Debug.Log("Illegal PF Move");
                break;
            case "CP":
                Result = Vector3.Project(Dir, Vector3.Cross(Vector3.up, Vector3.ProjectOnPlane(transform.position-LastHit.position, Vector3.up))).normalized;
                Result -= Vector3.Normalize(transform.position - LastHit.position);
                Debug.Log("Illegal CP Move");
                break;
            default:
                Result = Vector3.zero;
                break;
        }
        Debug.DrawRay(transform.position, Result, Color.yellow, 1000);
        return Result;
    }

    void Update()
    {

        if (!Game_Manager.UpdateAllowed || !myPlayer.isAlive()) return;

        RaycastHit[] Hits;
        Hits = Physics.RaycastAll(myPlayer.Mesh.position + Vector3.up, Vector3.down, 5);
        int HitAmount = 0;
        bool CPHit = false;
        foreach (RaycastHit Hit in Hits)
        if (Hit.transform.tag == "CP" || Hit.transform.tag == "PF")
            {
                HitAmount++;
                if (LastHit == Hit.transform) return;
                LastHit = Hit.transform;
            //handles Invincibility if Alive and something is below
            if (Hit.transform.gameObject.tag == "CP")
            {
                //Debug.Log("CollisionRay hit CP", this);
                myPlayer.ChangeStatus(2);
                    if (!Hit.transform.GetComponent<ParticleSystem>().isPlaying)
                    {
                        Hit.transform.GetComponent<ParticleSystem>().Play();
                        Hit.transform.GetChild(0).GetComponent<Light>().enabled = true;
                        myPlayer.GetComponentInChildren<Light>().spotAngle += 5;
                    }
                //Loading next Level if its  the last Checkpoint
                if (Game_Manager.LevelManager.IsInGoal(myPlayer.Mesh.position))
                {
                    Debug.Log("Final Checkpoint!", this);
                    myPlayer.ChangeStatus(0);
                    Game_Manager.InvokePrepareNextLvl(3);
                }
                    CPHit = true;
            }
            else if (Hit.transform.gameObject.tag == "PF" && !CPHit)
            {
                myPlayer.ChangeStatus(1);
                //Debug.Log("CollisionRay hit PF", this);
            }
        }
        if (HitAmount == 0){ //don't just use Hits.Length because it might contain the Playermodel itself as well
            Debug.Log("Player killed: Outside map", this);
            Game_Manager.PlayerManager.KillPlayer(transform.GetComponent<Player>());
            Game_Manager.PlayerManager.RespawnInvoke(3, myPlayer);
        }
        
    }
    /*
    private void OnCollisionStay(Collision collision)
    {
        if (!Game_Manager.UpdateAllowed || !myPlayer.isAlive()) return;
        if (collision.collider.transform.tag == "CP")
        {
            Debug.Log("CollisionRay hit CP", this);
            myPlayer.SetLastCP(collision.collider.transform.position);
            myPlayer.ChangeStatus(2);
            //Loading next Level if its  the last Checkpoint
            if (GameManager.getLevelManager().IsInGoal(Mesh.position))
            {
                Debug.Log("Final Checkpoint!", this);
                myPlayer.ChangeStatus(0);
                GameManager.InvokePrepareNextLvl(3);
            }
        }
        else if (collision.collider.transform.tag == "PF")
        {
            myPlayer.ChangeStatus(1);
            Debug.Log("CollisionRay hit PF", this);
        }
    }*/

    private void OnTriggerEnter(Collider Other)
    {
        if (!Game_Manager.UpdateAllowed || !myPlayer.isAlive()) return;
        if (Other.transform.parent.tag == "Enemy" && !myPlayer.isInvincible())
        {
            Debug.Log("collision with Enemy", this);
            Game_Manager.PlayerManager.KillPlayer(transform.GetComponent<Player>());
            Game_Manager.PlayerManager.RespawnInvoke(3, myPlayer);
        }
    }

}
