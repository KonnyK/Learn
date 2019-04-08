using UnityEngine;

public class DaylightCycle : MonoBehaviour {

    [SerializeField] Light DirectionalLight = null;
    [SerializeField, Range(1, 3600)] float CycleLengthInSeconds = 1;
    [SerializeField] Vector3 Axis = Vector3.right;

	void Update () {
        if (DirectionalLight != null) DirectionalLight.transform.Rotate(Axis, 360 * Time.deltaTime / CycleLengthInSeconds); 
	}
}
