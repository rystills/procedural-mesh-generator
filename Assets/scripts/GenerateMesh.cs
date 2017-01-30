using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GenerateMesh : MonoBehaviour {
    public Material material;
    List<Vector3> newVertices;
    List<Vector3> newNormals;
    List<int> newTrianglePoints;
    List<Vector2> newUVs;
    Dictionary<Vector3, Dictionary<string[],int>> vertIndicesAxes;
    Dictionary<Vector3, Dictionary<Quaternion, int>> vertIndices;
    Dictionary<Vector3, Dictionary<Quaternion, List<int>>> connectedVertIDs;
    Texture2D debugTex;
    const float smoothnessFloatTolerance = .5f; //tolerance applied to all direction comparisons to compensate for floating point imprecision
    const float normalAverageMaxDifference = 45; //normals of overlapping vertices will not be averaged if their starting normals are larger than this value

    void Start() {
        //init mesh lists
        newVertices = new List<Vector3>();
        newNormals = new List<Vector3>();
        newTrianglePoints = new List<int>();
        newUVs = new List<Vector2>();
        vertIndicesAxes = new Dictionary<Vector3, Dictionary<string[], int>>();
        vertIndices = new Dictionary<Vector3, Dictionary<Quaternion, int>>();
        connectedVertIDs = new Dictionary<Vector3, Dictionary<Quaternion, List<int>>>();
        //build debug texture as a fallback if no material is supplied
        debugTex = new Texture2D(2, 2);
        debugTex.SetPixel(0, 0, Color.red);
        debugTex.SetPixel(1, 0, Color.magenta);
        debugTex.SetPixel(0, 1, Color.blue);
        debugTex.SetPixel(1, 1, Color.cyan);
        debugTex.wrapMode = TextureWrapMode.Repeat;
        debugTex.Apply();
        //generateMesh("normal", 3,5);
        List<int> boxVerts = generateBox(2, 3, 4);
        displaceVerts(.2f, boxVerts[0], boxVerts[1]);
        //generateBoxAxes(3, 5, 7);
        //List<int> spiralVerts = generateSpiral(2,1,100,16);
        //if (spiralVerts != null) {
            //displaceVerts(.2f,spiralVerts[0],spiralVerts[1]);
        //}
        finalizeMesh();
    }

    //apply a random displacement between -maxDisp and +maxDisp from vert startIndex to vert endIndex (both inclusive)
    void displaceVerts(float maxDisp, int startIndex, int endIndex) {
        //finalizeMesh(); //finalize mesh to get current normals
        Debug.Log(vertIndices.Count());
        Dictionary<Vector3, float> vertPosDiplacements = new Dictionary<Vector3, float>();
        for (int i = startIndex; i <= endIndex; ++i) {
            float curDisp;
            if (!vertPosDiplacements.ContainsKey(newVertices[i])) {
                vertPosDiplacements[newVertices[i]] = Random.Range(-1 * maxDisp, maxDisp);

            }
            curDisp = vertPosDiplacements[newVertices[i]];
            newVertices[i] += (newNormals[i].normalized * curDisp);
        }
    }

    Quaternion getVertRotation(int vertIndex) {
        Dictionary<Quaternion, int> curVertCandidates = vertIndices[newVertices[vertIndex]];
        foreach (Quaternion dir in curVertCandidates.Keys) {
            if (curVertCandidates[dir] == vertIndex) {
                return dir;
            }
        }
        throw new System.Exception();
    }

    //construct a box, with length, width, height segs
    List<int> generateBox(float length, float width, float height) {
        int startVertIndex = newVertices.Count;
        Vector3 rotAxis = Vector3.forward;
        Quaternion rot = new Quaternion(0, 0, 0, 1);
        Vector3 pos = new Vector3(0, 0, 0);
        for (int i = 0; i < 4; ++i) { //generate a strip of 4 sides
            pos = propagateQuad(pos, rot, 1, 1, true); //generate forward-facing quad and update current vertex position
            rot = rotateQuaternion(rot, rotAxis, 90); //update rotation
        }
        Debug.Log(vertIndices.Count());
        Quaternion leftRot = rotateQuaternion(rot, Vector3.up, 90);
        propagateQuad(pos, leftRot, 1,1,false); //generate 'left' sidee
        propagateQuad(pos + Vector3.forward.normalized, leftRot, 1,1,true); //generate 'right' side
        return new List<int> { startVertIndex, newVertices.Count - 1 };
    }

    //construct a spiral, with segs quads of width, extents, rotating each quad by iterAngle
    List<int> generateSpiral(float width, float extents, int segs, float iterAngle) {
        int startVertIndex = newVertices.Count;
        Vector3 rotAxis = Vector3.forward;
        Quaternion rot = new Quaternion(0,0,0,1);
        Vector3 pos = new Vector3(0, 0, 0);
        float curExtents = extents;
        for (int i = 0; i < segs; ++i) {
            propagateQuad(pos, rot, width, curExtents, true); //generate back-facing quad (flipped normal)
            pos = propagateQuad(pos,rot,width, curExtents, false); //generate forward-facing quad and update current vertex position
            rot = rotateQuaternion(rot, rotAxis, iterAngle); //update rotation
            curExtents -= (extents/segs); //decrease segment length
        }
        if (segs == 0 || startVertIndex == newVertices.Count) { //if we didnt make any new verts, return an empty list
            return null;
        }
        return new List<int> { startVertIndex, newVertices.Count - 1 };
        
    }

    Quaternion rotateQuaternion(Quaternion quat, Vector3 rotAxis, float amount) {
        return quat * Quaternion.Euler(rotAxis * amount);
    }

    //construct a closed box, with length, width, height segments; adapted from generateMesh
    void generateBoxAxes(int length, int width, int height) {
        float quadSize = 1;
        string[] allAxes = { "x", "y", "z" };
        int[] allDims = { length, width, height };
        //generate sides in groups of 2; front and back, then left and right, then finally top and bottom
        for (int k = 0; k < 3; ++k) {
            string[] axes = { allAxes[k], allAxes[(k + 1) % allAxes.Length] };
            for (int l = 0; l < 2; ++l) {
                float[] position = { 0, 0, 0 };
                //if l is 0, generate first side (no offset); otherwise, generate opposte side (quadSize offset)
                position[(k+2)%3] = l == 0 ? 0 : allDims[(k+2)%allDims.Length] * quadSize;
                //outer loop: iterate over the primary axis
                for (int i = 0; i < allDims[k]; ++i) {
                    position[(k + 1) % 3] = 0;
                    //inner loop: create quads while iterating over the secondary axis
                    for (int j = 0; j < allDims[(k + 1) % allDims.Length]; ++j) {
                        propagateQuadAxes(position, axes, quadSize, l != 0);
                        position[(k + 1) % 3] += quadSize;
                    }
                    position[k] += quadSize;
                }
            }
        }
        finalizeMesh(true);
    }

    // construct a flat mxn rectangular mesh (m = x segments, n = y segments)
    void generateMesh(string mode, int m, int n = 0) {
        //give n a default value of m if it is not specified
        n = (n == 0 ? m : n);
        if (mode == "normal") {
            float quadSize = 1;
            float xPos = 0;
            string[] axes = new string[2];
            axes[0] = "x"; axes[1] = "y";
            //outer loop: iterate over the x axis
            for (int i = 0; i < m; ++i) {
                float yPos = 0;
                //inner loop: create quads while iterating over the y axis
                for (int j = 0; j < n; ++j) {
                    propagateQuadAxes(xPos, yPos, 0, axes, quadSize);
                    yPos += quadSize;
                }
                xPos += quadSize;
            }
        }
        finalizeMesh();
    }
    
    //create an additional quad from position[] of size quadsize in direction dir (returns ending position)
    Vector3 propagateQuad(Vector3 pos, Quaternion dir, float width, float extents, bool flip = false, float vertSmoothnessthreshold = 0, string uvMode = "per face") {
        //Debug.Log("calling propagateQuad");
        //step 1: generate the necessary verts, and corresponding UVs
        //setup direction vector from rotation Quaternion 
        Vector3 forwardDir = dir * Vector3.forward;
        Vector3 topRightPos = pos + (forwardDir.normalized * width);
        Quaternion leftRotation = rotateQuaternion(dir, new Vector3(1, 0, 0), 90);
        Vector3 leftDir = leftRotation * Vector3.forward;
        Vector3 botLeftPos = pos + (leftDir.normalized * extents);
        Vector3 topLeftPos = botLeftPos + (forwardDir.normalized * width);
        //generate 2 verts for first side
        if (addVert(pos, dir, flip, vertSmoothnessthreshold)) {
            addUV(pos, dir, pos, topRightPos, botLeftPos, uvMode);
        }
        if (addVert(topRightPos, dir, flip, vertSmoothnessthreshold)) {
            addUV(topRightPos, dir, pos, topRightPos, botLeftPos, uvMode);
        }
        //generate 2 verts for second sdie
        if (addVert(botLeftPos, dir, flip, vertSmoothnessthreshold)) {
            addUV(botLeftPos, dir, pos, topRightPos, botLeftPos, uvMode);
        }
        if (addVert(topLeftPos, dir, flip, vertSmoothnessthreshold)) {
            addUV(topLeftPos, dir, pos, topRightPos, botLeftPos, uvMode);
        }

        //step 2: generate the necessary tris (because this method adds a single quad, we need two new triangles, or 6 points in our list of tris)
        int topLeftIndex = getVert(topLeftPos, dir, vertSmoothnessthreshold);
        topLeftIndex = topLeftIndex == -1 ? getVert(topLeftPos, flipQuaternion(dir), vertSmoothnessthreshold) : topLeftIndex;
        int botLeftIndex = getVert(botLeftPos, dir, vertSmoothnessthreshold);
        botLeftIndex = botLeftIndex == -1 ? getVert(botLeftPos, flipQuaternion(dir), vertSmoothnessthreshold) : botLeftIndex;
        int topRightIndex = getVert(topRightPos, dir, vertSmoothnessthreshold);
        topRightIndex = topRightIndex == -1 ? getVert(topRightPos, flipQuaternion(dir), vertSmoothnessthreshold) : topRightIndex;
        int botRightIndex = getVert(pos, dir, vertSmoothnessthreshold);
        botRightIndex = botRightIndex == -1 ? getVert(pos, flipQuaternion(dir), vertSmoothnessthreshold) : botRightIndex;
        //first new tri
        addTri(botRightIndex, topRightIndex, botLeftIndex, dir, flip);
        //second new tri
        addTri(topRightIndex, topLeftIndex, botLeftIndex, dir, flip);
        return botLeftPos;
    }

    //create an additional quad from position[] of size quadSize on axes
    void propagateQuadAxes(float xPos, float yPos, float zPos, string[] axes, float quadSize, bool flip = false) {
        //step 1: generate the necessary verts, and corresponding UVs
        //generate 2 verts for first side
        addVertAxes(xPos, yPos, zPos, axes);
        addVertAxes(xPos + (axes[0] == "x" ? 0 : axes.Contains("x") ? quadSize : 0), yPos + (axes[0] == "y" ? 0 : axes.Contains("y") ? quadSize : 0), zPos + (axes[0] == "z" ? 0 : axes.Contains("z") ? quadSize : 0), axes);
        //generate 2 verts for second sdie
        addVertAxes(xPos + (axes[0] == "x" ? quadSize : 0), yPos + (axes[0] == "y" ? quadSize : 0), zPos + (axes[0] == "z" ? quadSize : 0), axes);
        addVertAxes(xPos + (axes.Contains("x") ? quadSize : 0), yPos + (axes.Contains("y") ? quadSize : 0), zPos + (axes.Contains("z") ? quadSize : 0), axes);

        //step 2: generate the necessary tris (because this method adds a single quad, we need two new triangles, or 6 points in our list of tris)
        int topLeftIndex = getVertAxes(xPos + (axes.Contains("x") ? quadSize : 0), yPos + (axes.Contains("y") ? quadSize : 0), zPos + (axes.Contains("z") ? quadSize : 0),axes);
        int botLeftIndex = getVertAxes(xPos + (axes[0] == "x" ? quadSize : 0), yPos + (axes[0] == "y" ? quadSize : 0), zPos + (axes[0] == "z" ? quadSize : 0), axes);
        int topRightIndex = getVertAxes(xPos + (axes[0] == "x" ? 0 : axes.Contains("x") ? quadSize : 0), yPos + (axes[0] == "y" ? 0 : axes.Contains("y") ? quadSize : 0), zPos + (axes[0] == "z" ? 0 : axes.Contains("z") ? quadSize : 0), axes);
        int botRightIndex = getVertAxes(xPos, yPos, zPos, axes);
        //first new tri
        addTriAxes(botRightIndex, topRightIndex, botLeftIndex, flip);
        //second new tri
        addTriAxes(topRightIndex, topLeftIndex, botLeftIndex, flip);
    }

    void propagateQuadAxes(float[] positions, string[] axes, float quadSize, bool flip = false) {
        propagateQuadAxes(positions[0],positions[1],positions[2], axes, quadSize, flip);
    }

    Quaternion flipQuaternion(Quaternion quat) {
        return (new Quaternion(1, 0, 0, 0)) * quat;
    }

    //add a new vert with corresponding UVs if xPos,yPos does not already contain one, and add this vert's position in newVertices to vertIndicesAxes
    bool addVert(Vector3 pos, Quaternion dir, bool flip, float vertSmoothnessthreshold) {
        //if flipped, rotate the quaternion by 180 degrees on any axis
        Quaternion finalDir = dir;
        if (flip) {
            finalDir = flipQuaternion(finalDir);
        }
        //make sure there are no vertices within vertSmoothnessthreshold at pos
        if (vertIndices.ContainsKey(pos)) {
            Dictionary<Quaternion, int> quatKey = vertIndices[pos];
            foreach (Quaternion key in quatKey.Keys) {
                float angleDiff = Quaternion.Angle(dir, key);
                //Debug.Log("add vert angle diff: " + angleDiff);
                if (angleDiff < vertSmoothnessthreshold + smoothnessFloatTolerance) {
                    return false;
                }
            }
        }

        newVertices.Add(pos);
        setVert(newVertices[newVertices.Count - 1], dir, newVertices.Count - 1);
        return true;
    }

    //calculate UV for point pos given points a,b,c (pos will typically be equivalent to one of these 3 points)
    void addUV(Vector3 pos, Quaternion dir, Vector3 a, Vector3 b, Vector3 c, string uvMode) {
        newNormals.Add(calculateNormal(a, c, b));
        if (uvMode == "per face") {
            newUVs.Add(pos == a ? new Vector2(0, 0) : pos == b ? new Vector2(0, 1) : pos == c ? new Vector2(1, 0) : new Vector2(1, 1));
        }
        else if (uvMode == "per face merge duplicates") {
            int id = vertIndices[pos][dir];
            if (id <= 3) {
                id = 0;
            }
            else {
                id = ((int)((id - 2) / 2));
            }
            newUVs.Add(pos == a ? new Vector2(id, id) : pos == b ? new Vector2(id, id+1) : pos == c ? new Vector2(id+1, id) : new Vector2(id+1, id+1));
        }
    }

    Vector3 calculateNormal(Vector3 a, Vector3 b, Vector3 c) {
        Vector3 side1 = b - a;
        Vector3 side2 = c - a;
        Vector3 perp = Vector3.Cross(side1, side2);
        return perp / perp.magnitude;
    }

    Vector2 getVertUV(Vector3 pos, Quaternion dir, float vertSmoothnessthreshold) {
        int vertIndex = getVert(pos, dir, vertSmoothnessthreshold);
        if (vertIndex == -1) {
            throw new System.Exception();
        }
        return newUVs[vertIndex];
    }

    //set vertex at pos, facing in dir axes
    void setVert(Vector3 pos, Quaternion dir, int index) {
        if (!vertIndices.ContainsKey(pos)) {
            vertIndices[pos] = new Dictionary<Quaternion, int>();
        }
        vertIndices[pos][dir] = index;
    }

    //return index of vert at xPos, yPos, zPos facing in dir axes, -1 if not present
    int getVert(Vector3 pos, Quaternion dir, float vertSmoothnessthreshold) { //check if vertex exists at pos
        if (!vertIndices.ContainsKey(pos)) {
            return -1;
        }
        Dictionary<Quaternion, int> quatKey = vertIndices[pos]; //check for a vertex within float tolerance
        foreach (Quaternion key in quatKey.Keys) {
            float angleDiff = Quaternion.Angle(dir, key);
            //Debug.Log("angleDiff: " + angleDiff + ", dir: " + dir + ", key: " + key);
            if (angleDiff < vertSmoothnessthreshold + smoothnessFloatTolerance) {
                return quatKey[key];
            }
        }
        return -1;
    }

    //add a new vert with corresponding UVs if xPos,yPos does not already contain one, and add this vert's position in newVertices to vertIndicesAxes
    void addVertAxes(float xPos, float yPos, float zPos, string[] axes) {
        //make sure there is not already a vertex at xPos,yPos 
        if (getVertAxes(xPos,yPos,zPos,axes) != -1) {
            return;
        }
        newVertices.Add(new Vector3(xPos, yPos, zPos));
        newUVs.Add(new Vector2(axes[0] == "x" ? xPos : axes[0] == "y" ? yPos : zPos, axes[1] == "x" ? xPos : axes[1] == "y" ? yPos : zPos));
        setVertAxes(newVertices[newVertices.Count - 1],axes, newVertices.Count - 1);
    }

    //set vertex at pos, facing in dir axes
    void setVertAxes(Vector3 pos, string[] axes, int index) {
        if (!vertIndicesAxes.ContainsKey(pos)) {
            vertIndicesAxes[pos] = new Dictionary<string[], int>();
        }
        vertIndicesAxes[pos][axes] = index;
    }

    //return index of vert at xPos, yPos, zPos facing in dir axes, -1 if not present
    int getVertAxes(float xPos, float yPos, float zPos, string[] axes) {
        if (!vertIndicesAxes.ContainsKey(new Vector3(xPos, yPos, zPos))) {
            return -1;
        }
        Dictionary<string[], int> key = vertIndicesAxes[new Vector3(xPos, yPos, zPos)];
        if (!key.ContainsKey(axes)) {
            return -1;
        }
        return key[axes];
    }

    //simple helper method to add 3 points to the newTrianglePoints list
    void addTri(int index1, int index2, int index3, Quaternion dir, bool flip = false) {
        newTrianglePoints.Add(flip ? index3 : index1);
        newTrianglePoints.Add(index2);
        newTrianglePoints.Add(flip ? index1 : index3);
        //Debug.Log("index1: " + index1 + ", index2: " + index2 + ", index3: " + index3 + ", dir: " + dir);
        connectedVertIDs[newVertices[index1]] = new Dictionary<Quaternion, List<int>>();
        connectedVertIDs[newVertices[index1]][dir] = new List<int>();
        connectedVertIDs[newVertices[index1]][dir].Add(index2);
        connectedVertIDs[newVertices[index1]][dir].Add(index3);
        connectedVertIDs[newVertices[index2]] = new Dictionary<Quaternion, List<int>>();
        connectedVertIDs[newVertices[index2]][dir] = new List<int>();
        connectedVertIDs[newVertices[index2]][dir].Add(index1);
        connectedVertIDs[newVertices[index2]][dir].Add(index3);
        connectedVertIDs[newVertices[index3]] = new Dictionary<Quaternion, List<int>>();
        connectedVertIDs[newVertices[index3]][dir] = new List<int>();
        connectedVertIDs[newVertices[index3]][dir].Add(index1);
        connectedVertIDs[newVertices[index3]][dir].Add(index2);
    }

    //average the normals of verts which have the same position, to create smooth lighting
    void averageNormals() {
        Dictionary<Quaternion, int>[] vertGroups = vertIndices.Values.ToArray();
        for (int i = 0; i < vertGroups.Length; ++i) { //loop over all vertices for each position
            Dictionary<Quaternion, int> curVertGroup = vertGroups[i];
            List<Vector3> normals = new List<Vector3>();
            foreach (Quaternion dir in curVertGroup.Keys) { //list all of the normals for the current position
                normals.Add(newNormals[curVertGroup[dir]]);
            }
            Vector3 avgNormal = new Vector3(0, 0, 0);
            for (int r = 0; r < normals.Count; ++r) { //add up the normals
                avgNormal += normals[r];
            }
            avgNormal /= normals.Count; //divide by the total number of normals to get the average
            foreach (Quaternion dir in curVertGroup.Keys) { //replace all of the normals with the calculated average
                newNormals[curVertGroup[dir]] = avgNormal;
            }
        }
    }

    //simple helper method to add 3 points to the newTrianglePoints list
    void addTriAxes(int index1, int index2, int index3, bool flip = false) {
        newTrianglePoints.Add(flip ? index3 : index1);
        newTrianglePoints.Add(index2);
        newTrianglePoints.Add(flip ? index1 : index3);
    }

    //construct the new mesh, and attach the appropriate components
    void finalizeMesh(bool useUnityNormals = false) {
        
        Mesh mesh = new Mesh();
        MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
        if (!meshFilter) {
            meshFilter = gameObject.AddComponent<MeshFilter>();
        }
        meshFilter.mesh = mesh;
        mesh.vertices = newVertices.ToArray();
        mesh.triangles = newTrianglePoints.ToArray();
        mesh.uv = newUVs.ToArray();
        meshFilter.mesh.RecalculateBounds();
        if (useUnityNormals) {
            meshFilter.mesh.RecalculateNormals();
        }
        else {
            averageNormals();
            meshFilter.mesh.normals = newNormals.ToArray();
        }
        MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
        if (!meshRenderer) {
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
        }
        Renderer renderer = meshRenderer.GetComponent<Renderer>();
        renderer.material.color = Color.blue;
        MeshCollider meshCollider = gameObject.GetComponent<MeshCollider>();
        if (!meshCollider) {
            meshCollider = gameObject.AddComponent<MeshCollider>();
        }
        meshCollider.sharedMesh = mesh;
        if (material) {
            renderer.material = material;
        }
        else {
            renderer.material.mainTexture = debugTex;
            renderer.material.color = Color.white;
        }
    }

    void Update() {
        //gameObject.GetComponent<MeshFilter>().mesh.uv = newUVs.ToArray();
        //Debug.Log(gameObject.transform.localRotation);
        //Debug.Log(gameObject.transform.forward);

        //undulate verts based on timer and position in list
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = mesh.vertices;
        /*for (int i = 0; i < newVertices.Count; ++i) {
            vertices[i] = newVertices[i] + newNormals[i].normalized * (Mathf.Sin(2 * Time.time + i)/2);
        }
        mesh.vertices = vertices;
    */}
}