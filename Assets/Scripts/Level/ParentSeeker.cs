using UnityEngine;

public class ParentSeeker : MonoBehaviour {

    [SerializeField] private string RealParentName; //has to be set in editor

    // this script ensures that every GameObject spawned automatically finds a parent instead of sticking around inn the main scene like that

    private void Start()
    {
        this.transform.parent = GameObject.Find(RealParentName).transform;
    }
}
