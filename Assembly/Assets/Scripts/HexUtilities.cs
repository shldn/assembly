using UnityEngine;
using System.Collections;

public class HexUtilities {

    // Converts hex coordinates to world coordinates.
    public static Vector3 HexToWorld(Triplet hexCoords){
        return new Vector3(
            hexCoords.x + (hexCoords.y * 0.5f) + (hexCoords.z * 0.5f),
            (hexCoords.y * Apothem) + (hexCoords.z * 0.288675f),
            (hexCoords.z * 0.816495f)
        );
    } // End of HexToWorld().

	public static Triplet WorldToHex(Vector3 worldCoords){
		Triplet hexCoords = Triplet.zero;
		hexCoords.z = Mathf.RoundToInt(worldCoords.z / 0.816495f);
		hexCoords.y = Mathf.RoundToInt((worldCoords.y - (0.288675f * (float)hexCoords.z)) / Apothem);
		hexCoords.x = Mathf.RoundToInt(worldCoords.x - ((float)hexCoords.y / 2f) - ((float)hexCoords.z / 2f));
		return hexCoords;
	} // End of WorldToHex().

    public static float Apothem{
        get{ return 0.8660254f; }
    } // End of Apothem.

    public static Triplet RandomAdjacent(){
        return Adjacent(Random.Range(0, 12));
    } // End of RandomAdjacent().

    public static Triplet Adjacent(int dir){
        if((dir < 0) || (dir > 11))
            return Triplet.zero;

        return Triplet.hexDirection[dir];
    } // End of Adjacent().

    public static Quaternion HexDirToRot(int dir){
        return Quaternion.LookRotation(HexToWorld(Triplet.hexDirection[dir % 12]));
    } // End of HexDirToRot().

	public static Triplet HexRotateAxis(Triplet hexPoint, int direction){
		Vector3 rotVectorEuc = HexToWorld(Adjacent(direction));
		Vector3 inputVectorEuc = HexToWorld(hexPoint);

		Quaternion rotQuat = Quaternion.FromToRotation(HexToWorld(Adjacent(0)), rotVectorEuc);

		Vector3 rotatedWorldCoord = rotQuat * inputVectorEuc;
		return WorldToHex(rotatedWorldCoord);
	} // End of HexRotateAxis

} // End of HexUtilities.
