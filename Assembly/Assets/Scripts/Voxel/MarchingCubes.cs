using UnityEngine;
using System;						// for Array.Copy
using System.Collections;
using System.Collections.Generic;	// include for List<>
using System.Threading;

/// <summary>
/// Marching cubes: Implementation of the classic algorithm for Unity. Requires helper classes with data sets to opperate.
/// 	Part of Ruaumoko for Unity 3D. Not to be redistributed without explicit permission.
/// 	Created: BDS Feb, 2013.
/// 
/// How To Use:
/// 	1) Access the singleton through MarchingCubes.Singleton
/// 	2) Pass in a 3D jagged array of density values (floats, use SetDensityMap()) representing the mesh you want to construct
/// 		Density values < 0 are considered outside the mesh, and density values > 0 are inside the mesh. (0 = surface threshold)
/// 		The property "surfaceThreshold" determines the cross over point where the geometry will be constructed, and is typically left at 0.
/// 	2b) (optionally) set the gutter size. This is how many extra values you have supplied around the edges of your array. This must be > 0
/// 		If you intend to calculate Ambient Occlusion values (see below) then this is also the number of steps the algorithm will take and
/// 		higher values will produce noticeably better results (gutter size 4 or greater is typically good).
/// 	3) Call "March" (or MarchCoroutine for the asynchronous non-blocking version).
/// 	3b) (optionally) Call GenerateAmbientOcclusion if you will be using an ambient occlusion shader.
/// 	4) Call "UpdateMesh", passing in the mesh you want to hold the geometry.
/// and that's it!
/// 
/// Utility Methods:
/// 	GenerateAmbientOcclusion - calculates ambient occlusion values based on the folding of the surface, storing the results in the alpha
/// 		component of the vertex colors. This can then be used by Ambient Occlusion shaders.
/// 
/// 	CenterAndScaleVerts - transforms the generated vertices to [-1,1] range, rather than 0-(density map length). This allows the transform
/// 		of the mesh to be prime determinant in the overall scale of the object.
/// </summary>
public class MarchingCubes {
	/// <summary>
		/// Constant epsilon: how many units away to sample for normal calculations. Higher is smoother, lower is sharper.
		/// </summary>
	const float epsilon = 0.5f;
	/// <summary>
	/// The surface threshold. Values in the density field less than this are considered outside the surface and
	/// values greater than this are considered inside the surface.
	/// </summary>
//		public static float surfaceThreshold = 0f;		// NOW set in MarchingCubesData and associated with the specific scalar field
	/// <summary>
	/// The number of extra voxels you are supplying in the density field. These will be used to improve the mesh
	/// but will be trimmed off when the final geometry is generated. Must be 1 or higher.
	/// </summary>
	public int GutterSize {
		set {
			if (value >= 0)
				gutterSize = value;
			if (value == 1)		// TODO: fix the bug that crashes with gutterSize 1. But 0, 2, etc. all work!
				gutterSize = 2;
		}
		get {
			return gutterSize;
		}
	}
	protected int gutterSize = 2;
	/// <summary>
	/// Calculate Normals: will generate highquality normals using the surface gradient of the density field. 
	/// However this process is costly and Mesh.RecalculateNormals() is much cheaper so if that is satisfactory
	/// then set this to false and use Unity's version. The primary use of calcNormals is for paged meshes that
	/// need seamless normals between segments.
	/// </summary>
	public bool calcNormals = true;
	/// <summary>
	/// Smoothness: if set to 1 it causes smooth isosurfaces. If set to 0 it will make mine-craft-esque cubes.
	/// </summary>
	public static float Smoothness = 1f;
	/// <summary>
	/// The density map: holds our "map"-the density of each cell. Each point is a "voxel"
	/// </summary>
//		protected float[][][] densityMap;
	/// <summary>
	/// The cube case: this will indicate which verts of each cube are in/out of the surface
	/// </summary>
	protected int[][][] cubeCase;
	/// <summary>
	/// holds the vertex IDs for each edge that will have a vertex on it.
	/// </summary>
	protected Triplet[][][] vertIDs;
	/// <summary>
	/// The dimensions of the block that we should currently be working on.
	/// </summary>
	protected Triplet workingDimensions;
	/// <summary>
	/// Flag indicating that Marching Cubes needs recalculation.
	/// </summary>
	protected bool needRecalculation;
		
	protected Triplet stride;
	protected float[] densityField;
	protected byte[] densityBytes;
			
	private bool working = false;
	public bool Working {
		get {
			return working;
		}
	}		
	#region WorkingArrays
	protected Vector3[] vertices;
	protected Vector2[] uvs;
	protected Vector3[] normals;
	protected int[] triangles;
	protected Color[] colors;
		
	protected int newVerticesCounter;
	protected Vector3[] newVerts;	// Q: how much of a performance hit does the List incur?
	protected Vector2[] newUVs;		// A: almost nothing, only allocation takes extra time.
	protected Vector3[] newNormals;
	protected List<int> newTriangles;
	#endregion WorkingArrays
			
	#region Constructor
	/// <summary>
	/// We use the singleton pattern because allocation is a significant factor, especially if we are marching every
	/// frame over many different meshes. We need to preallocate the arrays to save time on subsequent calls.
	/// </summary>
	protected static List<MarchingCubes> singleton;
	public MarchingCubes() {
		newVerts = new Vector3[65535];	// preallocate working space at maximum # of allowed vertices and triangles.
		newUVs = new Vector2[65535];
		newNormals = new Vector3[65535];
		newTriangles = new List<int>(196608);
			
		densityField = null;
		densityBytes = null;
			
		colors = null;
	}
	public static MarchingCubes Singleton {
		get {
			if (singleton == null)
				singleton = new List<MarchingCubes>();
			if (singleton.Count < 1)
				singleton.Add(new MarchingCubes());
			return singleton[0];
		}
	}
	public static MarchingCubes Instance(int index) {
		if (singleton == null)
			singleton = new List<MarchingCubes>();
		while (singleton.Count < index)
			singleton.Add(new MarchingCubes());
		return singleton[index];
	}
	#endregion Constructor
		
//		/// <summary>
//		/// Deprecated! Get/set DensityMap directly.
//		/// </summary>
//		public void SetDensityMap(float[][][] newDensity) {
//				DensityMap = newDensity;
//		}
//		/// <summary>
//		/// Gets or sets the density map. Must be done before the algorithm can produce any geometry!
//		/// </summary>
//		/// <value>
//		/// The density map.
//		/// </value>
//		public float[][][] DensityMap {
//			get {
//				return densityMap;
//			}
//			set {
//				if (densityMap != value) {
//					densityMap = value;
//				if (value != null) {
//						needRecalculation = true;
//						workingDimensions = new Triplet(densityMap.Length, densityMap[0].Length, densityMap[0][0].Length);
//					}
//				}
//			}
//		}
		
		
	/// <summary>
	/// Sets the density field/map data before March will be called.
	/// </summary>
	/// <param name='newDensity'>
	/// New density map/field.
	/// </param>
	/// <param name='dim'>
	/// The 3D dimensions of the data, encoded in a single dimensional array. Data must be dim.x*dim.y*dim.z or errors will arrise.
	/// </param>
	public void SetDensityMap(float[] newDensity, Triplet dim) {
		densityField = newDensity;
		workingDimensions = dim;
		stride = new Triplet(workingDimensions.z * workingDimensions.y, workingDimensions.z, 1);
	}
	public void SetDensityMap(float[][][] newDensity) {
		if (newDensity == null)
			return;
			
		workingDimensions = new Triplet(newDensity.Length, newDensity[0].Length, newDensity[0][0].Length);
			
		int size = (newDensity.Length * newDensity[0].Length * newDensity[0][0].Length);
		if (densityField == null || densityField.Length != size)
			densityField = new float[size];
			
		stride = new Triplet(workingDimensions.z * workingDimensions.y, workingDimensions.z, 1);
			
		int pointer = 0;
		for (int i = 0; i < workingDimensions.x; i++) {
			for (int j = 0; j < workingDimensions.y; j++) {
				Array.Copy(newDensity[i][j], 0, densityField, pointer, workingDimensions.z);
				pointer += workingDimensions.z;
			}
		}
	}
//		public void SetDensityMap(byte[][][] newDensity) {
//			if (newDensity == null)
//				return;
//			
//			workingDimensions = new Triplet(newDensity.Length, newDensity[0].Length, newDensity[0][0].Length);
//			
//			int size = (newDensity.Length * newDensity[0].Length * newDensity[0][0].Length);
//			if (densityField == null || densityField.Length != size)
//				densityField = new byte[size];
//			
//			stride = new Triplet(workingDimensions.z * workingDimensions.y, workingDimensions.z, 1);
//			
//			int pointer = 0;
//			for (int i = 0; i < workingDimensions.x; i++) {
//				for (int j = 0; j < workingDimensions.y; j++) {
//					Array.Copy(newDensity[i][j], 0, densityField, pointer, workingDimensions.z);
//					pointer += workingDimensions.z;
//				}
//			}
//		}
	/// <summary>
	/// Gets or sets the density map. Must be done before the algorithm can produce any geometry!
	/// </summary>
	/// <value>
	/// The density map.
	/// </value>
	public float[] DensityMap {
		set {
			if (densityField != value) {
				densityField = value;
				if (value != null) {
					needRecalculation = true;
				}
			}
		}
	}
	/// <summary>
	/// Sets the dimensions of the marching cubes block. Enough data must be present in the density map!
	/// </summary>
	/// <value>
	/// The dimensions of the data.
	/// </value>
	public Triplet Dimensions {
		set {
			workingDimensions = value;
			stride.z = 1;
			stride.y = workingDimensions.z;
			stride.x = stride.y * workingDimensions.y;
		}
		get {
			return workingDimensions;
		}
	}
	
