using UnityEngine;

public class Player_Manager : MonoBehaviour
{
    [SerializeField] private GameObject[] PlayerModels; // set in Editor
    private Vector3 Spawn = Vector3.zero;
    private Player RespawningPlayer = null;

    public void Initialize()
    {
        CreatePlayer(0, 1);
    }

    public void CreatePlayer(int Type, int Number)
    {
        Player P = Instantiate(PlayerModels[Type], transform).GetComponent<Player>();
        P.transform.name = "Player" + Number;
        P.SetNumber(Number);
    }

    public void RespawnAll()
    {
        foreach (Player P in transform.GetComponentsInChildren<Player>()) RespawnPlayer();
    }

    public void ChangeStatusInAll(int newStatus)
    {
        foreach (Player P in transform.GetComponentsInChildren<Player>()) P.ChangeStatus(newStatus);
    }

    public void RespawnInvoke(int Seconds, Player P)
    {
        RespawningPlayer = P;
        RespawningPlayer.ChangeStatus(-2);
        Invoke("RespawnPlayer",Seconds + System.Convert.ToInt16(RespawningPlayer.GetComponent<ParticleSystem>().main.duration));
    }

    private void RespawnPlayer()
    {
        if (RespawningPlayer == null) RespawningPlayer = GetComponentInChildren<Player>();
        else if (RespawningPlayer.isAlive()) return;
        FindNewSpawn();
        RespawningPlayer.ChangeStatus(2);
        RespawningPlayer.transform.position = Spawn;
        RespawningPlayer.transform.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(Random.insideUnitSphere, Game_Manager.LevelManager.transform.up), Game_Manager.LevelManager.transform.up);
        RespawningPlayer = null;
    }

    public void KillPlayer(Player P)
    {
        Debug.Log("Player died!", this);
        P.GetComponent<ParticleSystem>().Play();
        P.transform.rotation = Quaternion.LookRotation(-Game_Manager.LevelManager.transform.up, P.transform.forward);
        transform.position += Vector3.up * PlayerModels[P.Type].transform.localScale.z/2;
        //P.Mesh.GetComponent<AudioSource>().Play();
        P.IncDeathCount();
        P.ChangeStatus(-1);
    }


    //randomly choose a new Spawnlocation and check if Spawning is safe
    private void FindNewSpawn()
    {
        
        //draw Debug Box
        Vector3 V0 = Game_Manager.LevelManager.GetSpawnArea()[0];
        Vector3 V1 = V0 + Game_Manager.LevelManager.GetSpawnArea()[1];
        Debug.DrawLine(new Vector3(V0.x, V1.y, V1.z), V1, Color.green, 1000);//Possible SpawnArea
        Debug.DrawLine(new Vector3(V1.x, V1.y, V0.z), V1, Color.green, 1000);
        Debug.DrawLine(new Vector3(V0.x, V0.y, V1.z), V0, Color.green, 1000);
        Debug.DrawLine(new Vector3(V1.x, V0.y, V0.z), V0, Color.green, 1000);

            Vector3 newPos = Game_Manager.LevelManager.getFirstPos() + Vector3.ProjectOnPlane(Random.insideUnitSphere * Level_Manager.VoidWidth/2, Game_Manager.LevelManager.transform.up);
            newPos += Game_Manager.LevelManager.transform.up * 2;
            Spawn = newPos + Vector3.up;
    }
}