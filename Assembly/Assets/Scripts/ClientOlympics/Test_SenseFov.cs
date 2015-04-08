using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Test_SenseFov : ClientTest
{

    PhysAssembly[] testAssemblies = null;
    float maxSenseFov = -1.0f;
    int maxSenseFovIdx = 0;
    int testIdx = 0;
    int testNodeIdx = 0;

    Color highlightColor = new Color(1.0f, 42.0f/255.0f, 91.0f/255.0f, 70.0f/255.0f);
    Color origColor = Color.white;

    List<PhysNode> testNodes = new List<PhysNode>();

    protected override void Awake()
    {
        base.Awake();
        nodePower = 0.05f;
        unlockFrameRate = false;
    }

    void Start()
    {
        testAssemblies = new PhysAssembly[PhysAssembly.getAll.Count];
        PhysAssembly.getAll.CopyTo(testAssemblies);
        StartAssemblyTest(testIdx);
        InvokeRepeating("UpdateTest", 0.5f, 0.1f);
    }

    protected override void Update()
    {
        base.Update();

        if (testIdx >= testAssemblies.Length)
            EndTest();

    } // End of Update().

    void StartAssemblyTest(int idx)
    {
        if (testAssemblies.Length <= idx)
            return;

        testNodes.Clear();

        foreach(KeyValuePair<Triplet, PhysNode> kvp in testAssemblies[idx].NodeDict)
        {
            if( kvp.Value.IsSense )
            {
                testNodes.Add(kvp.Value);
                if (maxSenseFov < kvp.Value.nodeProperties.fieldOfView)
                {
                    maxSenseFov = kvp.Value.nodeProperties.fieldOfView;
                    maxSenseFovIdx = idx;
                    winner = testAssemblies[idx];
                }

                if( origColor == Color.white )
                    origColor = kvp.Value.ViewCone.gameObject.renderer.material.GetColor("_TintColor");
            }
        }
        testNodeIdx = 0;
    }

    void UpdateTest()
    {
        if (testNodes.Count == 0)
            return;

        if (testNodeIdx < testNodes.Count)
            testNodes[testNodeIdx].ViewCone.gameObject.renderer.material.SetColor("_TintColor", highlightColor);
        if (testNodeIdx > 0)
            testNodes[testNodeIdx-1].ViewCone.gameObject.renderer.material.SetColor("_TintColor", origColor);

        if (testNodeIdx >= testNodes.Count)
            StartAssemblyTest(++testIdx);
        else
            ++testNodeIdx;
    }

}