	/// <summary>
	/// Updates the mesh with the calculated geometry.
	/// </summary>
	/// <param name='mesh'>
	/// Mesh to receive the new geometry.
	/// </param>
	/// <returns>
	/// True if collider needs to be updated, false if not.
	/// </returns>
	public bool UpdateMesh(Mesh mesh) {
		if (mesh != null) {
			mesh.Clear();
				
			if (vertices == null || vertices.Length < 3)
				return false;	// bail if we have no geometry.
				
			mesh.vertices = vertices;
			mesh.uv = uvs;
//			mesh.uv2 = uv2s;
			if (colors != null && colors.Length == vertices.Length)
				mesh.colors = colors;
			else
				mesh.colors = null;
			if (calcNormals)
				mesh.normals = normals;	// our normals consider the density field extending around the edges, not just the new tri's
				
			mesh.triangles = triangles;
			//Debug.Log(mesh.vertices.Length + ", " + mesh.triangles.Length);
//				mesh.RecalculateBounds();		// assigning triangles automatically triggers this
			return true;
		} else
			Debug.LogWarning("MarchingCubes: mesh to update is null!");
		return false;
	}
	public static bool UpdateMesh(ref MarchingCubesData mcd, ref Mesh mesh) {
		try {
			if (mesh != null) {
				mesh.Clear();
					
				if (mcd.vertices == null || mcd.vertices.Count < 3)
					return false;	// bail if we have no geometry.
					
				mesh.vertices = mcd.vertices.ToArray();
				mesh.uv = mcd.uvs.ToArray();
	//			mesh.uv2 = uv2s;
				if (mcd.colors != null && mcd.colors.Count == mcd.vertices.Count)
					mesh.colors = mcd.colors.ToArray();
				else
					mesh.colors = null;
				if (mcd.CalculateNormals)
					mesh.normals = mcd.normals.ToArray();	// our normals consider the density field extending around the edges, not just the new tri's
					
//					Debug.Log ("UpdateMesh with " + mesh.vertexCount + " verts, " + mcd.triangles.Count + " tris");
				mesh.triangles = mcd.triangles.ToArray();
				return true;
			} else
				Debug.LogWarning("MarchingCubes: mesh to update is null!");
		} catch (System.Exception e1) {
			Debug.LogError (e1.Message + " " + e1.StackTrace);
		}
			
		return false;	
	}		
	/// <summary>
	/// Marches the cubes through the density field, generating geometry. This is the non-blocking asynchronous version. Start as a coroutine!
	/// </summary>
	virtual public IEnumerator MarchCoroutine(float surfaceThreshold = 0f) {
		if (densityField == null) {
			throw new Exception("DensityMap must be supplied before calling March.");
		} else {				
			if (densityField.Length < Dimensions.x * Dimensions.y * Dimensions.z)
				throw new Exception("DensityMap dimensions are too small");
			MarchingCubesData mcd = MarchingCubesDataFactory.Instance.pop();
			mcd.SetDensityMap(densityField, workingDimensions);
			mcd.surfaceThreshold = surfaceThreshold;
				
			CalcCubeCases(ref mcd);
			yield return new WaitForSeconds(0.0001f);
			CalcVertIDs(ref mcd);
			yield return new WaitForSeconds(0.0001f);
			GenerateGeometry(ref mcd);
			yield return new WaitForSeconds(0.0001f);
				
			vertices = mcd.vertices.ToArray();
			normals = mcd.normals.ToArray();
			uvs = mcd.uvs.ToArray();
			triangles = mcd.triangles.ToArray();
				
			MarchingCubesDataFactory.Instance.push(mcd);
		}
			
//			if (densityField == null) {
//				throw new Exception("DensityMap must be supplied before calling March.");
//			} else {				
//				if (densityField.Length < Dimensions.x * Dimensions.y * Dimensions.z)
//					throw new Exception("DensityMap dimensions are too small");
//				
//				bool jobsDone = false;
//				Loom.RunAsync(()=>{
//					CheckWorkingArrays();
////					yield return new WaitForSeconds(0.0001f);
//					CalcCubeCases(null);
////					yield return new WaitForSeconds(0.0001f);
//					CalcVertIDs();
////					yield return new WaitForSeconds(0.0001f);
//					GenerateGeometry();
//					jobsDone = true;
//				});
//				while (!jobsDone)
//					yield return new WaitForSeconds(0.0001f);
//			}
		yield break;
	}
	/// <summary>
	/// Marches the cubes through the density field, generating geometry.
	/// </summary>
//		private bool jobsDone;
	public virtual void March(float surfaceThreshold = 0f) {
		if (densityField == null) {
			throw new Exception("DensityMap must be supplied before calling March.");
		} else {				
			if (densityField.Length < Dimensions.x * Dimensions.y * Dimensions.z)
				throw new Exception("DensityMap dimensions are too small");
//				bool jobsDone = false;
//				Loom.RunAsync(()=>{
			MarchingCubesData mcd = MarchingCubesDataFactory.Instance.pop();
			mcd.SetDensityMap(densityField, workingDimensions);
			mcd.gutterSize = gutterSize;
			mcd.surfaceThreshold = surfaceThreshold;
				
//					CheckWorkingArrays();
			CalcCubeCases(ref mcd);
			CalcVertIDs(ref mcd);
			GenerateGeometry(ref mcd);
				
			vertices = mcd.vertices.ToArray();
			normals = mcd.normals.ToArray();
			uvs = mcd.uvs.ToArray();
			triangles = mcd.triangles.ToArray();
				
			MarchingCubesDataFactory.Instance.push(mcd);
//					jobsDone = true;
//				});
//				while (!jobsDone)
//					;
		}
	}
	public static bool March(float[] densityMap, Triplet Dimensions, ref Mesh mesh, int gutterSize = 0, bool GenAmbOcclusion = false, float surfaceThreshold = 0f) {
		if (densityMap == null) {
			throw new Exception("DensityMap must be supplied before calling March.");
		} else {				
			if (densityMap.Length < Dimensions.x * Dimensions.y * Dimensions.z)
				throw new Exception("DensityMap dimensions are too small");
//				bool jobsDone = false;
//				Loom.RunAsync(()=>{
			MarchingCubesData mcd = MarchingCubesDataFactory.Instance.pop();
			mcd.SetDensityMap(densityMap, Dimensions);
			mcd.gutterSize = gutterSize;
			mcd.surfaceThreshold = surfaceThreshold;
			
//					CheckWorkingArrays();
			CalcCubeCases(ref mcd);
			CalcVertIDs(ref mcd);
			GenerateGeometry(ref mcd);
			
			if (GenAmbOcclusion)
				GenerateAmbientOcculsion(ref mcd);
							
			bool ret = UpdateMesh(ref mcd, ref mesh);
				
			MarchingCubesDataFactory.Instance.push(mcd);
			return ret;
//					jobsDone = true;
//				});
//				while (!jobsDone)
//					;
		}
	}
	public static bool March(byte[] densityMap, Triplet Dimensions, ref Mesh mesh, int gutterSize = 0, bool GenAmbOcclusion = false, byte surfaceThreshold = 128) {
		if (densityMap == null) {
			throw new Exception("DensityMap must be supplied before calling March.");
		} else {				
			if (densityMap.Length < Dimensions.x * Dimensions.y * Dimensions.z)
				throw new Exception("DensityMap dimensions are too small");
//				bool jobsDone = false;
//				Loom.RunAsync(()=>{
			MarchingCubesByte mcd = MarchingCubesDataFactory.Instance.pop<byte>() as MarchingCubesByte;
			((MarchingCubesByte)mcd).SetDensityMap(densityMap, Dimensions);
			mcd.gutterSize = gutterSize;
			mcd.surfaceThreshold = surfaceThreshold;
			
//					CheckWorkingArrays();
			CalcCubeCases(ref mcd);
			MarchingCubesData mcd_cast = mcd as MarchingCubesData;
			CalcVertIDs(ref mcd_cast);
			GenerateGeometry(ref mcd_cast);
			
			if (GenAmbOcclusion)
				GenerateAmbientOcculsion(ref mcd_cast);
							
			bool ret = UpdateMesh(ref mcd_cast, ref mesh);
				
			MarchingCubesDataFactory.Instance.push(mcd as MarchingCubesData);
			return ret;
//					jobsDone = true;
//				});
//				while (!jobsDone)
//					;
		}
	}
	/// <summary>
	/// Marchs the cubes in a seperate thread. <c>Callback</c> is executed on the main thread after Marching is complete.
	/// </summary>
	/// <param name='Callback'>
	/// Callback to be executed on the main thread after marching is done.
	/// </param>
	/// <exception cref='Exception'>
	/// Throws an error if DensityMap has not been set prior to calling MarchThreaded
	/// </exception>
//		private static Semaphore marchMutex = new Semaphore(1,1);
	public delegate void MarchCallback(ref Mesh mesh);
	public static void MarchThreaded(float[] densityMap, Triplet Dimensions, Mesh mesh, MarchCallback Callback, int gutterSize = 0, bool GenAmbO = false, float surfaceThreshold = 0f) { //, out Vector3[] _vertices, out Vector3[] _normals, out Vector2[] _uvs, out int[] _triangles) {
		if (densityMap == null) {
			throw new Exception("DensityMap must be supplied before calling March.");
		} else {
			if (densityMap.Length < Dimensions.x * Dimensions.y * Dimensions.z)
				throw new Exception("DensityMap dimensions are too small");
				
			Loom.RunAsync(()=>{
				MarchingCubesData mcd = MarchingCubesDataFactory.Instance.pop();
				try {
					mcd.SetDensityMap(densityMap, Dimensions);
					mcd.gutterSize = gutterSize;
					mcd.surfaceThreshold = surfaceThreshold;
							
					CalcCubeCases(ref mcd);
					CalcVertIDs(ref mcd);
					GenerateGeometry(ref mcd);
					
					if (GenAmbO)
						GenerateAmbientOcculsion(ref mcd);
				} finally {
					Loom.QueueOnMainThread(()=>{
						if (mcd.triangles.Count > 2)
							UpdateMesh(ref mcd, ref mesh);
						else
							mesh = null;
						MarchingCubesDataFactory.Instance.push(mcd);
							
						if (Callback != null)
							Callback(ref mesh);
					});
				}
			});
		}
	}
//		
//		public static void ReleaseMutex() {
//			marchMutex.Release();
//		}
//		
//	#region Threading
//	/// <summary>
//	/// March complete delegate.
//	/// </summary>
//	public delegate void MarchCompleteDelegate();
//	private MarchCompleteDelegate MarchCompleteNotification;
//	
//	/// <summary>
//	/// March the cubes through the density field, generating Geometry. This is the blocking version. It executes faster, but will have an effect on your frame-rate.
//	/// </summary>
//	public void MarchThreaded(float[][][] density, MarchCompleteDelegate MarchComplete) {				// NOTE: code needs to be exactly the same as Coroutine above, without any yield statements
//		if (density == null) {
//			throw new Exception("DensityMap must be supplied before calling March.");
//		} else {
//			working = true;		
//			SetDensityMap(density);
//			MarchCompleteNotification = MarchComplete;
//			mcThread = new Thread(ThreadedMarch) {Name = "Marching1"};
//			mcThread.Start();
//		}
//	}
//	/// <summary>
//	/// March as a seperate thread. This won't block the main thread.
//	/// </summary>
//	private void ThreadedMarch() {
//		CheckWorkingArrays();
//		CalcCubeCases();
//		CalcVertIDs();
//		GenerateGeometry();
//		// tell our host that we're done and they can process the geometry.
//		MarchCompleteNotification();
//		working = false;
//	}
//	#endregion
			
