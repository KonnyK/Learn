using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Player : NetworkBehaviour
{
    [SyncVar] private int P_Number;
    public int PlayerNumber() { return P_Number; }

    [SyncVar] private int P_Status = 0; //used to check wether player is alive, dead, invincible, etc.
    public bool isAlive() { return (P_Status > 0); }
    [SyncVar] private bool Invincible; //used for Checkpoints
    public bool isInvincible() { return Invincible; }

    [SyncVar] private uint Deathcount = 0;
    public uint getDeathcount() { return Deathcount; }
    public void IncDeathCount() { Deathcount++; }

    [SerializeField] private Controls P_Controls = new Controls(); //contains KeyCodes
    public Controls getControls() { return P_Controls; }

    public Transform Mesh;
    [Command] public void CmdOrientateMesh(Vector3 Dir, Vector3 up) { RpcOrientateMesh(Mesh.position, Quaternion.LookRotation(Dir, up)); }
    [ClientRpc] public void RpcOrientateMesh(Vector3 Pos, Quaternion Rot) { if (hasAuthority) { Mesh.position = Pos; Mesh.rotation = Rot; } }
    [ClientRpc] private void RpcShowMesh(bool MeshActive) { if (hasAuthority) Mesh.gameObject.SetActive(MeshActive); }
    private Rigidbody MeshRB = null;
    public Rigidbody getRB() { return MeshRB; }

    public void SetMesh(Transform NewMesh) 
    {
        Mesh = NewMesh;
        Mesh.name = "Mesh" + P_Number;
        MeshRB = Mesh.GetComponent<Rigidbody>();
    }

    public void SetNewControls()
    {
        GetComponent<Movement>().Initialize();
        Mesh.parent.GetComponentInChildren<PlayerCamControl>().SetKeyCode(P_Controls.getKey("Show Map"));
    }

    [ClientRpc] public void RpcSetNumber(int Num)
    {
        if (isServer) P_Number = Num;
        gameObject.name = "Player" + P_Number;
        foreach (Transform Child in transform) Child.name += P_Number;
    }


    [Command] public void CmdSetNewCourse(Vector3 Pos, Vector3 Vel) { RpcSetNewCourse(Pos, Vel); }
    [Client] private void RpcSetNewCourse(Vector3 Pos, Vector3 Vel)
    {
        if (!hasAuthority) return;
        Mesh.position = Pos;
        MeshRB.velocity = Vel;
        Mesh.rotation = Quaternion.LookRotation(MeshRB.velocity, Vector3.up);
    }

    [Server] public void ChangeStatus(int newStatus)
    ///Status:
    /// -2: dead, invisible, no collision, invincible, no gravity        (respawning)
    /// -1: dead, visible, no collision, invincible, gravity             (not respawning)
    ///  0: none, invisible, no collision, invincible, no gravity        (just joined the Game, never spawned yet)
    ///  1: alive, visible, collision, vincible, gravity                 (on Platform)
    ///  2: alive, visible, collision, invincible, gravity               (on Checkpoint)
    {
        Debug.Log("Changing Status to:" + newStatus, this);
        if (newStatus >= -2 && 2 >= newStatus)
        {
            P_Status = newStatus;
            switch (newStatus)
            {
                case -2:
                    {
                        MeshRB.velocity = Vector3.zero;
                        RpcShowMesh(false); MeshRB.isKinematic = true; Invincible = true; MeshRB.useGravity = false;
                    }
                    break;
                case -1:
                    {
                        MeshRB.velocity = Vector3.zero;
                        RpcShowMesh(true); MeshRB.isKinematic = true; Invincible = true; MeshRB.useGravity = true;
                    }
                    break;
                case 0:
                    {
                        MeshRB.velocity = Vector3.zero;
                        RpcShowMesh(false); MeshRB.isKinematic = true; Invincible = true; MeshRB.useGravity = false;
                    }
                    break;
                case 1:
                    {
                        RpcShowMesh(true); MeshRB.isKinematic = false; Invincible = false; MeshRB.useGravity = true;
                    }
                    break;
                case 2:
                    {
                        RpcShowMesh(true); MeshRB.isKinematic = false; Invincible = true; MeshRB.useGravity = true;
                    }
                    break;
            }
        }
    }

}

