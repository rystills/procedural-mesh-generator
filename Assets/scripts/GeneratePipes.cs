using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneratePipes : MonoBehaviour {
	GenerateMesh meshGenerator;
	MeshShapes shapes;
	bool firstRun = true;

	void Awake() {
		meshGenerator = this.GetComponent<GenerateMesh>();
	}

	void Update() {
		if (!firstRun) {
			return;
		}
		firstRun = false;
		float startTime = Time.realtimeSinceStartup; //record start time at beginning of function
		shapes = this.GetComponent<MeshShapes>();
		populatePipes();
		Debug.Log("Time to generate pipes: " + (Time.realtimeSinceStartup - startTime).ToString());
	}

	//generate numerous bricks with sproperties between the input ranges
	void populatePipes(int pipeSegs = 8, float width = 3, float minExtents = .5f, float maxExtents = 3f) {
		GameObject pipeParent = new GameObject();
		pipeParent.name = "pipe container";
		//setup initial rotation and position
		Vector3 curPos = new Vector3(0, 0, 0);
		Quaternion curRot = new Quaternion(0, 0, 0, 1);
		Vector3 forwardDir = Vector3.zero;

		//generate 50 pipe segments
		for (int i = 0; i < 50; ++i) {
			float extents = Random.Range(minExtents, maxExtents);
			GameObject go = new GameObject();
			go.name = "pipe segment";
			/*if (i != 0) {
				curPos = curPos + (forwardDir.normalized * (width / 2f));
			}*/
			go.transform.position = curPos;
			GenerateMesh newGenerator = go.AddComponent<GenerateMesh>();
			shapes = go.AddComponent<MeshShapes>();
			shapes.generateCylinder(extents, width, pipeSegs, false, "centerCap", null,curRot);
			//update position to center of end cap
			forwardDir = curRot * Vector3.forward;
			curPos = curPos + (forwardDir.normalized * extents);

			newGenerator.finalizeMesh();
			go.GetComponent<MeshRenderer>().material = this.GetComponent<GenerateMesh>().material;
			go.transform.SetParent(pipeParent.transform, true);

			int rotAxis = Random.Range(0, 2);
			int rotOrientation = Random.Range(0, 1);
			curRot = meshGenerator.rotateQuaternion(curRot, rotAxis == 0 ? Vector3.forward : rotAxis == 1 ? Vector3.up : Vector3.left, 90 * (rotOrientation == 0 ? 1 : -1));
		}
	}
}