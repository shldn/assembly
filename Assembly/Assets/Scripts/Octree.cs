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
public class Octree<T>{

    int maxNodesPerLevel;
    Bounds boundary;
    List<T> nodes;
    Octree<T> parent = null; // if null this is the root of the tree.
    Octree<T>[] children;
    Func<T, Vector3> GetPosition; // TODO: optimization - grab function from the root instead of each node holding a pointer to it?

    public Octree(Bounds bounds, Func<T, Vector3> GetPosition_, int maxNodesPerLevel_ = 10, Octree<T> parent = null)
    {
        boundary = bounds;
        GetPosition = GetPosition_;
        maxNodesPerLevel = maxNodesPerLevel_;        
        nodes = new List<T>(maxNodesPerLevel);
    }


    public Octree<T> GetRoot() {
        if (parent == null)
            return this;
        return parent.GetRoot();
    }


    public bool Insert(T elem)
    {
        if (!boundary.Contains(GetPosition(elem)))
            return false;
        if (nodes.Count < maxNodesPerLevel) {
            nodes.Add(elem);
            return true;
        }
        if (children == null)
            Subdivide();
        for (int i = 0; i < children.Length; ++i) {
            if (children[i].Insert(elem))
                return true;
        }
        return false;
    }

    public bool Remove(T elem) {
        if (!boundary.Contains(GetPosition(elem)))
            return false;
        if (nodes.Remove(elem))
            return true;
        for (int i = 0; i < children.Length; ++i)
            if (children[i].Remove(elem))
                return true;
        return false;

    }

    /*
    // this is the slow part :(, add parent Octree to OctreeNode to speed it up. Change OctreeNodes to structs
    // can do a more efficient remove and insert, since the insert will be near the remove point.
    public void Update(T elem) {
        Remove(node);
        node.position = newPos;
        Insert(node);
    }
    */


    // Runs a function on a subset of the elements in the tree, determined by what is inside subsetBounds
    // more efficient than returning a list of elements in range and then applying the function on them
    // use this when you can, no copying
    public void RunActionInRange(Action<T> action, Bounds subsetBounds) {
        if (!boundary.Intersects(subsetBounds))
            return;
        for (int i = 0; i < nodes.Count; ++i)
            if (subsetBounds.Contains(GetPosition(nodes[i])))
                action(nodes[i]);
        if (children == null)
            return;
        for (int i = 0; i < children.Length; ++i)
            children[i].RunActionInRange(action, subsetBounds);
    }


    // Gets all the elements within the subsetBounds.
    // prefer RunActionInRange to avoid the copy of elements from the octree to the list
    public void GetElementsInRange(Bounds subsetBounds, ref List<T> elements) {

        if (!boundary.Intersects(subsetBounds))
            return;
        for (int i = 0; i < nodes.Count; ++i)
            if (subsetBounds.Contains(GetPosition(nodes[i])))
                elements.Add(nodes[i]);
        if (children == null)
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
    }
}
