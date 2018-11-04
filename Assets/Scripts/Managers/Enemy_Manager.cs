using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class Enemy_Manager : NetworkBehaviour {

    [SerializeField] private GameObject defaultEnemy;
    [SyncVar] private Vector3 SpawnPos = Vector3.zero;
    [SerializeField, SyncVar] private float MaxDistance = 100;
    private Level_Manager LevelManager;
    private Transform EnemyParent;

    //is only called from within "if (isServer)"
    [Server]
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
        EnemyParent = GameObject.Find(EnemyParentName).transform;
        LevelManager = gameObject.GetComponent<Game_Manager>().getLevelManager();
        SpawnPos = transform.position;
        if (gameObject.name == Game_Manager.ServerGameManagerName) Enemy.EnemyManager = this;
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
        foreach (Transform Child in EnemyParent) Child.GetComponent<Enemy>().NewMaxDistance(MaxDistance);
    }
    
    [Client]
    public void EnemyDied(int Index)
    {
        if (isServer)
        {
            EnemyParent.GetChild(Index).GetComponent<Enemy>().ReActivate();
            Rigidbody RB = EnemyParent.GetChild(Index).GetComponent<Rigidbody>();
            RpcReActivateEnemy(Index);
        }
    }

    public float getMaxDistance() { return LevelManager.getLevelRadius(); }

    public Vector3 getSpawn() { return SpawnPos; }

}
