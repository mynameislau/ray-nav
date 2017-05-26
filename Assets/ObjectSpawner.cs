using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Functional;

public class ObjectSpawner : MonoBehaviour {

	public Mesh mesh;
	public Material material;

	private int objNB = 200;
	private float range = 10;
	private GameObject[] array;
	// Use this for initialization
	void Start () {
		array = new GameObject[50];
		for (int i = 0; i < objNB; i++)
		{
			GameObject child = new GameObject("Spawner Child");
			array[i] = child;
			child.AddComponent<MeshFilter>().mesh = mesh;
			child.AddComponent<MeshRenderer>().material = material;
			child.AddComponent<MeshCollider>().sharedMesh = mesh;
			child.transform.parent = gameObject.transform;
			child.transform.position = new Vector3(
				UnityEngine.Random.Range(-range, range),
				UnityEngine.Random.Range(-range, range),
				UnityEngine.Random.Range(-range, range)
			);
		}

	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
