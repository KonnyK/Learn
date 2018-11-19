using UnityEngine;
using UnityEngine.Networking;

private class Enemy {
    /*
    private float[] RValues = new float[2];
    private int Index;
    [SyncVar] private int MeshObjectIndex = 0;
    [SyncVar] private int Type = 0;


    public void Initialize(int SiblingIndex, int Type)
    {
        MeshObject = transform.GetChild(SiblingIndex).transform;
        if (isServer)
        {
            SetNewRValues();
            this.Type = Type;
        }
        else CmdRequestRandoms();
    }

    [Command] public void CmdDelete() { RpcDelete(); }

    private void RpcDelete()
    {
        Destroy(transform.GetChild(MeshObjectIndex).gameObject);
    }

    [Server]
    private void SetNewRValues()
    {
        for (int i = 0; i < RValues.Length; i++)
        {
            float value = RValues[i] = Random.value;
            RpcSetRandom(i, value);
        }
    }
    [Command] private void CmdRequestRandoms() { for (int i = 0; i < RValues.Length; i++) RpcSetRandom(i, RValues[i]); }
    [ClientRpc] private void RpcSetRandom(int Index, float value) { RValues[Index] = value; }

    void FixedUpdate()
    {
        if (!Game_Manager.UpdateAllowed) return;
        if (isServer)
        {
            if (Vector3.Magnitude(MeshObject.localPosition - Enemy_Manager.getSpawn()) > Enemy_Manager.getMaxDistance())
            {
                SetNewRValues();
                RpcRespawn();
            }
            else EnemyTypes.Animations[Type](MeshObject, RValues);
        }
    }

    [ClientRpc] private void RpcRespawn()
    {
        MeshObject.position = Enemy_Manager.getSpawn();
        MeshObject.rotation = Quaternion.identity;
        GetComponent<Rigidbody>().angularVelocity = RValues[0] * Vector3.up;
    }
    */
    /*
    private Enemy_Manager EnemyManager = null; //Server only

    private float RandValue;
    public new int GetType() { return Type;} //überschreibt alte GetType Funktion, daher "new"
    public void SetType(int Type, Enemy_Manager EM) { this.Type = Type; EnemyManager = EM; }
    public void ReActivate()
    {
        EnemyTypes.getType(Type).Animate(transform);
        Debug.Log("Enemy animated"+Type +"," + transform.GetSiblingIndex() + ": " + GetComponent<Rigidbody>().velocity); 
    }

    public void Initialize()
    {
        RandValue = Random.value;
    }

    private void GoBackToSpawn()
    {

        Rigidbody RB = transform.GetComponent<Rigidbody>();
        RB.velocity = Vector3.zero;
        RB.angularVelocity = Vector3.zero;
        transform.position = Enemy_Manager.getSpawn();
        transform.rotation = Quaternion.identity;
    }

    */
}
