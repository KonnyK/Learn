using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Player : NetworkBehaviour {

    [SerializeField] private string P_Name;
    [SerializeField] private int P_Number;//PlayerNumber

    [SerializeField] private int P_Status = 0; //used to check wether player is alive, dead, invincible, etc.
    [SerializeField] private Controls P_Controls = new Controls(); //contains KeyCodes

    [SerializeField] private float SP; //Stamina
    [SerializeField] private float MaxSP = 100;
    [SerializeField] private bool Invincible; //used for Checkpoints

    [SerializeField] private Vector3 SpawnPos; //Spawnpoint
    [SerializeField] private readonly int RespawnTime = 3; //Time it will take for the Player to respawn once respawning was started
    [SerializeField] private uint Deathcount = 0; //how many times the Player died

    [SerializeField] private Game_Manager GameManager;
    [SerializeField] private Transform Mesh; //set in editor


    //Getters
    public uint getDeathcount() { return Deathcount; }
    public bool isAlive() { return (P_Status > 0); }
    public bool isInvincible() { return Invincible; }
    public int PlayerNumber() { return P_Number; }
    public string GetName() { return P_Name; }
    private Controls getControls() { return P_Controls; }

    [ClientRpc]
    private void RpcInitialize(string Name, int Number)
    {
        if (!hasAuthority && !isServer)
        {
            transform.GetComponent<CollisionDetect>().enabled = false;
            transform.GetComponent<Movement>().enabled = false;
        }
        P_Number = Number;
        P_Name = Name;
        gameObject.name = "Player" + P_Number;

        GameManager = transform.GetComponentInParent<Game_Manager>();

        //rename all Objects with the Playernumber at the end
        for (int i = 0; i < transform.childCount; i++) transform.GetChild(i).name += P_Number;
        //Initialize Children
        transform.GetComponentInChildren<PlayerCamControl>().Initialize(this);
    }

    [Command]
    public void CmdInitialize(string Name, uint Number)
    {
        int Num = checked((int)Number); //checked returns overflow error if uint > int.maxvalue
        RpcInitialize(Name, Num);
    }

    private void FixedUpdate()
    {
        if (isAlive() & GameManager.CanUpdate()) RecoverStamina(1);
        //else transform.Find("Info" + P_Number).GetComponent<PlayerFeed>().DeathMessage(Time.time - TimeofDeath, transform.parent.name);
        if (hasAuthority) CmdSyncTransforms(transform.position, transform.rotation, Mesh.forward); 
    }

    [Command]
    private void CmdSyncTransforms(Vector3 Pos, Quaternion Rot, Vector3 Facing)
    {
        RpcSyncTransforms(Pos, Rot, Facing);
    }
    [ClientRpc]
    private void RpcSyncTransforms(Vector3 Pos, Quaternion Rot, Vector3 Facing)
    {
        if (!hasAuthority)
        {
            transform.position = Pos;
            transform.rotation = Rot;
            OrientateMesh(Facing, Vector3.up);
        }
    }

    //Stamina management
    public bool ConsumeStamina(float Amount)
    {
        if (SP >= Amount) {SP -= Amount; return true;}
        else return false;
    }
    public bool RecoverStamina(float Amount)
    {
        if (SP + Amount <= MaxSP) {SP += Amount; return true;}
        else return false;
    }



    //turns PlayerModel
    public void OrientateMesh(Vector3 Forward, Vector3 Up)
    {
        if (Vector3.Magnitude(Forward) >= 0.05f) Mesh.rotation = Quaternion.LookRotation(Forward, Up);
    }
    //activates/deactivates Playermodel
    public void ShowMesh(bool MeshActive) { Mesh.gameObject.SetActive(MeshActive); }

    //randomly choose a new Spawnlocation and check if Spawning is safe
    public bool FindNewSpawn()
    {
        Level_Manager LevelManager = GameManager.getLevelManager();
        RaycastHit Hit;
        Vector3 newPos = Vector3.zero;

        //draw Debug Box
        Vector3 V0 = LevelManager.GetSpawnArea()[0];
        Vector3 V1 = V0 + LevelManager.GetSpawnArea()[1];
        Debug.DrawLine(new Vector3(V0.x, V1.y, V1.z), V1, Color.green, 1000);//Possible SpawnArea
        Debug.DrawLine(new Vector3(V1.x, V1.y, V0.z), V1, Color.green, 1000);
        Debug.DrawLine(new Vector3(V0.x, V0.y, V1.z), V0, Color.green, 1000);
        Debug.DrawLine(new Vector3(V1.x, V0.y, V0.z), V0, Color.green, 1000);


        int MaxTries = 100; //after more tries to find a spawn it stops searching
        int TryAmount = 0; 

        do
        {
            //randomly generating Position
            newPos = LevelManager.GetSpawnArea()[0];
            newPos.x += UnityEngine.Random.value * (LevelManager.GetSpawnArea()[1].x);
            newPos.y += UnityEngine.Random.value * (LevelManager.GetSpawnArea()[1].y);
            newPos.z += UnityEngine.Random.value * (LevelManager.GetSpawnArea()[1].z);            
            TryAmount++;

        } while ((!Physics.Raycast(newPos, Vector3.down, out Hit) || Hit.transform.tag != "CP") && TryAmount <= MaxTries);
        if (TryAmount <= MaxTries)
        {
            Debug.Log("Found new Spawn after " + TryAmount + " tries.");
            Debug.DrawLine(Vector3.zero, newPos, Color.red, 1000); // newfound SpawnPos
            SpawnPos = newPos;
            return true;
        }
        else
        {
            Debug.Log("No Spawn Found");
            return false;
        }
    }

    //switch-case for P_Status
    [Command]
    public void CmdChangeStatus(int newStatus) { RpcChangeStatus(newStatus); }
    [ClientRpc]
    private void RpcChangeStatus(int newStatus)
        ///Status:
        /// -2: dead, invisible, no collision, invincible, no gravity        (respawning)
        /// -1: dead, visible, no collision, invincible, gravity             (not respawning)
        ///  0: none, invisible, no collision, invincible, no gravity        (just joined the Game, never spawned yet)
        ///  1: alive, visible, collision, vincible, gravity                 (on Platform)
        ///  2: alive, visible, collision, invincible, gravity               (on Checkpoint)
    {
        Rigidbody RB = transform.GetComponent<Rigidbody>();
        if (newStatus >= -2 && 2 >= newStatus)
        {
            P_Status = newStatus;
            switch (newStatus)
            {
                case -2:
                    {
                        RB.velocity = Vector3.zero;
                        ShowMesh(false); RB.isKinematic = true;   Invincible = true;    RB.useGravity = false;
                    }
                    break;
                case -1:
                    {
                        RB.velocity = Vector3.zero;
                        ShowMesh(true);  RB.isKinematic = true;   Invincible = true;     RB.useGravity = true;
                    }
                    break;
                case 0:
                    {
                        RB.velocity = Vector3.zero;
                        ShowMesh(false); RB.isKinematic = true;   Invincible = true;     RB.useGravity = false;
                    }
                    break;
                case 1:
                    {
                        ShowMesh(true);  RB.isKinematic = false;  Invincible = false;    RB.useGravity = true;
                    }
                    break;
                case 2:
                    {
                        ShowMesh(true);  RB.isKinematic = false;  Invincible = true;     RB.useGravity = true;
                    }
                    break;
            }
        }
    }

    [Command]
    public void CmdKill() { RpcKill(); }
    [ClientRpc]
    private void RpcKill()
    {
        Debug.Log("Player died!");
        Deathcount++;
        CmdChangeStatus(-1);
        Mesh.rotation = Quaternion.LookRotation(Vector3.up);
    }

    //usually called (from outside) this delays the Respawn
    //runs only on 1 Client!!!!!
    [Client]
    private void RequestRespawn()
    {
        CmdChangeStatus(-2);
        Invoke("CmdRespawn", RespawnTime);
    }

    //internally called and  exceptionally for instant (re)spawning
    [Command]
    private void CmdRespawn() { RpcRespawn(); }
    [ClientRpc]
    private void RpcRespawn()
    {
        if (FindNewSpawn())
        {
            transform.localPosition = SpawnPos;
            Mesh.rotation = Quaternion.LookRotation(new Vector3(2 * UnityEngine.Random.value - 1, 0, 2 * UnityEngine.Random.value - 1), Vector3.up); ;
            CmdChangeStatus(2);
            SP = MaxSP;
        }
    }  

}
