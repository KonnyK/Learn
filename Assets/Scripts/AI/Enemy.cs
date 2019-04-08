using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy: MonoBehaviour
{
    private int Seed;
    private bool Started = false;
    private int Type;
    private Rigidbody RB;
    public int getType() { return Type; }
    public void setSeed(int NewSeed) { Seed = NewSeed; }
    public float getRandomNumber(int Index) { return System.Convert.ToSingle(new System.Random(Seed + Index).NextDouble()); } //return a double, 0 <= X < 1

    public void Restart(int Type)
    {
        this.Type = Type;
        RB = GetComponent<Rigidbody>();
        RB.velocity = Vector3.zero;
        Seed = new System.Random().Next();
        if (Started)
        {
            transform.localPosition = Vector3.zero;
            if (Type == 0) RB.velocity = EnemyTypes.GetMoving[0](transform, new float[] { getRandomNumber(0), getRandomNumber(1) });
        } 
        else
        {
            transform.localPosition = Vector3.ProjectOnPlane(Random.insideUnitSphere * Game_Manager.LevelManager.getLevelRadius(), transform.parent.up);
            if (Type == 0) RB.velocity = transform.localPosition.normalized * 20;
        }
        Started = true;
    }

    private void Update()
    {
        if (!Started) return;
        if (RB.velocity.magnitude != 0) transform.rotation = Quaternion.LookRotation(RB.velocity.normalized, transform.parent.up);
        if (transform.localPosition.magnitude > Game_Manager.LevelManager.getLevelRadius()) Restart(Type);
        if (Type == 0) return;
        switch (Type)
        {
            case 1:
                break;
            default:
                break;
        }
    }
}