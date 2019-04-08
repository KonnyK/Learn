using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapCamera : MonoBehaviour
{
    [SerializeField] private bool AlwaysFaceNorth = true;
    void Update()
    {
        if (AlwaysFaceNorth) transform.rotation = Quaternion.LookRotation(-Game_Manager.LevelManager.transform.up, Game_Manager.LevelManager.transform.forward);
        else transform.rotation = Quaternion.LookRotation(-Game_Manager.LevelManager.transform.up, transform.parent.forward);
    }
}
