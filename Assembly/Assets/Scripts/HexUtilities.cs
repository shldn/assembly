using UnityEngine;
using System.Collections;

public class HexUtilities {

    // Converts hex coordinates to world coordinates.
    public static Vector3 HexToWorld(IntVector3 hexCoords){
        return new Vector3(
            hexCoords.x + (hexCoords.y * 0.5f) + (hexCoords.z * 0.5f),
            (hexCoords.y * Apothem) + (hexCoords.z * 0.288675f),
            (hexCoords.z * 0.816495f)
        );
    } // End of HexToWorld().

    public static float Apothem{
        get{ return 0.8660254f; }
    } // End of Apothem.

    public static IntVector3 RandomAdjacent(){
        return Adjacent(Random.Range(0, 12));
    } // End of RandomAdjacent().

    public static IntVector3 Adjacent(int dir){
        if((dir < 0) || (dir > 11))
            return IntVector3.zero;

        return IntVector3.hexDirection[dir];
    } // End of Adjacent().

    public static Quaternion HexDirToRot(int dir){
        if((dir < 0) || (dir > 11))
            return Quaternion.identity;

        return Quaternion.LookRotation(HexToWorld(IntVector3.hexDirection[dir]));
    } // End of HexDirToRot().

} // End of HexUtilities.


public struct IntVector3 {

    public int x;
    public int y;
    public int z;

    public IntVector3(int x, int y, int z){
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public string ToString()
    {
        return "(" + x + "," + y + "," + z + ")";
    }
 
    public static IntVector3 operator +(IntVector3 a, IntVector3 b){
        return new IntVector3(a.x + b.x, a.y + b.y, a.z + b.z);
    } // End of +.

    public static IntVector3 operator -(IntVector3 a, IntVector3 b){
        return new IntVector3(a.x - b.x, a.y - b.y, a.z - b.z);
    } // End of -.

    public static bool operator ==(IntVector3 a, IntVector3 b){
        return (a.x == b.x) && (a.y == b.y) && (a.z == b.z);
    } // End of ==.

    public static bool operator !=(IntVector3 a, IntVector3 b){
        return (a.x != b.x) || (a.y != b.y) || (a.z != b.z);
    } // End of !=.

    int sqrMagnitude{
        get{ return x * x + y * y + z * z; }
    } // End of sqrMagnitude.


    public static float Distance(IntVector3 a, IntVector3 b){
        return Vector3.Distance(HexUtilities.HexToWorld(a), HexUtilities.HexToWorld(b));
    } // End of Distance().

    public static IntVector3 zero{
        get{ return new IntVector3(0, 0, 0); }
    } // End of zero{}.

    public static IntVector3 one{
        get{ return new IntVector3(1, 1, 1); }
    } // End of one{}.

    public static IntVector3[] hexDirection{
        get{ return new IntVector3[]{   new IntVector3(1, 0, 0),
                                        new IntVector3(0, 1, 0),
                                        new IntVector3(-1, 1, 0),
                                        new IntVector3(-1, 0, 0),
                                        new IntVector3(0, -1, 0),
                                        new IntVector3(1, -1, 0),
                                        new IntVector3(0, 0, 1),
                                        new IntVector3(-1, 0, 1),
                                        new IntVector3(0, -1, 1),
                                        new IntVector3(0, 0, -1),
                                        new IntVector3(1, 0, -1),
                                        new IntVector3(0, 1, -1) }; }
    } // End of directions{}.

} // End of IntVector3.