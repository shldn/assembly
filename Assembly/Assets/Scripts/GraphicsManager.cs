using UnityEngine;
using System.Collections;


public class GraphicsManager : MonoBehaviour {

    public Material lineMaterial;
    public Texture2D frontTex;
    public Texture2D endTex;

    public Texture2D senseFlare;
    public Texture2D controlFlare;
    public Texture2D muscleFlare;

    // Texture (crosshair) shown around selected ndoes.
    public Texture2D nodeSelectTex;
    public Texture2D nodeModifyTex;

    public Mesh twoPolyPlane;
    public Material senseArcMat;

    // Bond textures
    public Material signalBondMat;
    public Material synapseBondMat;
    public Material synapsePropBondMat;
    public Material thrustPropBondMat;
    public Material uselessBondMat;

} // End of GraphicsManager.