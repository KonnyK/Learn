using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Player : NetworkBehaviour
{
    private int P_Number;
    [Server] private void SetPlayerNumber(int Num) { P_Number = Num; RpcSetPlayerNumber(Num); }
    [ClientRpc] private void RpcSetPlayerNumber(int Num) { P_Number = Num; }
    public int PlayerNumber() { return P_Number; }

    private int P_Status = 0; //used to check wether player is alive, dead, invincible, etc.
        
    public bool isAlive() { return (P_Status > 0); }
    private bool Invincible; //used for Checkpoints
    [ClientRpc] private void RpcSetInvincible(bool State) { if (!isServer) Invincible = State; }
    public bool isInvincible() { return Invincible; }

    private uint Deathcount = 0;
    [Server] private void SetDeathCount(uint NewCount) { Deathcount = NewCount; RpcSetDeathCount(NewCount); }
    [ClientRpc] private void RpcSetDeathCount(uint NewCount) { Deathcount = NewCount; }
    public uint getDeathcount() { return Deathcount; }
    public void IncDeathCount() { SetDeathCount(Deathcount + 1); }

    [SerializeField] private Controls P_Controls = new Controls(); //contains KeyCodes
    public Controls getControls() { return P_Controls; }

    public Transform Mesh = null;
    private void ShowMesh(bool MeshActive) {foreach (Transform Child in Mesh) Child.gameObject.SetActive(MeshActive); }
    private Rigidbody MeshRB = null;
    public Rigidbody getRB() { return MeshRB; }
    private bool MeshSet = false;

    public void SetMesh(Transform NewMesh)
    {
        Debug.Log("Setting Mesh" + NewMesh.name, this);
        Mesh = NewMesh;
        MeshRB = Mesh.GetComponent<Rigidbody>();
        Mesh.parent.name = "PlayerModel" + P_Number;
        MeshSet = true;
    }

    private void RenameChildren() { foreach (Transform Child in transform) Child.name += P_Number; }
    [ClientRpc] private void RpcRenameChildren() { RenameChildren(); }
    [Server] public void SetNumber(int Num)
    {
        SetPlayerNumber(Num);
        RenameChildren();
        RpcRenameChildren();
    }
    public void SetNewControls()
    {
        Mesh.GetComponent<Movement>().Initialize(this);
        Mesh.parent.GetComponentInChildren<PlayerCamControl>().SetKeyCode(P_Controls.getKey("Show Map"));
    }

    [Command]
    private void CmdSetImpulse(Vector3 Pos, Vector3 Vel, Quaternion Rot)
    {
        Mesh.position = Pos;
        Mesh.rotation = Rot;
        MeshRB.velocity = Vel;
    }

    [ClientRpc]
    public void RpcRefreshImpulse(Vector3 Pos, Vector3 Vel, Quaternion Rot, bool OverwriteAuthority)
    {
        if (!MeshSet || (hasAuthority && !OverwriteAuthority)) return;
        //Debug.Log("Syncing PlayerImpulse", this);
        Mesh.position = Pos;
        Mesh.rotation = Rot;
        MeshRB.velocity = Vel;
    }

    public void Update()
    {
        if (!Game_Manager.UpdateAllowed) return;
        if (Time.frameCount % 2 != 0) return;
        if (isServer) RpcRefreshImpulse(Mesh.position, MeshRB.velocity, Mesh.rotation, false);
        else if (hasAuthority) CmdSetImpulse(Mesh.position, MeshRB.velocity, Mesh.rotation);
    } 

    [ClientRpc] private void RpcChangeStatus(int NewState)
    {
        if (isServer) return;
        ChangeStatus(NewState);
        Debug.Log("RpcChanged Status", this);
    }
    public void ChangeStatus(int newStatus)
    ///Status:
    /// -2: dead, invisible, no collision, invincible, no gravity        (respawning)
    /// -1: dead, visible, no collision, invincible, gravity             (not respawning)
    ///  0: none, invisible, no collision, invincible, no gravity        (just joined the Game, never spawned yet)
    ///  1: alive, visible, collision, vincible, gravity                 (on Platform)
    ///  2: alive, visible, collision, invincible, gravity               (on Checkpoint)
    {
        if (P_Status == newStatus) return;
        Debug.Log("Changing Status to:" + newStatus, this);
        if (Math.Abs(newStatus) <= 2)
        {
            P_Status = newStatus;
            if (isServer) RpcChangeStatus(newStatus);
            switch (newStatus)
            {
                case -2:
                    {
                        MeshRB.velocity = Vector3.zero;
                        ShowMesh(false); Invincible = true; MeshRB.useGravity = false;
                    }
                    break;
                case -1:
                    {
                        MeshRB.velocity = Vector3.zero;
                        ShowMesh(true); Invincible = true; MeshRB.useGravity = true;
                    }
                    break;
                case 0:
                    {
                        MeshRB.velocity = Vector3.zero;
                        ShowMesh(false); Invincible = true; MeshRB.useGravity = false;
                    }
                    break;
                case 1:
                    {
                        ShowMesh(true); Invincible = false; MeshRB.useGravity = true;
                    }
                    break;
                case 2:
                    {
                        ShowMesh(true); Invincible = true; MeshRB.useGravity = true;
                    }
                    break;
            }
            RpcSetInvincible(Invincible);
        }
    }

}

