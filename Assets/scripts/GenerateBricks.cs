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

	//generate numerous bricks with sproperties between the input ranges
	void populateBricks(float length = .5f, float width = .7f, float minHeight = 1f, float maxHeight = 1.2f, float minSpacing = .1f, float maxSpacing = .2f) {
		GameObject brickParent = new GameObject();
		brickParent.name = "brick container";
		for (int i = 0; i < 50; ++i) {
			for (int r = 0; r < 50; ++r) {
				Vector3 curPos = new Vector3(i* (width + .01f),r*(length + .01f),0);
				GameObject go = new GameObject();
				go.name = "brick";
				go.transform.position = curPos;
				//go.transform.rotation = meshGenerator.rotateQuaternion(go.transform.rotation, Vector3.up, Random.Range(0, 359f));
				float height = Random.Range(minHeight, maxHeight);
				GenerateMesh newGenerator = go.AddComponent<GenerateMesh>();
				MeshShapes shapes = go.AddComponent<MeshShapes>();
				shapes.generateBox(length, width, height, null, meshGenerator.rotateQuaternion(new Quaternion(0, 0, 0, 1), Vector3.left, -90f));
				newGenerator.finalizeMesh();
				//go.GetComponent<MeshRenderer>().material.color = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
				//go.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
				go.transform.SetParent(brickParent.transform, true);
			}
		}
	}
}