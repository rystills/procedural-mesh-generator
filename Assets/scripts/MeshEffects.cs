using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MeshEffects : MonoBehaviour {

	public string animMode;
	GenerateMesh meshGenerator;

	// Use this for initialization
	void Start () {
		meshGenerator = this.GetComponent<GenerateMesh>();
	}
	
	// Update is called once per frame
	void Update () {
		if (animMode == "wave") {
			//wave vert groups by the first vert's normal for the sake of simplicity
			int count = 0;
			foreach (Dictionary<Quaternion, VertexData> dict in meshGenerator.vertDict.verts.Values) {
				VertexData[] curVerts = dict.Values.ToArray();
				for (int i = 0; i < curVerts.Length; ++i) {
						meshGenerator.vertices[curVerts[i].verticesIndex] = meshGenerator.vertices[curVerts[i].verticesIndex] + meshGenerator.normals[curVerts[0].verticesIndex].normalized * 
						(Mathf.Sin(2 * Time.time + (6f*6.28f / (float)meshGenerator.vertDict.verts.Values.Count)* count) / 2000);
				}
				++count;
			}
			Mesh mesh = GetComponent<MeshFilter>().mesh;
			mesh.vertices = meshGenerator.vertices.ToArray();
			this.GetComponent<MeshCollider>().sharedMesh = mesh;
		}
	
	}
}
