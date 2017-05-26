using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Functional;
using RayNav;
public class Agent : MonoBehaviour {

	private Rigidbody rb;
	private GameObject childObj;
	// Use this for initialization
	private const float Infinity = 1/0f;
	private const float feelerLength = 3;
	private Feeler[] feelers = {
		new Feeler("right", Normalize(new Vector3(1, 0, 1))),
		new Feeler("left", Normalize(new Vector3(-1, 0, 1))),
		new Feeler("up", Vector3.forward)
	};
	private bool[] hitting;
	private Vector3[] feelerDirs;
	private string[] feelerNames;

	void Start () {
		feelerDirs = F.Map(Feeler.getDirection, feelers);
		feelerNames = F.Map(Feeler.getName, feelers);

		childObj = gameObject.transform.GetChild(0).gameObject;
		rb = childObj.GetComponent(typeof(Rigidbody)) as Rigidbody;
		rb.velocity = new Vector3(
			Random.Range(-1, 1),
			0,
			Random.Range(-1, 1)
		);
	}

	static Vector3 Normalize (Vector3 vec) {
		Vector3 nVec = vec;
		nVec.Normalize();
		return nVec;
	}
	
	// Update is called once per frame
	void Update () {
		debugFeelers();
	}

	bool? isSuperior (float? valA, float? valB) {
		if (valA.HasValue && valB.HasValue) {
			return valA.Value > valB.Value;
		}
		else {
			return valA.HasValue;
		}
	}

	Vector3 action (Vector3[]? results) {
		if (isScastResults[0]
	}

	void FixedUpdate () {

		Vector3?[] castResults = F.Map(feelerDir => cast(childObj, feelerDir), feelerDirs);

		// Vector3[] responses = F.Map((feelerDir) => {
		// 	Vector3? castResult = cast(childObj, feelerDir);
		// 	return -exponential(castResult) * 10;
		// }, feelerDirs);

		// hitting = F.Map(result => result.HasValue, castResults);

		// Vector3 summed = F.Reduce(Vector3.zero, (acc, curr) => {
		// 	return acc + curr;
		// }, responses);

		// if (summed.magnitude > 0) {
		// 	rb.AddForce(summed * 0.1f);
		// }
		// else {
		// 	rb.AddForce(childObj.transform.TransformDirection(Vector3.forward));
		// }

		// if (right.HasValue) { rb.AddForce(new Vector3(-1, 0, -1) * right.Value); }
		// if (left.HasValue) { rb.AddForce(new Vector3(1, 0, -1) * left.Value); }
		
	}

	Vector3 exponential (Vector3? vec) {
		//\frac{\cos \left(x\pi \right)}{2}+\ 0.5
		if (vec.HasValue) {
			float linear = vec.Value.magnitude;
			double exp = Mathf.Cos(linear * Mathf.PI) / 2 + 0.5;
			print("exp:" + exp + " linear : " + linear);
			return vec.Value * ((float)exp);
		}
		else {
			return Vector3.zero;
		}
	}

	Vector3 feelerData (Vector3 vec) {
		float linear = vec.magnitude / feelerLength;
		Vector3 norm = Normalize(vec);
		return norm * linear;
	}

	Vector3? cast(GameObject gameObj, Vector3 localDir) {
		Vector3 pos = gameObj.transform.TransformPoint(Vector3.zero);
		Vector3 dir = gameObj.transform.TransformDirection(localDir);
		RaycastHit rh = new RaycastHit();
		if (Physics.Raycast(pos, dir, out rh, feelerLength)) {
			Vector3 local =  gameObj.transform.InverseTransformDirection(rh.point - pos);
			return feelerData(local);
		}
		else {
			return null;
		}
	}

	void debugFeelers () {
		F.ForEach((feelerDir, index) => {
			Vector3 pos = childObj.transform.TransformPoint(Vector3.zero);
			Vector3 dir = childObj.transform.TransformDirection(feelerDir);
			Debug.DrawLine(pos, pos + dir * feelerLength, hitting[index] ? Color.red : Color.blue);
		}, feelerDirs);
	}
}
