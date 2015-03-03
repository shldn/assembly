using UnityEngine;
using System.Collections;

public class TrilinearSample {
		/// <summary>
		/// Samples the data but does not check out of range conditions
		/// </summary>
		/// <returns>
		/// The sampled value
		/// </returns>
		/// <param name='data'>
		/// Data: array to sample from. Must be a 3D data set.
		/// </param>
		/// <param name='xyz'>
		/// The position in the 3D array to sample
		/// </param>
	public static float SampleRaw(float[][][] data, Vector3 xyz) {
//	    xyz.x = (Mathf.Repeat(xyz.x, data.Length-0.05f)); //Mathf.Max((xyz.x % data.Length-2), 0)); 
		int i = Mathf.FloorToInt(xyz.x);
		float u = xyz.x - i;
//		xyz.y = (Mathf.Repeat(xyz.y, data[i].Length-0.05f));
	    int j = Mathf.FloorToInt(xyz.y); //Mathf.Max((xyz.y % data[i].Length-2), 0)); 
		float v = xyz.y - j;
//		xyz.z = (Mathf.Repeat(xyz.z, data[i][j].Length-0.05f));;
	    int k = Mathf.FloorToInt(xyz.z); //Mathf.Max((xyz.z % data[i][j].Length-2), 0)); 
		float w = xyz.z - k;
		
//		Debug.Log(i + "," + j + "," + k);
		int i1 = (i+1) % data.Length; // (int)(Mathf.Repeat(i+1, data.Length));
		int j1 = (j+1) % data[i1].Length; //(int)(Mathf.Repeat(j+1, data[i].Length));
		int k1 = (k+1) % data[i1][j1].Length; //(int)(Mathf.Repeat(k+1, data[i][j].Length));
		
	    float a = data[ i][j][ k ];
	    float b = data[ i1][ j][ k ];
	    float c = data[ i][ j1][ k ];
	    float d = data[ i1][ j1][ k ];
	    float e = data[ i][ j][ k1 ];
	    float f = data[ i1][ j][ k1 ];
	    float g = data[ i][ j1][ k1 ];
	    float h = data[ i1][ j1][ k1 ];
		
		float invU = 1 - u;
		float invV = 1 - v;
		float invW = 1 - w;
		
		float Vxyz = a * (invU) * (invV) * (invW) +
				b * u * (invV) * (invW) +
				c * (invU) * v * (invW) +
				e * (invU) * (invV) * w +
				f * u * (invV) * w +
				g * (invU) * v * w +
				d * u * v * (invW) +
				h * u * v * w;
		return Vxyz;
	}
		/// <summary>
		/// Samples the data but does not check out of range conditions
		/// </summary>
		/// <returns>
		/// The sampled value
		/// </returns>
		/// <param name='data'>
		/// Data: array to sample from. Must be a 3D data set stored in a flattened array
		/// </param>
		/// <param name='dim'>
		/// Dimensions of the 3D array.
		/// </param>
		/// <param name='xyz'>
		/// The position in the 3D array to sample
		/// </param>
	public static float SampleRaw(float[] data, Triplet dim, Vector3 xyz) {
//	    xyz.x = (Mathf.Repeat(xyz.x, data.Length-0.05f)); //Mathf.Max((xyz.x % data.Length-2), 0)); 
		int i = Mathf.FloorToInt(xyz.x);
		float u = xyz.x - i;
//		xyz.y = (Mathf.Repeat(xyz.y, data[i].Length-0.05f));
	    int j = Mathf.FloorToInt(xyz.y); //Mathf.Max((xyz.y % data[i].Length-2), 0)); 
		float v = xyz.y - j;
//		xyz.z = (Mathf.Repeat(xyz.z, data[i][j].Length-0.05f));;
	    int k = Mathf.FloorToInt(xyz.z); //Mathf.Max((xyz.z % data[i][j].Length-2), 0)); 
		float w = xyz.z - k;
		
//		Debug.Log(i + "," + j + "," + k);
		int i1 = (i+1) % dim.x; // (int)(Mathf.Repeat(i+1, data.Length));
		int j1 = (j+1) % dim.y; //(int)(Mathf.Repeat(j+1, data[i].Length));
		int k1 = (k+1) % dim.z; //(int)(Mathf.Repeat(k+1, data[i][j].Length));
		
		int strideY = dim.z;
		int strideX = dim.y * strideY;
		float a = 0, b=0, c=0, d=0, e=0, f=0, g=0, h=0;
		try {
		    a = data[ i*strideX+j*strideY+ k ];
		    b = data[ i1*strideX+ j*strideY+ k ];
		    c = data[ i*strideX+ j1*strideY+ k ];
		    d = data[ i1*strideX+ j1*strideY+ k ];
		    e = data[ i*strideX+ j*strideY+ k1 ];
		    f = data[ i1*strideX+ j*strideY+ k1 ];
		    g = data[ i*strideX+ j1*strideY+ k1 ];
		    h = data[ i1*strideX+ j1*strideY+ k1 ];
		} catch (System.Exception e1) {
			Debug.LogError (e1.Message + " " + e1.StackTrace + ":" + dim + ", " + xyz + ", " + i + " " + j + " " + k + " " + i1 + " " + j1 + " " + k1);
		}
		
		float invU = 1 - u;
		float invV = 1 - v;
		float invW = 1 - w;
		
		float Vxyz = a * (invU) * (invV) * (invW) +
				b * u * (invV) * (invW) +
				c * (invU) * v * (invW) +
				e * (invU) * (invV) * w +
				f * u * (invV) * w +
				g * (invU) * v * w +
				d * u * v * (invW) +
				h * u * v * w;
		return Vxyz;
	}
	public static float SampleRaw(byte[][][] data, Vector3 xyz) {
//	    xyz.x = (Mathf.Repeat(xyz.x, data.Length-0.05f)); //Mathf.Max((xyz.x % data.Length-2), 0)); 
		int i = Mathf.FloorToInt(xyz.x);
		float u = xyz.x - i;
//		xyz.y = (Mathf.Repeat(xyz.y, data[i].Length-0.05f));
	    int j = Mathf.FloorToInt(xyz.y); //Mathf.Max((xyz.y % data[i].Length-2), 0)); 
		float v = xyz.y - j;
//		xyz.z = (Mathf.Repeat(xyz.z, data[i][j].Length-0.05f));;
	    int k = Mathf.FloorToInt(xyz.z); //Mathf.Max((xyz.z % data[i][j].Length-2), 0)); 
		float w = xyz.z - k;
		
//		Debug.Log(i + "," + j + "," + k);
		int i1 = (i+1) % data.Length; // (int)(Mathf.Repeat(i+1, data.Length));
		int j1 = (j+1) % data[i1].Length; //(int)(Mathf.Repeat(j+1, data[i].Length));
		int k1 = (k+1) % data[i1][j1].Length; //(int)(Mathf.Repeat(k+1, data[i][j].Length));
		
	    byte a = data[ i][j][ k ];
	    byte b = data[ i1][ j][ k ];
	    byte c = data[ i][ j1][ k ];
	    byte d = data[ i1][ j1][ k ];
	    byte e = data[ i][ j][ k1 ];
	    byte f = data[ i1][ j][ k1 ];
	    byte g = data[ i][ j1][ k1 ];
	    byte h = data[ i1][ j1][ k1 ];
		
		float invU = 1 - u;
		float invV = 1 - v;
		float invW = 1 - w;
		
		float Vxyz = a * (invU) * (invV) * (invW) +
				b * u * (invV) * (invW) +
				c * (invU) * v * (invW) +
				e * (invU) * (invV) * w +
				f * u * (invV) * w +
				g * (invU) * v * w +
				d * u * v * (invW) +
				h * u * v * w;
		return Vxyz;
	}
	
