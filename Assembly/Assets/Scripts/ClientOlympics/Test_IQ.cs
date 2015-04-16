using UnityEngine;
using System.Collections;

public class Test_IQ : ClientTest
{

    void Awake()
    {
        base.Awake();
        nodePower = 0.2f;
        testDuration = 500; // frames
    }

	void Start () {
        float foodDistAway = 20.0f;
        new FoodPellet(transform.position + foodDistAway * transform.forward);
		foreach(Assembly someAssembly in Assembly.getAll)
			someAssembly.energy = 1f;
	}

    protected override void Update()
    {
        base.Update();

        if (IsDone)
        {
            AssignWinnerByHighestEnergy();
            EndTest();
        }
    } // End of Update().

    protected override void EndTest()
    {
        FoodPellet.DestroyAll();
        base.EndTest();
    }
}
