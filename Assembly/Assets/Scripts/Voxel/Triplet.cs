using UnityEngine;
using System.Collections;
using System;

[Serializable]
/// <summary>
/// Triplet: a simple structure representing 3 integers, ostensibly an integer variant of Unity's Vector3
/// </summary>
public class Triplet {
	public int x, y, z;
			
	public Triplet(int _x, int _y, int _z) {
		x = _x;
		y = _y;
		z = _z;
	}
		
	public Triplet(Vector3 vec) {
		x = Mathf.RoundToInt(vec.x);
		y = Mathf.RoundToInt(vec.y);
		z = Mathf.RoundToInt(vec.z);
	}
		
	public Triplet(Triplet source) {
		x = source.x;
		y = source.y;
		z = source.z;
	}
		
	public override String ToString() {
		return "(" + x + "," + y + "," + z + ")";
	}
	public static Triplet operator + (Triplet a, Triplet b) {
		Triplet ret = new Triplet(a);
		ret.x += b.x;
		ret.y += b.y;
		ret.z += b.z;
		return ret;
	}
	public static Triplet operator - (Triplet a, Triplet b) {
		Triplet ret = new Triplet(a);
		ret.x -= b.x;
		ret.y -= b.y;
		ret.z -= b.z;
		return ret;
	}
	public static Triplet operator * (Triplet a, int b) {
		Triplet ret = new Triplet(a);
		ret.x *= b;
		ret.y *= b;
		ret.z *= b;
		return ret;
	}
	public static bool operator != (Triplet a, Triplet b) {
		return !(a == b);
	}
	public static bool operator == (Triplet a, Triplet b) {
		if ( ( ((object)a == null) || ((object)b == null) ) && ((object)a != null) || ((object)b != null) )
			return false;
		else if ((object)a == null)
			return true;
		return a.Equals(b);
	}
		
	public int sqrMagnitude {
		get {
			return (x * x + y * y + z * z);
		}
	}
	public float magnitude {
		get {
			return Mathf.Sqrt(sqrMagnitude);
		}
	}
		
	public static Triplet one {
		get {
			return new Triplet(1,1,1);
		}
	}
	public static Triplet zero {
		get {
			return new Triplet(0,0,0);
		}
	}
	public override bool Equals(object obj)
	{		
		if( System.Object.ReferenceEquals(obj, null) || this.GetType() != obj.GetType())  
			return false;
		Triplet other = (Triplet)obj;
		return this.Equals(other);
	}
		
	public bool Equals(Triplet other)
	{		
		if (!System.Object.ReferenceEquals(other,null))
		{
		    return this.x.Equals(other.x) && this.y.Equals(other.y) && this.z.Equals(other.z);
		} else
			return false;
 
	}
//		public override bool Equals(System.Object obj)
//	    {
//	        // If parameter is null return false.
//	        if (obj == null)
//	        {
//	            return false;
//	        }
//	
//	        // If parameter cannot be cast to Point return false.
//	        Triplet p = obj as Triplet;
//	        if ((System.Object)p == null)
//	        {
//	            return false;
//	        }
//	
//	        // Return true if the fields match:
//	        return this == p;
//	    }
//	
//	    public bool Equals(Triplet p)
//	    {
//	        // If parameter is null return false:
//	        if ((object)p == null)
//	        {
//	            return false;
//	        }
//	
//	        // Return true if the fields match:
//	        return this == p;
//	    }
	
	public override int GetHashCode()
	{
	    return (x ^ y) ^ z;
	}
		
	public Vector3 ToVector3() {
		return new Vector3(x, y, z);
	}

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
}
