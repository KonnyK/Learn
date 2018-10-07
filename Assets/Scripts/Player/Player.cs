using System;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

    [SerializeField] private string P_Name;
    [SerializeField] private int P_Number;//PlayerNumber

    [SerializeField] private int P_Status = 0; //used to check wether player is alive, dead, invincible, etc.
    [SerializeField] private Controls P_Controls = new Controls(); //contains KeyCodes

    [SerializeField] private float SP; //Stamina
    [SerializeField] private float MaxSP = 100;
    [SerializeField] private bool Invincible; //used for Checkpoints

    [SerializeField] private Vector3 SpawnPos; //Spawnpoint
    [SerializeField] private Quaternion SpawnRot; //Spawnrotation
    [SerializeField] private readonly int RespawnTime = 3; //Time it will take for the Player to respawn once respawning was started
    [SerializeField] private uint Deathcount = 0; //how many times the Player died
    [SerializeField] private float TimeofDeath;

    [SerializeField] private Transform Mesh; //set in editor


    //Getters
    public uint getDeathcount() { return Deathcount; }
    public bool isAlive() { return (P_Status > 0); }
    public bool isInvincible() { return Invincible; }
    public int PlayerNumber() { return P_Number; }
    public string GetName() { return P_Name; }
    public Controls getControls() { return P_Controls; }


    public void Initialize(string Name)
    {
        P_Number = transform.GetSiblingIndex();
        P_Name = Name;
        gameObject.name = "Player" + P_Number;

        //renaming all Objects with the Playernumber at the end
        for (int i = 0; i < transform.childCount; i++) transform.GetChild(i).name += P_Number;

        //Initialize Children
        transform.GetComponentInChildren<PlayerCamControl>().Initialize(this);
    }

    private void FixedUpdate()
    {
        if (isAlive() & Game_Manager.CanUpdate()) RecoverStamina(1);
        //else transform.Find("Info" + P_Number).GetComponent<PlayerFeed>().DeathMessage(Time.time - TimeofDeath, transform.parent.name);
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
        if (Vector3.Magnitude(Forward) >= 0.1f) Mesh.rotation = Quaternion.LookRotation(Forward, Up);
    }
    //activates/deactivates Playermodel
    public void ShowMesh(bool MeshActive) { Mesh.gameObject.SetActive(MeshActive); }



    //randomly choose a new Spawnlocation and check if Spawning is safe
    public bool FindNewSpawn()
    {
        RaycastHit Hit;
        Vector3 newPos = Vector3.zero;
        Quaternion newRot = Quaternion.Euler(0, 0, 0);

        //draw Debug Box
        Vector3 V0 = Game_Manager.Levels().GetSpawnArea()[0];
        Vector3 V1 = V0 + Game_Manager.Levels().GetSpawnArea()[1];
        Debug.DrawLine(new Vector3(V0.x, V1.y, V1.z), V1, Color.green, 1000);//Possible SpawnArea
        Debug.DrawLine(new Vector3(V1.x, V1.y, V0.z), V1, Color.green, 1000);
        Debug.DrawLine(new Vector3(V0.x, V0.y, V1.z), V0, Color.green, 1000);
        Debug.DrawLine(new Vector3(V1.x, V0.y, V0.z), V0, Color.green, 1000);


        int MaxTries = 100; //after more tries to find a spawn it stops searching
        int TryAmount = 0; 

        do
        {
            //randomly generating Position
            newPos = Game_Manager.Levels().GetSpawnArea()[0];
            newPos.x += UnityEngine.Random.value * (Game_Manager.Levels().GetSpawnArea()[1].x);
            newPos.y += UnityEngine.Random.value * (Game_Manager.Levels().GetSpawnArea()[1].y);
            newPos.z += UnityEngine.Random.value * (Game_Manager.Levels().GetSpawnArea()[1].z);
            //randomly generating Rotation
            newRot = Quaternion.LookRotation(new Vector3(2 * UnityEngine.Random.value - 1, 0, 2 * UnityEngine.Random.value - 1), Vector3.up);
            TryAmount++;
        } while ((!Physics.Raycast(newPos, Vector3.down, out Hit) || Hit.transform.tag != "CP") && TryAmount <= MaxTries);
        if (TryAmount <= MaxTries)
        {
            Debug.Log("Found new Spawn after " + TryAmount + " tries.");
            Debug.DrawLine(Vector3.zero, newPos, Color.red, 1000); // newfound SpawnPos
            SpawnPos = newPos;
            SpawnRot = newRot;
            return true;
        }
        else
        {
            Debug.Log("No Spawn Found");
            return false;
        }
    }



    //switch-case for P_Status
    public void ChangeStatus(int newStatus)
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



    public void Kill()
    {
        Debug.Log("Player died!");
        Deathcount++;
        ChangeStatus(-1);
        Mesh.rotation = Quaternion.LookRotation(Vector3.up);
    }

    //usually called (from outside) this delays the Respawn
    public void RequestRespawn()
    {
        ChangeStatus(-2);
        Invoke("Respawn", RespawnTime);
    }

    //internally called and  exceptionally for instant (re)spawning
    public void Respawn()
    {
        if (FindNewSpawn())
        {
            transform.localPosition = SpawnPos;
            Mesh.rotation = SpawnRot;
            ChangeStatus(2);
            SP = MaxSP;
        }
    }  

}
