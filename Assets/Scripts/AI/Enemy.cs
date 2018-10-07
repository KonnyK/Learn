using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Enemy : NetworkBehaviour {

    [SyncVar] private float MaxDistance = 0;
    [SyncVar] private  Vector3 Spawn = Vector3.zero;
    [SyncVar] private int Type = 0;

    public new int GetType() { return Type; } //überschreibt alte GetType Funktion, daher "new"
    public void CmdNewMaxDistance(float newMax) { MaxDistance = newMax; }

    public void Initialize(int Type, float MaxDistance,  Transform Spawn)
    { //constructor
        this.Type = Type;
        this.MaxDistance = MaxDistance;
        transform.localPosition = this.Spawn = Spawn.position; //reset Position
        transform.localRotation = Spawn.rotation; //reset Rotation
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
                Game_Manager.Enemies().AddRemove(Type, 1);
                GameObject.Destroy(this.gameObject);
            }
        }
    }

}
