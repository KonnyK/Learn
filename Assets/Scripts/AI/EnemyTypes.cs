using UnityEngine;

public class EnemyTypes {

    public class TypeValues //each different type of enemy is an instance od TypeValues
    {
        public delegate void Func(Transform Entity);//just used for handing over values on initialization, somehow has to be public

        private readonly string Name; //the Name of an EnemyType, NOT the name of a specific entity
        private readonly float BaseSpeed = 1; //actual speed of each enemy is randomly set within (BaseSpeed +/- SpeedTolerance) everytime it respawns
        private readonly float SpeedTolerance = 0.2f;
        public Func Animate;

        public TypeValues(string Name, Func Animate) //constructor apparently needs to be public too?
        {
            this.Name = Name;
            this.Animate = Animate;
        }

        public string getName() { return Name; }
        public float newRandomSpeed() { return UnityEngine.Random.Range(BaseSpeed - SpeedTolerance, BaseSpeed + SpeedTolerance);  }

    }

    [SerializeField]
    private static TypeValues[] Types = 
    {
        new TypeValues("Standard", (Entity) =>
        {
            Vector3 Vel = (0.25f + 0.125f * Random.value) * Vector3.Normalize(new Vector3(2 * Random.value - 1, 0, 2 * Random.value - 1));
            Debug.Log("New VEL:" + Vel);
            Entity.GetComponent<Rigidbody>().velocity=Vel*50;
            //Entity.GetComponent<Rigidbody>().AddForce(50* Vel, ForceMode.VelocityChange);
            //Entity.GetComponent<Rigidbody>().AddTorque((5+10*Random.value)*Vector3.up, ForceMode.VelocityChange);
        })
                      
        //implement new types here
    };

    public static int EnemyTypeAmount() { return Types.Length; }

    public static TypeValues getType(int Type) { return Types[Type];  }
}
 