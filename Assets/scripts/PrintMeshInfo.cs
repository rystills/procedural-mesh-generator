using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrintMeshInfo : MonoBehaviour {

	// Use this for initialization
	void Start () {
        MeshFilter mf = gameObject.GetComponent<MeshFilter>();
        Vector2[] uvs = mf.mesh.uv;
        Vector3[] verts = mf.mesh.vertices;
        for (int i = 0; i < verts.Length; ++i) {
            //Debug.Log(uvs[i]);
            Debug.Log(verts[i]);
        }
	}
}
