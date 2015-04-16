using UnityEngine;
using System.Collections.Generic;

public class Test_IQ : ClientTest
{
    Vector3[] targetDir = { Vector3.forward, Vector3.up, -2f * Vector3.forward, Vector3.right };
    int[] targetEndFrame = {100, 200, 400, 500};
    int targetIdx = 0;

    Quaternion rotation = Quaternion.identity;

    void Awake()
    {
        base.Awake();
        nodePower = 0.2f;
        testDuration = 500; // frames
        unlockFrameRate = false;
    }

	void Start () {
        rotation = GetRotationToFirstSense();
        AddFoodToAllSenseCenters(10f);
        AddFoodToAllSenseCenters(30f, true, false);
	}

    protected override void Update()
    {
        base.Update();
        if(targetEndFrame[targetIdx] <= runTime && targetIdx < targetDir.Length-1)
           MoveAllFood(15 * (rotation * targetDir[++targetIdx]));

        if (IsDone || FoodPellet.all.Count == 0)
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

    void MoveAllFood(Vector3 offset)
    {
        foreach (FoodPellet p in FoodPellet.all)
            p.WorldPosition += offset;
    }

    Quaternion GetRotationToFirstSense()
    {
        foreach (Assembly someAssembly in Assembly.getAll)
            if (!someAssembly.cull)
                foreach (KeyValuePair<Triplet, Node> kvp in someAssembly.NodeDict)
                    if (kvp.Value.IsSense)
                        return Quaternion.FromToRotation(Vector3.forward, kvp.Value.SenseForward);
        return Quaternion.identity;
    }

    void AddFoodAtCenterOfSense(Assembly a, float dist = 10f, bool setAsOwner = true, bool hide = false)
    {
        foreach (KeyValuePair<Triplet, Node> kvp in a.NodeDict)
            if (kvp.Value.IsSense)
            {
                FoodPellet f = new FoodPellet(kvp.Value.Position + dist * (kvp.Value.SenseForward), setAsOwner ? kvp.Value.PhysAssembly : null);
                f.gameObject.renderer.enabled = !hide;
                Renderer[] renderers = f.gameObject.GetComponentsInChildren<Renderer>();
                for (int i = 0; i < renderers.Length; ++i)
                    renderers[i].enabled = !hide;
            }
    } // End of AddFoodAtCenterOfSense().

    void AddFoodToAllSenseCenters(float dist = 10f, bool setAsOwner = true, bool clearFood = true, bool showOnlyOne = true)
    {
        if (clearFood)
            FoodPellet.DestroyAll();
        int count = 0;
        foreach (Assembly someAssembly in Assembly.getAll)
        {
            if (!someAssembly.cull)
                AddFoodAtCenterOfSense(someAssembly, dist, setAsOwner, showOnlyOne && count != 0);
            ++count;
        }
    } // End of AddFoodToAllSenseCenters().
}
