using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Manager : MonoBehaviour {

    
    [SerializeField] private GameObject[] EnemyDesigns; //set in Prefab

    ///
    ///

    public int EnemyAmount() { return transform.childCount; }

    public void Initialize()
    {
    }

    public void SpawnEnemies(int Type, int Amount)
    {
        if (Amount <= 0) return;
        for (int i = 0; i < Amount; i++)
        {
            Enemy E = Instantiate(EnemyDesigns[Type], transform).GetComponent<Enemy>();
            E.Restart(Type);
        }
    }

}
