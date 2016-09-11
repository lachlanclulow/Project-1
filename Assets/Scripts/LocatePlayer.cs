using UnityEngine;
using System.Collections;

public class LocatePlayer : MonoBehaviour {

    public GameObject terrain;
    public GameObject water;
    private const int HEIGHT = 30;
	
	// Use this for initialization
	void Start () {
        TerrainData terrainData = terrain.GetComponent<Terrain>().terrainData;
        float planeHeight = water.transform.position.y;

        float[] heights = new float[4];

        // Get 4 closest height points
        heights[0] = terrainData.GetHeight((int)this.transform.position.x, (int)this.transform.position.z);
        heights[1] = terrainData.GetHeight((int)this.transform.position.x + 1, (int)this.transform.position.z);
        heights[2] = terrainData.GetHeight((int)this.transform.position.x, (int)this.transform.position.z + 1);
        heights[3] = terrainData.GetHeight((int)this.transform.position.x + 1, (int)this.transform.position.z + 1);

        // Find the highest point
        float highest = planeHeight;

        for (int i = 0; i < heights.Length; i++)
            if (heights[i] > highest) highest = heights[i];

        // place player HEIGHT above highest point
        this.transform.position = new Vector3(this.transform.position.x, highest + HEIGHT, this.transform.position.z);
    }
}
