using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Networking;

public class Level_Manager : NetworkBehaviour {

    [SerializeField] private GameObject LevelTransformObject; //set in editor
    [SerializeField] private GameObject[] Checkpoints; //array of all Checkpointprefabs Index=style
    [SerializeField] private GameObject[] Platforms; //array of all platform prefabs
    private Enemy_Manager EnemyManager;//set on initialize

    private static List<Level> Levels = new List<Level>();
    private static Level CurrentLevel; //set in Editor, Parent of all Platforms & Checkpoints

    [SerializeField] private uint LevelAmount = 20; //Amount of Levels generated on initialize
    private static readonly Vector2 AngleRange = new Vector2(360 / 4, 360 / 12);
    private static readonly float VoidWidth = 30; //distance between tthe paths and also Path width
    private static readonly float MinRadius = 50; //minimal Radius so it doesn't get impossible hard, the last Checkpoint of a level has always this distance to the center

    private static Transform LevelTransform = null;
    private readonly string LevelTransformTag = "LvlTrans"; //if you change this, change the taglist in Unity too!!!
    private static readonly string LevelTransformName = "Level";

    private static int Difficulty = 0; //single increments by 1 or 2 won't do much
    private static float CurrentLvlRadius = 0; //used for Enemy MaxDistance and MapCam
    private static Vector3 SpawnAreaPos = Vector3.zero; //all lower coordinates of spawnarea-box
    private static Vector3 SpawnAreaSize = Vector3.one; //width, height and depth of spawnarea-box
    private static Vector3 GoalPosition = Vector3.zero; //width, height and depth of spawnarea-box


    /// 
    /// 
    /// 
    ///
    ///



    public Vector3[] GetSpawnArea() { return new Vector3[] { SpawnAreaPos, SpawnAreaSize }; } 
    public float getLevelRadius() { return CurrentLvlRadius; }
    public GameObject GetCheckpointDesign(int DesignNum) { return Checkpoints[DesignNum]; }
    public GameObject GetPlatformDesign(int DesignNum) { return Platforms[DesignNum]; }
    public static float GetWidth() { return VoidWidth; }
    public static float GetMinRadius() { return MinRadius; }


    //called on (hasAuthority || isServer)
    public void Initialize()
    {
        EnemyManager = gameObject.GetComponent<Game_Manager>().getEnemyManager();
        if (hasAuthority)
        {
            if (isServer)
            {
                for (int i = -1; i < LevelAmount; i++) Levels.Add(BuildNewLevel());
                LevelTransform = Instantiate(LevelTransformObject).transform;
                LevelTransform.name = LevelTransformName;
                LevelTransform.tag = LevelTransformTag;
                NetworkServer.Spawn(LevelTransform.gameObject);
                Debug.Log("Created LvLTrans: " + LevelTransform.GetComponent<NetworkIdentity>().netId.Value, this);
                SyncValues();
            }
            else CmdRequestSyncValues();
        }
    }

    [Server]
    private Level BuildNewLevel()
    {
        Difficulty += 10;
        System.Random R = new System.Random();
        return new Level(5 * Difficulty,
                             R.Next(Math.Min(Checkpoints.Length, Platforms.Length) - 1),
                             Difficulty * 10 + 50,
                             AngleRange,
                             (R.Next(1) == 0)
                        );
    }


    [Server]
    public void LoadNextLevel()
    {
        if (!hasAuthority) return;
        Debug.Log("Loading next Level", this);

        if (LevelTransform.childCount != 0) LevelTransform.GetComponent<ObjectPoolManager>().RpcClear();

        if (LevelAmount == 0) Levels[0] = CurrentLevel = BuildNewLevel();
        else
        {
            int CurrentLvlNum = 0;
            for (int i = 0; i < Levels.Count; i++) if (Levels[i].Equals(CurrentLevel)) { Debug.Log("Found a similiar Level"); CurrentLvlNum = i; }
            CurrentLevel = Levels[CurrentLvlNum + 1];
        }

        GameObject[] Designs = { Checkpoints[CurrentLevel.getDesign()], Platforms[CurrentLevel.getDesign()] };
        CurrentLevel.Instantiate(LevelTransform, Designs);
        LevelTransform.GetComponent<ObjectPoolManager>().OverwriteChildren();

        RefreshLvlRadius(); //needed for MapCamera and EnemyMaxDistance
        GoalPosition = CurrentLevel.getLastPos();
        CalculateSpawnArea();
        SyncValues();
        

        EnemyManager.SpawnEnemies(CurrentLevel.getDesign(), CurrentLevel.getEnemyAmount()); //spawn Enemies                   
        Debug.Log("Spawning Enemies: " + CurrentLevel.getDesign() + "," + CurrentLevel.getEnemyAmount());
    }

