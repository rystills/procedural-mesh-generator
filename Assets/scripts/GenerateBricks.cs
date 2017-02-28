using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateBricks : MonoBehaviour {
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
		populateBricks(); 
		Debug.Log("Time to generate bricks: " + (Time.realtimeSinceStartup - startTime).ToString());
	}

	//generate numerous bricks with properties between the input ranges
	void populateBricks(float length = .3f, float width = .6f, float height = .4f, float maxShiftOut = .15f) {
		GameObject brickParent = new GameObject();
		brickParent.name = "brick container";
		//first generate filling between the bricks, as a single wall
		GameObject go = new GameObject();
		go.name = "brick seams";
		GenerateMesh newGenerator = go.AddComponent<GenerateMesh>();
		MeshShapes shapes = go.AddComponent<MeshShapes>();
		shapes.generateBox(50 * (length + .02f) - .12f, 50 * (width + .02f) - .12f, height - maxShiftOut - .1f, 
			new Vector3(.05f, 49 * (length + .02f) - .05f, -.05f), meshGenerator.rotateQuaternion(new Quaternion(0, 0, 0, 1), Vector3.left, -90f));
		newGenerator.finalizeMesh();
		go.GetComponent<MeshRenderer>().material = this.GetComponent<GenerateMesh>().material2;
		go.transform.SetParent(brickParent.transform, true);

		//generate 50x50 array of bricks on the x,y axes
		for (int i = 0; i < 50; ++i) {
			for (int r = 0; r < 50; ++r) {
				float shiftOut = Random.Range(0, maxShiftOut);
				Vector3 curPos = new Vector3(i * (width + .02f), r * (length + .02f), shiftOut);
				go = new GameObject();
				go.name = "brick";
				go.transform.position = curPos;
				newGenerator = go.AddComponent<GenerateMesh>();
				shapes = go.AddComponent<MeshShapes>();
				shapes.generateBox(length, width, height, null, meshGenerator.rotateQuaternion(new Quaternion(0, 0, 0, 1), Vector3.left, -90f));
				newGenerator.finalizeMesh();
				go.GetComponent<MeshRenderer>().material = this.GetComponent<GenerateMesh>().material;
				go.transform.SetParent(brickParent.transform, true);
			}
		}
	}
}