	public static float Sample(float[][][] data, Vector3 xyz) {
		while (xyz.x < 0)
			xyz.x += data.Length;
		xyz.x = xyz.x % (data.Length); //Mathf.Repeat(xyz.x, data.Length-0.1f);
		while (xyz.y < 0)
			xyz.y += data[0].Length;
		xyz.y = xyz.y % (data[0].Length); //Mathf.Repeat(xyz.y, data[0].Length-0.1f);
		while (xyz.z < 0)
			xyz.z += data[0][0].Length;
		xyz.z = xyz.z % (data[0][0].Length); //Mathf.Repeat(xyz.z, data[0][0].Length-0.1f);
		return SampleRaw(data, xyz);
	}
	public static float Sample(float[] data, Triplet dim, Vector3 xyz) {
		while (xyz.x < 0)
			xyz.x += dim.x;
		xyz.x = xyz.x % (dim.x); //Mathf.Repeat(xyz.x, data.Length-0.1f);
		while (xyz.y < 0)
			xyz.y += dim.y;
		xyz.y = xyz.y % (dim.y); //Mathf.Repeat(xyz.y, data[0].Length-0.1f);
		while (xyz.z < 0)
			xyz.z += dim.z;
		xyz.z = xyz.z % (dim.z); //Mathf.Repeat(xyz.z, data[0][0].Length-0.1f);
		return SampleRaw(data, dim, xyz);
	}
	
