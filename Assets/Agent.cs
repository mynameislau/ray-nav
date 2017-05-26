using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Functional;
using RayNav;
public class Agent : MonoBehaviour {

	private Rigidbody rb;
	private GameObject childObj;
	// Use this for initialization
	private const float Infinity = 1/0f;
	private const float feelerLength = 3;
	private const float avoidanceStrength = 0.5f;
	private Feeler[] feelers = {
		new Feeler("top", Normalize(new Vector3(0, 1, 1))),
		// new Feeler("bottom", Normalize(new Vector3(0, -1, 1))),
		new Feeler("right", Normalize(new Vector3(1, 0, 1))),
		new Feeler("left", Normalize(new Vector3(-1, 0, 1))),
		new Feeler("forward", Vector3.forward)
	};
	private bool[] hitting;
	private Vector3[] feelerDirs;
	//private string[] feelerNames;

	void Start () {
		feelerDirs = F.Map(Feeler.getDirection, feelers);
		//feelerNames = F.Map(Feeler.getName, feelers);

		childObj = gameObject.transform.GetChild(0).gameObject;
		rb = childObj.GetComponent(typeof(Rigidbody)) as Rigidbody;
		// rb.velocity = new Vector3(
		// 	UnityEngine.Random.Range(-1, 1),
		// 	0,
		// 	UnityEngine.Random.Range(-1, 1)
		// );
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
	Vector3 LimitedSteer (Vector3 source, Vector3 target, float steer = (float) Math.PI * 0.5f) {
		float magnitude = target.magnitude;
		Vector3 rotated = Vector3.RotateTowards(Normalize(source), Normalize(target), steer, 1f);
		return rotated * magnitude;
	}

	Vector3? Average (Vector3[] vectors) {
		if (vectors.Length == 0) { return null; }
		Vector3 sum = F.Reduce(Vector3.zero, (acc, curr) => acc + curr, vectors);
		return sum / vectors.Length;
	}

	Vector3 ExponentialSimple (Vector3 vec) {
		float magnitude = vec.magnitude;
		Vector3 normalized = Vector3.Normalize(vec);
		return normalized * (1 - magnitude);
	}

	Vector3? ComputeAvoidanceVector () {
		Vector3?[] castResults = F.Map(feelerDir => cast(childObj, feelerDir), feelerDirs); 
		hitting = F.Map(result => result.HasValue, castResults);

		Vector3[] filtered = F.FilterOutNulls(castResults);

		if (filtered.Length > 0) {
			Vector3? average = Average(filtered);
			Vector3 averageDefault = average.HasValue ? average.Value : Vector3.zero;
			//Vector3 normalized = Normalize(summed);

			//Vector3 exp = ExponentialSimple(averageDefault);

			Vector3 invert = -averageDefault;
			Vector3 localVelocity = childObj.transform.InverseTransformDirection(rb.velocity);
			print(localVelocity);
			Vector3 steered = Quaternion.AngleAxis(90, Vector3.up) * invert;

			// Vector3 outVector = steered * avoidanceStrength;
			Vector3 outVector = steered;

			//debug stuff
			drawAgentVector(steered, () => Color.yellow);
		  drawAgentVector(invert, () => Color.green);
			//end debug

			return outVector;
		}
		else {
			return null;
		}
	}

	void FixedUpdate () {

		Vector3? avoidanceVec = ComputeAvoidanceVector();
		Vector3 outVector;
		Vector3 expOutVector;

		//outVector = -outVector;
		if (avoidanceVec.HasValue) {
			outVector = avoidanceVec.Value;
		}
		else {
			outVector = Vector3.forward;
		}
		expOutVector = ExponentialSimple(outVector);

		//outVector = outVector * 2;
		// print(rb.velocity);
		// drawAgentVector(childObj.transform.TransformDirection(rb.velocity), () => Color.magenta);
		Vector3 torque = expOutVector * 0.1f;
		rb.AddRelativeTorque(new Vector3(-torque.y, torque.x, -torque.z));
		rb.AddRelativeForce(Vector3.forward * outVector.magnitude);

		//max velocity
		//rb.velocity = Vector3.ClampMagnitude(rb.velocity, 2f);

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

	Vector3 Exponential (Vector3 vec) {
		//\frac{\cos \left(x\pi \right)}{2}+\ 0.5
		float linear = vec.magnitude;
		double exp = Mathf.Cos(linear * Mathf.PI) / 2 + 0.5;
		print("exp :" + exp + " linear : " + linear);
		return vec * ((float)exp);
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

	void drawAgentVector (Vector3 vec, Func<Color> colorFn) {
			Vector3 pos = childObj.transform.TransformPoint(Vector3.zero);
			Vector3 dir = childObj.transform.TransformDirection(vec);
			Debug.DrawLine(pos, pos + dir * feelerLength, colorFn());
	}

	void debugFeelers () {
		F.ForEach((feelerDir, index) => {
			drawAgentVector(feelerDir, () => hitting[index] ? Color.red : Color.grey);
		}, feelerDirs);
	}
}
