using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowObject : MonoBehaviour {

	public GameObject obj;
	private Vector3 offset = new Vector3(0, -2, 2);

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void LateUpdate () {
		float desiredAngle = obj.transform.eulerAngles.y;
		Quaternion rotation = Quaternion.Euler(0, desiredAngle, 0);
		transform.position = obj.transform.position - (rotation * offset);
		transform.LookAt(obj.transform);
		// transform.rotation = obj.transform.rotation;
	}
}
