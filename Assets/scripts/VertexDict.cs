using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class VertexDict {
	public const float smoothnessFloatTolerance = .01f;//tolerance applied to all direction comparisons to compensate for floating point imprecision
	public const int smoothnessFloatDigits = 2; //inverse of smoothnessFloatToleranceconst int smoothnessFloatDigits = 2; //inverse of smoothnessFloatTolerance
	//public const float normalAverageMaxDifference = 45; //normals of overlapping vertices will not be averaged if their starting normals are larger than this value
	public Dictionary<Vector3, Dictionary<Quaternion, VertexData>> verts; //input position and direction to get a single vertex (lookups are rounded using a smoothness constant)

	public VertexDict() {
		verts = new Dictionary<Vector3, Dictionary<Quaternion, VertexData>>();
	}

	//return Vector3 vec rounded to smoothnessFloatDigits
	public Vector3 roundVector(ref Vector3 vec) {
		return new Vector3((float)Math.Round((double)vec.x, smoothnessFloatDigits), (float)Math.Round((double)vec.y, smoothnessFloatDigits),
		   (float)Math.Round((double)vec.z, smoothnessFloatDigits));
	}

	//get all vertices at position pos, regardless of normal dir
	public VertexData[] getVerts(Vector3 pos) {
		return getVertDict(pos).Values.ToArray();
	}

	//add a vert at position pos with normal normal
	public VertexData addVert(int verticesIndex, Vector3 pos, Vector3 normal, List<int> trianglesIndices = null) {
		Dictionary<Quaternion, VertexData> vertDict = getVertDict(pos);
		if (vertDict == null) {
			vertDict = new Dictionary<Quaternion, VertexData>();
			verts[roundVector(ref pos)] = vertDict;
		}
		VertexData newVert = new VertexData(verticesIndex, trianglesIndices);
		vertDict[Quaternion.Euler(normal.x, normal.y, normal.z)] = newVert;
		return newVert;
	}

	//given Vector3 pos, check if vertex exists at this position within rounding tolerance
	public Dictionary<Quaternion, VertexData> getVertDict(Vector3 pos) {
		//check if rounded position is a valid key first
		Vector3 roundedPos = roundVector(ref pos);
		if (verts.ContainsKey(roundedPos)) {
			return verts[roundedPos];
		}
		//check if the position iterated up to one time in any of the axes is a valid key
		float incrementAmount = (float)Math.Pow(10, -1 * smoothnessFloatDigits);
		for (int i = -1; i <= 1; ++i) {
			for (int r = -1; r <= 1; ++r) {
				for (int j = -1; j <= 1; ++j) {
					Vector3 curPos = new Vector3(roundedPos.x + incrementAmount * i, roundedPos.y + incrementAmount * i, roundedPos.z + incrementAmount * i);
					if (verts.ContainsKey(curPos)) {
						return verts[curPos];
					}
				}
			}
		}
		//no vector could be found at this position
		return null;
	}

	//get vertex at position pos with normal normal
	public VertexData getVert(Vector3 pos, Vector3 normal, float quatSmoothnessThreshhold = 0) {
		Dictionary<Quaternion, VertexData> vertDict = getVertDict(pos);
		if (vertDict == null) { //vector not found at position pos
			return null;
		}

		//found a valid position; search for the closest normal at this position that falls within smoothnessFloatTolerance
		bool foundCandidate = false;
		Quaternion smallestKey = Quaternion.identity;
		float smallestDiff = smoothnessFloatTolerance + quatSmoothnessThreshhold;
		foreach (Quaternion key in vertDict.Keys) {
			float angleDiff = Quaternion.Angle(Quaternion.Euler(normal.x,normal.y,normal.z), key);
			if (angleDiff <= smallestDiff) {
				smallestDiff = angleDiff;
				smallestKey = key;
				foundCandidate = true;
			}
		}
		if (foundCandidate) {
			return vertDict[smallestKey];
		}
		return null; //a vector was found at this position, but no matching normal within tolerance was found at that position
	}
}

public class VertexData {
	public int verticesIndex; //can be used to lookup vertex, UV, or normal
	public List<int> trianglesIndices; //to be used as a list of all instances of this Vertex's index in mesh's triangle list

	public VertexData(int verticesIndex, List<int> trianglesIndices = null) {
		if (trianglesIndices == null) {
			trianglesIndices = new List<int>();
		}
		this.verticesIndex = verticesIndex;
		this.trianglesIndices = trianglesIndices;
	}	

	//add another index to trianglesIndices
	public void adadTriangleIndex(int index) {
		this.trianglesIndices.Add(index);
	}
}
