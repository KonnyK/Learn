using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class Enemy_Manager : NetworkBehaviour {

    [SerializeField] private GameObject defaultEnemy;
    [SerializeField, SyncVar] private Vector3 SpawnPos = Vector3.zero;
    [SerializeField, SyncVar] private float MaxDistance = 100;
    [SerializeField] private Level_Manager LevelManager;
    [SerializeField] private Transform EnemyParent;

    //is only called from within "if (isServer)"
    public void Initialize(string EnemyParentName)
    {
        GameObject.Instantiate(new GameObject(EnemyParentName));
        EnemyParent = GameObject.Find(EnemyParentName).transform;
        EnemyParent.gameObject.AddComponent<NetworkIdentity>();
        EnemyParent.gameObject.AddComponent<ObjectPoolManager>();
        NetworkServer.Spawn(EnemyParent.gameObject);
        RpcInitialize(EnemyParentName);
    }

    [ClientRpc]
    private void RpcInitialize(string EnemyParentName)
    {
        if (gameObject.name == Game_Manager.ServerGameManagerName) Enemy.SetGameManager(transform.GetComponent<Game_Manager>());
        EnemyParent = GameObject.Find(EnemyParentName).transform;
        LevelManager = gameObject.GetComponent<Game_Manager>().getLevelManager();
    }

    public int EnemyAmount() { return transform.childCount; }
    public int EnemyAmount(int Type) { return EnemyParent.childCount; }

    [Command]
    public void CmdSpawnEnemies(int Type, int Amount)
    {
        EnemyParent.GetComponent<ObjectPoolManager>().RpcClear();
        if (Amount > 0) for (int i = 0; i < Amount; i++)
            {
                Instantiate(defaultEnemy, EnemyParent);
                EnemyParent.GetChild(EnemyParent.childCount - 1).GetComponent<Enemy>().Initialize(Type); //change the values of the new object
            }
        NetworkServer.Spawn(EnemyParent.gameObject);
        RefreshMaxDistance();
    }

    [Server]
    public void RefreshMaxDistance()
    {
        MaxDistance = LevelManager.getLevelRadius();
        foreach (Transform Child in transform) Child.GetComponent<Enemy>().RpcNewMaxDistance(MaxDistance);
    }
    
    public float getMaxDistance() { return LevelManager.getLevelRadius(); }

    public Vector3 getSpawn() { return SpawnPos; }

}
