using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ObjectPoolManager : NetworkBehaviour {

    [SerializeField] GameObject[] Types; //set in Editor

    [ClientRpc] public void RpcClear(){ if (!isServer) ClearChildren(); }
    [ClientRpc] private void RpcAddChild(Vector3 Pos, Quaternion Rot, Vector3 Scale, int Type, Vector3 Vel, Vector3 AngVel)
    {
        if (isServer) return;
        Transform Child = Instantiate(Types[Type], transform).transform;
        Child.position = Pos;
        Child.rotation = Rot;
        Child.localScale = Scale;
        if (Child.GetComponent<Rigidbody>())
        {
            Rigidbody RB = Child.GetComponent<Rigidbody>();
            RB.velocity = Vel;
            RB.angularVelocity = AngVel;
        }
    }
    [Server]
    public void OverwriteChild(int Index)
    {
        if (transform.childCount <= Index) return;
        Transform Child = transform.GetChild(Index);
        int Type = 0;
        switch (Child.tag)
        {
            case "Enemy":
                {
                    Type = 0;
                    break;
                }
            case "CP":
                {
                    Type = 1;
                    break;
                }
            case "PF":
                {
                    Type = 2;
                    break;
                }
        }
        if (Child.GetComponent<Rigidbody>())
        {
            Rigidbody RB = Child.GetComponent<Rigidbody>();
            RpcAddChild(Child.position, Child.rotation, Child.localScale, Type, RB.velocity, RB.angularVelocity);
        }
        else RpcAddChild(Child.position, Child.rotation, Child.localScale, Type, Vector3.zero, Vector3.zero);
    }

    [Server] public void OverwriteChildren()
    {
        RpcClear();
        foreach (Transform Child in transform) OverwriteChild(Child.GetSiblingIndex());
    }
	private void ClearChildren()
    {
        foreach (Transform Child in transform) Destroy(Child.gameObject);
    }
    [ClientRpc]
    public void RpcRename(string Name, string Tag)
    {
        gameObject.name = Name;
        gameObject.tag = Tag;
    }

}
