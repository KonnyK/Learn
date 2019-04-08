using System;
using System.Linq; //used for Pth.Last()
using System.Collections.Generic;
using UnityEngine;

public class Level
{

    [SerializeField] private List<Vector3> Path = new List<Vector3>(); //only Positions of Checkpoints
    [SerializeField] private float LvlRotation = 0; //in degrees
    [SerializeField] int Design = 0; //determines which type of checkpoint and platform will be used
    [SerializeField] private int EnemyAmount; //first number is type, second is Amount

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
    public Vector3 getLastPos() { return getPos(Path.Count - 1); }
    public Vector3 getPos(int Index) { return LocalToGlobalRotation(Path[Index]); }
    public int getCPAmount() { return Path.Count; }
    public int getEnemyAmount() { return this.EnemyAmount; }

    //constructor
    public Level(int EnemyAmount, int DesignNum, float PathLength, Vector2 AngleRange, bool Clockwise) //Angle in Radiant
    { //notice that "generating is referring to saving Vector3 values in Path[] not actual Instatiation/Spawning
        this.EnemyAmount = EnemyAmount;
        Design = DesignNum;
        LvlRotation = UnityEngine.Random.Range(-180, 180); //used when loading the Level so not every level finishes at (X=0, Y=0, Z=MinRadius) this is rotation around y-axis

        Path.Add(Vector3.forward * Level_Manager.GetMinRadius());//Goal Checkpoint

        float Angle = UnityEngine.Random.Range(AngleRange.x, AngleRange.y);
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

    public void Instantiate(Transform parent, GameObject[] Designs) //IMPORTANT: Scale of Platform has to be (1,1,1)
    {
        parent.rotation = Quaternion.identity;

        //Checkpoints
        foreach (Vector3 CP in Path)
        {
            Transform Checkpoint = GameObject.Instantiate(Designs[0], CP, Quaternion.LookRotation(Vector3.forward, Vector3.up), parent).transform;
            Checkpoint.localScale = Vector3.Scale(Checkpoint.localScale, new Vector3(Level_Manager.GetWidth(), 1, Level_Manager.GetWidth()));
        }

        //Platforms
        for (int i = 1; i < Path.Count; i++)
        {
            //position between this CHeckpoint and the last one
            Vector3 Pos = (Path[i] + Path[i - 1]) / 2;
            //Rotation so that z-axis points form last to this Checkpoint
            Quaternion Rot = Quaternion.LookRotation(Path[i] - Path[i - 1], Vector3.up);
            Transform Current = GameObject.Instantiate(Designs[1], Pos, Rot, parent).transform;
            //scaling
            foreach (Transform Child in Current)
            {
                if (Child.GetSiblingIndex() == 0)
                {
                    Child.localScale = new Vector3(Level_Manager.GetWidth(), Current.GetChild(0).localScale.y, Vector3.Magnitude(Path[i] - Path[i - 1]) - Level_Manager.GetWidth()); //changing scale
                    Child.GetComponent<MeshRenderer>().material = new Material(Child.GetComponent<MeshRenderer>().material);
                    Child.GetComponent<MeshRenderer>().material.mainTextureScale = new Vector2(1, Child.localScale.z / Child.localScale.x);
                }
                else
                {
                    Child.localScale = new Vector3(Level_Manager.GetWidth(), Child.localScale.y, Level_Manager.GetWidth()); //changing scale
                    Child.GetChild(0).GetComponent<MeshRenderer>().material = new Material(Child.GetChild(0).GetComponent<MeshRenderer>().material);
                    Child.GetChild(0).GetComponent<MeshRenderer>().material.mainTextureScale = Vector2.one * Child.localScale.x / 18;
                }
            }
            Current.GetChild(1).localPosition = new Vector3(0,0,Current.GetChild(0).localScale.z/2);
            Current.GetChild(2).localPosition = new Vector3(0,0,-Current.GetChild(0).localScale.z/2);

        }

        Debug.Log("Level Instantiated", parent);
        parent.Rotate(Vector3.up, LvlRotation);
    }
}

