using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TestClass
{
    public TestClass(Vector3 pos_) { pos = pos_; }
    public Vector3 pos;
}

public class TestOctree {

    static public void RunTests() {

        if (!TestMultipleInsert())
            Debug.LogError("TestMultipleInsert Failed");
        if (!TestGetAllElements())
            Debug.LogError("TestGetAllElements Failed");
    }

    static private bool TestMultipleInsert() {
        bool success = true;
        Octree<TestClass> tree = new Octree<TestClass>(new Bounds(Vector3.zero, new Vector3(100, 100, 100)), (TestClass x) => x.pos, 1);
        success &= tree.Insert(new TestClass(new Vector3(1, 1, 1)));
        success &= tree.Insert(new TestClass(new Vector3(-1, 1, 1)));
        success &= tree.Insert(new TestClass(new Vector3(-1, -1, 1)));
        success &= tree.Insert(new TestClass(new Vector3(-1, -1, -1)));
        return success;
    }

    static private bool TestGetAllElements() {
        Bounds bounds = new Bounds(Vector3.zero, new Vector3(100, 100, 100));
        Octree<TestClass> tree = new Octree<TestClass>(bounds, (TestClass x) => x.pos, 1);
        tree.Insert(new TestClass(new Vector3(1, 1, 1)));
        tree.Insert(new TestClass(new Vector3(-1, 1, 1)));
        tree.Insert(new TestClass(new Vector3(-1, -1, 1)));
        tree.Insert(new TestClass(new Vector3(-1, -1, -1)));
        List<TestClass> elems = new List<TestClass>();
        tree.GetElementsInRange(bounds, ref elems);
        return elems.Count == 4;
    }
}
