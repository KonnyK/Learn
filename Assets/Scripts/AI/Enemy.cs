using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Enemy : NetworkBehaviour {

    private float MaxDistance = 10;
    [SyncVar] private  Vector3 Spawn = Vector3.zero;
    [SyncVar] private int Type = 0;

    public new int GetType() { return Type; } //überschreibt alte GetType Funktion, daher "new"
    [ClientRpc] public void RpcNewMaxDistance(float newMax) { MaxDistance = newMax; }

    public void Initialize(int Type)
    { //constructor
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
        if (Game_Manager.UpdateAllowed && Vector3.Magnitude(transform.localPosition - Spawn) > MaxDistance) //kills this Object if too far away and lets EnemyMAnager create a new one
        {
                CmdRespawn();
        }
    }

}
