using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamControl: MonoBehaviour {

    [SerializeField] private Controls currentControls;
    [SerializeField] private float Height = 12f;
    [SerializeField] private float Angle = 60f;
    [SerializeField] private Game_Manager GameManager;


    public void Initialize(Player Owner) //called by Player
    {
        currentControls = Owner.getControls();
        GameManager = transform.GetComponentInParent<Game_Manager>();
    }

    void Update () {
        if (GameManager.CanUpdate())
        {

            if (Input.GetKeyDown(currentControls.getKey("Show Map"))) transform.GetComponent<Camera>().enabled = false;
            if (Input.GetKeyUp(currentControls.getKey("Show Map"))) transform.GetComponent<Camera>().enabled = true;


            if (Angle > 90) Angle = 90;
            if (Angle < 0) Angle = 0;
            transform.localPosition = Height * Vector3.up - Height / Mathf.Tan(Mathf.PI * Angle / 180f) * Vector3.forward;
            transform.rotation = Quaternion.LookRotation(transform.parent.position - transform.position, Vector3.up);
        }
    }
}
