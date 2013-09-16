using UnityEngine;
using System.Collections;
using Vectrosity;


public class GraphicsManager : MonoBehaviour {

    public Material lineMaterial;
    public Texture2D frontTex;
    public Texture2D endTex;

    void Awake()
    {
        // Set up line types.
        VectorLine.SetEndCap("CalorieCaps", EndCap.Back, lineMaterial, endTex);
    } // End of Awake().
}
