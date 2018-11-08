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
    private static Game_Manager LocalManager = null;
    private static uint MasterNetId = 0;

    public Level_Manager getLevelManager() { return LevelManager; }
    public Player_Manager getPlayerManager() { return PlayerManager; }
    public Enemy_Manager getEnemyManager() { return EnemyManager; }

    [ClientRpc] private void RpcAllowUpdate(bool NewState) { UpdateAllowed = NewState; }
    [Command] private void CmdAllowUpdate(bool NewState) { UpdateAllowed = NewState; }

    public void AllowUpdate(bool NewState)
    {
        if (!(hasAuthority || isServer)) return;
        UpdateAllowed = NewState;
        if (isServer) RpcAllowUpdate(NewState);
        else CmdAllowUpdate(NewState);
    }

    [Command] void CmdFetchMasterNetId() { RpcSetMasterNetId(MasterNetId); }
    [ClientRpc] void RpcSetMasterNetId(uint ID) { MasterNetId = ID; }

    public void Start()
    {
        if (isLocalPlayer)
        {
            LocalManager = this;
            if (isServer) MasterNetId = this.netId.Value;
            else CmdFetchMasterNetId();
        }

        if (LocalManager == null)
        {//making sure Players executing Start() before the localauthority will retry refreshing their name after a localauthority is done
            Invoke("Start", 1);
            return;
        }
        else
        {
            //PlayerManager.RegisterNewPlayer(transform.GetComponent<Player>());
            //transform.GetComponentInChildren<MapCamControl>().Initialize();
            if (hasAuthority)
            {
                LevelManager = transform.GetComponent<Level_Manager>();
                PlayerManager = transform.GetComponent<Player_Manager>();
                EnemyManager = transform.GetComponent<Enemy_Manager>();

                if (isServer)
                {
                    Debug.Log("Authority On Server", this);
                    gameObject.name = ServerGameManagerName;
                    LevelManager.Initialize("Level");
                    //EnemyManager.Initialize("Enemies");
                    PrepareNextLvl();
                }
                else
                {
                    Debug.Log("Authority On Client", this);
                    gameObject.name = LocalAuthorityGameManagerName;
                }
            }
            else
            {
                LevelManager = LocalManager.GetComponent<Level_Manager>();
                PlayerManager = LocalManager.GetComponent<Player_Manager>();
                EnemyManager = LocalManager.GetComponent<Enemy_Manager>();
                transform.GetComponent<Level_Manager>().enabled = false;
                transform.GetComponent<Player_Manager>().enabled = false;
                transform.GetComponent<Enemy_Manager>().enabled = false;

                if (isServer)
                {
                    Debug.Log("No Authority On Server", this);
                    gameObject.name = NoAuthorityGameManagerName;
                }
                else
                {
                    Debug.Log("No Authority On Client", this);
                    if (this.netId.Value == MasterNetId) gameObject.name = ServerGameManagerName;
                    else gameObject.name = NoAuthorityGameManagerName;
                }
            }
        }
    }                

    [Server]
    public void PrepareNextLvl()
    {
        if (hasAuthority)
        {
            AllowUpdate(false);
            Debug.Log("Preparing next lvl", this);
            //PlayerManager.RespawnAll();
            if (isServer) LevelManager.CmdLoadNextLevel();
            AllowUpdate(true);
        }
        else LocalManager.PrepareNextLvl();
    }
}
