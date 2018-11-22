﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Player : MonoBehaviour
{

    [SerializeField] private int P_Number;//PlayerNumber

    [SerializeField] private int P_Status = 0; //used to check wether player is alive, dead, invincible, etc.
    [SerializeField] private Controls P_Controls = new Controls(); //contains KeyCodes

    [SerializeField] private float SP; //Stamina
    [SerializeField] private float MaxSP = 100;
    [SerializeField] private bool Invincible; //used for Checkpoints

    [SerializeField] private Vector3 SpawnPos; //Spawnpoint
    private readonly int RespawnTime = 3; //Time it will take for the Player to respawn once respawning was started
    [SerializeField] private uint Deathcount = 0; //how many times the Player died

    [SerializeField] private GameObject PlayerModel; //set in Editor
    private Transform Mesh;


    //Getters
    public uint getDeathcount() { return Deathcount; }
    public bool isAlive() { return (P_Status > 0); }
    public bool isInvincible() { return Invincible; }
    public int PlayerNumber() { return P_Number; }
    public string GetName() { return P_Name; }
    public Controls getControls() { return P_Controls; }

    public void SetMesh(Transform Mesh) { this.Mesh = Mesh; }

    public void Initialize(string Name, uint Number)
    {
        int Num = checked((int)Number); //checked returns overflow error if uint > int.maxvalue
        RpcInitialize(Name, Num);
    }

    [Client]
    private void FixedUpdate()
    {
        if (isAlive() & Game_Manager.UpdateAllowed) RecoverStamina(1);
        //else transform.Find("Info" + P_Number).GetComponent<PlayerFeed>().DeathMessage(Time.time - TimeofDeath, transform.parent.name);
        if (hasAuthority) CmdSyncTransforms(transform.position, transform.rotation, Mesh.forward);
    }

    //Stamina management
    public bool ConsumeStamina(float Amount)
    {
        if (SP >= Amount) { SP -= Amount; return true; }
        else return false;
    }
    public bool RecoverStamina(float Amount)
    {
        if (SP + Amount <= MaxSP) { SP += Amount; return true; }
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
        Level_Manager LevelManager = transform.GetComponent<Game_Manager>().getLevelManager();
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
            SpawnPos = newPos + Vector3.up * 5;
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
                        ShowMesh(false); RB.isKinematic = true; Invincible = true; RB.useGravity = false;
                    }
                    break;
                case -1:
                    {
                        RB.velocity = Vector3.zero;
                        ShowMesh(true); RB.isKinematic = true; Invincible = true; RB.useGravity = true;
                    }
                    break;
                case 0:
                    {
                        RB.velocity = Vector3.zero;
                        ShowMesh(false); RB.isKinematic = true; Invincible = true; RB.useGravity = false;
                    }
                    break;
                case 1:
                    {
                        ShowMesh(true); RB.isKinematic = false; Invincible = false; RB.useGravity = true;
                    }
                    break;
                case 2:
                    {
                        ShowMesh(true); RB.isKinematic = false; Invincible = true; RB.useGravity = true;
                    }
                    break;
            }
        }
    }

   

}
