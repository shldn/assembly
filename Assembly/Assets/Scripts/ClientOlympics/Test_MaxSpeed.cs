using UnityEngine;
using System.Collections;

// Create this at the location of the cluster spawn. It will cull Assemblies after the test.
public class Test_MaxSpeed : ClientTest {


	// Use this for initialization
	protected override void Awake(){
		base.Awake();
        testDuration = 250; // frames
		foreach(Assembly someAssem in AssemblyEditor.Inst.testAssemblies)
			someAssem.distanceCovered = 0f;
	} // End of Awake().
	

	// Update is called once per frame
	protected override void Update(){
		base.Update();

		if(IsDone){
			float furthestDistance = 0f;
			foreach(Assembly someAssem in AssemblyEditor.Inst.testAssemblies) {
				if(someAssem.distanceCovered > furthestDistance){
					furthestDistance = someAssem.distanceCovered;
					winner = someAssem;
				}
			}

            EndTest();
		}
	} // End of Update().

} // End of Test_TopSpeed.
