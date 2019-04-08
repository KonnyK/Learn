using System;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private int P_Status = 0; //used to check wether player is alive, dead, invincible, etc.
    private int P_Number = 0;
    public int Type = 0;
        
    public bool isAlive() { return (P_Status > 0); }
    private bool Invincible; //used for Checkpoints
    public bool isInvincible() { return Invincible; }

    private uint Deathcount = 0;
    private void SetDeathCount(uint NewCount) { Deathcount = NewCount; }
    public uint getDeathcount() { return Deathcount; }
    public void IncDeathCount() { SetDeathCount(Deathcount + 1); }
    [SerializeField] private Controls P_Controls = new Controls(); //contains KeyCodes
    public Controls getControls() { return P_Controls; }
    public Transform Mesh;
    private void ShowMesh(bool MeshActive) {foreach (Transform Child in Mesh) Child.GetChild(0).gameObject.SetActive(MeshActive); }

    private void Start()
    {
        SetNewControls();
    }

    public void SetMesh(Transform NewMesh)
    {
        Debug.Log("Setting Mesh" + NewMesh.name, this);
        Mesh = NewMesh;
    }
    public void SetNewControls()
    {
        GetComponent<Movement>().enabled = true;
        GetComponentInChildren<PlayerCamControl>().SetKeyCode(P_Controls.getKey("Show Map"));
        GetComponentInChildren<MapCamControl>().FindValues(this);
    }

    public void SetNumber(int Num)
    {
        P_Number = Num;
        RenameChildren();
    }
    private void RenameChildren() { foreach (Transform Child in transform) Child.name += P_Number; }

    public void ChangeStatus(int newStatus)
    ///Status:
    /// -2: dead, invisible, no collision, invincible, no gravity        (respawning)
    /// -1: dead, visible, no collision, invincible, gravity             (not respawning)
    ///  0: none, invisible, no collision, invincible, no gravity        (just joined the Game, never spawned yet)
    ///  1: alive, visible, collision, vincible, gravity                 (on Platform)
    ///  2: alive, visible, collision, invincible, gravity               (on Checkpoint)
    {
        if (P_Status == newStatus) return;
        if (Math.Abs(newStatus) <= 2)
        {
            Debug.Log("Changing Status to:" + newStatus, this);
            P_Status = newStatus;
            Rigidbody RB = GetComponent<Rigidbody>();
            switch (newStatus)
            {
                case -2:
                    {
                        RB.velocity = Vector3.zero;
                        ShowMesh(false); Invincible = true; RB.useGravity = false;
                    }
                    break;
                case -1:
                    {
                        RB.velocity = Vector3.zero;
                        ShowMesh(true); Invincible = true; RB.useGravity = true;
                    }
                    break;
                case 0:
                    {
                        RB.velocity = Vector3.zero;
                        ShowMesh(false); Invincible = true; RB.useGravity = false;
                    }
                    break;
                case 1:
                    {
                        ShowMesh(true); Invincible = false; RB.useGravity = true;
                    }
                    break;
                case 2:
                    {
                        ShowMesh(true); Invincible = true; RB.useGravity = true;
                    }
                    break;
            }
        }
    }

}

