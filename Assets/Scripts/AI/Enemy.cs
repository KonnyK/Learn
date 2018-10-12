using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Enemy : NetworkBehaviour {

    [SyncVar] private float MaxDistance = 0;
    [SyncVar] private  Vector3 Spawn = Vector3.zero;
    [SyncVar] private int Type = 0;
    private static Enemy_Manager EnemyManager;

    public new int GetType() { return Type; } //überschreibt alte GetType Funktion, daher "new"
    public void CmdNewMaxDistance(float newMax) { MaxDistance = newMax; }

    //this needs to be called before any Enemy initializes
    public static void SetEnemyManager(Enemy_Manager EM) { Enemy.EnemyManager = EM; }

    public void Initialize(int Type)
    { //constructor
        this.Type = Type;
        MaxDistance = EnemyManager.getMaxDistance();
        transform.localPosition = this.Spawn = EnemyManager.getSpawn(); //reset Position
        EnemyTypes.getType(Type).Animate(transform);
        NetworkServer.Spawn(this.gameObject);
    }
    
    //    Velocity = EnemyTypes.getType(Type).newRandomSpeed() * Vector3.Normalize(new Vector3(UnityEngine.Random.value * 2 - 1, 0, UnityEngine.Random.value * 2 - 1));

    void FixedUpdate()
    {
        if (Game_Manager.CanUpdate())
        {
            if (Vector3.Magnitude(transform.localPosition - Spawn) > MaxDistance) //kills this Object if too far away and lets EnemyMAnager create a new one
            {
                EnemyManager.AddRemove(Type, 1);
                GameObject.Destroy(this.gameObject);
            }
        }
    }

}
