using UnityEngine;
using System.Collections;

public class Test_IQ : ClientTest
{

    void Awake()
    {
        base.Awake();
        nodePower = 0.2f;
        testDuration = 1500; // frames
    }

	void Start () {
        float foodDistAway = 20.0f;
        new PhysFood(transform.position + foodDistAway * transform.forward);
		foreach(PhysAssembly someAssembly in PhysAssembly.getAll)
			someAssembly.energy = 1f;
	}

    protected override void Update()
    {
        base.Update();

        if (IsDone)
        {
            float maxEnergy = -1.0f;

            foreach (PhysAssembly someAssem in PhysAssembly.getAll)
            {
                if( someAssem.energy > maxEnergy )
                {
                    winner = someAssem;
                    maxEnergy = someAssem.energy;
                }
            }

            EndTest();
        }
    } // End of Update().

    protected override void EndTest()
    {
        PhysFood.DestroyAll();
        base.EndTest();
    }
}