	/// <summary>
	/// Reset/create working arrays if they'er null or too small.
	/// </summary>
	protected void CheckWorkingArrays() {
		if (densityField != null && (cubeCase==null || cubeCase.Length < workingDimensions.x)) {
			cubeCase = new int[workingDimensions.x][][];
			vertIDs = new Triplet[workingDimensions.x][][];
			for (int i = 0; i < cubeCase.Length; i++) {
				cubeCase[i] = new int[workingDimensions.y][];
				vertIDs[i] = new Triplet[workingDimensions.y][];
				for (int j = 0; j < cubeCase[i].Length; j++) {
					cubeCase[i][j] = new int[workingDimensions.z];
					vertIDs[i][j] = new Triplet[workingDimensions.z];
				}
			}
		}
	}
		
	/// <summary>
	/// Calculates the threshold point between two samples
	/// </summary>
	/// <returns>
	/// How far between the two samples the thresholdValue lies.
	/// </returns>
	/// <param name='sample1'>
	/// Sample1.
	/// </param>
	/// <param name='sample2'>
	/// Sample2.
	/// </param>
	/// <param name='thresholdValue'>
	/// Threshold value.
	/// </param>
	protected static float CalcThreshold(float sample1, float sample2, float thresholdValue) {
		sample1 -= thresholdValue;
		sample2 -= thresholdValue;
		float n = sample1 - sample2;
		if (n == 0) {
			return 0.5f;	/// should be 0? This is a problem if a huge number of vertex tests fall right on the threshold. !!!!!!
		}
		else
			return Smoothness * sample1 / n;
	}
					
