using UnityEngine;
using System.Collections;

public class GenerateMesh : MonoBehaviour {
	public Material material;

	void Start() {
		GenerateMeshDemo();
	}

	// construct a flat nxn rectangular mesh
	void GenerateMeshDemo() {
		int n = 10; //number of pieces to split the mesh into
		Vector3[] newVertices = new Vector3[(n+1)*(n+1)]; //one square mesh has 4 vertices, and each subsequent triange adds one new vert (therefore two per additional quad)
		int[] newTrianglePoints = new int[6*n*n]; //n^2 total quads. each quad is 2 tris. multiply by 3 as each tri is described by 3 points
        Vector2[] newUVs = new Vector2[newVertices.Length]; //number of UVs should match number of vertices for proper mapping

       
		int positionInList = 0;
		float averageLocalY = 0f;
		
		for (int x = 0; x < n+1; x++) {
			for (int y = 0; y < n+1; y++) {
				if (x == 0 && y == 0) { //first vertex so the average local y is the model's local y
					averageLocalY = 0;
				}
				else if (x == 0) {
					averageLocalY = newVertices[positionInList - 1].y; //in the first row so only have to check vertex behind you
				}
				else {
					if (y == 0) { //in the first column but not first row so must check below you and below - infront of you
						averageLocalY = (newVertices[positionInList - (n+1)].y + newVertices[positionInList - (n+1) + 1].y) / 2;
					}
					else if (y == n+1) { //in the last column but not first row so must check behind you, below behind you, and below you
						averageLocalY = (newVertices[positionInList - 1].y + newVertices[positionInList - (n+1) - 1].y + newVertices[positionInList - (n+1)].y) / 3;
					}
					else { //somewhere not on the outside. must check behind, below-behind, below, below-infront
						averageLocalY = (newVertices[positionInList - 1].y + newVertices[positionInList - (n+1) - 1].y + newVertices[positionInList - (n+1)].y + newVertices[positionInList - (n+1) + 1].y) / 4;
					}
				}
				newVertices.SetValue(new Vector3(x*.55f, averageLocalY + (Random.Range(-0.4f,0.4f)), y*.55f),positionInList);
				positionInList++;
				
			}
		}
		
		positionInList = 0;
		for (int x = 0; x < (n); x+=1) {
			for (int y = 0; y < (n); y+=1) {
				newTrianglePoints.SetValue(((n+1)*x)+ (y),positionInList);
				positionInList++;
				newTrianglePoints.SetValue(((n+1)*x) + (y+1),positionInList);
				positionInList++;
				newTrianglePoints.SetValue(((n+1)*x) + (y+(n+1)),positionInList);
				positionInList++;
				newTrianglePoints.SetValue(((n+1)*x) + (y+(n+1)),positionInList);
				positionInList++;
				newTrianglePoints.SetValue(((n+1)*x) + (y+1),positionInList);
				positionInList++;
				newTrianglePoints.SetValue(((n+1)*x) + (y+(n+2)),positionInList);
				positionInList++;
			}
		}
		
		for (int i = 0 ; i < newUVs.Length; i++)
			newUVs[i] = new Vector2(newVertices[i].x/(5), newVertices[i].z/(5));
		
		Mesh mesh = new Mesh();
        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
		meshFilter.mesh = mesh;
		mesh.vertices = newVertices;
		mesh.triangles = newTrianglePoints;
		mesh.uv = newUVs;
		meshFilter.mesh.RecalculateBounds();
		meshFilter.mesh.RecalculateNormals();
        MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
        Renderer renderer = meshRenderer.GetComponent<Renderer>();
        renderer.material.color = Color.blue;
        MeshCollider meshCollider = gameObject.AddComponent<MeshCollider>();
        meshCollider.sharedMesh = mesh;
        renderer.material = material;
	}	
}