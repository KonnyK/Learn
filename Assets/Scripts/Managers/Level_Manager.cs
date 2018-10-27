﻿using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Networking;

public class Level_Manager : NetworkBehaviour {

    //read-only
    [SerializeField] private static readonly Vector2 AngleRange = new Vector2(360/4, 360/12);
    [SerializeField] private GameObject[] Checkpoints; //array of all Checkpointprefabs Index=style
    [SerializeField] private GameObject[] Platforms; //array of all platform prefabs
    [SerializeField] private static readonly float VoidWidth = 30; //distance between tthe paths and also Path width
    [SerializeField] private static readonly float MinRadius = 50; //minimal Radius so it doesn't get impossible hard, the last Checkpoint of a level has always this distance to the center
    [SerializeField] private readonly uint LevelAmount = 20; //Amount of Leveö´ls generated on initialize

    public GameObject GetCheckpointDesign(int DesignNum) { return Checkpoints[DesignNum]; }
    public GameObject GetPlatformDesign(int DesignNum) { return Platforms[DesignNum]; }
    public static float GetWidth() { return VoidWidth; }
    public static float GetMinRadius() { return MinRadius; }

    [SerializeField, SyncVar] private int Difficulty = 0; //single increments by 1 or 2 won't do much
    [SerializeField, SyncVar] private float CurrentLvlRadius = 0; //used for Enemy MaxDistance and MapCam
    [SerializeField, SyncVar] private Vector3 SpawnAreaPos = Vector3.zero; //all lower coordinates of spawnarea-box
    [SerializeField, SyncVar] private Vector3 SpawnAreaSize = Vector3.one; //width, height and depth of spawnarea-box

    [SerializeField] private List<Level> Levels;
    [SerializeField] private Level CurrentLevel; //set in Editor, Parent of all Platforms & Checkpoints
    [SerializeField] private Transform LevelTransform;
    [SerializeField] private Game_Manager GameManager;//set on initialize

    public float getLevelRadius() { return CurrentLvlRadius; }


    //is only called from within "if (isServer)"
    public void Initialize(string LevelTransformName) //has to be called at the start of the game
    {
        for (int i = -1; i < LevelAmount; i++) Levels.Add(BuildNewLevel());
        GameObject.Instantiate(new GameObject(LevelTransformName));
        LevelTransform = GameObject.Find(LevelTransformName).transform;
        LevelTransform.gameObject.AddComponent<NetworkIdentity>();
        LevelTransform.gameObject.AddComponent<ObjectPoolManager>();
        NetworkServer.Spawn(LevelTransform.gameObject);
        RpcInitialize(LevelTransformName);
    }

    [ClientRpc]
    private void RpcInitialize(string LevelTransformName)
    { 
        GameManager = gameObject.GetComponent<Game_Manager>();
        LevelTransform = GameObject.Find(LevelTransformName).transform;
    }

    [Server]
    private Level BuildNewLevel()
    {
        Difficulty += 10;
        System.Random R = new System.Random();
        return new Level(    Difficulty + 10,
                             R.Next(Math.Min(Checkpoints.Length, Platforms.Length) - 1),
                             Difficulty * 30 + 50,
                             AngleRange,
                             (R.Next(1) == 0)
                        );
    }

    [Command]
    public void CmdLoadNextLevel() //firstly called by Game_Manager
    { //creates as many GameObjects of type Checkpoint and Platform as saved in Level to load
        Debug.Log("Loading next Level");

        if (LevelTransform.childCount != 0) LevelTransform.GetComponent<ObjectPoolManager>().RpcClear();

        if (LevelAmount == 0) Levels[0] = CurrentLevel = BuildNewLevel();
        else
        {
            int CurrentLvlNum = 0;
            for (int i = 0; i < Levels.Count; i++) if (Levels[i].Equals(CurrentLevel)) { Debug.Log("Found a similiar Level"); CurrentLvlNum = i; }
            CurrentLevel = Levels[CurrentLvlNum + 1];
        }
        GameObject[] Designs = {Checkpoints[CurrentLevel.getDesign()], Checkpoints[CurrentLevel.getDesign()]};
        CurrentLevel.Instantiate(LevelTransform, Designs);
      
        RefreshLvlRadius(); //needed for MapCamera and EnemyMaxDistance
        NetworkServer.Spawn(LevelTransform.gameObject);

        GameManager.getEnemyManager().CmdSpawnEnemies(CurrentLevel.getDesign(), CurrentLevel.getEnemyAmount()); //spawn Enemies                   
        Debug.Log("Spawning Enemies: " + CurrentLevel.getDesign() + "," + CurrentLevel.getEnemyAmount());
    }
    
    //first Vector is position (lowest coordinate values) second is edge length of cube
    [Command]
    private void CmdCalculateSpawnArea()
    {
        Vector3 Tolerance = Vector3.zero; //half the width of the spawnarea on each axis

        Tolerance.x = Tolerance.z = VoidWidth / 2;
        Tolerance.y = Checkpoints[CurrentLevel.getDesign()].transform.lossyScale.y;
        Vector3 newPos = Vector3.zero;
        newPos =
            new Vector3(
                Mathf.Min(CurrentLevel.getFirstPos().x + Tolerance.x, CurrentLevel.getFirstPos().x - Tolerance.x),
                CurrentLevel.getFirstPos().y + Tolerance.y + GameManager.getPlayerManager().getLocalPlayer().transform.lossyScale.y / 2, //CP Position + half of CP height + half of Playermodel height
                Mathf.Min(CurrentLevel.getFirstPos().z + Tolerance.z, CurrentLevel.getFirstPos().z - Tolerance.z));
        Vector3 newSize = Vector3.one;
        newSize = Tolerance * 2;

        Debug.DrawLine(Vector3.zero, CurrentLevel.getFirstPos(), Color.white, 1000); //Position  of the Checkpoint everything is based on
        SpawnAreaPos = newPos;
        SpawnAreaSize = newSize;
    }

    public Vector3[] GetSpawnArea()
    {
        CmdCalculateSpawnArea();       
        return new Vector3[] {SpawnAreaPos, SpawnAreaSize};
    } 

    [Command]
    public bool CmdIsInGoal(Vector3 Pos) //returns true when Pos is above the final checkpoint of a level
    {
        return (Vector3.Magnitude(Vector3.ProjectOnPlane(CurrentLevel.getLastPos() - Pos, Vector3.up)) <= VoidWidth/2); 
    }

    private void RefreshLvlRadius() //called everytime a Lvl loads
    { //recalculates the maxiumum distance from center any object could have in current level
        if (!isServer) Debug.Log("Refreshing Lvl Radius on Client! This is not intended", this);
        CurrentLvlRadius = Vector3.Magnitude(CurrentLevel.getFirstPos()) //Distance of furthest Checkpoint
                           + Checkpoints[CurrentLevel.getDesign()].transform.lossyScale.x; //distance fromcenter of a checkpoint to a corner of the transform 
    }
}
