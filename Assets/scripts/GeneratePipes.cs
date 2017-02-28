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

	//generate numerous pipes with properties between the input ranges
	void populatePipes(int pipeSegs = 16, float circumference = 3, float minExtents = .8f, float maxExtents = 3f) {
		GameObject pipeParent = new GameObject();
		pipeParent.name = "pipe container";
		//setup initial rotation and position
		Vector3 curPos = new Vector3(0, 0, 0);
		Quaternion curRot = new Quaternion(0, 0, 0, 1);
		Vector3 prevForwardDir = Vector3.zero;
		Vector3 forwardDir = Vector3.zero;
		GameObject prevPipe = null;
		GameObject go = null;
		int rotAxis = 0;

		//generate 2000 pipe segments, each with a random color
		for (int i = 0; i < 2000; ++i) {
			float extents = Random.Range(minExtents, maxExtents);
			prevForwardDir = forwardDir;
			prevPipe = go;
			forwardDir = curRot * Vector3.forward;
			go = new GameObject();
			go.name = "pipe segment";
			if (i != 0 && rotAxis != 0) { //move all but the first pipe so that they do not intersect, unless current pipe rotation was a spin
				curPos = curPos + (prevForwardDir.normalized * (circumference / 2f / Mathf.PI));
				curPos = curPos + (forwardDir.normalized * (circumference / 2f / Mathf.PI));				
			}
			go.transform.position = curPos;
			GenerateMesh newGenerator = go.AddComponent<GenerateMesh>();
			shapes = go.AddComponent<MeshShapes>();
			shapes.generateCylinder(extents, circumference, pipeSegs, false, "centerCap", null,curRot);

			newGenerator.finalizeMesh();
			go.GetComponent<MeshRenderer>().material = this.GetComponent<GenerateMesh>().material;
			go.GetComponent<MeshRenderer>().material.color = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
			go.transform.SetParent(pipeParent.transform, true);

			if (i != 0 && rotAxis != 0) { //if the current pipe rotation was not a spin, add an elbow joint
				GameObject elbow = new GameObject();
				elbow.name = "elbow";
				elbow.transform.position = curPos;
				GenerateMesh elbowGenerator = elbow.AddComponent<GenerateMesh>();

				//generate triangles between all end verts of prevPipe, and all start verts of go
				GenerateMesh prevGenerator = prevPipe.GetComponent<GenerateMesh>();
				GenerateMesh curGenerator = go.GetComponent<GenerateMesh>();

				for (int r = 0; r < pipeSegs; ++r) {
					Vector3 vert0 = prevGenerator.vertices[6 + r*8] + (prevPipe.transform.position - curPos); //start front vert index is 6, each subsequent vert is 8 more
					Vector3 vert1 = prevGenerator.vertices[6 + (r+1)%pipeSegs * 8] + (prevPipe.transform.position - curPos);
					Vector3 vert2 = curGenerator.vertices[5 + r * 8]; //start back vert index is 5, each subsequent vert is 8 more
					Vector3 vert3 = curGenerator.vertices[5 + (r + 1) % pipeSegs * 8];
					elbowGenerator.generateQuad(vert0, vert1, vert2, vert3); //generate outside quad
					elbowGenerator.generateQuad(vert0, vert2, vert1, vert3); //generate inside (flipped) quad
				}

				elbowGenerator.finalizeMesh();
				elbow.GetComponent<MeshRenderer>().material = this.GetComponent<GenerateMesh>().material;
				elbow.GetComponent<MeshRenderer>().material.color = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
				elbow.transform.SetParent(pipeParent.transform, true);

			}

			//update position to center of end cap
			curPos = curPos + (forwardDir.normalized * extents);

			rotAxis = Random.Range(0, 2);
			int rotOrientation = Random.Range(0, 1);
			curRot = meshGenerator.rotateQuaternion(curRot, rotAxis == 0 ? Vector3.forward : rotAxis == 1 ? Vector3.up : Vector3.left, 90 * (rotOrientation == 0 ? 1 : -1));
		}
	}
}