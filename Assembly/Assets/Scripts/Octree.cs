using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class OctreeNode<T>
{
    public OctreeNode(T elem_, Vector3 pos_) {
        elem = elem_;
        position = pos_;
    }
    public T elem;
    public Vector3 position;
}

// Octree
//
// Data structure to hold objects in spacial proximity to optimize searching for nearby objects.
// Note: If a node is inserted outside the tree boundary, Root will hold these nodes.
public class Octree<T>{

    static bool nodesOnlyInLeaves = true;
    static bool enableViewer = false;
    int maxNodesPerLevel;
    Bounds boundary;
    List<T> nodes;
    Octree<T> parent = null; // if null this is the root of the tree.
    Octree<T>[] children;
    LinkedList<Octree<T>> leaves; // should only be populated for the root node, will help us maintain changes in positions efficiently, can't have a sorted data structure with just keys in C#?
    Func<T, Vector3> GetPosition; // TODO: optimization - grab function from the root instead of each node holding a pointer to it?

    public Octree(Bounds bounds, Func<T, Vector3> GetPosition_, int maxNodesPerLevel_ = 10, Octree<T> parent_ = null)
    {
        boundary = bounds;
        GetPosition = GetPosition_;
        maxNodesPerLevel = maxNodesPerLevel_;        
        nodes = new List<T>(maxNodesPerLevel);
        parent = parent_;
        if (IsRoot)
            leaves = new LinkedList<Octree<T>>();
        if (enableViewer)
            OctreeViewer.Inst.AddBounds(bounds);
    }


    public Octree<T> GetRoot() {
        if (parent == null)
            return this;
        return parent.GetRoot();
    }


    public bool Insert(T elem)
    {
        if (!boundary.Contains(GetPosition(elem)))
        {
            if (IsRoot)
            {
                nodes.Add(elem);
                return true;
            }
            else
                return false;
        }
        if (IsLeaf && nodes.Count < maxNodesPerLevel) {
            nodes.Add(elem);
            return true;
        }
        if (IsLeaf)
            Subdivide();
        for (int i = 0; i < children.Length; ++i) {
            if (children[i].Insert(elem))
                return true;
        }
        return false;
    }

    public bool Remove(T elem, bool checkPos = true) {
        if (checkPos && !boundary.Contains(GetPosition(elem)) && !IsRoot)
            return false;
        if (nodes.Remove(elem))
            return true;
        if( !IsLeaf )
            for (int i = 0; i < children.Length; ++i)
                if (children[i].Remove(elem))
                    return true;
        return false;

    }


    // This checks all the nodes and reorganizes them if they have moved outside their boundaries. 
    // Would be better at some point to only do this for the nodes that we know have changed position.
    public void Maintain() {
        if (!IsRoot) {
            Debug.LogError("Octree: Please only call Maintain on the Root node");
            return;
        }

        if( leaves == null || leaves.Count == 0 )
            return;

        LinkedListNode<Octree<T>> current = leaves.First;
        while(current != leaves.Last)
        {
            current.Value.MaintainImpl();
            if (!current.Value.IsLeaf) {
                LinkedListNode<Octree<T>> toRemove = current;
                current = current.Next;
                leaves.Remove(toRemove);
            }
            else
                current = current.Next;
        }
    }

    private void MaintainImpl() {
        if (!IsLeaf)
            return;
        for (int i = 0; i < nodes.Count; ++i) {
            if (!boundary.Contains(GetPosition(nodes[i]))) {
                GetRoot().Insert(nodes[i]); // TODO: this can be greatly optimized
                nodes.RemoveAt(i);
                --i; // don't miss the next element in the loop after the removal
            }
        }
    }


    // Runs a function on a subset of the elements in the tree, determined by what is inside subsetBounds
    // more efficient than returning a list of elements in range and then applying the function on them
    // use this when you can, no copying
    public void RunActionInRange(Action<T> action, Bounds subsetBounds) {
        if (!boundary.Intersects(subsetBounds) && !IsRoot)
            return;
        for (int i = 0; i < nodes.Count; ++i)
            if (subsetBounds.Contains(GetPosition(nodes[i])))
                action(nodes[i]);
        if (IsLeaf)
            return;
        for (int i = 0; i < children.Length; ++i)
            children[i].RunActionInRange(action, subsetBounds);
    }


    // Gets all the elements within the subsetBounds.
    // prefer RunActionInRange to avoid the copy of elements from the octree to the list
    public void GetElementsInRange(Bounds subsetBounds, ref List<T> elements) {

        if (!boundary.Intersects(subsetBounds) && !IsRoot) // Root may have nodes outside the boundary
            return;
        for (int i = 0; i < nodes.Count; ++i)
            if (subsetBounds.Contains(GetPosition(nodes[i])))
                elements.Add(nodes[i]);
        if (IsLeaf)
            return;
        for (int i = 0; i < children.Length; ++i)
            children[i].GetElementsInRange(subsetBounds, ref elements);
    }


    void Subdivide() {
        children = new Octree<T>[8]; // 8 children for octree

        Vector3 newSize = 0.5f * boundary.size;
        float hw = 0.5f * newSize.x;
        children[0] = new Octree<T>(new Bounds(boundary.center + new Vector3(hw, hw, hw), newSize), GetPosition, maxNodesPerLevel, this);
        children[1] = new Octree<T>(new Bounds(boundary.center + new Vector3(hw, -hw, hw), newSize), GetPosition, maxNodesPerLevel, this);
        children[2] = new Octree<T>(new Bounds(boundary.center + new Vector3(hw, hw, -hw), newSize), GetPosition, maxNodesPerLevel, this);
        children[3] = new Octree<T>(new Bounds(boundary.center + new Vector3(hw, -hw, -hw), newSize), GetPosition, maxNodesPerLevel, this);
        children[4] = new Octree<T>(new Bounds(boundary.center + new Vector3(-hw, hw, hw), newSize), GetPosition, maxNodesPerLevel, this);
        children[5] = new Octree<T>(new Bounds(boundary.center + new Vector3(-hw, -hw, hw), newSize), GetPosition, maxNodesPerLevel, this);
        children[6] = new Octree<T>(new Bounds(boundary.center + new Vector3(-hw, hw, -hw), newSize), GetPosition, maxNodesPerLevel, this);
        children[7] = new Octree<T>(new Bounds(boundary.center + new Vector3(-hw, -hw, -hw), newSize), GetPosition, maxNodesPerLevel, this);

        if (nodesOnlyInLeaves) {
            if( IsRoot )
            {
                // Handle keeping nodes outside of the boundary in the root node list.
                for (int i = nodes.Count-1; i >= 0; --i)
                    if (boundary.Contains(GetPosition(nodes[i])))
                    {
                        Insert(nodes[i]);
                        nodes.RemoveAt(i);
                    }
            }
            else
            {
                for (int i = 0; i < nodes.Count; ++i)
                    Insert(nodes[i]);
                nodes.Clear();
            }
            //GetRoot().leaves.Remove(this); // let the Maintanence step do this for now.
        }

        if( enableViewer )
        {
            for (int i = 0; i < 8; ++i )
                OctreeViewer.Inst.AddBounds(children[i].boundary);
        }
    }

    // Accessors
    public bool IsRoot { get { return parent == null; } }
    public bool IsLeaf { get { return children == null; } }
}
