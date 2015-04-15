using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Will keep track of each Assembly trail and analyze the amount of rotation at the end of the test.
public class Test_MaxRotation : ClientTest {

    Dictionary<Assembly, Trail> assemblyTrails = new Dictionary<Assembly, Trail>();

    protected override void Awake()
    {
        base.Awake();
        nodePower = 1.0f;
        testDuration = 500; // frames

        // Initialize trail tracker
        foreach (Assembly someAssem in Assembly.getAll)
            assemblyTrails.Add(someAssem, new Trail(someAssem.Position));
    }

    protected override void Update()
    {
        base.Update();

        AddToTrails();

        if (IsDone)
        {
            float maxRotation = -1f;
            foreach (KeyValuePair<Assembly, Trail> kvp in assemblyTrails)
            {
                float rotation = Mathf.Abs(kvp.Value.GetRotation());
                if( rotation > maxRotation)
                {
                    maxRotation = rotation;
                    winner = kvp.Key;
                }
            }
            EndTest();
        }
    } // End of Update().

    void AddToTrails()
    {
        foreach (KeyValuePair<Assembly,Trail> kvp in assemblyTrails)
            kvp.Value.Add(kvp.Key.Position);

    } // End of AddToTrails().

}
