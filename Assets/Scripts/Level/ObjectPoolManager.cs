using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ObjectPoolManager : NetworkBehaviour {

    [SerializeField] GameObject[] Types; //set in Editor

    [ClientRpc] public void RpcClear(){ ClearChildren(); }
    [ClientRpc] private void RpcAddChild(Vector3 Pos, Quaternion Rot, Vector3 Scale, int Type)
    {
        Transform Child = Instantiate(Types[Type], transform).transform;
        Child.position = Pos;
        Child.rotation = Rot;
        Child.localScale = Scale;
    }
    [Server] public void OverwriteChildren()
    {
        RpcClear();
        foreach (Transform Child in transform)
        {
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
            RpcAddChild(Child.position, Child.rotation, Child.localScale, Type);
        }
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
