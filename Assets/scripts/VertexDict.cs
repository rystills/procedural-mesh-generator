using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class VertexDict {
	const float smoothnessFloatTolerance = .01f;//tolerance applied to all direction comparisons to compensate for floating point imprecision
	const int smoothnessFloatDigits = 2; //inverse of smoothnessFloatToleranceconst int smoothnessFloatDigits = 2; //inverse of smoothnessFloatTolerance
	const float normalAverageMaxDifference = 45; //normals of overlapping vertices will not be averaged if their starting normals are larger than this value
	Dictionary<Vector3, Dictionary<Quaternion, VertexData>> verts; //input position and direction to get a single vertex (lookups are rounded using a smoothness constant)

	public VertexDict() {
		verts = new Dictionary<Vector3, Dictionary<Quaternion, VertexData>>();
	}

	public VertexData[] getVerts(Vector3 pos) {
		return verts[pos].Values.ToArray();
	}

	//get vertex at position pos with normal normal
	public VertexData getVert(Vector3 pos, Vector3 normal) {
		//check if rounded position is a valid key first
		Vector3 roundedPos = new Vector3((float)Math.Round((double)pos.x, smoothnessFloatDigits), (float)Math.Round((double)pos.y, smoothnessFloatDigits), 
			(float)Math.Round((double)pos.z, smoothnessFloatDigits));
		Dictionary<Quaternion, VertexData> quatKey = null;
		if (verts.ContainsKey(roundedPos)) {
			quatKey = verts[roundedPos];
		}
		else {
			//check if the position iterated one time in any of the axes is a valid key
			Vector3 curPos;
			float incrementAmount = (float)Math.Pow(10, -1 * smoothnessFloatDigits);
			for (int i = -1; i <= 1; ++i) {
				for (int r = -1; r <= 1; ++r) {
					for (int j = -1; j <= 1; ++j) {
						curPos = new Vector3(roundedPos.x + incrementAmount * i, roundedPos.y + incrementAmount * i, roundedPos.z + incrementAmount * i);
						if (verts.ContainsKey(curPos)) {
							quatKey = verts[curPos];
							i = r = j = 2; //break out of all 3 loops
						}
					}
				}
			}
		}
		if (quatKey == null) { //no vector could be found at this position
			return null;
		}
		//found a valid position; search for the closest rotation at this position that falls within smoothnessFloatTolerance
		
		bool foundCandidate = false;
		Quaternion smallestKey = Quaternion.identity;
		float smallestDiff = smoothnessFloatTolerance;
		foreach (Quaternion key in quatKey.Keys) {
			float angleDiff = Quaternion.Angle(Quaternion.Euler(normal.x,normal.y,normal.z), key);
			if (angleDiff <= smallestDiff) {
				smallestDiff = angleDiff;
				smallestKey = key;
				foundCandidate = true;
			}
		}
		if (foundCandidate) {
			return quatKey[smallestKey];
		}
		return null; //a vector was found at this position, but no matching direction within tolerance was found at that position
	}
}

public class VertexData {
	public int verticesIndex;
	public int normalsIndex;
	public List<int> trianglesIndex;

	public VertexData(int verticesIndex, int normalsIndex, List<int> trianglesIndex) {
		this.verticesIndex = verticesIndex;
		this.normalsIndex = normalsIndex;
		this.trianglesIndex = trianglesIndex;
	}	
}
