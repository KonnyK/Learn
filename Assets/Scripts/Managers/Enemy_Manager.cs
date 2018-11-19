using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class Enemy_Manager : NetworkBehaviour {

    [SerializeField] private GameObject[] EnemyDesigns; //set in Prefab
    private static Level_Manager LevelManager;

    private static Vector3 SpawnPos = Vector3.zero;
    private static float MaxDistance = 100;

    private static Transform EnemyParent;
    private static readonly string EnemyParentName = "Enemies";
    private readonly string EnemyParentTag = "EnemyParent";
    [SerializeField] private GameObject EnemyParentObject; //set in prefab
    
    ///
    ///


    public int EnemyAmount() { return transform.childCount; }
    public int EnemyAmount(int Type) { return EnemyParent.childCount; }
    public static float getMaxDistance() { return LevelManager.getLevelRadius(); }
    public static Vector3 getSpawn() { return SpawnPos; }

    public void Initialize()  
    {
        LevelManager = gameObject.GetComponent<Game_Manager>().getLevelManager();
        if (hasAuthority)
        {
            if (isServer)
            {
                EnemyParent = Instantiate(EnemyParentObject).transform;
                EnemyParent.name = EnemyParentName;
                EnemyParent.tag = EnemyParent.tag;
                NetworkServer.Spawn(EnemyParent.gameObject);
                SyncValues();
            }
            else CmdRequestSyncValues();
        }
    }

    [Command]
    private void CmdRequestSyncValues()
    {
        EnemyParent.GetComponent<ObjectPoolManager>().RpcRename(EnemyParentName, EnemyParentTag);
        RpcSyncValues(SpawnPos, MaxDistance);
        EnemyParent.GetComponent<ObjectPoolManager>().OverwriteChildren();

    }
    [Server] private void SyncValues()
    {
        MaxDistance = LevelManager.getLevelRadius();
        EnemyParent.GetComponent<ObjectPoolManager>().RpcRename(EnemyParentName, EnemyParentTag);
        foreach (GameObject Player in GameObject.FindGameObjectsWithTag("Player"))
            Player.GetComponent<Enemy_Manager>().RpcSyncValues(SpawnPos, MaxDistance);
    }
    [ClientRpc] public void RpcSyncValues(Vector3 Spawn, float MaxDist)
    {
        if (isServer) return;
        foreach (GameObject GO in GameObject.FindGameObjectsWithTag(EnemyParentTag))
            if (GO.name == EnemyParentName) EnemyParent = GO.transform;
        SpawnPos = Spawn;
        MaxDistance = MaxDist;
    }

    [Server]
    public void SpawnEnemies(int Type, int Amount)
    {
        EnemyParent.GetComponent<ObjectPoolManager>().RpcClear();
        if (Amount > 0) for (int i = 0; i < Amount; i++)
            {
                RpcAddEnemyComponent(Instantiate(EnemyDesigns[Type], EnemyParent).transform.GetSiblingIndex(), Type);
            }
        EnemyParent.GetComponent<ObjectPoolManager>().OverwriteChildren();
        SyncValues();
    }

    [ClientRpc]
    private void RpcAddEnemyComponent(int SiblingIndex, int Type)
    {
        Enemy E = EnemyParent.gameObject.AddComponent<Enemy>();
        E.Initialize(SiblingIndex, Type);
    }
    /*
    [Server]
    public void EnemyDied(int Index)
    {
        EnemyParent.GetChild(Index).GetComponent<Enemy>().ReActivate();
        Rigidbody RB = EnemyParent.GetChild(Index).GetComponent<Rigidbody>();
        RpcReActivateEnemy(Index, RB.transform.position, RB.velocity, RB.angularVelocity);
    }

    [ClientRpc]
    private void RpcReActivateEnemy(int Index, Vector3 Pos, Vector3 Vel, Vector3 AngVel)
    {
        Rigidbody RB = EnemyParent.GetChild(Index).GetComponent<Rigidbody>();
        RB.transform.position = Pos;
        RB.velocity = Vel;
        RB.angularVelocity = AngVel;
    }
    */

}
