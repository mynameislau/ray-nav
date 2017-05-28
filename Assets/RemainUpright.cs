using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemainUpright : MonoBehaviour {

	private Rigidbody rb;
	// Use this for initialization
	void Start () {
		
		rb = gameObject.GetComponent(typeof(Rigidbody)) as Rigidbody;

	}
	
	// Update is called once per frame
	void FixedUpdate () {
		var rot = Quaternion.FromToRotation(transform.up, Vector3.up);
		rb.AddTorque(new Vector3(rot.x, rot.y, rot.z)*0.1f);
	}
}