	/// <summary>
	/// CalcCubeCases: construct an array of bytes that indicate how many faces, and on which edges, each
	///	cell (voxel) in our density field will contain in the final geometry. This is the "March"!
	/// </summary>
	protected static void CalcCubeCases(ref MarchingCubesData mcd) {
		int x, y, z, x1, y1, z1;
		// grab necessary elements from mcd
		Triplet stride = mcd.stride;
		Triplet workingDimensions = mcd.workingDimensions;
		float[] densityField = mcd.densityField;
		float surfaceThreshold = mcd.surfaceThreshold;
			
		float strideX_1 = 1f / stride.x;
		float strideY_1 = 1f / stride.y;
		int bit0 = 0, bit1 = 0, bit2 = 0, bit3 = 0, bit4 = 0, bit5 = 0, bit6 = 0, bit7 = 0;
		int maxX = mcd.workingDimensions.x * stride.x;
		int maxY = mcd.workingDimensions.y * stride.y;
		for (x = 0; x < maxX; x+=stride.x) {
//			for (x = 0; x < (workingDimensions.x-1)*stride.x; x+=stride.x) {
			x1 = Math.Min(x+stride.x, maxX-stride.x);
			for (y = 0; y < maxY; y+=stride.y) {
				y1 = Math.Min(y+stride.y, maxY-stride.y);
				bit4 = densityField[x+y] <= surfaceThreshold ? 0 : 1;
				bit5 = densityField[x+y1] <= surfaceThreshold ? 0 : 1;
				bit6 = densityField[x1+y1] <= surfaceThreshold ? 0 : 1;
				bit7 = densityField[x1+y] <= surfaceThreshold ? 0 : 1;
				for (z = 0; z < workingDimensions.z; z+=stride.z) {
					z1 = Math.Min(z+stride.z, workingDimensions.z-1);
					bit0 = bit4;
					bit1 = bit5;
					bit2 = bit6;
					bit3 = bit7;
					bit4 = densityField[x+y+z1] <= surfaceThreshold ? 0 : 1;
					bit5 = densityField[x+y1+z1] <= surfaceThreshold ? 0 : 1;
					bit6 = densityField[x1+y1+z1] <= surfaceThreshold ? 0 : 1;
					bit7 = densityField[x1+y+z1] <= surfaceThreshold ? 0 : 1;
					mcd.cubeCase[Mathf.FloorToInt(x*strideX_1)][Mathf.FloorToInt(y*strideY_1)][z] = bit0 | bit1 << 1 | bit2 << 2 | bit3 << 3
									| bit4 << 4 | bit5 << 5 | bit6 << 6 | bit7 << 7;
				}
			}
		}
	}
	protected static void CalcCubeCases(ref MarchingCubesByte mcd) {
		int x, y, z, x1, y1, z1;
		// grab necessary elements from mcd
		Triplet stride = mcd.stride;
		Triplet workingDimensions = mcd.workingDimensions;
		byte[] densityField = mcd.densityField;
		byte surfaceThreshold = mcd.surfaceThreshold;
			
		float strideX_1 = 1f / stride.x;
		float strideY_1 = 1f / stride.y;
		int bit0 = 0, bit1 = 0, bit2 = 0, bit3 = 0, bit4 = 0, bit5 = 0, bit6 = 0, bit7 = 0;
		int maxX = mcd.workingDimensions.x * stride.x;
		int maxY = mcd.workingDimensions.y * stride.y;
		for (x = 0; x < maxX; x+=stride.x) {
//			for (x = 0; x < (workingDimensions.x-1)*stride.x; x+=stride.x) {
			x1 = Math.Min(x+stride.x, maxX-stride.x);
			for (y = 0; y < maxY; y+=stride.y) {
				y1 = Math.Min(y+stride.y, maxY-stride.y);
				bit4 = densityField[x+y] <= surfaceThreshold ? 0 : 1;
				bit5 = densityField[x+y1] <= surfaceThreshold ? 0 : 1;
				bit6 = densityField[x1+y1] <= surfaceThreshold ? 0 : 1;
				bit7 = densityField[x1+y] <= surfaceThreshold ? 0 : 1;
				for (z = 0; z < workingDimensions.z; z+=stride.z) {
					z1 = Math.Min(z+stride.z, workingDimensions.z-1);
					bit0 = bit4;
					bit1 = bit5;
					bit2 = bit6;
					bit3 = bit7;
					bit4 = densityField[x+y+z1] <= surfaceThreshold ? 0 : 1;
					bit5 = densityField[x+y1+z1] <= surfaceThreshold ? 0 : 1;
					bit6 = densityField[x1+y1+z1] <= surfaceThreshold ? 0 : 1;
					bit7 = densityField[x1+y+z1] <= surfaceThreshold ? 0 : 1;
					mcd.cubeCase[Mathf.FloorToInt(x*strideX_1)][Mathf.FloorToInt(y*strideY_1)][z] = bit0 | bit1 << 1 | bit2 << 2 | bit3 << 3
									| bit4 << 4 | bit5 << 5 | bit6 << 6 | bit7 << 7;
				}
			}
		}
	}
	/// <summary>
	/// CalcVertIDs: go through the cubeCases and generate and assign vertices for the edges that will need them.
	///	We only look at edges 0, 3, and 8 because the other edges will be 0,3, or 8 in another cube. This allows
	///	a single pass to generate all the verticies.
	/// </summary>
	protected static void CalcVertIDs(ref MarchingCubesData mcd, bool normalizeNormals = true) { //out Vector3[] _vertices, out Vector3[] _normals, out Vector2[] _uvs) {
		float increment = 0.5f;
		int vertCounter = 0;
		int x, y, z, bit0, thisCase;
		Vector3 vert;
		bool edge0, edge3, edge8;
	
//			int newVerticesCounter = 0;
			
		int gutterSize = mcd.gutterSize;
		Triplet workingDimensions = mcd.workingDimensions;
		int[][][] cubeCase = mcd.cubeCase;
		float[] densityField = mcd.densityField;
		Triplet stride = mcd.stride;
		float surfaceThreshold = mcd.surfaceThreshold;
		bool calcNormals = mcd.CalculateNormals;
			
		mcd.vertices.Clear();
		mcd.normals.Clear();
		mcd.uvs.Clear();
			
//			float uvScale = 1f / (float)(workingDimensions.x-2*gutterSize);	// subtract 2 to compensate for the (x=1; < map-1) two rows that we're trimming
		Vector2 uvOffset = Vector2.one*gutterSize;
		int l1 = workingDimensions.x-Math.Max(gutterSize-2,0);	// cubeCases at the max dim are all 0 because they're missing the far side of density values
		int l2 = workingDimensions.y-Math.Max(gutterSize-2,0);
		int l3 = workingDimensions.z-Math.Max(gutterSize-2,0);
		for (x = Math.Max(gutterSize-1,0); x < l1; x++) {	// ignore the gutter in the loop.
			for (y = Math.Max(gutterSize-1,0); y < l2; y++) {
				for (z = Math.Max(gutterSize-1,0); z < l3; z++) {
					thisCase = cubeCase[x][y][z];
					if (thisCase > 0) {
						// verts on edges 0, 3, 8 - +y, +x, +z, bits 0:1, 0:3, 0:4
						bit0 = thisCase & 1;
						edge0 = (((thisCase >> 1) & 1) != bit0);
						edge3 = (((thisCase >> 3) & 1) != bit0);
						edge8 = (((thisCase >> 4) & 1) != bit0);
						mcd.vertIDs[x][y][z] = new Triplet(edge3 ? vertCounter++ : -1, edge0 ? vertCounter++ : -1, edge8 ? vertCounter++ : -1);	// a -1 is a null.
						if (edge3) {
//								if (x < l1-1)
								increment = CalcThreshold(densityField[x*stride.x+y*stride.y+z], densityField[(x+1)*stride.x+y*stride.y+z], surfaceThreshold);
//								else increment = 0f;
//								if (float.IsNaN(increment))
//									Debug.Log (x + ", " + y + ", " + z);
									
							vert = new Vector3(x+increment, y, z);
								
							mcd.vertices.Add(vert);
							if (calcNormals)
								mcd.normals.Add(CalcNormalAt(vert, densityField, workingDimensions, normalizeNormals));
							mcd.uvs.Add((new Vector2(x+increment, z) - uvOffset)); // * uvScale);
//							newUV2s.Add((new Vector2(y, z) - Vector2.one*gutterSize) * uvScale);
						}
						if (edge0) {
//								if (y < l2-1)
								increment = CalcThreshold(densityField[x*stride.x+y*stride.y+z], densityField[x*stride.x+(y+1)*stride.y+z], surfaceThreshold);
//								else increment = 0f;
//								if (float.IsNaN(increment))
//									Debug.Log (x + ", " + y + ", " + z);
							vert = new Vector3(x, y+increment, z);
							mcd.vertices.Add (vert);
							if (calcNormals)
								mcd.normals.Add(CalcNormalAt(vert, densityField, workingDimensions, normalizeNormals));
							mcd.uvs.Add((new Vector2(x, z) - uvOffset)); // * uvScale);
//							newUV2s.Add((new Vector2(y+increment, z) - Vector2.one*gutterSize) * uvScale);
						}
						if (edge8) {
//								if  (z < l3-1)
								increment = CalcThreshold(densityField[x*stride.x+y*stride.y+z], densityField[x*stride.x+y*stride.y+z+1], surfaceThreshold);
//								else increment = 0f;
//								if (float.IsNaN(increment))
//									Debug.Log (x + ", " + y + ", " + z);
							vert = new Vector3(x, y, z+increment);
							mcd.vertices.Add(vert);
							if (calcNormals)
								mcd.normals.Add(CalcNormalAt(vert, densityField, workingDimensions, normalizeNormals));
							mcd.uvs.Add((new Vector2(x, z+increment) - uvOffset)); // * uvScale);
//							newUV2s.Add((new Vector2(y, z+increment) - Vector2.one*gutterSize) * uvScale);
						}
					}
				}	// for z
			}	// for y
		}	// for x
			
//			if (calcNormals && normalizeNormals)			// 5/28/2014 Normalizing here for MC, not for DC
//				for (int i = 0; i < mcd.normals.Count; i++)
//					mcd.normals[i].Normalize();
			
//			vertices = new Vector3[newVerticesCounter];
//			Array.Copy(newVerts, 0, vertices, 0, newVerticesCounter);
//			uvs = new Vector2[newVerticesCounter];
//			Array.Copy(newUVs, 0, uvs, 0, newVerticesCounter);
//			normals = new Vector3[newVerticesCounter];
//			Array.Copy(newNormals, 0, normals, 0, newVerticesCounter);
	}
		
