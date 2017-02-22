using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MeshEffects : MonoBehaviour {

	public string animMode;
	public List<string> args;
	GenerateMesh meshGenerator;

	// Use this for initialization
	void Start () {
		meshGenerator = this.GetComponent<GenerateMesh>();
	}

	//rotate gameObject over time
	void animateRotation(float xRot, float yRot, float zRot, float speed) {
		transform.Rotate(new Vector3(xRot,yRot,zRot), speed * Time.deltaTime);
	}

	//wave vert groups by the first vert's normal for the sake of simplicity
	void animateWave(float speed, float numWaves, float amplitude) {
		int count = 0; //only increase count after each vert group, rather than each individual vert
		foreach (Dictionary<Quaternion, VertexData> dict in meshGenerator.vertDict.verts.Values) {
			VertexData vert0 = null;
			foreach (VertexData vert in dict.Values) {
				if (vert0 == null) {
					vert0 = vert;
				}
				meshGenerator.vertices[vert.verticesIndex] = meshGenerator.vertices[vert.verticesIndex] + meshGenerator.normals[vert0.verticesIndex] *
				(Mathf.Sin(speed * Time.time + (numWaves * 6.28f / (float)meshGenerator.vertDict.verts.Values.Count) * count) / (2000 / amplitude));
			}
		++count;
		}	
	}
	
	// Update is called once per frame
	void Update () {
		if (animMode == "wave") {
			animateWave(float.Parse(args[0]), float.Parse(args[1]), float.Parse(args[2]));
		}
		else if (animMode == "rotate") {
			animateRotation(float.Parse(args[0]), float.Parse(args[1]), float.Parse(args[2]), float.Parse(args[3]));
		}
		Mesh mesh = GetComponent<MeshFilter>().mesh;
		mesh.vertices = meshGenerator.vertices.ToArray();
		MeshCollider mc = this.GetComponent<MeshCollider>();
		if (mc) {
			mc.sharedMesh = mesh;
		}

	}
}
