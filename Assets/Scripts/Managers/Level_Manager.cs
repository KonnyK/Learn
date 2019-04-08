using System.Collections.Generic;
using System;
using UnityEngine;

public class Level_Manager : MonoBehaviour {

    [SerializeField] private GameObject[] Checkpoints; //array of all Checkpointprefabs Index=style
    [SerializeField] private GameObject[] Platforms; //array of all platform prefabs

    private static List<Level> Levels = new List<Level>();
    private static Level CurrentLevel; //set in Editor, Parent of all Platforms & Checkpoints

    [SerializeField] private uint LevelAmount = 20; //Amount of Levels generated on initialize
    private static readonly Vector2 AngleRange = new Vector2(360 / 4, 360 / 8);
    public static readonly float VoidWidth = 30; //distance between tthe paths and also Path width
    private static readonly float MinRadius = 50; //minimal Radius so it doesn't get impossible hard, the last Checkpoint of a level has always this distance to the center

    private static int Difficulty = 0; //single increments by 1 or 2 won't do much
    private static float CurrentLvlRadius = 0; //used for Enemy MaxDistance and MapCam
    private static Vector3 SpawnAreaPos = Vector3.zero; //all lower coordinates of spawnarea-box
    private static Vector3 SpawnAreaSize = Vector3.one; //width, height and depth of spawnarea-box
    private static Vector3 GoalPosition = Vector3.zero; //width, height and depth of spawnarea-box

    public Vector3[] GetSpawnArea() { return new Vector3[] { SpawnAreaPos, SpawnAreaSize }; } 
    public float getLevelRadius() { return CurrentLvlRadius; }
    public Vector3 getFirstPos() { return CurrentLevel.getPos(0); }
    public GameObject GetCheckpointDesign(int DesignNum) { return Checkpoints[DesignNum]; }
    public GameObject GetPlatformDesign(int DesignNum) { return Platforms[DesignNum]; }
    public static float GetWidth() { return VoidWidth; }
    public static float GetMinRadius() { return MinRadius; }


    /// 
    /// 
    /// 
    ///
    ///

    public int FindCheckpoint(Vector3 Pos)
    {
        int Closest = 0;
        float MinimalDistance = float.MaxValue;
        for (int i = 0; i < CurrentLevel.getCPAmount(); i++) if (Vector3.Distance(Pos, CurrentLevel.getPos(i)) < MinimalDistance)
            {
                MinimalDistance = Vector3.Distance(Pos, CurrentLevel.getPos(i));
                Closest = i;
            }
        //Debug.Log("Closest Checkpoint Found: " + Closest + "|" + MinimalDistance, this);
        return Closest;
    }

    public void Initialize()
    {
        for (int i = -1; i < LevelAmount; i++) Levels.Add(BuildNewLevel());
    }

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

    public void LoadNextLevel()
    {
        Debug.Log("Loading next Level", this);

        if (transform.childCount != 0) foreach (Transform Child in transform) Destroy(Child.gameObject);

        if (LevelAmount == 0) Levels[0] = CurrentLevel = BuildNewLevel();
        else
        {
            int CurrentLvlNum = 0;
            for (int i = 0; i < Levels.Count; i++) if (Levels[i].Equals(CurrentLevel)) { Debug.Log("Found a similiar Level"); CurrentLvlNum = i; }
            CurrentLevel = Levels[CurrentLvlNum + 1];
        }
        GameObject[] Designs = { Checkpoints[CurrentLevel.getDesign()], Platforms[CurrentLevel.getDesign()] };
        CurrentLevel.Instantiate(transform, Designs);

        RefreshLvlRadius(); //needed for MapCamera and EnemyMaxDistance
        GoalPosition = CurrentLevel.getLastPos();
        CalculateSpawnArea();
        

        Game_Manager.EnemyManager.SpawnEnemies(CurrentLevel.getDesign(), CurrentLevel.getEnemyAmount()); //spawn Enemies                   
        Debug.Log("Spawning Enemies: " + CurrentLevel.getDesign() + "," + CurrentLevel.getEnemyAmount());
    }

    //first Vector is position (lowest coordinate values) second is edge length of cube
    private void CalculateSpawnArea()
    {
        Vector3 Tolerance = Vector3.zero; //half the width of the spawnarea on each axis

        Tolerance.x = Tolerance.z = VoidWidth / 2;
        Tolerance.y = Checkpoints[CurrentLevel.getDesign()].transform.lossyScale.y;
        Vector3 newPos = Vector3.Min(CurrentLevel.getPos(0) + Tolerance, CurrentLevel.getPos(0) - Tolerance) + Vector3.up * Tolerance.y;
        Vector3 newSize = Tolerance * 2;

        Debug.DrawLine(Vector3.zero, CurrentLevel.getPos(0), Color.white, 1000); //Position  of the Checkpoint everything is based on
        SpawnAreaPos = newPos + Checkpoints[CurrentLevel.getDesign()].transform.lossyScale.y * Vector3.up;
        SpawnAreaSize = newSize + Checkpoints[CurrentLevel.getDesign()].transform.lossyScale.y * Vector3.up;

        //Debug.Log("CalculateNewSpawn: "+newPos + ", "+newSize, this);
    }


    public bool IsInGoal(Vector3 Pos) //returns true when Pos is above the final checkpoint of a level
    {
        return (Vector3.Magnitude(Vector3.ProjectOnPlane(GoalPosition - Pos, Vector3.up)) <= VoidWidth/2); 
    }

    private void RefreshLvlRadius() //called everytime a Lvl loads
    { //recalculates the maxiumum distance to center any object could have in current level
        CurrentLvlRadius = Vector3.Magnitude(CurrentLevel.getPos(0)) //Distance of furthest Checkpoint
                           + Checkpoints[CurrentLevel.getDesign()].transform.lossyScale.x; //distance fromcenter of a checkpoint to a corner of the transform 
    }

    public void Update()
    {
        RenderSettings.skybox.mainTextureOffset += Vector2.right;
    }
}
