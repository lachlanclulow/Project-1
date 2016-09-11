using UnityEngine;
using System.Collections;

public class SunMotion : MonoBehaviour {

    public float orbitSpeed;
    public GameObject sunLight;
    public GameObject boundary;
    	
	void Update () {
        this.transform.RotateAround(Vector3.zero, Vector3.forward, orbitSpeed);
        sunLight.transform.LookAt(boundary.transform);
	}
}
