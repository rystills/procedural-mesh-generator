using UnityEngine;
using System.Collections;

// Author - Vikram / aka Makubex
// URL - http://8bitmemories.blogspot.com
// WTF License
public class TrackGen : MonoBehaviour
{
	// Road
	public float width;
	public float height;
	
	// Rail
	public float railheight;
	public float railwidth;
	
	// public to help debug in editor
	public int n;
	public Vector3[] points;
	public Vector3[] vs;
	public int[] tris;
	
	// Start and end to be used by anyone
	public Vector3 start;
	public Vector3 end;
	
	void Start()
	{
		// Set the points for track in this function you create
		GenTrackPoints();
		
		// Create mesh filter
		MeshFilter meshfilter = this.gameObject.AddComponent(typeof(MeshFilter)) as MeshFilter;
		
		// Create and set the mesh
		Mesh mesh = new Mesh();
		meshfilter.mesh = mesh;
		
		// Blocks
		n = points.Length-1;
		
		// Start and end points
		start = points[0];
		end = points[n-1];
		
		// 18 + x8 vertices
		vs = new Vector3[ 18 + (n-1) * 8];
		
		// iterator
		int run = 0;
		
		float w = 0;
		float h = 0; 
		for (int s = 0; s < 8; s++)
		{
			// For the given sequence, set w and h offset from the center
			switch (s)
			{
			case 0:
				w = -width / 2f;
				h = height / 2f;
				break;
				
			case 1:
				w = width / 2f;
				h = height / 2f;
				break;
				
			case 2:
				w = width / 2f;
				h = height / 2f + railheight ;
				break;
				
			case 3:
				w = width/2f + railwidth;
				h = height / 2f + railheight;
				break;
				
			case 4:
				w = width/2f + railwidth;
				h = -height / 2f;
				break;
				
			case 5:
				w = -width/2f - railwidth;
				h = -height / 2f;
				break;
				
			case 6:
				w = -width/2f - railwidth;
				h = height/2f + railheight;
				break;
				
			case 7:
				w = -width / 2f;
				h = height/2f + railheight;
				break;
				
			default:
				break;
			}
			
			// Default initialize - cribbing compiler
			Vector3 fwd =  Vector3.forward, left = Vector3.left;
			
			for (int i = 0; i <= n; i++)
			{
				// Except for the last point
				if (i != n)
				{
					// Direction of track
					fwd = points[i + 1] - points[i];
					
					// Now assume no banking
					fwd.y = 0;
					fwd.Normalize();
					
					// Get left
					left = Vector3.Cross( Vector3.up, fwd);
				}
				
				vs[run++] = points[i] + left * w + Vector3.up * h;
			}
		}
		
		mesh.vertices = vs;
		
		//TODO: Do your UV mapping here
		
		// Triangle n x 16 x 3
		tris = new int[n * 16 * 3];
		
		// reset iterator
		run = 0;
		for (int s = 0; s < 8; s++)
		{
			for (int i = 0; i < n; i++)
			{
				// 1st Tri
				tris[run + 0] = s * (n + 1) + i;
				tris[run + 1] = s * (n + 1) + i + 1;
				tris[run + 2] = ((s + 1) % 8) * (n + 1) + i;
				
				// 2nd Tri
				tris[run + 3] = s * (n+1) + i + 1;
				tris[run + 4] = ((s + 1) % 8) * (n + 1) + i + 1;
				tris[run + 5] = ((s + 1) % 8) * (n + 1) + i;
				
				run += 6;
			}
		}
		
		mesh.triangles = tris;
		mesh.RecalculateNormals();
		
		//? mesh.Optimize();
		
		// Add collider
		this.gameObject.AddComponent(typeof(MeshCollider));      
	}
	
	// Update is called once per frame
	void Update()
	{
		
	}
	
