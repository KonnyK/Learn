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
        if (hasAuthority)
        {
            LevelManager = transform.GetComponent<Level_Manager>();
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

    [Command] private void CmdRequestSyncValues() { SyncValues(); }
    [Server] private void SyncValues()
    {
        MaxDistance = LevelManager.getLevelRadius();
        foreach (GameObject Player in GameObject.FindGameObjectsWithTag("Player")) Player.GetComponent<Enemy_Manager>().RpcSyncValues(SpawnPos, MaxDistance);
    }
    [ClientRpc] public void RpcSyncValues(Vector3 Spawn, float MaxDist)
    {
        SpawnPos = Spawn;
        MaxDistance = MaxDist;
        foreach (GameObject GO in GameObject.FindGameObjectsWithTag(EnemyParentTag)) if (GO.name == EnemyParentName) EnemyParent = GO.transform;
    }

    [Server]
    public void SpawnEnemies(int Type, int Amount)
    {
        EnemyParent.GetComponent<ObjectPoolManager>().RpcClear();
        if (Amount > 0) for (int i = 0; i < Amount; i++)
            {
                Instantiate(EnemyDesigns[Type], EnemyParent).GetComponent<Enemy>().SetType(Type, this); //change the values of the new object;
            }
        EnemyParent.GetComponent<ObjectPoolManager>().OverwriteChildren();
        SyncValues();
        foreach (GameObject Player in GameObject.FindGameObjectsWithTag("Player"))
        foreach (Transform Enemy in EnemyParent) Player.GetComponent<Enemy_Manager>().EnemyDied(Enemy.GetSiblingIndex());
    }

    [Server]
    public void EnemyDied(int Index)
    {
        EnemyParent.GetChild(Index).GetComponent<Enemy>().ReActivate();
        Rigidbody RB = EnemyParent.GetChild(Index).GetComponent<Rigidbody>();
        //RpcReActivateEnemy(Index, RB.transform.position, RB.velocity, RB.angularVelocity);
    }

    [ClientRpc]
    private void RpcReActivateEnemy(int Index, Vector3 Pos, Vector3 Vel, Vector3 AngVel)
    {
        Rigidbody RB = EnemyParent.GetChild(Index).GetComponent<Rigidbody>();
        RB.transform.position = Pos;
        RB.velocity = Vel;
        RB.angularVelocity = AngVel;
    }


}
