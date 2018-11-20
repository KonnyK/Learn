using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Player_Manager : NetworkBehaviour {

    private static List<Player> Players = new List<Player>() { };

    [Server] public void Initialize()
    {
        foreach 
    }
    [ClientRpc] private void RpcInitialize(string Name, int Number)
    {
        if (!hasAuthority && !isServer)
        {
            transform.GetComponent<CollisionDetect>().enabled = false;
            transform.GetComponent<Movement>().enabled = false;
        }
        else transform.GetComponent<CollisionDetect>().Initialize();
        P_Number = Number;
        P_Name = Name;
        gameObject.name = "Player" + P_Number;
        Mesh = Instantiate(PlayerModel, transform).transform;

        //rename all Objects with the Playernumber at the end
        for (int i = 0; i < transform.childCount; i++) transform.GetChild(i).name += P_Number;
        //Initialize Children
        transform.GetComponentInChildren<PlayerCamControl>().Initialize(this);
    }

    public Controls getLocalControls()
    {
        Player P = getLocalPlayer();
        if (P != null) return P.getControls();
        else return null;
    }

    public Player getLocalPlayer()
    {
        foreach (Player P in Players) if (P.transform.GetComponent<NetworkIdentity>().hasAuthority) return P;
        return null;
    }
    
    [Client]
    public void RegisterNewPlayer(Player NewPlayer)
    {
        int Index = Players.Count;
        uint NewPlayerNetID = NewPlayer.GetComponent<NetworkIdentity>().netId.Value;
        for (int i = 0; i < Players.Count; i++)
        {
            if (Players[i].GetComponent<NetworkIdentity>().netId.Value > NewPlayerNetID)
            {
                Index = i;
                break;
            }
        }
        Players.Insert(Index, NewPlayer);
        Debug.Log("Inserted Player at " +Index + ". PlayerCount:" + Players.Count,this);
        NewPlayer.CmdInitialize("Player", NewPlayerNetID);
    }

    public void RespawnAll()
    {
        foreach (Player P in Players)
        {
            P.CmdRespawn();
        }
    }
/*
    [SerializeField] private List<Player> Players = new List<Player>() { };
    [SerializeField] private List<int> LocalPlayerIndexes = new List<int>() { };

    public Player[] getLocalPlayers()
    {
        
        Player[] Result = new Player[LocalPlayerIndexes.Count];
        for (int i = 0; i < LocalPlayerIndexes.Count; i++) Result[i] = Players[LocalPlayerIndexes[i]];
        return Result;
        

    }

    public void RegisterNewPlayer(Player NewPlayer)
{
    int Index = Players.Count;
    uint NewPlayerNetID = NewPlayer.GetComponent<NetworkIdentity>().netId.Value;
    for (int i = 0; i < Players.Count; i++)
    {
        if (Players[i].GetComponent<NetworkIdentity>().netId.Value > NewPlayerNetID)
        {
            Index = i;
            break;
        }
    }
    Players.Insert(Index, NewPlayer);
    //if (isLocal) LocalPlayerIndexes.Add(Index);
    NewPlayer.CmdInitialize("Player", NewPlayerNetID);
}

*/
}