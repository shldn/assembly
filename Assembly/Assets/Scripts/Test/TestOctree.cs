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
        if (!TestMaintainStillReturnsMovedElement())
            Debug.LogError("TestMaintainStillReturnsMovedElement Failed");
        if (!TestInsertOutOfBoundary())
            Debug.LogError("TestInsertOutOfBoundary Failed");
        if (!TestInsertOutOfBoundaryAndInBoundary())
            Debug.LogError("TestInsertOutOfBoundaryAndInBoundary Failed");
        if (!TestRemoveMovedElement())
            Debug.LogError("TestRemoveMovedElement Failed");
        if (!TestRemoveMovedElementFromOutsideBounds())
            Debug.LogError("TestRemoveMovedElementFromOutsideBounds Failed");
        if (!TestRemoveMovedElementFromInsideToOutsideBounds())
            Debug.LogError("TestRemoveMovedElementFromInsideToOutsideBounds failed");
        if (!TestRemoveMovedElementFromLastSubTree())
            Debug.LogError("TestRemoveMovedElementFromLastSubTree failed");
        if (!TestSubdivingPointsAtSamePositionDoesntCauseStackOverflow())
            Debug.LogError("TestSubdivingPointsAtSamePositionDoesntCauseStackOverflow failed");

        // Stop editor
        UnityEditor.EditorApplication.isPlaying = false;
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

    static private bool TestMaintainStillReturnsMovedElement() {
        Bounds bounds = new Bounds(Vector3.zero, new Vector3(100, 100, 100));
        Octree<TestClass> tree = new Octree<TestClass>(bounds, (TestClass x) => x.pos, 1);
        TestClass tc = new TestClass(new Vector3(1, 1, 1));
        tree.Insert(tc);
        // Make sure tree is subdivided
        tree.Insert(new TestClass(new Vector3(-1, -1, -1)));
        tc.pos.x = -50;
        tree.Maintain();
        List<TestClass> elems = new List<TestClass>();
        tree.GetElementsInRange(new Bounds(new Vector3(-50,1,1), new Vector3(0.5f, 0.5f, 0.5f)), ref elems);
        return elems.Count == 1;
    }

    static private bool TestInsertOutOfBoundary()
    {
        Bounds bounds = new Bounds(Vector3.zero, new Vector3(100, 100, 100));
        Octree<TestClass> tree = new Octree<TestClass>(bounds, (TestClass x) => x.pos, 1);
        TestClass tc = new TestClass(new Vector3(101, 1, 1));
        tree.Insert(tc);
        List<TestClass> elems = new List<TestClass>();
        tree.GetElementsInRange(new Bounds(new Vector3(101, 1, 1), new Vector3(0.5f, 0.5f, 0.5f)), ref elems);
        return elems.Count == 1 && elems[0] == tc;
    }

    static private bool TestInsertOutOfBoundaryAndInBoundary()
    {
        Bounds bounds = new Bounds(Vector3.zero, new Vector3(100, 100, 100));
        Octree<TestClass> tree = new Octree<TestClass>(bounds, (TestClass x) => x.pos, 1);
        TestClass tc = new TestClass(new Vector3(101, 1, 1));
        tree.Insert(tc);
        tree.Insert(new TestClass(new Vector3(1, 0, 0)));
        List<TestClass> elems = new List<TestClass>();
        tree.GetElementsInRange(new Bounds(new Vector3(1, 1, 1), new Vector3(200, 200, 200)), ref elems);
        return elems.Count == 2;
    }

    static private bool TestRemoveMovedElement()
    {
        Bounds bounds = new Bounds(Vector3.zero, new Vector3(100, 100, 100));
        Octree<TestClass> tree = new Octree<TestClass>(bounds, (TestClass x) => x.pos, 1);
        TestClass tc = new TestClass(new Vector3(1, 1, 1));
        tree.Insert(tc);
        tc.pos.x = -1;
        List<TestClass> elems = new List<TestClass>();
        tree.GetElementsInRange(new Bounds(new Vector3(-1, 1, 1), new Vector3(0.5f, 0.5f, 0.5f)), ref elems);
        return tree.Remove(tc);
    }

    static private bool TestRemoveMovedElementFromOutsideBounds()
    {
        Bounds bounds = new Bounds(Vector3.zero, new Vector3(100, 100, 100));
        Octree<TestClass> tree = new Octree<TestClass>(bounds, (TestClass x) => x.pos, 1);
        TestClass tc = new TestClass(new Vector3(200, 1, 1));
        tree.Insert(tc);
        // Subdivide tree
        tree.Insert(new TestClass(new Vector3(1, 2, 1)));
        tree.Insert(new TestClass(new Vector3(-1, 1, 1)));
        tree.Insert(new TestClass(new Vector3(-1, -1, 1)));
        tree.Insert(new TestClass(new Vector3(-1, -1, -1)));
        tc.pos.x = -50;
        tree.Maintain();
        return tree.Remove(tc);
    }

    static private bool TestRemoveMovedElementFromInsideToOutsideBounds()
    {
        Bounds bounds = new Bounds(Vector3.zero, new Vector3(100, 100, 100));
        Octree<TestClass> tree = new Octree<TestClass>(bounds, (TestClass x) => x.pos, 1);
        TestClass tc = new TestClass(new Vector3(50, 1, 1));
        tree.Insert(tc);
        // Subdivide tree
        tree.Insert(new TestClass(new Vector3(1, 2, 1)));
        tree.Insert(new TestClass(new Vector3(-1, 1, 1)));
        tree.Insert(new TestClass(new Vector3(-1, -1, 1)));
        tree.Insert(new TestClass(new Vector3(-1, -1, -1)));
        tc.pos.x = 200;
        tree.Maintain();
        return tree.Remove(tc);
    }

    static private bool TestRemoveMovedElementFromLastSubTree()
    {
        Bounds bounds = new Bounds(Vector3.zero, new Vector3(100, 100, 100));
        Octree<TestClass> tree = new Octree<TestClass>(bounds, (TestClass x) => x.pos, 2);

        // Subdivide tree
        float f = 25.0f;
        tree.Insert(new TestClass(new Vector3(f, f, f)));
        tree.Insert(new TestClass(new Vector3(f, -f, f)));
        tree.Insert(new TestClass(new Vector3(f, f, -f)));
        tree.Insert(new TestClass(new Vector3(f, -f, -f)));
        tree.Insert(new TestClass(new Vector3(-f, f, f)));
        tree.Insert(new TestClass(new Vector3(-f, -f, f)));

        TestClass tc = new TestClass(new Vector3(-f, -f, -f));
        tree.Insert(tc);
        tc.pos.x = f;
        tree.Maintain();
        return tree.Remove(tc);
    }

    static private bool TestSubdivingPointsAtSamePositionDoesntCauseStackOverflow()
    {
        Bounds bounds = new Bounds(Vector3.zero, new Vector3(100, 100, 100));
        Octree<TestClass> tree = new Octree<TestClass>(bounds, (TestClass x) => x.pos, 2);
        
        tree.Insert(new TestClass(new Vector3(1, 1, 1)));
        tree.Insert(new TestClass(new Vector3(1, 1, 1)));
        tree.Insert(new TestClass(new Vector3(1, 1, 1)));
        tree.Insert(new TestClass(new Vector3(1, 1, 1)));

        List<TestClass> elems = new List<TestClass>();
        tree.GetElementsInRange(new Bounds(new Vector3(1, 1, 1), new Vector3(0.5f, 0.5f, 0.5f)), ref elems);
        return elems.Count == 4;
    }
}
