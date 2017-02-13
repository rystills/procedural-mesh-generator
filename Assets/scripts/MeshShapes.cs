using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MeshShapes : MonoBehaviour {
	GenerateMesh meshGenerator;

	public string shape;
	public List<string> args;
	public bool displace;

	// Use this for initialization
	void Start() {
		float startTime = Time.realtimeSinceStartup; //record start time at beginning of function
		List<int> verts = null;
		meshGenerator = this.GetComponent<GenerateMesh>();
		if (shape == "box") {
			verts = generateBox(float.Parse(args[0]), float.Parse(args[1]), float.Parse(args[2]));
		}
		else if (shape == "spiral") {
			verts = generateSpiral(float.Parse(args[0]), float.Parse(args[1]), int.Parse(args[2]), float.Parse(args[3]));
		}
		else if (shape == "cylinder") {
			verts = generateCylinder(float.Parse(args[0]), float.Parse(args[1]), int.Parse(args[2]), args[3] == "1");
		}
		if (displace) {
			displaceVerts(.2f, verts[0], verts[1]);
		}
		meshGenerator.finalizeMesh();

		//calculate end time and print args and time
		float endTime = Time.realtimeSinceStartup;
		string debugString = "time to generate " + shape + "(";
		for (int i = 0; i < args.Count; ++i) {
			debugString += args[i] + (i == args.Count - 1 ? ")" : ", "); 
		}
		debugString += ": " + (endTime - startTime) + " seconds";
		Debug.Log(debugString);
	}

	//apply a random displacement between -maxDisp and +maxDisp from vert startIndex to vert endIndex (both inclusive) - for simplicity, each vertex uses the normal of the first vert in its group
	public void displaceVerts(float maxDisp, int startIndex, int endIndex) {
		foreach (Dictionary<Quaternion, VertexData> dict in meshGenerator.vertDict.verts.Values) {
			float curDisp = Random.Range(-1 * maxDisp, maxDisp);
			VertexData[] curVerts = dict.Values.ToArray();
			for (int i = 0; i < curVerts.Length; ++i) {
				if (!(startIndex <= curVerts[i].verticesIndex && curVerts[i].verticesIndex <= endIndex)) {
					continue;
				}
				meshGenerator.vertices[curVerts[i].verticesIndex] += (meshGenerator.normals[curVerts[0].verticesIndex].normalized * curDisp);
			}
		}
	}

	//construct a spiral, with segs quads of width, extents, rotating each quad by iterAngle
	List<int> generateSpiral(float width, float extents, int segs, float iterAngle) {
		int startVertIndex = meshGenerator.vertices.Count;
		Vector3 rotAxis = Vector3.forward;
		Quaternion rot = new Quaternion(0, 0, 0, 1);
		Vector3 pos = new Vector3(0, 0, 0);
		float curExtents = extents;
		for (int i = 0; i < segs; ++i, curExtents -= extents / (float)segs) { //decrease segment length after each iteration
			meshGenerator.propagateQuad(pos, rot, width, curExtents, true); //generate back-facing quad (flipped normal)
			pos = meshGenerator.propagateQuad(pos, rot, width, curExtents, false); //generate forward-facing quad and update current vertex position
			rot = meshGenerator.rotateQuaternion(rot, rotAxis, iterAngle); //update rotation
		}
		if (segs == 0 || startVertIndex == meshGenerator.vertices.Count) { //if we didnt make any new verts, return an empty list
			return null;
		}
		return new List<int> { startVertIndex, meshGenerator.vertices.Count - 1 };

	}

	//construct a segs-sided cylinder with width and extents
	List<int> generateCylinder(float width, float extents, int segs, bool cap = false) {
		int startVertIndex = meshGenerator.vertices.Count;
		Vector3 rotAxis = Vector3.forward;
		Quaternion rot = new Quaternion(0, 0, 0, 1);
		Vector3 pos = new Vector3(0, 0, 0);
		float iterAngle = 360 / (float)segs;
		float iterExtents = extents / (float)segs;
		List<int> frontVerts = new List<int>();
		List<int> backVerts = new List<int>();
		for (int i = 0; i < segs; ++i) {
			
			if (!cap) { //don't bother generating interior faces if we are capped, since they will not be visible anyway
				meshGenerator.propagateQuad(pos, rot, width, iterExtents, false); //generate forward-facing quad and update current vertex position
			}
			pos = meshGenerator.propagateQuad(pos, rot, width, iterExtents, true); //generate back-facing quad (flipped normal)
			rot = meshGenerator.rotateQuaternion(rot, rotAxis, iterAngle); //update rotation
			if (cap) {
				frontVerts.Add(meshGenerator.vertices.Count - 2);
				//frontVerts.Add(meshGenerator.vertices.Count - 4);
				backVerts.Add(meshGenerator.vertices.Count - 3);
				//backVerts.Add(meshGenerator.vertices.Count - 1);
			}
		}
		if (segs == 0 || startVertIndex == meshGenerator.vertices.Count) { //if we didnt make any new verts, return an empty list
			return null;
		}

		if (cap) { //cap front and back of cylinder
			generateCap(frontVerts,false,false);
			generateCap(backVerts,true,false);			
		}
		return new List<int> { startVertIndex, meshGenerator.vertices.Count - 1 };
	}

	//generate cap-faces between verts list
	public void generateCap(List<int> verts, bool flip = false, bool createCenter = false) {
		Vector3 center = Vector3.zero;
		if (createCenter) {
			//calculate center position
			for (int i = 0; i < verts.Count; ++i) {
				center += meshGenerator.vertices[verts[0]];
			}
			center /= (float)verts.Count;
		}
		else {
			center = meshGenerator.vertices[verts[0]];
		}
		//Debug.Log(center);
		//add center as new vert, with two other reference verts to get the normal right
		//meshGenerator.generateQuad(meshGenerator.vertices[verts[flip ? 1 : 0]], meshGenerator.vertices[verts[flip ? 0 : 1]], center);
		for (int i = createCenter ? 0 : 1; i < verts.Count - 1; ++i) {
			Debug.Log("vector0: " + meshGenerator.vertices[verts[i + (flip ? 1 : 0)]]);
			Debug.Log("vector1: " + meshGenerator.vertices[verts[i + (flip ? 0 : 1)]]);
			Debug.Log("vector2: " + (center));
			meshGenerator.generateQuad(meshGenerator.vertices[verts[i + (flip ? 1 : 0)]], meshGenerator.vertices[verts[i + (flip ? 0 : 1)]], center);
		}
		if (createCenter) {
			meshGenerator.generateQuad(meshGenerator.vertices[verts[flip ? 0 : verts.Count - 1]], meshGenerator.vertices[verts[flip ? verts.Count - 1 : 0]], center);
		}
	}

	//construct a box, with length, width, height segs
	List<int> generateBox(float length, float width, float height) {
		int startVertIndex = meshGenerator.vertices.Count;
		Vector3 rotAxis = Vector3.forward;
		Quaternion rot = new Quaternion(0, 0, 0, 1);
		Vector3 pos = new Vector3(0, 0, 0);
		for (int i = 0; i < 4; ++i) { //generate a strip of 4 sides
			pos = meshGenerator.propagateQuad(pos, rot, 1, 1, true); //generate forward-facing quad and update current vertex position
			rot = meshGenerator.rotateQuaternion(rot, rotAxis, 90); //update rotation
		}
		Quaternion leftRot = meshGenerator.rotateQuaternion(rot, Vector3.up, 90);
		meshGenerator.propagateQuad(pos, leftRot, 1, 1, false); //generate 'left' sidee
		meshGenerator.propagateQuad(pos + Vector3.forward.normalized, leftRot, 1, 1, true); //generate 'right' side
		return new List<int> { startVertIndex, meshGenerator.vertices.Count - 1 };
	}
}