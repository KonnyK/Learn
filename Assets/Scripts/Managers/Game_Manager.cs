using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Game_Manager : NetworkBehaviour {

    public static readonly string ServerGameManagerName = "Master"; //Name of the GameObject/GameManager that has local authority on Server, name is for all clients
    public static readonly string LocalAuthorityGameManagerName = "Local"; //Name of the GameObject/GameManager with localAuthority, different for everyone, there's no local on the server
    public static readonly string NoAuthorityGameManagerName = "Other";
    [SerializeField] public static bool UpdateAllowed = false; //used for pausing the game
    private Level_Manager LevelManager;
    private Player_Manager PlayerManager;
    private Enemy_Manager EnemyManager;
    private Game_Manager ServerManager; //used by not master only
    
    public Level_Manager getLevelManager() { return LevelManager; }
    public Player_Manager getPlayerManager() { return PlayerManager; }
    public Enemy_Manager getEnemyManager() { return EnemyManager; }

    [ClientRpc]
    public void RpcRename(string newName)
    {
        gameObject.name = newName;
    }

    public void Start()
    {
        gameObject.name = NoAuthorityGameManagerName;
        if (localPlayerAuthority)
        {//making sure the server is called Server on every Client
            if (isServer)
            {
                gameObject.name = ServerGameManagerName;
                RpcRename(ServerGameManagerName);//maybe not needed
            }
            else gameObject.name = LocalAuthorityGameManagerName;
        }

        if (gameObject.name == ServerGameManagerName)
        {
            ServerManager = null; //not "this" to avoid unseen endless loops with canupdate
            LevelManager = transform.GetComponent<Level_Manager>();
            PlayerManager = transform.GetComponent<Player_Manager>();
            EnemyManager = transform.GetComponent<Enemy_Manager>();
            if (isServer)
            {
                LevelManager.Initialize("Level");
                EnemyManager.Initialize("Enemies");
            }
        } else
        {
            ServerManager = GameObject.Find(ServerGameManagerName).GetComponent<Game_Manager>();
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

        PlayerManager.RegisterNewPlayer(transform.GetComponent<Player>());
        if (localPlayerAuthority)
        {
            transform.GetComponentInChildren<MapCamControl>().Initialize();
            if (isServer)
            {
                RpcPrepareNextLvl();
                CmdAllowUpdate(true);
            }
        }
        
    }

    [Command]
    public void CmdAllowUpdate(bool NewState) { RpcSetAllowUpdate(NewState); }
    [ClientRpc]
    private void RpcSetAllowUpdate(bool NewState) { UpdateAllowed = NewState; }

    [Command]
    public void CmdPrepareNextLvl()
    {
        if (gameObject.name == ServerGameManagerName)
        {
            CmdAllowUpdate(false);
            RpcPrepareNextLvl();
            CmdAllowUpdate(true);
        }
        else ServerManager.CmdPrepareNextLvl();
    }
    [ClientRpc]
    private void RpcPrepareNextLvl()
    {
            Debug.Log("Preparing next lvl", this);
            PlayerManager.RespawnAll();
        if (isServer) LevelManager.CmdLoadNextLevel();
    }
}
