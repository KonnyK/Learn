using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Game_Manager : NetworkBehaviour {

    [SerializeField] private GameObject[] Prefabs = new GameObject[3];
    [SerializeField] private static Level_Manager LvlManager;
    [SerializeField] private static Player_Manager PManager;
    [SerializeField] private static Enemy_Manager AiManager;
    [SerializeField] private static bool allowUpdate = false;  //allow other scripts to do  sth.  in Update()

    public static Level_Manager Levels() { return LvlManager; }
    public static Player_Manager Players() { return PManager; }
    public static Enemy_Manager Enemies() { return AiManager; }

    public void Start()
    {
        if (!isServer) return;
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
