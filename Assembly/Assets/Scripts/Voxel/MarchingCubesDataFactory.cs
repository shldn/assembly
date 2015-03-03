using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

	
public class MarchingCubesData {
	public List<Vector3> vertices;
	public List<Vector3> normals;
	public List<Vector2> uvs;
	public List<int> triangles;
	public List<Color> colors;
		
	// used by DC algorithm
	public List<Vector3> QEFPoints, QEFNormals;
	public int[][][] QEFPointIDs;		// holds the vertex IDs for each edge that will have a vertex on it
		
	public float[][] matrix;
	public Vector3[] vertex;
	public float[] vector;
		
	public float[] densityField = null;
	public int[][][] cubeCase = null;
	public Triplet[][][] vertIDs = null;
		
	public int gutterSize = 0;
//		public float smoothness = 1f, 
	public float surfaceThreshold = 0f;
	public Triplet workingDimensions, stride;
	public bool CalculateNormals = true;
		
	public MarchingCubesData(int init_size = 32768) {
		vertices = new List<Vector3>(init_size);
		normals = new List<Vector3>(init_size);
		uvs = new List<Vector2>(init_size);
		triangles = new List<int>(init_size);
		colors = new List<Color>(init_size);
			
		// init DC working data structures
		QEFNormals = new List<Vector3>(init_size);
		QEFPoints = new List<Vector3>(init_size);
			
	    matrix = new float[12][];
		for (int i = 0; i < 12; i++)
			matrix[i] = new float[3];
		vertex = new Vector3[12];
	    vector = new float[12];
		//---------
		workingDimensions = new Triplet(0,0,0);
		stride = new Triplet(1,1,1);
	}
		
	public void Initialize() {
		vertices.Clear();
		normals.Clear();
		uvs.Clear ();
		triangles.Clear();
		colors.Clear();
			
		// init DC working data structures
		QEFNormals.Clear();
		QEFPoints.Clear ();
	}
		
	public void SetDensityMap(float[] newDensity, Triplet dim, bool setupForDC = false) {
		densityField = newDensity;
		workingDimensions = dim;
		stride.z = 1;
		stride.y = workingDimensions.z;
		stride.x = stride.y * workingDimensions.y;
			
		InitWorkingData(setupForDC);
	}
		
	public void InitWorkingData(bool initDCData = false) {
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
			
		if (initDCData && densityField != null && (QEFPointIDs==null || QEFPointIDs.Length < workingDimensions.x)) {
			QEFPointIDs = new int[workingDimensions.x][][];
			for (int i = 0; i < workingDimensions.x; i++) {
				QEFPointIDs[i] = new int[workingDimensions.y][];
				for (int j = 0; j < workingDimensions.y; j++) {
					QEFPointIDs[i][j] = new int[workingDimensions.z];
				}
			}
		}
	}
}
	
public class MarchingCubesByte : MarchingCubesData {
	new public byte[] densityField = null;
	new public byte surfaceThreshold = 127;
		
	public MarchingCubesByte(int init_size = 32768) {
		vertices = new List<Vector3>(init_size);
		normals = new List<Vector3>(init_size);
		uvs = new List<Vector2>(init_size);
		triangles = new List<int>(init_size);
		colors = new List<Color>(init_size);
			
		workingDimensions = new Triplet(0,0,0);
		stride = new Triplet(1,1,1);
	}
		
	public void SetDensityMap(byte[] newDensity, Triplet dim, bool setupForDC = false) {
		densityField = newDensity;
		workingDimensions = dim;
		stride.z = 1;
		stride.y = workingDimensions.z;
		stride.x = stride.y * workingDimensions.y;
			
		InitWorkingData(setupForDC);
	}
}

public class MarchingCubesDataFactory
{
	private List<MarchingCubesData> inUse;
	private List<MarchingCubesData> storage;
		
	private static MarchingCubesDataFactory _instance;
	private static bool initialized = false;
	public static MarchingCubesDataFactory Instance {
		get {
			if (!initialized) {
				_instance = new MarchingCubesDataFactory();
				initialized = true;
			}
			return _instance;
		}
	}
	private MarchingCubesDataFactory ()
	{
		inUse = new List<MarchingCubesData>();
		storage = new List<MarchingCubesData>();
	}
		
	public MarchingCubesData pop() {
		return pop<float>();
	}
	public MarchingCubesData pop<T>() {
		MarchingCubesData mcd = null;
		bool foundOne = false;
			
		if (typeof(T) == typeof(float)) {
			lock(storage) {
				foreach (var member in storage) {
					if (member.GetType() == typeof(MarchingCubesData)) {
						mcd = member;
						storage.Remove(member);
						foundOne = true;
						break;
					}
				}
			}
				
			if (!foundOne)
				mcd = new MarchingCubesData(32768);
			else
				mcd.Initialize();
		} else if (typeof(T) == typeof(byte)) {
			lock(storage) {
				foreach (var member in storage) {
					if (member.GetType() == typeof(MarchingCubesByte)) {
						mcd = member;
						storage.Remove(member);
						foundOne = true;
						break;
					}
				}
			}
				
			if (!foundOne)
				mcd = new MarchingCubesByte(32768);
			else
				mcd.Initialize();
		}
			
		lock(inUse) {
			inUse.Add(mcd);
		}
		return mcd;
	}
		
	public void push(MarchingCubesData mcd) {
		lock(inUse) {
			inUse.Remove(mcd);
		}
		lock(storage) {
			storage.Add(mcd);
		}
	}
}

