using UnityEngine;
using System.Collections;

public class TerrainGen : MonoBehaviour
{


    public int terrainFactor;
    [Range(0.0f, 12.0f)]
    public float smoothness;
    public float terrainHeight;
    public GameObject water;
    public GameObject boundary;
    public GameObject sun;
    public Texture2D[] textures;
    public PhysicMaterial material;
    private float[,] heightMap;
    private System.Random rnd = new System.Random();

    void Awake()
    {
        /** Generate height map */
        int terrainSize = 2;

        for (int i = 0; i < terrainFactor; i++)
            terrainSize *= 2;

        terrainSize++;
        heightMap = createHeightMap(terrainSize);

        /** Apply TerrainData to Terrain Componenet */
        TerrainData terrainData = new TerrainData();
        Terrain terrainComponent = this.gameObject.AddComponent<Terrain>();
        TerrainCollider terrainCollider = this.gameObject.AddComponent<TerrainCollider>();
        
        /** Initialise */
        terrainData.name = "Terrain";
        terrainData.size = new Vector3(1000, terrainHeight, 1000);
        terrainData.heightmapResolution = terrainSize;
        terrainData.SetHeights(0, 0, heightMap);
        terrainData.thickness = 1.0f;
        terrainData.splatPrototypes = setTextures();
        splatGen(terrainData);
        terrainData.SetDetailResolution(512, 512);
        terrainComponent.terrainData = terrainData;
        terrainCollider.terrainData = terrainData;
        terrainCollider.material = material;
        terrainComponent.Flush();

        /** centre terrain and water plane */
        this.transform.position = new Vector3(-terrainData.size.x / 2, 0, -terrainData.size.z / 2);
        water.transform.localPosition = new Vector3(0, terrainData.size.y / 3, 0);
        water.transform.localScale = new Vector3(terrainData.size.x / 2, 0, terrainData.size.z / 2);

        /** position sun and boundary */
        sun.transform.localPosition = initialiseSun(terrainData.size.x / 2);
        setBoundary(boundary, terrainData.size.x);

    }

    SplatPrototype[] setTextures()
    {
        SplatPrototype[] splatPrototypes = new SplatPrototype[3];

        for (int i = 0; i < splatPrototypes.Length; i++)
        {
            splatPrototypes[i] = new SplatPrototype();
            splatPrototypes[i].texture = textures[i];
        }

        return splatPrototypes;
    }

    /**
     * Initialises and returns a procedurally generated height map
     */
    float[,] createHeightMap(int terrainSize)
    {
        float[,] heightMap = new float[terrainSize, terrainSize];

        //initialise corner values
        heightMap[0, 0] = (float)rnd.NextDouble();
        heightMap[0, terrainSize - 1] = (float)rnd.NextDouble();
        heightMap[terrainSize - 1, 0] = (float)rnd.NextDouble();
        heightMap[terrainSize - 1, terrainSize - 1] = (float)rnd.NextDouble();

        diamondSquareBFS(heightMap, terrainSize, terrainSize, 64.0f, 0, 0);

        return heightMap;
    }

    /** 
     * Calculates the radius of the sun's orbit about the terrain and returns its starting position 
     */
    Vector3 initialiseSun(float terrainWidth)
    {
        Vector3 position = new Vector3(terrainWidth, Mathf.Sqrt(2) * terrainWidth, terrainWidth);
        return position;
    }

    /**
     * Transforms boundary walls to correct positions
     */
     void setBoundary(GameObject boundary, float terrainWidth)
    {
        Transform[] transforms = boundary.GetComponentsInChildren<Transform>();

        //top
        transforms[1].localPosition = new Vector3(0, terrainWidth/2, 0);
        transforms[1].localScale = new Vector3(terrainWidth, 1, terrainWidth);
        //front
        transforms[2].localPosition = new Vector3(0, 0, terrainWidth/2);
        transforms[2].localScale = new Vector3(terrainWidth, terrainWidth, 1);
        //back
        transforms[3].localPosition = new Vector3(0, 0, -terrainWidth/2);
        transforms[3].localScale = new Vector3(terrainWidth, terrainWidth, 1);
        //left
        transforms[4].localPosition = new Vector3(-terrainWidth/2, 0, 0);
        transforms[4].localScale = new Vector3(1, terrainWidth, terrainWidth);
        //right
        transforms[5].localPosition = new Vector3(terrainWidth/2, 0, 0);
        transforms[5].localScale = new Vector3(1, terrainWidth, terrainWidth);
        //bottom
        transforms[6].localPosition = new Vector3(0, -terrainWidth/2, 0);
        transforms[6].localScale = new Vector3(terrainWidth, 1, terrainWidth);
    }

