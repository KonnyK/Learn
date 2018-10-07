using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class Level : NetworkBehaviour
{

    //this class is made by Zoelovezle on https://answers.unity.com/questions/1122411/synclist-to.html
    public class SyncListVector : SyncList<Vector3>
    {
        protected override void SerializeItem(NetworkWriter writer, Vector3 item)
        {
            writer.Write(item);
        }
        protected override Vector3 DeserializeItem(NetworkReader reader)
        {
            return reader.ReadVector3();
        }
    }

    [SerializeField] private SyncListVector Path = new SyncListVector() { }; //only Positions of Checkpoints
    [SerializeField, SyncVar] private float LvlRotation = 0; //in degrees
    [SerializeField, SyncVar] int Design = 0; //determines which type of checkpoint and platform will be used
    [SerializeField, SyncVar] private int EnemyAmount; //first number is type, second is Amount
    [SerializeField, SyncVar] private int EnemyType;

    private Vector3 LocalToGlobalRotation(Vector3 V) //rotates the Vector around the Y-Axis so it align with the rotation of the level
    {
        float Vectorlength = Vector3.Magnitude(Vector3.ProjectOnPlane(V, Vector3.up));
        Vector3 result = V;

        float AngleFromX = Vector3.SignedAngle(Vector3.right, Vector3.ProjectOnPlane(V, Vector3.up), Vector3.down); //with Vector3.down signedAngle returns counterclockwise > 0
        AngleFromX -= LvlRotation;
        result = new Vector3((float)Math.Cos(Math.PI * AngleFromX / 180) * Vectorlength,
                                        V.y,
                             (float)Math.Sin(Math.PI * AngleFromX / 180) * Vectorlength);
        return result;
    }
    public int getDesign() { return Design; }
    public Vector3 getFirstPos() { return LocalToGlobalRotation(Path[0]); }
    public Vector3 getLastPos() { return LocalToGlobalRotation(Path.Last()); }
    public int AI_Type() { return EnemyType; }
    public int AI_Amount() { return EnemyAmount; }

    //constructor
    [Command]
    public void CmdReNew(int EnemyType, int EnemyAmount, int DesignNum, float PathLength, Vector2 AngleRange, bool Clockwise) //Angle in Radiant
    { //notice that "generating is referring to saving Vector3 values in Path[] not actual Instatiation/Spawning
        this.EnemyAmount = EnemyAmount;
        this.EnemyType = EnemyType;
        Design = DesignNum;
        LvlRotation = UnityEngine.Random.Range(-180, 180); //used when loading the Level so not every level finishes at (X=0, Y=0, Z=MinRadius) this is rotation around y-axis

        Path.Add(Vector3.forward * Level_Manager.GetMinRadius());//Goal Checkpoint

        float Angle = AngleRange.x;// UnityEngine.Random.Range(AngleRange.x, AngleRange.y);
        if (!Clockwise) Angle *= -1; //negative Angle = counterclockwise generation of checkpoints

        float CurrentPathLength = 0; //Lebgth of Path already generated
        while (CurrentPathLength < PathLength)
        { //Generating starts inside and moves outwards however the outermost checkpoint is saved at Path[0]
            Vector3 Next = Vector3.zero; //Next ist the next outer Checkpoint position
            Next.x = (float)Math.Sin(Angle * Path.Count) * (Level_Manager.GetMinRadius() + Path.Count * Level_Manager.GetWidth());
            Next.z = (float)Math.Cos(Angle * Path.Count) * (Level_Manager.GetMinRadius() + Path.Count * Level_Manager.GetWidth());

            float AdditionalLength = Vector3.Magnitude(Next - Path[0]); //Distance between LastCheckpoint and Next
            //normal case: Next is added to Path at Position 0
            if (CurrentPathLength + AdditionalLength <= PathLength) Path.Insert(0, Next);
            else //should only be called once at the end of the process
            { //if PathLength is overshot Next becomes Last Position + missing Distance and the LastPos is overwritten by Next
                AdditionalLength = PathLength - CurrentPathLength;  //missing length
                Next = Path[0] + AdditionalLength * Vector3.Normalize(Path[0] - Path[1]);
                Path[0] = Next;
            }
            CurrentPathLength += AdditionalLength;
        }
    }

    [ClientRpc]
    private void RpcClean()
    {
        foreach (Transform Child in transform) GameObject.Destroy(Child.gameObject);
        transform.rotation = Quaternion.Euler(0, 0, 0);
        Debug.Log("Cleaning on behalf of " + Game_Manager.Levels().transform.GetComponent<NetworkIdentity>().netId, this);
    }
    [ClientRpc]
    private void RpcSetRotation(Quaternion Rot) { transform.rotation = Rot; }

    [Command]
    public void CmdInstantiate() //IMPORTANT: Scale of Platform has to be (1,1,1)
    {
        RpcClean();

        //Checkpoints
        foreach (Vector3 CP in Path)
        {
            GameObject.Instantiate(Level_Manager.GetCheckpointDesign(Design), CP, Quaternion.LookRotation(Vector3.forward, Vector3.up), transform);
            Vector3 CurrentScale = transform.GetChild(transform.childCount - 1).transform.localScale;
            Debug.Break();
            NetworkServer.Spawn(transform.GetChild(transform.childCount - 1).gameObject);
            transform.GetChild(transform.childCount - 1).transform.localScale = new Vector3(Level_Manager.GetWidth(), CurrentScale.y, Level_Manager.GetWidth());
        }

        //Platforms
        for (int i = 1; i < Path.Count; i++)
        {
            //position between this CHeckpoint and the last one
            Vector3 Pos = (Path[i] + Path[i - 1]) / 2;
            //Rotation so that z-axis points form last to this Checkpoint
            Quaternion Rot = Quaternion.LookRotation(Path[i] - Path[i - 1], Vector3.up);
            GameObject.Instantiate(Level_Manager.GetPlatformDesign(Design), Pos, Rot, transform);
            //scaling
            Vector3 CurrentScale = transform.GetChild(transform.childCount - 1).transform.localScale;
            transform.GetChild(transform.childCount - 1).transform.localScale =
                new Vector3(Level_Manager.GetWidth(), CurrentScale.y, Vector3.Magnitude(Path[i] - Path[i - 1])); //changing scale
            NetworkServer.Spawn(transform.GetChild(transform.childCount - 1).gameObject);
        }

        //rotating parent around y-axis
        RpcSetRotation(Quaternion.Euler(0, LvlRotation, 0));
    }
}

