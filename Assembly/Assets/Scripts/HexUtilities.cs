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
        if((dir < 0) || (dir > 11))
            return Quaternion.identity;

        return Quaternion.LookRotation(HexToWorld(Triplet.hexDirection[dir]));
    } // End of HexDirToRot().

} // End of HexUtilities.


/*
public struct Triplet {

    public int x;
    public int y;
    public int z;

    public Triplet(int x, int y, int z){
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public string ToString()
    {
        return "(" + x + "," + y + "," + z + ")";
    }
 
    public static Triplet operator +(Triplet a, Triplet b){
        return new Triplet(a.x + b.x, a.y + b.y, a.z + b.z);
    } // End of +.

    public static Triplet operator -(Triplet a, Triplet b){
        return new Triplet(a.x - b.x, a.y - b.y, a.z - b.z);
    } // End of -.

    public static bool operator ==(Triplet a, Triplet b){
        return (a.x == b.x) && (a.y == b.y) && (a.z == b.z);
    } // End of ==.

    public static bool operator !=(Triplet a, Triplet b){
        return (a.x != b.x) || (a.y != b.y) || (a.z != b.z);
    } // End of !=.

    int sqrMagnitude{
        get{ return x * x + y * y + z * z; }
    } // End of sqrMagnitude.


    public static float Distance(Triplet a, Triplet b){
        return Vector3.Distance(HexUtilities.HexToWorld(a), HexUtilities.HexToWorld(b));
    } // End of Distance().

    public static Triplet zero{
        get{ return new Triplet(0, 0, 0); }
    } // End of zero{}.

    public static Triplet one{
        get{ return new Triplet(1, 1, 1); }
    } // End of one{}.

    public static Triplet[] hexDirection{
        get{ return new Triplet[]{   new Triplet(1, 0, 0),
                                        new Triplet(0, 1, 0),
                                        new Triplet(-1, 1, 0),
                                        new Triplet(-1, 0, 0),
                                        new Triplet(0, -1, 0),
                                        new Triplet(1, -1, 0),
                                        new Triplet(0, 0, 1),
                                        new Triplet(-1, 0, 1),
                                        new Triplet(0, -1, 1),
                                        new Triplet(0, 0, -1),
                                        new Triplet(1, 0, -1),
                                        new Triplet(0, 1, -1) }; }
    } // End of directions{}.

} // End of Triplet.
*/