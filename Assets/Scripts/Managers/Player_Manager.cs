using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Player_Manager : NetworkBehaviour
{
    private static readonly string PlayerPoolTag = "PlayerPool";
    private static readonly string PlayerPoolName = "PlayerPool";
    private static Transform PlayerPool = null;
    [SerializeField] private GameObject PlayerPoolObject; //set in editor
    [SerializeField] private GameObject[] PlayerModels; // set in Editor
    [SyncVar] private Vector3 Spawn = Vector3.zero;

    public void Initialize()
    {
        if (isServer && hasAuthority)
        {
            PlayerPool = Instantiate(PlayerPoolObject).transform;
            PlayerPool.name = PlayerPoolName;
            PlayerPool.tag = PlayerPoolTag;
            NetworkServer.Spawn(PlayerPool.gameObject);
        }
        if (PlayerPool == null) {
            if (hasAuthority)
            {
                PlayerPool = GameObject.FindGameObjectWithTag(PlayerPoolTag).transform;
                PlayerPool.name = PlayerPoolName;
            }
            else { Invoke("Initialize", 1); return; }
        }
        
        Player P = GetComponent<Player>();
        if (isServer)
        {
            int P_Number = checked((int)this.netId.Value);
            P.RpcSetNumber(P_Number);
        }
        Transform Mesh = Instantiate(PlayerModels[0], PlayerPool).transform;
        foreach (Transform T in Mesh) if (T.tag == "Mesh") { Mesh = T; break; }
        P.SetMesh(Mesh);
        P.SetNewControls();
        transform.GetComponent<Movement>().enabled = Mesh.parent.GetComponentInChildren<PlayerCamControl>().enabled = hasAuthority;
        Mesh.GetComponent<CollisionDetect>().enabled = isServer;
        if (isServer) Mesh.GetComponent<CollisionDetect>().Initialize(transform);
    }

    [Server] public void RespawnAll()
    {
        foreach (GameObject P in GameObject.FindGameObjectsWithTag("Player")) P.GetComponent<Player_Manager>().RespawnPlayer();
    }

    [Server] public void RespawnInvoke(int Seconds)
    {
        GetComponent<Player>().ChangeStatus(-2);
        Invoke("RespawnPlayer", Seconds);
    }
    [Server] private void RespawnPlayer()
    {
        FindNewSpawn();
        Player P = GetComponent<Player>();
        P.RpcOrientateMesh(Spawn, Quaternion.LookRotation(Vector3.ProjectOnPlane(Random.insideUnitSphere, Vector3.up), Vector3.up));
        P.ChangeStatus(2);
    }

    [Server] public void KillPlayer()
    {
        Debug.Log("Player died!");
        Player P = GetComponent<Player>();
        P.IncDeathCount();
        P.RpcOrientateMesh(P.Mesh.position, Quaternion.LookRotation(Vector3.up, -P.getRB().velocity));
        P.ChangeStatus(-1);
    }


    //randomly choose a new Spawnlocation and check if Spawning is safe
    [Server] private void FindNewSpawn()
    {
        //Debug.Log("FindNewSpawn", this);
        Level_Manager LevelManager = transform.GetComponent<Game_Manager>().getLevelManager();

        //draw Debug Box
        Vector3 V0 = LevelManager.GetSpawnArea()[0];
        Vector3 V1 = V0 + LevelManager.GetSpawnArea()[1];
        Debug.DrawLine(new Vector3(V0.x, V1.y, V1.z), V1, Color.green, 1000);//Possible SpawnArea
        Debug.DrawLine(new Vector3(V1.x, V1.y, V0.z), V1, Color.green, 1000);
        Debug.DrawLine(new Vector3(V0.x, V0.y, V1.z), V0, Color.green, 1000);
        Debug.DrawLine(new Vector3(V1.x, V0.y, V0.z), V0, Color.green, 1000);

        int MaxTries = 100; //after more tries to find a spawn it stops searching
        int TryAmount = 0;
        RaycastHit Hit;
        Vector3 newPos = Vector3.zero;
        do
        {
            //randomly generating Position
            newPos = LevelManager.GetSpawnArea()[0];
            newPos.x += UnityEngine.Random.value * (LevelManager.GetSpawnArea()[1].x);
            newPos.y += UnityEngine.Random.value * (LevelManager.GetSpawnArea()[1].y);
            newPos.z += UnityEngine.Random.value * (LevelManager.GetSpawnArea()[1].z);
            TryAmount++;

        } while ((!Physics.Raycast(newPos, Vector3.down, out Hit) || Hit.transform.tag != "CP") && TryAmount <= MaxTries);
        if (TryAmount <= MaxTries)
        {
            Debug.Log("Found new Spawn after " + TryAmount + " tries.", this);
            Debug.DrawLine(Vector3.zero, newPos, Color.red, 1000); // newfound SpawnPos
            Spawn = newPos + Vector3.up * 5;
        }
    }
}