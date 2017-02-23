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
		List<int> verts = populateFlowers();
		meshGenerator.finalizeMesh();
		shapes.printBuildTime(startTime);
	}

	List<int> generateFlower(Vector3? basePos = null, Quaternion? baseRot = null, int stemSides = 6, float stemHeight = .18f, float stemWidth = .02f, 
		int numPetals = 12, float petalLength = .09f, float petalWidth = .025f) {
		int startVertIndex = meshGenerator.vertices.Count;
		if (!baseRot.HasValue) {
			baseRot = new Quaternion(0, 0, 0, 1);
		}
		if (!basePos.HasValue) {
			basePos = Vector3.zero;
		}
		Vector3 curPos = basePos.Value;
		shapes.generateCylinder(stemHeight, stemWidth, stemSides, true, "centerCap", curPos);
		//int petalNum = Random.Range(3, 8);
		Quaternion rot = meshGenerator.rotateQuaternion(baseRot.Value, Vector3.left, 45);
		rot = meshGenerator.rotateQuaternion(rot, Vector3.up, 50);
		float rotIncr = 360f / numPetals;
		for (int j = 0; j < numPetals; ++j) {
			shapes.generatePlane(1, 1, petalLength, petalWidth, "centerCap", curPos, rot, true, "fit");
			rot = meshGenerator.rotateQuaternion(rot, Vector3.left, rotIncr);
		}
		return new List<int> { startVertIndex, meshGenerator.vertices.Count - 1 };
	}

	List<int> populateFlowers(int minStemSides = 3, int maxStemSides = 12, float minStemHeight = .12f, float maxStemHeight = .24f, float minStemWidth = .01f, float maxStemWidth = .03f, 
		int minPetals = 8, int maxPetals = 18, float minPetalLength = .04f, float maxPetalLength = .14f, float minPetalWidth = .015f, float maxPetalWidth = .035f) {
		int startVertIndex = meshGenerator.vertices.Count;
		for (int i = 0; i < 3; ++i) {
			Vector3 curPos = new Vector3(i, 0, 0);
			for (int r = 0; r < 3; ++r) {
				int stemSides = Random.Range(minStemSides, maxStemSides);
				float stemHeight = Random.Range(minStemHeight, maxStemHeight);
				float stemWidth = Random.Range(minStemWidth, maxStemWidth);
				int numPetals = Random.Range(minPetals, maxPetals);
				float petalLength = Random.Range(minPetalLength, maxPetalLength);
				float petalWidth = Random.Range(minPetalWidth, maxPetalWidth);
				generateFlower(curPos, null, stemSides,stemHeight,stemWidth, numPetals, petalLength,petalWidth);
				curPos.y += 1;
			}
		}
		transform.rotation = meshGenerator.rotateQuaternion(transform.rotation, Vector3.left, -90);
		return new List<int> { startVertIndex, meshGenerator.vertices.Count - 1 };
	}
}