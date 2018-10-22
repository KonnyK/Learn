using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ObjectPoolManager : NetworkBehaviour {

    [ClientRpc]
	public void RpcClear()
    {
        foreach (Transform Child in transform) Destroy(Child.gameObject);
    }
}
