using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Enemy : NetworkBehaviour {

    private float MaxDistance = 0;
    [SyncVar] private  Vector3 Spawn = Vector3.zero;
    [SyncVar] private int Type = 0;
    private static Game_Manager GameManager = null;

    public new int GetType() { return Type; } //überschreibt alte GetType Funktion, daher "new"
    [ClientRpc] public void RpcNewMaxDistance(float newMax) { MaxDistance = newMax; }

    //called on Enemy_Manager initialize on every client
    public static void SetGameManager(Game_Manager GM) { GameManager = GM; }

    public void Initialize(int Type)
    { //constructor
        MaxDistance = GameManager.getEnemyManager().getMaxDistance();
        if (isServer)
        {
            this.Type = Type;
            this.Spawn = transform.localPosition;
        }
        if (hasAuthority) CmdRespawn(); 
    }

    [Command]
    private void CmdRespawn()
    {
        EnemyTypes.getType(Type).Animate(transform);
        Rigidbody RB = transform.GetComponent<Rigidbody>();
        RpcReAnimate(RB.velocity, RB.angularVelocity);
    }
    [ClientRpc]
    private void RpcReAnimate(Vector3 Vel, Vector3 AngVel)
    {
        transform.position = Spawn;
        transform.GetComponent<Rigidbody>().velocity = Vel;
        transform.GetComponent<Rigidbody>().angularVelocity = AngVel;
    }

    void FixedUpdate()
    {
        if (GameManager.CanUpdate() && Vector3.Magnitude(transform.localPosition - Spawn) > MaxDistance) //kills this Object if too far away and lets EnemyMAnager create a new one
        {
                CmdRespawn();
        }
    }

}
