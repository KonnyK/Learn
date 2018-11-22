using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Player_Manager : NetworkBehaviour {

    private static List<Player> Players = new List<Player>() { }; //order is different on every  client
    [SerializeField] private GameObject[] PlayerModels; // set in Editor

    [Client]
    public void Initialize()
    {
        if (!isServer)
        {
            CmdRequestPlayerValues();
            transform.GetComponent<CollisionDetect>().enabled = false;
        }
        else transform.GetComponent<CollisionDetect>().Initialize();

        Player P = GetComponent<Player>();
        P.SetMesh(Instantiate(PlayerModels[0]).transform);

        transform.GetComponent<Movement>().enabled = transform.GetComponent<PlayerCamControl>().enabled = hasAuthority;
        if (hasAuthority) transform.GetComponentInChildren<PlayerCamControl>().Initialize(P.getControls());
        

        int P_Number = checked((int)this.netId.Value);
        int Index = Players.Count;
        for (int i = 0; i < Players.Count; i++) if (Players[i].GetComponent<NetworkIdentity>().netId.Value > P_Number)
            {
                Index = i;
                break;
            }
        P_Number = Index;
        P.SetNumber(P_Number);
        gameObject.name = "Player" + P_Number;
        foreach (Transform Child in transform) Child.name += P_Number;
        Players.Insert(Index, P);
        Debug.Log("Inserted Player at " + Index + ". PlayerCount:" + Players.Count, this);
    }

    [Server] public void RespawnAll()
    {
        foreach (Player P in Players)
        {
            Respawn(P);
        }
    }

    [Server] public void Respawn(int Number) {  };
    [Server] private void Respawn(Player P)
    {

        if (P.FindNewSpawn())
        {
            transform.localPosition = SpawnPos;
            Mesh.rotation = Quaternion.LookRotation(new Vector3(2 * UnityEngine.Random.value - 1, 0, 2 * UnityEngine.Random.value - 1), Vector3.up); ;
            CmdChangeStatus(2);
            SP = MaxSP;
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
    [Command]
    public void CmdRequestRespawn()
    {
        RpcChangeStatus(-2);
        Invoke("RpcRespawn", RespawnTime);
    }
