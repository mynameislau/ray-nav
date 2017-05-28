using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Functional;
using RayNav;
public class Sensing : MonoBehaviour {

	private Rigidbody rb;
	private GameObject agentObj;
	// Use this for initialization
	private const float Infinity = 1/0f;
	private const float feelerLength = 3;
	private const float avoidanceStrength = 0.1f;
	private Feeler[] feelers = {
		new Feeler("top", Normalize(new Vector3(0, 1, 1))),
		new Feeler("bottom", Normalize(new Vector3(0, -1, 1))),
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

		agentObj = gameObject;
		// agentObj = gameObject.transform.GetChild(0).gameObject;
		rb = agentObj.GetComponent(typeof(Rigidbody)) as Rigidbody;
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

	struct AvoidanceData {
		public Vector3 reflection;

	}

	AvoidanceData? ComputeAvoidanceVector () {
		FeelerData?[] castResults = F.Map(feelerDir => cast(agentObj, feelerDir), feelerDirs); 
		hitting = F.Map(result => result.HasValue, castResults);

		FeelerData[] filtered = F.FilterOutNulls(castResults);

		if (filtered.Length > 0) {
			Vector3? averageReflection = Average(F.Map(current => current.reflection, filtered));
			Vector3 averageReflectionDefault = averageReflection.HasValue ? averageReflection.Value : Vector3.zero;
			Vector3? average = Average(F.Map(current => current.hitData, filtered));
			Vector3 averageDefault = average.HasValue ? average.Value : Vector3.zero;
			
			//Vector3 normalized = Normalize(summed);

			//Vector3 exp = ExponentialSimple(averageDefault);

			//Vector3 invert = -averageDefault;
			//Vector3 localVelocity = agentObj.transform.InverseTransformDirection(rb.velocity);
			// Vector3 steered = Quaternion.AngleAxis(90, Vector3.up) * invert;
			//Vector3 steered = Quaternion.AngleAxis(90, Vector3.right + Vector3.up) * invert;
			//Vector3 steered = LimitedSteer(-invert, invert);


			// Vector3 outVector = steered * avoidanceStrength;
			Vector3 outVector = averageDefault;

			//end debug
			AvoidanceData data;
			data.reflection = averageReflectionDefault;
			return data;
		}
		else {
			return null;
		}
	}

	Vector3 GetAngleTruc (Vector3 eulerAngle) {
		Vector3 bla = eulerAngle / 360;
		return new Vector3(
			bla.x > 0.5 ? -(1 - bla.x) : bla.x,
			bla.y > 0.5 ? -(1 - bla.y) : bla.y,
			bla.z > 0.5 ? -(1 - bla.z) : bla.z
		);
	}

	void FixedUpdate () {

		AvoidanceData? avoidanceVec = ComputeAvoidanceVector();
		Vector3 outVector;
		Vector3 expOutVector;
		Vector3 torque;
		//outVector = -outVector;
		if (avoidanceVec.HasValue) {
			outVector = avoidanceVec.Value.reflection;
			// torque = (Vector3.forward - outVector) * 0.3f;
			expOutVector = ExponentialSimple(outVector);
			// print(torque.x);
			// Quaternion diff = Quaternion.Slerp(Quaternion.LookRotation(Vector3.forward), Quaternion.LookRotation(outVector), 0.1f);
			// Vector3 euler = diff.eulerAngles;
			// euler = euler * 0.0001f;
			// rb.AddRelativeTorque(Vector3.Cross(outVector, Vector3.up) * 0.01f);
			Quaternion rot = Quaternion.FromToRotation(Vector3.forward, outVector);
			Vector3 truc = GetAngleTruc(rot.eulerAngles);
			print(rot.eulerAngles);
			truc.z = 0;
			rb.AddRelativeTorque(truc * avoidanceStrength);

			//debug stuff
			drawAgentVector(rot * Vector3.forward * 10f, () => Color.yellow);
		  // drawAgentVector(Normalize(euler) * 10f, () => Color.green);
		}
		else {
			outVector = Vector3.forward;
			float zRotation = rb.rotation.eulerAngles.z;
			zRotation = zRotation / 360;
			zRotation = zRotation > 0.5 ? -(1 - zRotation) : zRotation;
			print(zRotation);
			rb.AddRelativeTorque(new Vector3(0, 0, -zRotation * 0.1f));
		}
		
		rb.AddRelativeForce(Vector3.forward * outVector.magnitude);
		//outVector = outVector * 2;
		// print(rb.velocity);
		// drawAgentVector(agentObj.transform.TransformDirection(rb.velocity), () => Color.magenta);
		//Vector3 torque = expOutVector * rb.velocity.magnitude * 0.1f;
		//print(expOutVector + " torque : " + expOutVector.y + " " + (expOutVector.y * rb.velocity.magnitude * 0.1f));
		//rb.AddRelativeTorque(new Vector3(-expOutVector.y, -expOutVector.x, 0));

		//max velocity
		//rb.velocity = Vector3.ClampMagnitude(rb.velocity, 2f);

		// Vector3[] responses = F.Map((feelerDir) => {
		// 	Vector3? castResult = cast(agentObj, feelerDir);
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
		// 	rb.AddForce(agentObj.transform.TransformDirection(Vector3.forward));
		// }

		// if (right.HasValue) { rb.AddForce(new Vector3(-1, 0, -1) * right.Value); }
		// if (left.HasValue) { rb.AddForce(new Vector3(1, 0, -1) * left.Value); }
		
	}

	Vector3 Exponential (Vector3 vec) {
		//\frac{\cos \left(x\pi \right)}{2}+\ 0.5
		float linear = vec.magnitude;
		double exp = Mathf.Cos(linear * Mathf.PI) / 2 + 0.5;
		return vec * ((float)exp);
	}

	FeelerData feelerData (Vector3 vec, Vector3 normal) {
		float linear = vec.magnitude / feelerLength;
		Vector3 norm = Normalize(vec);
		FeelerData data;
		data.hitData = norm * linear;
		data.reflection = Vector3.Reflect(data.hitData, normal);
		return data;
	}

	struct FeelerData
	{
		public Vector3 hitData;
		public Vector3 reflection;
	}

	FeelerData? cast(GameObject gameObj, Vector3 localDir) {
		Vector3 pos = gameObj.transform.TransformPoint(Vector3.zero);
		Vector3 dir = gameObj.transform.TransformDirection(localDir);
		RaycastHit rh = new RaycastHit();
		if (Physics.Raycast(pos, dir, out rh, feelerLength)) {
			Vector3 local =  gameObj.transform.InverseTransformDirection(rh.point - pos);
			return feelerData(local, gameObj.transform.InverseTransformDirection(rh.normal));
		}
		else {
			return null;
		}
	}

	void drawAgentVector (Vector3 vec, Func<Color> colorFn) {
			Vector3 pos = agentObj.transform.TransformPoint(Vector3.zero);
			Vector3 dir = agentObj.transform.TransformDirection(vec);
			Debug.DrawLine(pos, pos + dir, colorFn());
	}

	void debugFeelers () {
		F.ForEach((feelerDir, index) => {
			drawAgentVector(feelerDir * feelerLength, () => hitting[index] ? Color.red : Color.grey);
		}, feelerDirs);
	}
}
