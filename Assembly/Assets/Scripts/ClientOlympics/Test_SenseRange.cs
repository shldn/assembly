﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Test_SenseRange : ClientTest
{

    PhysAssembly[] testAssemblies = null;
    float maxSenseRange = -1.0f;
    int maxSenseRangeIdx = 0;
    int testIdx = 0;
    int testNodeIdx = 0;
    float testDelay = 0.1f;
    float delayDuration = 0.0f;

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
		winner = testAssemblies[0];
        StartAssemblyTest(testIdx);
    }

    protected override void Update()
    {
        base.Update();

        delayDuration += Time.deltaTime;

        if (delayDuration >= testDelay)
            UpdateTest();

        if ((testIdx >= testAssemblies.Length) || IsDone)
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
                if( maxSenseRange < kvp.Value.nodeProperties.senseRange)
                {
                    maxSenseRange = kvp.Value.nodeProperties.senseRange;
                    maxSenseRangeIdx = idx;
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
        {
            StartAssemblyTest(++testIdx);
            delayDuration = 0.0f;
            return;
        }

        if (testNodeIdx < testNodes.Count)
        {
            testNodes[testNodeIdx].ViewCone.gameObject.renderer.material.SetColor("_TintColor", highlightColor);
            testNodes[testNodeIdx].viewConeSize *= 2f;
        }

        if (testNodeIdx > 0)
        {
            testNodes[testNodeIdx - 1].ViewCone.gameObject.renderer.material.SetColor("_TintColor", origColor);
            testNodes[testNodeIdx - 1].viewConeSize *= 0.5f;
        }

        if (testNodeIdx >= testNodes.Count)
            StartAssemblyTest(++testIdx);
        else
            ++testNodeIdx;
        delayDuration = 0.0f;
    }

}
