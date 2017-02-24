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
		populateFlowers(3,3); //restrict stems to 3 sides only
		Debug.Log("Time to generate flowers: " + (Time.realtimeSinceStartup - startTime).ToString());
	}

	//generate numerous flowers with properties between the input ranges
	void populateFlowers(int minStemSides = 3, int maxStemSides = 12, float minStemHeight = .12f, float maxStemHeight = .24f, float minStemWidth = .01f, float maxStemWidth = .03f, 
		int minPetals = 8, int maxPetals = 18, float minPetalLength = .04f, float maxPetalLength = .14f, float minPetalWidth = .015f, float maxPetalWidth = .035f,
		int minPetalSegs = 1, int maxPetalSegs = 6, float minPetalSegRot = 5f, float maxPetalSegRot = 14f) {
		int startVertIndex = meshGenerator.vertices.Count;
		for (int i = 0; i < 50; ++i) {
			Vector3 curPos = new Vector3(1.5f + i, .5f, .25f);
			for (int r = 0; r < 50; ++r) {
				int stemSides = Random.Range(minStemSides, maxStemSides);
				float stemHeight = Random.Range(minStemHeight, maxStemHeight);
				float stemWidth = Random.Range(minStemWidth, maxStemWidth);
				int numPetals = Random.Range(minPetals, maxPetals);
				float petalLength = Random.Range(minPetalLength, maxPetalLength);
				float petalWidth = Random.Range(minPetalWidth, maxPetalWidth);
				int petalSegs = Random.Range(minPetalSegs, maxPetalSegs);
				float petalSegRot = Random.Range(minPetalSegRot, maxPetalSegRot);
				GameObject go = new GameObject();
				go.transform.position = curPos;
				GenerateMesh newGenerator = go.AddComponent<GenerateMesh>();
				MeshShapes shapes = go.AddComponent<MeshShapes>();
				shapes.generateFlower(null, meshGenerator.rotateQuaternion(new Quaternion(0,0,0,1),Vector3.left,-90f), stemSides,stemHeight,stemWidth, numPetals, petalLength, petalWidth, petalSegs, petalSegRot);
				newGenerator.finalizeMesh();
				go.GetComponent<MeshRenderer>().material.color = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
				
				//move flower to the ground
				RaycastHit hit;
				if (Physics.Raycast(go.transform.position, Vector3.down, out hit)) {
					Debug.Log("hit");
					go.transform.position = new Vector3(go.transform.position.x, go.transform.position.y - hit.distance, go.transform.position.z);
				}
				curPos.z += 1;
			}
		}
	}
}