    /**
     * Assigns textures to terrain data
     * Author: Alastair Aitchison
     * Source: https://alastaira.wordpress.com/2013/11/14/procedural-terrain-splatmapping/
     * Accessed: 8/9/16
     * Modified: 8/9/16
     */
    void splatGen(TerrainData terrainData)
    {
        // Splatmap data is stored internally as a 3d array of floats, so declare a new empty array ready for your custom splatmap data:
        float[,,] splatmapData = new float[terrainData.alphamapWidth, terrainData.alphamapHeight, terrainData.alphamapLayers];

        for (int y = 0; y < terrainData.alphamapHeight; y++)
        {
            for (int x = 0; x < terrainData.alphamapWidth; x++)
            {
                // Normalise x/y coordinates to range 0-1 
                float y_01 = (float)y / (float)terrainData.alphamapHeight;
                float x_01 = (float)x / (float)terrainData.alphamapWidth;

                // Sample the height at this location (note GetHeight expects int coordinates corresponding to locations in the heightmap array)
                float height = terrainData.GetHeight(Mathf.RoundToInt(y_01 * terrainData.heightmapHeight), Mathf.RoundToInt(x_01 * terrainData.heightmapWidth));

                // Setup an array to record the mix of texture weights at this point
                float[] splatWeights = new float[terrainData.alphamapLayers];

                // CHANGE THE RULES BELOW TO SET THE WEIGHTS OF EACH TEXTURE ON WHATEVER RULES YOU WANT

                // Texture[0] increases with height but only on surfaces facing positive Z axis 
                splatWeights[0] = Mathf.Clamp01(height);

                // Texture[1] has constant influence
                splatWeights[1] = 0.3f;

                // Texture[2] is stronger at lower altitudes
                splatWeights[2] = Mathf.Clamp01((terrainData.heightmapHeight - height));

                // Sum of all textures weights must add to 1, so calculate normalization factor from sum of weights
                float z = 0f;
                for (int i = 0; i < splatWeights.Length; i++)
                    z += splatWeights[i];

                // Loop through each terrain texture
                for (int i = 0; i < terrainData.alphamapLayers; i++)
                {

                    // Normalize so that sum of all texture weights = 1
                    splatWeights[i] /= z;

                    // Assign this point to the splatmap array
                    splatmapData[x, y, i] = splatWeights[i];
                }
            }
        }

        // Finally assign the new splatmap to the terrainData:
        terrainData.SetAlphamaps(0, 0, splatmapData);
    }


    /** BFS Recursive Diamond Square Algorithm
     * 
     *  Author: Stack Exchange User "Jimmy" 
     *  http://gamedev.stackexchange.com/questions/37389/diamond-square-terrain-generation-problem
     *  Accessed: 31/8/16
     *  Modified: 31/8/16
     *  
     *  @param heightMap 2D height map array
     *  @param edgelength length in x and y directions between corner values
     *  @param startX x coordinate of top left cell of any "sub-square" within height map
     *  @param startY y coordinate of top left cell of any "sub-square" within height map 
     */
    private void diamondSquareBFS(float[,] heightMap, int edgeLength, int totalLength, float magnitude, int startX, int startY)
    {
        /** base case */
        if (edgeLength < 1) return;

        /** diamond step */
        for (int i = startX + edgeLength; i < totalLength; i += edgeLength)
        {
            for (int j = startY + edgeLength; j < totalLength; j += edgeLength)
            {
                // Calculate average
                float average = (heightMap[i - edgeLength, j - edgeLength] +
                    heightMap[i, j - edgeLength] +
                    heightMap[i - edgeLength, j] +
                    heightMap[i, j]) / 4.0f;

                heightMap[i - edgeLength / 2, j - edgeLength / 2] = (average + (float)rnd.NextDouble() * magnitude) % 1;
            }
        }

        /** square step */
        for (int i = startX + 2 * edgeLength; i < totalLength; i += edgeLength)
        {
            for (int j = startY + 2 * edgeLength; j < totalLength; j += edgeLength)
            {
                float topLeft = heightMap[i - edgeLength, j - edgeLength];
                float topRight = heightMap[i, j - edgeLength];
                float bottomLeft = heightMap[i - edgeLength, j];
                float bottomRight = heightMap[i, j];
                float centre = heightMap[i - edgeLength / 2, j - edgeLength / 2];

                // Calculate averages
                float left = ((topLeft + centre + bottomLeft) / 3.0f);
                float top = (topLeft + topRight + centre) / 3.0f;
                float right = (topRight + centre + bottomLeft) / 3.0f;
                float bottom = (bottomLeft + centre + bottomRight) / 3.0f;

                heightMap[i - edgeLength, j - edgeLength / 2] = (left + (float)rnd.NextDouble() * magnitude) % 1;
                heightMap[i - edgeLength / 2, j - edgeLength] = (top + (float)rnd.NextDouble() * magnitude) % 1;
                heightMap[i, j - edgeLength / 2] = (right + (float)rnd.NextDouble() * magnitude) % 1;
                heightMap[i - edgeLength / 2, j] = (bottom + (float)rnd.NextDouble() * magnitude) % 1;
            }
        }

        magnitude *= (float)System.Math.Pow(2, -smoothness);

        diamondSquareBFS(heightMap, edgeLength / 2, totalLength, magnitude, startX, startY);
    }
}