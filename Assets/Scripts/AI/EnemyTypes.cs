using UnityEngine;

public class EnemyTypes {

    public delegate Vector3 Func(Transform Entity, float[] Specifications);

    [SerializeField]
    public static Func[] GetMoving = 
    {
        new Func((Entity,Specs) =>
        {
            float BaseSpeed = 20;
            float SpeedTolerance = (Specs[1] * 2 - 1) * 0.25f * BaseSpeed;
            return new Vector3(Mathf.Cos(Mathf.PI*(Specs[0]*2-1)), 0,Mathf.Sin(Mathf.PI*(Specs[0]*2-1))).normalized * (BaseSpeed + SpeedTolerance);
            /*
            Vector3 Vel = (0.25f + 0.125f * Random.value) * Vector3.Normalize(new Vector3(2 * Random.value - 1, 0, 2 * Random.value - 1));
            Debug.Log("New VEL:" + Vel);
            Entity.GetComponent<Rigidbody>().AddForce(50* Vel, ForceMode.VelocityChange);
            Entity.GetComponent<Rigidbody>().AddTorque((5+10*Random.value)*Vector3.up, ForceMode.VelocityChange);
            */
        })
        //implement new types here
    };
}
 