	// CalcNormalAt: calculates a normal by sampling the density map. We assume a uniform gradient, locally,
	//  which makes the calculation relatively efficient. However it will break down for sharp changes in the
	//  density map (producing erroneous normals).
	protected static Vector3 CalcNormalAt(Vector3 vert, float[] densityField, Triplet workingDimensions, bool normalize = true) {
		Vector3 normal = new Vector3(TrilinearSample.SampleClipped(densityField, workingDimensions, vert+Vector3.right*epsilon) - TrilinearSample.SampleClipped(densityField, workingDimensions, vert-Vector3.right*epsilon),
								TrilinearSample.SampleClipped(densityField, workingDimensions, vert+Vector3.up*epsilon) - TrilinearSample.SampleClipped(densityField, workingDimensions, vert-Vector3.up*epsilon),
								TrilinearSample.SampleClipped(densityField, workingDimensions, vert+Vector3.forward*epsilon) - TrilinearSample.SampleClipped(densityField, workingDimensions, vert-Vector3.forward*epsilon));
//			if (normal.magnitude == 0)	// this would be an error.
//				normal = Vector3.one;
		if (normalize)
			return -normal.normalized;
		else
			return -normal;	// 5/28/2014 BDS no longer normalized here. DC seems to do better with non-normalized normals
	}
	/// <summary>
	/// Gets the vertex identifier for edge, given the cube corrdinates
	/// </summary>
	/// <returns>
	/// The vertex identifier for edge.
	/// </returns>
	/// <param name='x'>
	/// X cube coordinate.
	/// </param>
	/// <param name='y'>
	/// Y cube coordinate.
	/// </param>
	/// <param name='z'>
	/// Z cube coordinate
	/// </param>
	/// <param name='edge'>
	/// Edge (0-11).
	/// </param>
	protected static int GetVertIDForEdge(Triplet[][][] vertIDs, int x, int y, int z, int edge) {
//			Debug.Log (x + "," + y + "," + z + "," + edge + "," + vertIDs.Length);
		edge *= 4;
		Triplet output = vertIDs[x + MCTables.edge_start[ edge ]][y + MCTables.edge_start[ edge+1 ]][z + MCTables.edge_start[ edge+2 ]];
		
		if (output == null)
			return 0;
			
		int axis = MCTables.edge_axis[ edge ];
		int valueOut = -1;
//			Debug.Log (output);
		switch (axis) {
			case 0:
				valueOut = (int)output.x;
				break;
			case 1:
				valueOut = (int)output.y;
				break;
			case 2:
				valueOut = (int)output.z;
				break;
		}
		return valueOut;
	}
		
