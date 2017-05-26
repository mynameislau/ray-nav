using UnityEngine;
using System.Collections;

public class CompleteCameraController : MonoBehaviour {

	public GameObject obj;       //Public variable to store a reference to the obj game object


	private Vector3 offset;         //Private variable to store the offset distance between the obj and camera

	// Use this for initialization
	void Start () 
	{
		//Calculate and store the offset value by getting the distance between the obj's position and camera's position.
		offset = transform.position - obj.transform.position;
	}

	// LateUpdate is called after Update each frame
	void LateUpdate () 
	{
		// Set the position of the camera's transform to be the same as the obj's, but offset by the calculated offset distance.
		transform.position = obj.transform.position + offset;
	}
}