using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Enemy_Manager : NetworkBehaviour {

    private class Enemy
    {
        private int Seed;
        private int Index;
        private int Type;
        public int getIndex() { return Index; }
        public int getType() { return Type; }
        public void setSeed(int NewSeed) { Seed = NewSeed; }
        public float getRandomNumber(int Index) { return System.Convert.ToSingle(new System.Random(Seed + Index).NextDouble()); } //return a double X, 0 <= X < 1
        public Enemy(int SiblingIndex, int Type, int Seed)
        {
            Index = SiblingIndex;
            this.Type = Type;
            this.Seed = Seed;
        }
    }

    
    [SerializeField] private GameObject[] EnemyDesigns; //set in Prefab
    private static Level_Manager LevelManager;

    private static Vector3 SpawnPos = Vector3.up;
    private static float MaxDistance = 100;

    private static Transform EnemyParent;
    private static List<Enemy> Enemies = new List<Enemy>();
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
        if (!hasAuthority) return;

        if (isServer)
        {
            EnemyParent = Instantiate(EnemyParentObject).transform;
            EnemyParent.name = EnemyParentName;
            EnemyParent.tag = EnemyParent.tag;
            NetworkServer.Spawn(EnemyParent.gameObject);
            SyncValues();
        GetComponent<Game_Manager>().RpcDebug("EnemyManager on Server initialized");
        }
        else CmdRequestSyncValues();

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
        if (!hasAuthority) return;
        EnemyParent.GetComponent<ObjectPoolManager>().RpcClear();
        if (Amount > 0) for (int i = 0; i < Amount; i++)
            {
                int Index = Instantiate(EnemyDesigns[Type], EnemyParent).transform.GetSiblingIndex();
                //foreach (GameObject Player in GameObject.FindGameObjectsWithTag("Player")) Player.GetComponent<Enemy_Manager>().
                AddEnemy(new Enemy(Index, Type, new System.Random().Next()));
            }
        InitializeAll();
        EnemyParent.GetComponent<ObjectPoolManager>().OverwriteChildren();
        SyncValues();
    }

    [Server] public void InitializeAll() { foreach (Enemy E in Enemies) InitializeEnemy(E); }

    [Server] private void InitializeEnemy(Enemy E)
    {
        Transform EnemyBody = EnemyParent.GetChild(E.getIndex()).transform;
        Vector2 Random2DPos = Random.insideUnitCircle * E.getRandomNumber(0) * LevelManager.getLevelRadius();
        EnemyBody.position = new Vector3(Random2DPos.x, EnemyBody.position.y, Random2DPos.y);
        Rigidbody RB = EnemyBody.GetComponent<Rigidbody>();
        RB.velocity = Vector3.Normalize(EnemyBody.position - SpawnPos) * E.getRandomNumber(0) * 50;
        RB.angularVelocity = E.getRandomNumber(1) * Vector3.up * 10;
    }

    [Server] private void AddEnemy(Enemy E) { Enemies.Add(E); }

    [Server] private void Respawn(Enemy E)
    {
        Rigidbody RB = EnemyParent.GetChild(E.getIndex()).GetComponent<Rigidbody>();
        RB.velocity = EnemyTypes.GetMoving[E.getType()](EnemyParent.GetChild(E.getIndex()), new float[2]{ E.getRandomNumber(0), E.getRandomNumber(1)});
        RB.angularVelocity = E.getRandomNumber(1) * 10 * Vector3.up;
    }

    public void FixedUpdate()
    {
        if (!isServer) return;
        if ((Enemies.Count < 1) || !hasAuthority) return;
        for (int i = 0; i < Enemies.Count; i++) if (Vector3.Magnitude(EnemyParent.GetChild(Enemies[i].getIndex()).position - SpawnPos) > MaxDistance)
        {
            Transform T = EnemyParent.GetChild(Enemies[i].getIndex());
            T.position = SpawnPos;
            T.rotation = Quaternion.identity;
            Enemies[i].setSeed(new System.Random().Next());
            //foreach (GameObject Player in GameObject.FindGameObjectsWithTag("Player")) Player.GetComponent<Enemy_Manager>().OverwriteEnemy(i, Enemies[i]);
            Respawn(Enemies[i]);
            EnemyParent.GetComponent<ObjectPoolManager>().OverwriteChild(Enemies[i].getIndex());
        }
    }

    [Server] private void OverwriteEnemy(int ListIndex, Enemy E) { Enemies[ListIndex] = E; }

}
