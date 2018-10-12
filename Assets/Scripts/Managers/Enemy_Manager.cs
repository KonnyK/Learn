﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class Enemy_Manager : NetworkBehaviour {

    [SerializeField] private GameObject defaultEnemy;
    [SerializeField] private Vector3 SpawnPos = Vector3.zero;
    [SerializeField] private float MaxDistance = 100; //only for overwiew, not for killing Enemies when they're too far away, for that look directly into Enemy script
    [SerializeField] private Level_Manager LevelManager;

    public void Initialize()
    {
        LevelManager = gameObject.GetComponent<Level_Manager>();
    }

    public int EnemyAmount() { return transform.childCount; }
    public int EnemyAmount(int Type)
    {
        int Amount = 0;
        foreach (Transform Child in transform) if (Child.GetComponent<Enemy>().GetType() == Type) Amount++;
        return Amount;
    }

    public void Clear()
    { //removes all Enemies
        foreach (Transform Child in transform) GameObject.Destroy(Child.gameObject);
    }

    public void AddRemove(int Type, int Amount)
    {
        if (Amount > 0) for (int i = 0; i < Amount; i++)
            {
                Instantiate(defaultEnemy, transform);
                transform.GetChild(transform.childCount - 1).GetComponent<Enemy>().Initialize(Type); //change the values of the new object
            }
        else foreach (Transform Child in transform) //goes through all Enemies and removes those with type == Type until Amount <= 0
            {
                if (Child.GetComponent<Enemy>().GetType() == Type)
                {
                    GameObject.Destroy(Child.GetComponent<Enemy>().gameObject);
                    Type--;
                }
                if (Amount <= 0) break;
            }
    }

    public void RefreshMaxDistance()
    {
        MaxDistance = LevelManager.getLevelRadius();
        foreach (Transform Child in transform) Child.GetComponent<Enemy>().CmdNewMaxDistance(MaxDistance);
    }
    
    public float getMaxDistance() { return LevelManager.getLevelRadius(); }

    public Vector3 getSpawn() { return SpawnPos; }

}
