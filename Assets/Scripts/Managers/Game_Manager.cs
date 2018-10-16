using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Game_Manager : NetworkBehaviour {

    [SerializeField] private Level_Manager LevelManager;
    [SerializeField] private Player_Manager PlayerManager;
    [SerializeField] private Enemy_Manager EnemyManager;
    [SerializeField, SyncVar] private bool allowUpdate = false;  //used for pausing the game, only the "Server" has the the true value.
    [SerializeField] private Game_Manager ServerManager; //usen on by not servers only
    
    [ClientRpc]
    public void RpcRename(string newName)
    {
        gameObject.name = newName;
    }

    public void Start()
    {
        if (localPlayerAuthority)
        {//making sure the server is called Server on every Client
            if (isServer)
            {
                gameObject.name = "Server";
                RpcRename("Server");//maybe not needed
            }
            else if (gameObject.name != "Server") { gameObject.name = "Server"; Debug.Log("Server Benennung fehlgeschlagen.", this); }
        }

        if (gameObject.name == "Server")
        {
            ServerManager = null; //not "this" to avoid unseen endless loops with allowupdate
            LevelManager = transform.GetComponent<Level_Manager>();
            PlayerManager = transform.GetComponent<Player_Manager>();
            EnemyManager = transform.GetComponent<Enemy_Manager>();
            LevelManager.Initialize();
        } else
        {
            ServerManager = GameObject.Find("Server").GetComponent<Game_Manager>();
            LevelManager = ServerManager.GetComponent<Level_Manager>();
            PlayerManager = ServerManager.GetComponent<Player_Manager>();
            EnemyManager = ServerManager.GetComponent<Enemy_Manager>();
            transform.GetComponent<Level_Manager>().enabled = false;
            transform.GetComponent<Player_Manager>().enabled = false;
            transform.GetComponent<Enemy_Manager>().enabled = false;
        }
        Debug.Log("LevelManager: " + LevelManager.transform.GetComponent<NetworkIdentity>().netId, this);
        Debug.Log("PlayerManager: " + PlayerManager.transform.GetComponent<NetworkIdentity>().netId, this);
        Debug.Log("EnemyManager: " + EnemyManager.transform.GetComponent<NetworkIdentity>().netId, this);
        
        CmdPrepareNextLvl();
        allowUpdate = true;
    }

    public bool CanUpdate()
    {
        if (gameObject.name == "Server") return allowUpdate;
        else return ServerManager.CanUpdate();
    }

    public void CmdPrepareNextLvl()
    {
        Debug.Log("Preparing next lvl", this);
        allowUpdate = false;
        LevelManager.CmdLoadNextLevel();
        PlayerManager.RespawnAll(); 
        EnemyManager.RefreshMaxDistance();
        allowUpdate = true;
    }
}
