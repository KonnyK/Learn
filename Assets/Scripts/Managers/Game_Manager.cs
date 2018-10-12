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
        {
            if (isServer)
            {
                gameObject.name = "Server";
                RpcRename("Server");
            }
            else gameObject.name = "Server";
        }
        if (gameObject.name == "Server")
        {
            LevelManager = gameObject.GetComponent<Level_Manager>();
            PlayerManager = gameObject.GetComponent<Player_Manager>();
            EnemyManager = gameObject.GetComponent<Enemy_Manager>();
        } else
        {
            LevelManager = GameObject.Find("Server").GetComponent<Level_Manager>();
            PlayerManager = GameObject.Find("Server").GetComponent<Player_Manager>();
            EnemyManager = GameObject.Find("Server").GetComponent<Enemy_Manager>();
        }
        GameObject.Instantiate(Prefabs[0], this.transform);
        GameObject.Instantiate(Prefabs[1], this.transform);
        GameObject.Instantiate(Prefabs[2], this.transform);
        LvlManager = transform.GetComponentInChildren<Level_Manager>();
        PManager = transform.GetComponentInChildren<Player_Manager>();
        AiManager = transform.GetComponentInChildren<Enemy_Manager>();
        LvlManager.transform.parent = transform.parent;
        PManager.transform.parent = transform.parent;
        AiManager.transform.parent = transform.parent;
        NetworkServer.Spawn(LvlManager.gameObject);
        NetworkServer.Spawn(PManager.gameObject);
        NetworkServer.Spawn(AiManager.gameObject);
        Debug.Log( LvlManager.transform.GetComponent<NetworkIdentity>().netId+ "   " + PManager.transform.GetComponent<NetworkIdentity>().netId + "   " + AiManager.transform.GetComponent<NetworkIdentity>().netId, this);
        LvlManager.CmdInitialize();


        //PManager.NewPlayer("Bob");
        
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
