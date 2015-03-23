using UnityEngine;
using System.Collections;

public class SplatMapMeshPlacer : MonoBehaviour {
    public Texture2D map;
    public TerrainData tData;
    public Transform terrainTransform;
    public GameObject[] placer;
    public Vector3 placerScale = Vector3.one;
    public bool randomOffset = true;
    public float pixelValueCutoff = 0.7f;
    

	void Start () {

        if (map.width != map.height)
            Debug.LogError("Please use a square texture for SplatMapMeshPlacer!");

        if( placer.Length == 0)
        {
            Debug.LogError("Must add meshes to the placer array, to have something to place :)");
            return;
        }

        for(int i=0; i < map.width; ++i)
        {
            for(int j=0; j < map.height; ++j)
            {
                if (map.GetPixel(i, j).r > pixelValueCutoff)
                {
                    GameObject meshObj = Instantiate(placer[Random.Range(0,placer.Length)]) as GameObject;
                    meshObj.transform.position = GetMap3DPositionOnTerrain(i, j, map.width);
                    meshObj.transform.localScale = placerScale;
                    meshObj.transform.parent = this.transform;
                }
            }
        }
	}

    Vector3 GetMap3DPositionOnTerrain(int x, int y, int mapWidth)
    {
        float mapWidthReciprical = 1.0f / (float)mapWidth;
        float xPercent = (float)x * mapWidthReciprical;
        float yPercent = (float)y * mapWidthReciprical;
        if( randomOffset )
        {
            xPercent += Random.Range(0.0f, 0.5f * mapWidthReciprical);
            yPercent += Random.Range(0.0f, 0.5f * mapWidthReciprical);
        }
        float tHeight = tData.GetHeight((int)(xPercent * tData.heightmapWidth), (int)(yPercent * tData.heightmapWidth));
        return terrainTransform.position + xPercent * tData.heightmapWidth * tData.heightmapScale.x * Vector3.right + yPercent * tData.heightmapWidth * tData.heightmapScale.z * Vector3.forward + tHeight * Vector3.up;
    }
}
