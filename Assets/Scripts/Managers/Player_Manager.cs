using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Player_Manager : NetworkBehaviour {

    [SerializeField] private List<Player> Players = new List<Player>() { };
    [SerializeField] private GameObject defaultPlayer; //PlayerPrefab

    public Controls getLocalControls()
    {
        Player P = getLocalPlayer();
        if (P != null) return P.getControls();
        else return null;
    }

    public Player getLocalPlayer()
    {
        foreach (Player P in Players) if (P.transform.GetComponentInParent<NetworkIdentity>().hasAuthority) return P;
        return null;
    }
    
    public void RegisterNewPlayer(Player NewPlayer)
    {
        int Index = Players.Count;
        uint NewPlayerNetID = NewPlayer.GetComponentInParent<NetworkIdentity>().netId.Value;
        for (int i = 0; i < Players.Count; i++)
        {
            if (Players[i].GetComponentInParent<NetworkIdentity>().netId.Value > NewPlayerNetID)
            {
                Index = i;
                break;
            }
        }
        NewPlayer.Initialize("Player", NewPlayerNetID);
        Players.Insert(Index, NewPlayer);
    }

    public void RespawnAll()
    {
        foreach (Player P in Players)
        {
            P.Respawn();
        }
    }


}
