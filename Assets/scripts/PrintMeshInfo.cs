using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrintMeshInfo : MonoBehaviour {

	// Use this for initialization
	void Start () {
        MeshFilter mf = gameObject.GetComponent<MeshFilter>();
        Vector2[] uvs = mf.mesh.uv;
        for (int i = 0; i < uvs.Length; ++i) {
            Debug.Log(uvs[i]);
        }
	}
}