	/// <summary>
	/// Generates the geometry.
	/// if cubeCase and VertIDs have been generated then we can proceed to create the geometry (triangles).
	/// </summary>
	protected static void GenerateGeometry(ref MarchingCubesData mcd) {
		if (mcd.vertices == null || mcd.vertices.Count < 3)	// bail early if there's no geometry to build!
			return;
			
		int thisCube, numPolys, thisPoly, edge0, edge1, edge2;
		Vector3 tri;
			
		int[][][] cubeCase = mcd.cubeCase;
		int gutterSize = mcd.gutterSize;
		Triplet workingDimensions = mcd.workingDimensions;
			
		mcd.triangles.Clear();
		//newTriangles.Clear();		// clear out triangles
		try {
			int l1 = workingDimensions.x-Math.Max(gutterSize,0);	// cubeCases at the max dim are all 0 because they're missing the far side of density values
			int l2 = workingDimensions.y-Math.Max(gutterSize,0);
			int l3 = workingDimensions.z-Math.Max(gutterSize,0);
			// first translate cubeCase into # of polys and edges
			for (int x = Math.Max(gutterSize,0); x < l1; x++) {	// -gutterSize here trims off the incomplete edge cases
				for (int y = Math.Max(gutterSize,0); y < l2; y++) {
					for (int z = Math.Max(gutterSize,0); z < l3; z++) {
						thisCube = cubeCase[x][y][z];
						numPolys = MCTables.case_to_numpolys[thisCube];
						
						thisCube *= 20; // * 5 * 4 -- indexes the array correctly.
						for (int i = 0; i < numPolys; i++) {
							thisPoly = thisCube + i * 4;
		    				// edgeN range: 0-11
							edge0 = MCTables.g_triTable[ thisPoly ];			// this tells us which edges of the voxel the verts are on
							edge1 = MCTables.g_triTable[ thisPoly+1 ];
							edge2 = MCTables.g_triTable[ thisPoly+2 ];
								
							// now we need to find those verts. They're stored in our big vertIDs array.
							// 3,0,8 are xyz at our position.
							// 1 is x at y+1, 9 is z at y+1
							// 2 is y at x+1, 11 is z at x+1
							// 7 is x at z+1, 4 is y at z+1 ...
								
							tri.x = GetVertIDForEdge(mcd.vertIDs, x,y,z, edge0);
							tri.y = GetVertIDForEdge(mcd.vertIDs, x,y,z, edge1);
							tri.z = GetVertIDForEdge(mcd.vertIDs, x,y,z, edge2);
							if (tri.x > -1 && tri.y > -1 && tri.z > -1) { // was there an error? should never happen!	// this is where a -1 could slip in and kill us!!
								mcd.triangles.Add((int)tri.x);
								mcd.triangles.Add((int)tri.y);
								mcd.triangles.Add((int)tri.z);
							} else
								throw new Exception("bad tri, numPolys = " + numPolys + ", i= " + i + " xyz: " + x + ","+y+","+z + " tri: " + tri.ToString());
							
//								Debug.Log();
						} // for i ... numPolys
					}	// for z
				} // for y
			} // for x
				
		} catch (Exception e1) {
			// assume something went really bad (we probably threw our own exception)
			Debug.Log("Exception: caught in MarchingCubes:GenerateGeometry: " + e1.Message);
			// clear out all data, although the caller won't know why we failed.
			mcd.Initialize();
		}
	}
		
	/// <summary>
	/// Centers the and scale verts. They are generated between 0 and WIDTH but will now be scaled to [-1,1]
	/// </summary>
	public static void CenterAndScaleVerts(ref Mesh mesh, Triplet dimensions, int gutterSize = 0) {
		Vector3 translate = (new Vector3(dimensions.x, dimensions.y, dimensions.z)) * -0.5f;
			
		Vector3 scale = new Vector3(1f / (float)(dimensions.x-gutterSize*2-1), 1f/(float)(dimensions.y-gutterSize*2-1), 
								1f/(float)(dimensions.z-gutterSize*2-1));
			
		Vector3[] vertices = mesh.vertices;
		for (int i = 0; i < vertices.Length; i++)
			vertices[i] = Vector3.Scale((vertices[i] + translate), scale);
		mesh.vertices = vertices;
	}
	public void CenterVerts() {
		Vector3 translate = (new Vector3(workingDimensions.x, workingDimensions.y, workingDimensions.z)) * -0.5f;
		for (int i = 0; i < vertices.Length; i++)
			vertices[i] += translate;
	}
	public void ScaleVerts() {
		Vector3 scale = new Vector3(1f / (float)(workingDimensions.x-gutterSize*2-1), 1f/(float)(workingDimensions.y-gutterSize*2-1), 
								1f/(float)(workingDimensions.z-gutterSize*2-1));
		for (int i = 0; i < vertices.Length; i++)
			vertices[i] = Vector3.Scale(vertices[i], scale);
	}
	public void ScaleAndTranslateGeometry(Vector3 scale, Vector3 translate) {
		for (int i = 0; i < vertices.Length; i++)
			vertices[i] = Vector3.Scale(vertices[i], scale) + translate;
	}
	public void TranslateAndScaleGeometry(Vector3 scale, Vector3 translate) {
		for (int i = 0; i < vertices.Length; i++)
			vertices[i] = Vector3.Scale(vertices[i] + translate, scale);
	}
		
	/// <summary>
	/// Generates the ambient occulsion values-stored as alpha of vertex color. This is the non-blocking Coroutine version.
	/// </summary>
	/// <param name='CALCS_PER_PASS'>
	/// How many calculations to perform between yields (try 1024 or greater).
	/// </param>
	public IEnumerator GenerateAmbientOcculsion(int CALCS_PER_PASS) {
		if (vertices.Length < 1)
			yield break;	// no vertices? then we have nothing to do.
		const int RAYS = 32;	// the number of rays we will cast per vertex
		const float INV_RAYS = 1f/(float)RAYS;
		const float cells_to_skip_at_start = 1f;
		float invVoxelDim = 1f/(float)workingDimensions.x;
		float amboRayCellDist = (float)gutterSize;
		int AMBO_STEPS = Mathf.FloorToInt(amboRayCellDist); // * 1.4f);	// let the diagnols reach the edge too
		int STEP_SIZE = MCTables.occlusion_amt.Length / AMBO_STEPS;	// calculate a step size that gets through our complete table, regardless of gutterSize
		STEP_SIZE = STEP_SIZE < 4 ? 4 : STEP_SIZE;
		float INV_AMBO_STEPS = 1f/AMBO_STEPS;
			
		if (colors == null || colors.Length != vertices.Length)
			colors = new Color[vertices.Length];
		for (int i = 0; i < colors.Length; i++) {
			float ambo = 0f;
			for (int j = 0; j < RAYS; j++) {
				int ray_index = j*3;
				Vector3 ray_dir = new Vector3(MCTables.g_ray_dirs_32[ray_index], MCTables.g_ray_dirs_32[ray_index+1], MCTables.g_ray_dirs_32[ray_index+2]);
				Vector3 ray_start = vertices[i];
				Vector3 ray_now = ray_start + ray_dir * invVoxelDim * cells_to_skip_at_start ;
				Vector3 ray_delta = ray_dir * amboRayCellDist; // then over 160f?? That seems very small.
					
				float ambo_this = 1;
					
				// SHORT RANGE:
				//  -step along the ray at AMBO_STEPS points,
				//     sampling the density volume texture
				//  -occlusion_amt[] LUT makes closer occlusions have more weight than far ones
				//  -start sampling a few cells away from the vertex, to reduce noise.
				ray_delta *= INV_AMBO_STEPS;
				for (int k = 0; k < MCTables.occlusion_amt.Length-3; k+=STEP_SIZE) {   
					ray_now += ray_delta;
					float t = TrilinearSample.SampleClipped(densityField, workingDimensions, ray_now) * 20f;	// 20f: oversaturates low values to intensify the occlusion!
					ambo_this = Mathf.Lerp(ambo_this, 0, Mathf.Clamp01(t) * MCTables.occlusion_amt[k+1]); //* pow(1-j/(float)AMBO_STEPS,0.4)
				}  
					
				// could do LONG RANGE next...
					
				ambo_this *= 1.5f; //1.4f;	// Not sure about this scalar, but it is about right to bring up the highlights
					
				ambo += ambo_this;
			}
			ambo *= INV_RAYS;
//			ambo = (ambo*ambo);
			colors[i] = new Color(1f, 1f, 1f, ambo);
//			Debug.Log(ambo);
			if (i % CALCS_PER_PASS == 0)
				yield return new WaitForEndOfFrame();
		}
	}
		
