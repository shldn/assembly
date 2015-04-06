﻿using UnityEngine;
using System.Collections;

// Create this at the location of the cluster spawn. It will cull Assemblies after the test.
public class Test_MaxTravel : ClientTest {
	

	// Use this for initialization
	protected override void Awake(){
		base.Awake();
	} // End of Awake().
	

	// Update is called once per frame
	protected override void Update(){
		base.Update();

		if(runTime > 500f){
			float furthestDistance = 0f;
			PhysAssembly winner = null;
			foreach(PhysAssembly someAssem in PhysAssembly.getAll){
				float distance = Vector3.Distance(transform.position, someAssem.Position);
				if(distance > furthestDistance){
					furthestDistance = distance;
					winner = someAssem;
				}
			}

			foreach(PhysAssembly someAssem in PhysAssembly.getAll)
				if(someAssem != winner)
					someAssem.Destroy();
				else
					AssemblyEditor.Inst.capturedAssembly = someAssem;

			AssemblyEditor.Inst.testRunning = false;
			Destroy(gameObject);
		}
	} // End of Update().

} // End of Test_TopSpeed.