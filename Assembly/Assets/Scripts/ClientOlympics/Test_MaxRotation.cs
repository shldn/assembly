using UnityEngine;
using System.Collections.Generic;

public class Test_MaxRotation : ClientTest {

    Dictionary<Assembly, Trail> assemblyTrails = new Dictionary<Assembly, Trail>();
    bool initialized = false;

    protected override void Awake()
    {
        base.Awake();
        nodePower = 0f;
        testDuration = 500; // frames
        unlockFrameRate = false;
    }

    protected void Start()
    {
        InvokeRepeating("AddFoodToAllPeriphery", 0.1f, 0.67f);
    }

    protected override void Update()
    {

        base.Update();

        AddToTrails();

        if (IsDone)
        {
            float maxEnergy = -999999999f;

            foreach (Assembly someAssem in Assembly.getAll)
            {
                if (someAssem.energy > maxEnergy)
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
        FoodPellet.DestroyAll();
        base.EndTest();
    } // End of EndTest().

    void AddToTrails()
    {
        foreach (KeyValuePair<Assembly,Trail> kvp in assemblyTrails)
            kvp.Value.Add(kvp.Key.Position);

    } // End of AddToTrails().

    void AddFoodToAllPeriphery()
    {
        FoodPellet.DestroyAll();
        foreach (Assembly someAssembly in Assembly.getAll)
        {
            if (!someAssembly.cull)
                AddFoodAtPeriphery(someAssembly);
        }
    }

    void AddFoodAtPeriphery(Assembly a)
    {
        float percentOfRange = 0.1f;
        float percentOfFov = 1f;
        bool addToCenter = false;

        foreach (KeyValuePair<Triplet, Node> kvp in a.NodeDict)
        {
            if( kvp.Value.IsSense )
            {
                if( addToCenter )
                    new FoodPellet(kvp.Value.Position + percentOfRange * kvp.Value.nodeProperties.senseRange * (kvp.Value.SenseForward));
                else
                {
                    // map offsets to the x-z plane
                    Vector3 axisOfRotation = (kvp.Value.SenseForward != Vector3.up) ? Vector3.Cross(kvp.Value.SenseForward, Vector3.up) : Vector3.Cross(kvp.Value.SenseForward, Vector3.right);
                    Quaternion rotOffset = Quaternion.AngleAxis(0.5f * percentOfFov * kvp.Value.nodeProperties.fieldOfView, axisOfRotation);
                    Vector3 foodPos = kvp.Value.Position + percentOfRange * kvp.Value.nodeProperties.senseRange * (rotOffset * kvp.Value.SenseForward);
                    new FoodPellet(foodPos, kvp.Value.PhysAssembly);
                }
            }
        }
    }

}