    //first Vector is position (lowest coordinate values) second is edge length of cube
    [Server]
    private void CalculateSpawnArea()
    {

        Vector3 Tolerance = Vector3.zero; //half the width of the spawnarea on each axis

        Tolerance.x = Tolerance.z = VoidWidth / 2;
        Tolerance.y = Checkpoints[CurrentLevel.getDesign()].transform.lossyScale.y;
        Vector3 newPos = Vector3.zero;
        newPos =
            new Vector3(
                Mathf.Min(CurrentLevel.getFirstPos().x + Tolerance.x, CurrentLevel.getFirstPos().x - Tolerance.x),
                CurrentLevel.getFirstPos().y + Tolerance.y + 3,
                Mathf.Min(CurrentLevel.getFirstPos().z + Tolerance.z, CurrentLevel.getFirstPos().z - Tolerance.z));
        Vector3 newSize = Vector3.one;
        newSize = Tolerance * 2;

        Debug.DrawLine(Vector3.zero, CurrentLevel.getFirstPos(), Color.white, 1000); //Position  of the Checkpoint everything is based on
        SpawnAreaPos = newPos;
        SpawnAreaSize = newSize;
    }


    public bool IsInGoal(Vector3 Pos) //returns true when Pos is above the final checkpoint of a level
    {
        return (Vector3.Magnitude(Vector3.ProjectOnPlane(GoalPosition - Pos, Vector3.up)) <= VoidWidth/2); 
    }

    [Server]
    private void RefreshLvlRadius() //called everytime a Lvl loads
    { //recalculates the maxiumum distance to center any object could have in current level
        CurrentLvlRadius = Vector3.Magnitude(CurrentLevel.getFirstPos()) //Distance of furthest Checkpoint
                           + Checkpoints[CurrentLevel.getDesign()].transform.lossyScale.x; //distance fromcenter of a checkpoint to a corner of the transform 
    }

    [Command]
    private void CmdRequestSyncValues()//called when a new client connects
    {
        LevelTransform.GetComponent<ObjectPoolManager>().RpcRename(LevelTransformName, LevelTransformTag);
        RpcSyncLevelValues(CurrentLvlRadius, Difficulty, SpawnAreaPos, SpawnAreaSize, GoalPosition);
        LevelTransform.GetComponent<ObjectPoolManager>().OverwriteChildren();
    }
    [Server] private void SyncValues()
    {
        LevelTransform.GetComponent<ObjectPoolManager>().RpcRename(LevelTransformName, LevelTransformTag);

        foreach (GameObject Player in GameObject.FindGameObjectsWithTag("Player"))
            Player.GetComponent<Level_Manager>().RpcSyncLevelValues(CurrentLvlRadius, Difficulty, SpawnAreaPos, SpawnAreaSize, GoalPosition);
    }
    [ClientRpc] private void RpcSyncLevelValues(float LvlRadius, int Diff, Vector3 SpawnPos, Vector3 SpawnSize, Vector3 GoalPos)
    {
        if (isServer) return;
        foreach (GameObject GO in GameObject.FindGameObjectsWithTag(LevelTransformTag)) if (GO.name == LevelTransformName) LevelTransform = GO.transform;
        CurrentLvlRadius = LvlRadius;
        Difficulty = Diff;
        SpawnAreaPos = SpawnPos;
        SpawnAreaSize = SpawnSize;
        GoalPosition = GoalPos;
    }
}
