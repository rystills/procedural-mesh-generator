using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateFlowers : MonoBehaviour {
	GenerateMesh meshGenerator;
	MeshShapes shapes;

	void Awake() {
		meshGenerator = this.GetComponent<GenerateMesh>();
	}

	// Use this for initialization
	void Start() {
		float startTime = Time.realtimeSinceStartup; //record start time at beginning of function
		shapes = this.GetComponent<MeshShapes>();
		populateFlowers();
		shapes.printBuildTime(startTime);
	}

	void populateFlowers(int minStemSides = 3, int maxStemSides = 12, float minStemHeight = .12f, float maxStemHeight = .24f, float minStemWidth = .01f, float maxStemWidth = .03f, 
		int minPetals = 8, int maxPetals = 18, float minPetalLength = .04f, float maxPetalLength = .14f, float minPetalWidth = .015f, float maxPetalWidth = .035f) {
		int startVertIndex = meshGenerator.vertices.Count;
		for (int i = 0; i < 20; ++i) {
			Vector3 curPos = new Vector3(i/2f, 0, 0);
			for (int r = 0; r < 20; ++r) {
				int stemSides = Random.Range(minStemSides, maxStemSides);
				float stemHeight = Random.Range(minStemHeight, maxStemHeight);
				float stemWidth = Random.Range(minStemWidth, maxStemWidth);
				int numPetals = Random.Range(minPetals, maxPetals);
				float petalLength = Random.Range(minPetalLength, maxPetalLength);
				float petalWidth = Random.Range(minPetalWidth, maxPetalWidth);
				GameObject go = new GameObject();
				go.transform.position = curPos;
				GenerateMesh newGenerator = go.AddComponent<GenerateMesh>();
				MeshShapes shapes = go.AddComponent<MeshShapes>();
				shapes.generateFlower(curPos, meshGenerator.rotateQuaternion(new Quaternion(0,0,0,1),Vector3.left,-90f), stemSides,stemHeight,stemWidth, numPetals, petalLength,petalWidth);
				newGenerator.finalizeMesh();
				go.GetComponent<MeshRenderer>().material.color = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
				curPos.z += .5f;
			}
		}
	}
}