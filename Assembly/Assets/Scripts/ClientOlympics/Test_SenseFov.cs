using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Test_SenseFov : ClientTest
{

    List<Assembly> testAssemblies = new List<Assembly>();
    float maxSenseFov = -1.0f;
    int maxSenseFovIdx = 0;
    int testIdx = 0;
    int testNodeIdx = 0;
    float testDelay = 0.1f;
    float delayDuration = 0.0f;

    Color highlightColor = new Color(1.0f, 42.0f/255.0f, 91.0f/255.0f, 70.0f/255.0f);
    Color origColor = Color.white;

    List<Node> testNodes = new List<Node>();

    protected override void Awake()
    {
        base.Awake();
        nodePower = 0.05f;
        unlockFrameRate = false;
    }

    void Start()
    {
        testAssemblies = Assembly.getAll;

		winner = testAssemblies[0];
        StartAssemblyTest(testIdx);
    }

    protected override void Update()
    {
        base.Update();
        delayDuration += Time.deltaTime;

        if (delayDuration >= testDelay)
            UpdateTest();

        if ((testIdx >= testAssemblies.Count) || IsDone)
            EndTest();
    } // End of Update().

    void StartAssemblyTest(int idx)
    {
        if (testAssemblies.Count <= idx)
            return;

        testNodes.Clear();

        foreach(KeyValuePair<Triplet, Node> kvp in testAssemblies[idx].NodeDict)
        {
            if( kvp.Value.IsSense )
            {
                testNodes.Add(kvp.Value);
                if (maxSenseFov < kvp.Value.Properties.fieldOfView)
                {
                    maxSenseFov = kvp.Value.Properties.fieldOfView;
                    maxSenseFovIdx = idx;
                    winner = testAssemblies[idx];
                }

                //if( origColor == Color.white )
                    //origColor = kvp.Value.ViewCone.gameObject.renderer.material.GetColor("_TintColor");
            }
        }
        testNodeIdx = 0;
    }

    void UpdateTest()
    {
        if (testNodes.Count == 0)
        {
			testIdx++;
            StartAssemblyTest(testIdx);
            delayDuration = 0.0f;
            return;
        }

        if (testNodeIdx < testNodes.Count)
        {
            //testNodes[testNodeIdx].ViewCone.gameObject.renderer.material.SetColor("_TintColor", highlightColor);
            if(PersistentGameManager.EmbedViewer)
                testNodes[testNodeIdx].viewer.viewConeSize *= 2f;
        }

        if (testNodeIdx > 0)
        {
            //testNodes[testNodeIdx-1].ViewCone.gameObject.renderer.material.SetColor("_TintColor", origColor);
            if (PersistentGameManager.EmbedViewer)
                testNodes[testNodeIdx-1].viewer.viewConeSize *= 0.5f;
        }

        if (testNodeIdx >= testNodes.Count){
			testIdx++;
            StartAssemblyTest(testIdx);
		}
        else
            testNodeIdx++;
        delayDuration = 0.0f;
    }

}