	/// <summary>
	/// Generates the ambient occulsion values-stored as alpha of vertex color. This is the regular, blocking version.
	/// </summary>
	public static void GenerateAmbientOcculsion(ref MarchingCubesData mcd) {
		if (mcd.vertices == null || mcd.vertices.Count < 3)
			return;	// no vertices? then we have nothing to do.
		const int RAYS = 32;	// the number of rays we will cast per vertex
		const float INV_RAYS = 1f/(float)RAYS;
		const float cells_to_skip_at_start = 1f;
		float invVoxelDim = 1f/(float)mcd.workingDimensions.x;
		float amboRayCellDist = (float)Math.Max(mcd.gutterSize, 2);
		int AMBO_STEPS = Mathf.FloorToInt(amboRayCellDist); // * 1.4f);	// let the diagnols reach the edge too
		int STEP_SIZE = MCTables.occlusion_amt.Length / AMBO_STEPS;	// calculate a step size that gets through our complete table, regardless of gutterSize
		STEP_SIZE = STEP_SIZE < 4 ? 4 : STEP_SIZE;
		float INV_AMBO_STEPS = 1f/AMBO_STEPS;
			
		mcd.colors.Clear();
			
		List<Vector3> vertices = mcd.vertices;
		float[] densityField = mcd.densityField;
//			if (colors == null || colors.Length != vertices.Length)
//				colors = new Color[vertices.Length];
		for (int i = 0; i < vertices.Count; i++) {
			float ambo = 0f;
			for (int j = 0; j < RAYS; j++) {
				int ray_index = j*3;
				Vector3 ray_dir = new Vector3(MCTables.g_ray_dirs_32[ray_index], MCTables.g_ray_dirs_32[ray_index+1], MCTables.g_ray_dirs_32[ray_index+2]);
				Vector3 ray_start = vertices[i];
				Vector3 ray_now = ray_start + ray_dir * invVoxelDim * cells_to_skip_at_start ;
				Vector3 ray_delta = ray_dir * amboRayCellDist; // then over 160f?? That seems very small.
					
				float ambo_this = 1;
					
				// SHORT RANGE:
				//  -step along the ray at AMBO_STEPS points,
				//     sampling the density volume texture
				//  -occlusion_amt[] LUT makes closer occlusions have more weight than far ones
				//  -start sampling a few cells away from the vertex, to reduce noise.
				ray_delta *= INV_AMBO_STEPS;
				for (int k = 0; k < MCTables.occlusion_amt.Length-3; k+=STEP_SIZE) {   
					ray_now += ray_delta;
					float t = TrilinearSample.SampleClipped(densityField, mcd.workingDimensions, ray_now) * 20f;	// 20f: oversaturates low values to intensify the occlusion!
					ambo_this = Mathf.Lerp(ambo_this, 0, Mathf.Clamp01(t) * MCTables.occlusion_amt[k+1]); //* pow(1-j/(float)AMBO_STEPS,0.4)
				}  
					
				// could do LONG RANGE next...
					
				ambo_this *= 1.5f; //1.4f;	// Not sure about this scalar, but it is about right to bring up the highlights
					
				ambo += ambo_this;
			}
			ambo *= INV_RAYS;
			mcd.colors.Add(new Color(1f, 1f, 1f, ambo));
		}
	}
		
