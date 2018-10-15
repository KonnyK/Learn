using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Game_Manager : NetworkBehaviour {

    [SerializeField] private GameObject[] Prefabs = new GameObject[3];
    [SerializeField] private Level_Manager LevelManager;
    [SerializeField] private Player_Manager PlayerManager;
    [SerializeField] private Enemy_Manager EnemyManager;
    [SerializeField] private bool allowUpdate = false;  //allow other scripts to do  sth.  in Update()
    
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
            LevelManager = transform.GetComponent<Level_Manager>();
            PlayerManager = transform.GetComponent<Player_Manager>();
            EnemyManager = transform.GetComponent<Enemy_Manager>();
            LevelManager.CmdInitialize();
        } else
        {
            LevelManager = GameObject.Find("Server").GetComponent<Level_Manager>();
            PlayerManager = GameObject.Find("Server").GetComponent<Player_Manager>();
            EnemyManager = GameObject.Find("Server").GetComponent<Enemy_Manager>();
        }
        Debug.Log("LevelManager: " + LevelManager.transform.GetComponent<NetworkIdentity>().netId, this);
        Debug.Log("PlayerManager: " + PlayerManager.transform.GetComponent<NetworkIdentity>().netId, this);
        Debug.Log("EnemyManager: " + EnemyManager.transform.GetComponent<NetworkIdentity>().netId, this);
        
        PrepareNextLvl();
        allowUpdate = true;
    }

    public static bool CanUpdate() { return allowUpdate; }

    public void PrepareNextLvl()
    {
        Debug.Log("Preparing next lvl", this);
        allowUpdate = false;
        LvlManager.CmdLoadNextLevel();
        PManager.RespawnAll(); 
        AiManager.RefreshMaxDistance();
        allowUpdate = true;
    }
}
