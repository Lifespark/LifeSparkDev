using UnityEngine;
using System.Collections;

public class Region : MonoBehaviour
{


    // Data
    public SparkPoint[] regionPoints;

    private Color Team1Color;
    private Color Team2Color;
    private GameObject regionPolygon;
    private Mesh regionPolygonMesh;
	private bool activated = false;
	private int team = -1;

    // Use this for initialization
    void Start()
    {
        // Initialize variables
        Team1Color = Color.Lerp(Color.red, Color.clear, 0.75f);
        Team2Color = Color.Lerp(Color.blue, Color.clear, 0.75f);
        
        // Setup regionPolygon
        //PrepareRegionPolygon();

        // Print Debug Warning if region is not triangular
        if (regionPoints.Length != 3)
        {
            Debug.LogWarning("REGIONS MUST BE TRIANGLES AND MUST HAVE EXACTLY 3 SPARK POINTS");
        }
    }


    // Update is called once per frame
    void Update()
    {


    }


    // Instnatiates and sets up the regionPolygon gameobject/mesh
    public void PrepareRegionPolygon()
    {
        // Instantiate new game object
        regionPolygon = new GameObject();
		regionPolygon.name = "Region";

        // Add Mesh Filter and Mesh Renderer components
        var meshFilter = (MeshFilter)regionPolygon.AddComponent("MeshFilter");
        var meshRenderer = (MeshRenderer)regionPolygon.AddComponent("MeshRenderer");

        // Setup mesh renderer
        meshRenderer.material = this.renderer.material;

        // Instantiate new mesh and assign vertices of polygon
        regionPolygonMesh = new Mesh();
        regionPolygonMesh.vertices = new Vector3[] {
            new Vector3(regionPoints[0].transform.position.x, this.gameObject.transform.position.y + 0.005f, regionPoints[0].transform.position.z),
            new Vector3(regionPoints[1].transform.position.x, this.gameObject.transform.position.y + 0.005f, regionPoints[1].transform.position.z),
            new Vector3(regionPoints[2].transform.position.x, this.gameObject.transform.position.y + 0.005f, regionPoints[2].transform.position.z)
        };


        // setup UV
        regionPolygonMesh.uv = new Vector2[] { new Vector2(1, 1), new Vector2(1, 0), new Vector2(0, 1) };

        // setup mesh filter
        meshFilter.mesh = regionPolygonMesh;



        /* *
         * Algorithm for calculating triangles of an n-sided region
         * Couldn't get this working properly so for now regions must be triangles.
         * Not sure if this is a feature the designers want right now anyway, but if it is I (Alex) can add it.
         * 
         * 
        // Calculate how triangles will be stitched together to draw regionPolygon. Save off data into regionPolygonTriangles array
        // IMPORTANT!!! - regionPoints must be assigned in a clockwise fashion
        //              - regionPoints must form a convex polygon
        int numTriangles = regionPolygonMesh.vertices.Length - 2;
        int[] triangleVertices = new int[numTriangles * 3];
        int index = 0;

        
        for (int i = 0; i < numTriangles; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (j == 0)
                {
                    triangleVertices[index] = 0;
                }
                else
                {
                    triangleVertices[index] = i + j;
                }
                index++;
            }
        }

        regionPolygonTriangles = triangleVertices;
         
         * 
         * 
         * */
    }


    // Redraws regionPolygon with the passed in color
    private void DrawRegionPolygon(Color teamColor)
    {
        regionPolygon.renderer.enabled = true;
        // Set regionPolygon color
        regionPolygon.renderer.material.color = teamColor;

        // Redraw polygon
        regionPolygonMesh.triangles = new int[] { 0, 1, 2 };
        regionPolygonMesh.RecalculateNormals();
        regionPolygonMesh.RecalculateBounds();
        regionPolygonMesh.Optimize();

    }

	public bool GetActivated() {
		return activated;
	}
	
	public int GetTeam() {
		return team;
	}
	
	/* Checks if region should be drawn or destroyed. -jk */
	public void CheckActivation() {
		if (regionPoints[0].gameObject == null || regionPoints[1].gameObject == null || regionPoints[2].gameObject == null)
		{
			// remove polygon
			activated = false;
			team = -1;
			regionPolygonMesh.Clear();
		}
		else if (regionPoints[0].GetOwner() == 1 && regionPoints[1].GetOwner() == 1 && regionPoints[2].GetOwner() == 1)
		{
			// draw polygon with team 1 color
			activated = true;
			team = 1;
			DrawRegionPolygon(Team1Color);
		}
		else if (regionPoints[0].GetOwner() == 2 && regionPoints[1].GetOwner() == 2 && regionPoints[2].GetOwner() == 2)
		{
			// draw polygon with team 2 color
			activated = true;
			team = 2;
			DrawRegionPolygon(Team2Color);
		}
	}

}
