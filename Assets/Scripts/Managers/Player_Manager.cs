using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Player_Manager : NetworkBehaviour {

    [SerializeField] private List<Player> Players = new List<Player>() { };
    [SerializeField] private GameObject defaultPlayer; //PlayerPrefab

    public Controls getControls(int PlayerNumber)
    {
        return Players[PlayerNumber].getControls();
    }

    public Player getLocalPlayer()
    {
        foreach (Player P in Players) if (P.transform.GetComponent<NetworkIdentity>().hasAuthority) return P;
        return null;
    }

    public void NewPlayer(string Name)
    {
        GameObject.Instantiate(defaultPlayer, transform);
        transform.GetChild(transform.childCount - 1).GetComponent<Player>().Initialize(Name);
        Players.Add(transform.GetChild(transform.childCount - 1).GetComponent<Player>());
    }

    public void RespawnAll()
    {
        foreach (Player P in Players)
        {
            P.Respawn();
        }
    }


}
