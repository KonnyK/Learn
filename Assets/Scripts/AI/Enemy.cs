using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour {

    private float MaxDistance = 10;
    private Vector3 Spawn = Vector3.zero;
    private int Type = 0;
    public static Enemy_Manager EnemyManager;

    public new int GetType() { return Type; } //überschreibt alte GetType Funktion, daher "new"
    public void NewMaxDistance(float newMax) { MaxDistance = newMax; }
    
    public void SetValues(int Type, Vector3 Spawn)
    { //constructor
        this.Type = Type;
        this.Spawn = Spawn;
    }

    public void ReActivate()
    {
        EnemyTypes.getType(Type).Animate(transform);

    }

    private void GoBackToSpawn()
    {

        Rigidbody RB = transform.GetComponent<Rigidbody>();
        RB.velocity = Vector3.zero;
        RB.angularVelocity = Vector3.zero;
        transform.position = Spawn;
        transform.rotation = Quaternion.identity;
    }

    public void ReAnimate(Vector3 Pos, Vector3 Vel, Vector3 AngVel)
    {
        transform.position = Pos;
        transform.GetComponent<Rigidbody>().velocity = Vel;
        transform.GetComponent<Rigidbody>().angularVelocity = AngVel;
    }

    void FixedUpdate()
    {
        if (Game_Manager.UpdateAllowed && Vector3.Magnitude(transform.localPosition - Spawn) > MaxDistance) //kills this Object if too far away and tell EnemyManager that ot died
        {
            GoBackToSpawn();
            EnemyManager.CmdEnemyDied(transform.GetSiblingIndex());
        }
    }

}
