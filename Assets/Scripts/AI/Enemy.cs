using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour {

    private int Type = 0; //Server only
    private Enemy_Manager EnemyManager = null; //Server only

    public new int GetType() { return Type;} //überschreibt alte GetType Funktion, daher "new"
    public void SetType(int Type, Enemy_Manager EM) { this.Type = Type; EnemyManager = EM; }
    public void ReActivate()
    {
        EnemyTypes.getType(Type).Animate(transform);
        Debug.Log("Enemy animated"+Type +"," + transform.GetSiblingIndex() + ": " + GetComponent<Rigidbody>().velocity); 
    }

    private void GoBackToSpawn()
    {

        Rigidbody RB = transform.GetComponent<Rigidbody>();
        RB.velocity = Vector3.zero;
        RB.angularVelocity = Vector3.zero;
        transform.position = Enemy_Manager.getSpawn();
        transform.rotation = Quaternion.identity;
    }

    void FixedUpdate()
    {
        if (transform.childCount == 1) Debug.Log(GetComponent<Rigidbody>().velocity);
        if (EnemyManager == null) return;
        if (Game_Manager.UpdateAllowed && Vector3.Magnitude(transform.localPosition - Enemy_Manager.getSpawn()) > Enemy_Manager.getMaxDistance()) //kills this Object if too far away and tell EnemyManager that ot died
        {
            GoBackToSpawn();
            EnemyManager.EnemyDied(transform.GetSiblingIndex());
        }
    }

}