	/// <summary>
	/// Samples the data but will return defaultValue if xyz is out of range of data array.
	/// </summary>
	/// <returns>
	/// The sampled value.
	/// </returns>
	/// <param name='data'>
	/// Data: array to sample from.
	/// </param>
	/// <param name='xyz'>
	/// Xyz: location to sample at.
	/// </param>
	/// <param name='defaultValue'>
	/// Default value.
	/// </param>
	public static float SampleClipped(float[][][] data, Vector3 xyz, float defaultValue = 0) {
		if (xyz.x < 0 || xyz.y < 0 || xyz.z < 0 || xyz.x > data.Length-1 || xyz.y > data[0].Length-1 || xyz.z > data[0][0].Length-1)
			return defaultValue;
		else return SampleRaw(data, xyz);	// the Repeat calls will now be redundant, but probably not worth duplicating code to eliminate in this case.
	}	
	public static float SampleClipped(byte[][][] data, Vector3 xyz, float defaultValue = 0) {
		if (xyz.x < 0 || xyz.y < 0 || xyz.z < 0 || xyz.x > data.Length-1 || xyz.y > data[0].Length-1 || xyz.z > data[0][0].Length-1)
			return defaultValue;
		else return SampleRaw(data, xyz);	// the Repeat calls will now be redundant, but probably not worth duplicating code to eliminate in this case.
	}
	public static float SampleClipped(float[] data, Triplet dim, Vector3 xyz, float defaultValue = 0) {
		if (xyz.x < 0 || xyz.y < 0 || xyz.z < 0 || xyz.x > dim.x-1 || xyz.y > dim.y-1 || xyz.z > dim.z-1)
			return defaultValue;
		else return SampleRaw(data, dim, xyz);	// the Repeat calls will now be redundant, but probably not worth duplicating code to eliminate in this case.
	}
	
	private static float SampleWrapped(float[][][] data, int x, int y, int z) {
		x = (int)Mathf.PingPong(x, data.Length);
		y = (int)Mathf.PingPong(y, data[x].Length);
		return data[x][y][(int)Mathf.PingPong(z, data[x][y].Length)];
	}
	
	public static Vector3 SampleRaw(Vector3[][][] data, Vector3 xyz) {
		int i = Mathf.FloorToInt(xyz.x);
		float u = xyz.x - i;
		
	    int j = Mathf.FloorToInt(xyz.y); //Mathf.Max((xyz.y % data[i].Length-2), 0)); 
		float v = xyz.y - j;
		
	    int k = Mathf.FloorToInt(xyz.z); //Mathf.Max((xyz.z % data[i][j].Length-2), 0)); 
		float w = xyz.z - k;
		
		int i1 = (i+1) % data.Length; // (int)(Mathf.Repeat(i+1, data.Length));
		int j1 = (j+1) % data[i1].Length; //(int)(Mathf.Repeat(j+1, data[i].Length));
		int k1 = (k+1) % data[i1][j1].Length; //(int)(Mathf.Repeat(k+1, data[i][j].Length));
		
	    Vector3 a = data[ i][j][ k ];
	    Vector3 b = data[ i1][ j][ k ];
	    Vector3 c = data[ i][ j1][ k ];
	    Vector3 d = data[ i1][ j1][ k ];
	    Vector3 e = data[ i][ j][ k1 ];
	    Vector3 f = data[ i1][ j][ k1 ];
	    Vector3 g = data[ i][ j1][ k1 ];
	    Vector3 h = data[ i1][ j1][ k1 ];
		
		float invU = 1 - u;
		float invV = 1 - v;
		float invW = 1 - w;
		
		Vector3 Vxyz = a * (invU) * (invV) * (invW) +
				b * u * (invV) * (invW) +
				c * (invU) * v * (invW) +
				e * (invU) * (invV) * w +
				f * u * (invV) * w +
				g * (invU) * v * w +
				d * u * v * (invW) +
				h * u * v * w;
		return Vxyz;
	}
}