	/// <summary>
	/// Applies the colors data to the verticies in the mesh (presumably generated by Marching Cubes). 
	/// 	The verticies of the mesh must be in the range of the dimensions of the current density map 
	/// 	and the colors array must be the same dimensions as the supplied density map.
	/// </summary>
	/// <param name='mesh'>
	/// The mesh to apply vertex colors to.
	/// </param>
	/// <param name='colors'>
	/// Colors to apply.
	/// </param>
	public static void ApplyColorsToMesh(ref Mesh mesh, Color[][][] colors) {
		Vector3[] verts = mesh.vertices;
		Color[] col = mesh.colors;
		if (col.Length != verts.Length)
			col = new Color[verts.Length];
		for (int i = 0; i < col.Length; i++) {
			Vector3 v = verts[i];
				
			v.x = Mathf.Clamp(Mathf.FloorToInt(v.x), 0, colors.Length-1);
			v.y = Mathf.Clamp(Mathf.FloorToInt(v.y), 0, colors.Length-1);
			v.z = Mathf.Clamp(Mathf.FloorToInt(v.z), 0, colors.Length-1);
			float a = col[i].a;		// preserve alpha as this is used for light mapping/ambient occlusion
			col[i] = colors[(int)v.x][(int)v.y][(int)v.z];
			col[i].a = a;
		}
		mesh.colors = col;
	}
	/// <summary>
	/// Applies the colors data to the verticies in the mesh (presumably generated by Marching Cubes). 
	/// 	The verticies of the mesh must be in the range of the dimensions of the current density map 
	/// 	and the colors array must be the same dimensions as the supplied density map.
	/// </summary>
	/// <param name='mesh'>
	/// The mesh to apply vertex colors to.
	/// </param>
	/// <param name='colors'>
	/// Colors to apply.
	/// </param>
	public static void ApplyColorsToMesh(Mesh mesh, Color[] colors, Triplet dimensions) {
		Vector3[] verts = mesh.vertices;
		Color[] col = mesh.colors;
		Triplet stride = new Triplet(dimensions.y * dimensions.z, dimensions.z, 1);
//			Loom.RunAsync(()=>{				// the commented-out lines will apply colors asynchronously. However this will cause the mesh colors to flicker in the scene. This is not a heavy function so it seems best to keep it synchronous for now
			if (col.Length != verts.Length)
				col = new Color[verts.Length];
			for (int i = 0; i < col.Length; i++) {
				Vector3 v = verts[i];
					
				v.x = Mathf.Clamp(Mathf.FloorToInt(v.x)*stride.x, 0, colors.Length-stride.x);
				v.y = Mathf.Clamp(Mathf.FloorToInt(v.y)*stride.y, 0, colors.Length-stride.y);
				v.z = Mathf.Clamp(Mathf.FloorToInt(v.z), 0, colors.Length-1);
				float a = col[i].a;		// preserve alpha as this is used for light mapping/ambient occlusion
				col[i] = colors[(int)v.x+(int)v.y+(int)v.z];
				col[i].a = a;
			}
//				Loom.QueueOnMainThread(()=>{
				mesh.colors = col;
//				});
//			});
	}
	/// <summary>
	/// Resamples a flattened 3D array. The boundries are preserved (i.e. the first element and last element of each row are the same, the internal divisions are resampled).
	/// </summary>
	/// <returns>
	/// The new array with the specified destination dimensions (x*y*z).
	/// </returns>
	/// <param name='source'>
	/// Source array (must be sourceDim x*y*z in size)
	/// </param>
	/// <param name='sourceDim'>
	/// Source dimensions (x,y,z) of the 3D array
	/// </param>
	/// <param name='destDim'>
	/// Destination dimensions: how big the output will be (x*y*z).
	/// </param>
	/// <typeparam name='T'>
	/// The 1st type parameter.
	/// </typeparam>
	public static T[] Resample3DArray<T>(T[] source, Triplet sourceDim, Triplet destDim) {
		if (source.Length != sourceDim.x * sourceDim.y * sourceDim.z)
			throw new Exception("error: source array dimension must be product of sourceDim");
			
		Vector3 stride;
		stride.z = (sourceDim.z-1f) / (float)(destDim.z > 1 ? destDim.z-1f : 1f);
		stride.y = (sourceDim.y-1f) / (float)(destDim.y > 1 ? destDim.y-1f : 1f);
		stride.x = (sourceDim.x-1f) / (float)(destDim.x > 1 ? destDim.x-1f : 1f);
			
		T[] output = new T[destDim.x * destDim.y * destDim.z];
		int xStride = destDim.y * destDim.z;
		for (int i = 0; i < destDim.x; i++) {
			int sourceX = sourceDim.z * sourceDim.y * Mathf.RoundToInt(i * stride.x);
			for (int j = 0; j < destDim.y; j++) {
				int sourceY = sourceDim.z * Mathf.RoundToInt(j * stride.y);
				for (int k = 0; k < destDim.z; k++) {
					output[i * xStride + j * destDim.z + k] = source[sourceX + sourceY + Mathf.RoundToInt(k * stride.z)];
				}
			}
		}
			
		return output;
	}
	public static T[] Resample3DArrayWithGutter<T>(T[] source, Triplet sourceDim, Triplet destDim, int gutterSize) {
		if (source.Length != sourceDim.x * sourceDim.y * sourceDim.z)
			throw new Exception("error: source array dimension must be product of sourceDim");
			
		Vector3 stride;
		Triplet core = new Triplet(destDim.x - gutterSize*2, destDim.y - gutterSize*2, destDim.z - gutterSize*2);
		core.x = Math.Max(1, core.x);
		core.y = Math.Max(1, core.y);
		core.z = Math.Max(1, core.z);
		stride.z = (sourceDim.z-gutterSize*2-1) / (float)(core.z > 1 ? core.z-1 : 1f);
		stride.y = (sourceDim.y-gutterSize*2-1) / (float)(core.y > 1 ? core.y-1 : 1f);
		stride.x = (sourceDim.x-gutterSize*2-1) / (float)(core.x > 1 ? core.x-1 : 1f);
			
		int sourceX, sourceY, sourceZ;
			
		T[] output = new T[destDim.x * destDim.y * destDim.z];
		int xStride = destDim.y * destDim.z;
		for (int i = 0; i < destDim.x; i++) {
//				int sourceX = Mathf.Min (Math.Max(i - gutterSize, 0) / core.x, 1f) * core.x 
			if (i <= gutterSize)
				sourceX = i;
			else if (i >= destDim.x - gutterSize-1)
				sourceX = sourceDim.x - (destDim.x-i);
			else
				sourceX = Mathf.RoundToInt(gutterSize + (Math.Max(i-gutterSize, 0)) * stride.x);
//					sourceX = Mathf.RoundToInt((Mathf.Min(Math.Max(i-gutterSize, 0) / (float)core.x, 1f) * (float)core.x * stride.x + gutterSize));
			sourceX *= sourceDim.z * sourceDim.y;
			for (int j = 0; j < destDim.y; j++) {
				if (j <= gutterSize)
					sourceY = j;
				else if (j >= destDim.y - gutterSize-1)
					sourceY = sourceDim.y - (destDim.y-j);
				else
					sourceY = Mathf.RoundToInt(gutterSize + (Math.Max(j-gutterSize, 0)) * stride.y);
//						sourceY = Mathf.RoundToInt((Mathf.Min(Math.Max(j-gutterSize, 0) / (float)core.y, 1f) * (float)core.y * stride.y + gutterSize));
//						sourceY = Mathf.RoundToInt(j * stride.y);
				sourceY *= sourceDim.z;
					
				for (int k = 0; k < destDim.z; k++) {
					if (k <= gutterSize)
						sourceZ = k;
					else if (k >= destDim.z - gutterSize-1)
						sourceZ = sourceDim.z - (destDim.z-k);
					else
						sourceZ = Mathf.RoundToInt(gutterSize + (Math.Max(k-gutterSize, 0)) * stride.z);
//							sourceZ = Mathf.RoundToInt((Mathf.Min(Math.Max(k-gutterSize, 0) / (float)core.z, 1f) * (float)core.z * stride.z + gutterSize));
//							sourceZ = Mathf.RoundToInt(k * stride.z);
					output[i * xStride + j * destDim.z + k] = source[sourceX + sourceY + sourceZ];
				}
			}
		}
			
		return output;
	}
//				public static T[] Resample3DArrayWithGutter<T>(T[] source, Triplet sourceDim, Triplet destDim, int gutterSize) {
//			if (source.Length != sourceDim.x * sourceDim.y * sourceDim.z)
//				throw new Exception("error: source array dimension must be product of sourceDim");
//			
//			Vector3 stride;
//			Triplet core = new Triplet(destDim.x - gutterSize*2, destDim.y - gutterSize*2, destDim.z - gutterSize*2);
//			core.x = Math.Max(1, core.x);
//			core.y = Math.Max(1, core.y);
//			core.z = Math.Max(1, core.z);
//			stride.z = (sourceDim.z-gutterSize*2-1f) / (float)(core.z > 1 ? core.z-1f : 1f);
//			stride.y = (sourceDim.y-gutterSize*2-1f) / (float)(core.y > 1 ? core.y-1f : 1f);
//			stride.x = (sourceDim.x-gutterSize*2-1f) / (float)(core.x > 1 ? core.x-1f : 1f);
//			
//			int sourceX, sourceY, sourceZ;
//			
//			T[] output = new T[destDim.x * destDim.y * destDim.z];
//			int xStride = destDim.y * destDim.z;
//			for (int i = 0; i < destDim.x; i++) {
////				int sourceX = Mathf.Min (Math.Max(i - gutterSize, 0) / core.x, 1f) * core.x 
//				if (i < gutterSize)
//					sourceX = i;
//				else if (i >= destDim.x - gutterSize)
//					sourceX = sourceDim.x - (destDim.x-i);
//				else
//					sourceX = Mathf.RoundToInt((Mathf.Min(Math.Max(i-gutterSize, 0) / (float)core.x, 1f) * (float)core.x * stride.x + gutterSize));
//				sourceX *= sourceDim.z * sourceDim.y;
//				for (int j = 0; j < destDim.y; j++) {
//					if (j < gutterSize)
//						sourceY = j;
//					else if (j >= destDim.y - gutterSize)
//						sourceY = sourceDim.y - (destDim.y-j);
//					else
//						sourceY = Mathf.RoundToInt((Mathf.Min(Math.Max(j-gutterSize, 0) / (float)core.y, 1f) * (float)core.y * stride.y + gutterSize));
////						sourceY = Mathf.RoundToInt(j * stride.y);
//					sourceY *= sourceDim.z;
//					
//					for (int k = 0; k < destDim.z; k++) {
//						if (k < gutterSize)
//							sourceZ = k;
//						else if (k >= destDim.z - gutterSize)
//							sourceZ = sourceDim.z - (destDim.z-k);
//						else
//							sourceZ = Mathf.RoundToInt((Mathf.Min(Math.Max(k-gutterSize, 0) / (float)core.z, 1f) * (float)core.z * stride.z + gutterSize));
////							sourceZ = Mathf.RoundToInt(k * stride.z);
//						output[i * xStride + j * destDim.z + k] = source[sourceX + sourceY + sourceZ];
//					}
//				}
//			}
//			
//			return output;
//		}
}	// MarchingCubes	
