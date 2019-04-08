using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game_Manager : MonoBehaviour {

    [SerializeField] public static bool UpdateAllowed = false; //used for pausing the game
    public static Level_Manager LevelManager;
    public static Enemy_Manager EnemyManager;
    public static Player_Manager PlayerManager;
    public float TimeScale = 1;

    [SerializeField] private Transform[] Managers; //set in Editor, 1:LM, 2:EM 3:PM

    private static void AllowUpdate(bool NewState)
    {
        UpdateAllowed = NewState;
    }

    private static bool hasStarted = false;
    public void Start()
    {
        if (hasStarted) return;
        LevelManager = Managers[0].GetComponent<Level_Manager>();
        PlayerManager = Managers[1].GetComponent<Player_Manager>();
        EnemyManager = Managers[2].GetComponent<Enemy_Manager>();

        FetchName();

        LevelManager.Initialize();
        EnemyManager.Initialize();
        PlayerManager.Initialize();

        PrepareNextLvl();
    }

    public static void InvokePrepareNextLvl(int Sec)
    {
        new WaitForSecondsRealtime(3);
        PrepareNextLvl();
    }
    private static void PrepareNextLvl()
    {
        AllowUpdate(false);
        Debug.Log("Preparing next lvl");
        LevelManager.LoadNextLevel();
        PlayerManager.RespawnAll();
        AllowUpdate(true);
    }

    private void FetchName()
    {
        transform.name = "Host";
    }
    private void Update()
    {
        Time.timeScale = TimeScale;
    }
}