	// Sample
	void GenTrackPoints()
	{
		points = new Vector3[]{
			new Vector3(-107.749f, 0.0f, -512.177f),
			new Vector3(-54.5753f, -91.8645f, -18.2249f),
			new Vector3(-4.39224f, -68.906f, 328.316f),
			new Vector3(55.1721f, -19.8289f, 596.923f),
			new Vector3(103.816f, 0.0f, 715.899f),
			new Vector3(116.448f, 0.0f, 734.519f),
			new Vector3(129.919f, 0.0f, 749.686f),
			new Vector3(146.244f, 0.0f, 762.377f),
			new Vector3(163.287f, 0.0f, 770.069f),
			new Vector3(191.751f, 0.0f, 773.779f),
			new Vector3(216.974f, 0.0f, 771.956f),
			new Vector3(234.099f, 0.0f, 767.216f),
			new Vector3(254.964f, 0.0f, 761.512f),
			new Vector3(403.15f, 0.0f, 731.987f),
			new Vector3(435.338f, 0.0f, 731.273f),
			new Vector3(459.583f, 0.0f, 733.384f),
			new Vector3(483.771f, 0.0f, 738.667f),
			new Vector3(505.242f, 0.0f, 747.098f),
			new Vector3(523.175f, 0.0f, 758.05f),
			new Vector3(540.104f, 0.0f, 773.324f),
			new Vector3(552.374f, 0.0f, 789.069f),
			new Vector3(563.035f, 0.0f, 808.026f),
			new Vector3(570.923f, 0.0f, 827.512f),
			new Vector3(576.537f, 0.0f, 846.625f),
			new Vector3(580.297f, 0.0f, 864.184f),
			new Vector3(584.542f, 0.0f, 886.775f),
			new Vector3(590.261f, 0.0f, 915.214f),
			new Vector3(633.769f, 0.0f, 1092.56f),
			new Vector3(643.902f, 0.0f, 1128.48f),
			new Vector3(655.667f, 0.0f, 1168.77f),
			new Vector3(738.94f, 0.0f, 1431.96f),
			new Vector3(744.972f, 0.0f, 1453.04f),
			new Vector3(750.752f, 0.0f, 1478.01f),
			new Vector3(753.825f, 0.0f, 1505.93f),
			new Vector3(752.466f, 0.0f, 1530.7f),
			new Vector3(747.18f, 0.0f, 1553.96f),
			new Vector3(737.949f, 0.0f, 1577.02f),
			new Vector3(725.932f, 0.0f, 1598.32f),
			new Vector3(714.298f, 0.0f, 1614.94f),
			new Vector3(698.457f, 0.0f, 1634.21f),
			new Vector3(680.296f, 0.0f, 1653.42f),
			new Vector3(659.177f, 0.0f, 1673.29f),
			new Vector3(638.744f, 0.0f, 1690.79f),
			new Vector3(487.317f, 0.0f, 1807.67f),
			new Vector3(363.045f, 0.0f, 1875.34f),
			new Vector3(269.704f, 0.0f, 1901.79f),
			new Vector3(196.216f, 0.0f, 1907.88f),
			new Vector3(120.336f, 0.0f, 1908.13f),
			new Vector3(42.0606f, 0.0f, 1904.21f),
			new Vector3(-7.80302f, 0.0f, 1895.39f),
			new Vector3(-52.9811f, 0.0f, 1880.97f),
			new Vector3(-96.7555f, 0.0f, 1860.28f),
			new Vector3(-152.358f, 0.0f, 1824.4f),
			new Vector3(-218.216f, 0.0f, 1769.43f),
			new Vector3(-288.061f, 0.0f, 1699.14f),
			new Vector3(-323.662f, 0.0f, 1655.43f),
			new Vector3(-367.344f, 0.0f, 1583.66f),
			new Vector3(-404.875f, 0.0f, 1498.63f),
			new Vector3(-436.182f, 0.0f, 1402.65f),
			new Vector3(-462.55f, 0.0f, 1297.14f),
			new Vector3(-502.421f, 0.0f, 1103.18f),
			new Vector3(-517.274f, 0.0f, 1037.94f),
			new Vector3(-532.872f, 0.0f, 983.134f),
			new Vector3(-550.423f, 0.0f, 938.31f),
			new Vector3(-578.61f, 0.0f, 895.077f),
			new Vector3(-612.755f, 0.0f, 868.495f),
			new Vector3(-649.657f, 0.0f, 855.282f),
			new Vector3(-695.862f, 0.0f, 855.257f),
			new Vector3(-741.431f, 0.0f, 870.587f),
			new Vector3(-784.001f, 0.0f, 898.653f),
			new Vector3(-813.477f, 0.0f, 926.9f),
			new Vector3(-841.827f, 0.0f, 962.503f),
			new Vector3(-865.998f, 0.0f, 1001.45f),
			new Vector3(-885.401f, 0.0f, 1040.79f),
			new Vector3(-901.112f, 0.0f, 1080.48f),
			new Vector3(-914.332f, 0.0f, 1122.49f),
			new Vector3(-923.809f, 0.0f, 1160.81f),
			new Vector3(-930.98f, 0.0f, 1198.17f),
			new Vector3(-937.072f, 0.0f, 1234.32f),
			new Vector3(-944.945f, 0.0f, 1279.87f),
			new Vector3(-958.1f, 0.0f, 1353.17f),
			new Vector3(-987.21f, 0.0f, 1503.88f),
			new Vector3(-1027.36f, 0.0f, 1690.85f),
			new Vector3(-1072.87f, 0.0f, 1886.1f),
			new Vector3(-1087.63f, 0.0f, 1939.26f),
			new Vector3(-1098.86f, 0.0f, 1978.58f),
			new Vector3(-1111.52f, 0.0f, 2020.79f),
			new Vector3(-1125.33f, 0.0f, 2063.57f),
			new Vector3(-1143.81f, 0.0f, 2114.53f),
			new Vector3(-1159.28f, 0.0f, 2151.46f),
			new Vector3(-1176.28f, 0.0f, 2186.19f),
			new Vector3(-1196.22f, 0.0f, 2219.72f),
			new Vector3(-1221.5f, 0.0f, 2252.39f),
			new Vector3(-1248.58f, 0.0f, 2277.03f),
			new Vector3(-1278.57f, 0.0f, 2294.04f),
			new Vector3(-1315.05f, 0.0f, 2302.96f),
			new Vector3(-1363.48f, 0.0f, 2299.14f),
			new Vector3(-1409.24f, 0.0f, 2282.81f),
			new Vector3(-1464.58f, 0.0f, 2243.24f),
			new Vector3(-1508.89f, 0.0f, 2195.24f),
			new Vector3(-1563.63f, 0.0f, 2118.48f),
			new Vector3(-1679.41f, 0.0f, 1885.68f),
			new Vector3(-1779.3f, 0.0f, 1522.46f),
			new Vector3(-1795.28f, 0.0f, 1399.92f),
			new Vector3(-1802.63f, 0.0f, 1256.66f),
			new Vector3(-1797.32f, 0.0f, 1116.44f),
			new Vector3(-1780.05f, 0.0f, 985.026f),
			new Vector3(-1752.6f, 0.0f, 864.818f),
			new Vector3(-1712.18f, 0.0f, 743.911f),
			new Vector3(-1658.56f, 0.0f, 622.318f),
			new Vector3(-1493.72f, 0.0f, 332.389f),
			new Vector3(-1329.12f, -42.3689f, 88.7799f),
			new Vector3(-1112.79f, -96.6482f, -203.524f),
			new Vector3(-890.161f, -114.054f, -513.434f),
			new Vector3(-686.149f, -88.3449f, -836.132f),
			new Vector3(-476.804f, -37.587f, -1179.55f),
			new Vector3(-336.531f, 0.0f, -1505.53f),
			new Vector3(-275.358f, 0.0f, -1681.14f),
			new Vector3(-148.687f, 0.0f, -1904.14f),
			new Vector3(-81.7364f, 0.0f, -1951.92f),
			new Vector3(-13.7099f, 0.0f, -1956.88f),
			new Vector3(44.8454f, 0.0f, -1930.32f),
			new Vector3(89.4252f, 0.0f, -1893.66f),
			new Vector3(142.234f, 0.0f, -1834.86f),
			new Vector3(198.365f, 0.0f, -1757.83f),
			new Vector3(250.236f, 0.0f, -1677.12f),
			new Vector3(290.052f, 0.0f, -1611.33f),
			new Vector3(324.723f, 0.0f, -1552.77f),
			new Vector3(1540.96f, 0.0f, 690.775f)};
		
		
	}
}