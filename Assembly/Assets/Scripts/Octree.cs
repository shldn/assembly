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
    List<OctreeNode<T>> nodes;
    List<Octree<T>> children; //= new List<Octree<T>>(8);

    public Octree(Bounds bounds, int maxNodesPerLevel_ = 10)
    {
        maxNodesPerLevel = maxNodesPerLevel_;
        boundary = bounds;
        nodes = new List<OctreeNode<T>>(maxNodesPerLevel);
    }


    public bool Insert(T elem, Vector3 position) {
        return Insert(new OctreeNode<T>(elem, position));
    }


    public bool Insert(OctreeNode<T> node)
    {
        if (!boundary.Contains(node.position))
            return false;
        if (nodes.Count < maxNodesPerLevel) {
            nodes.Add(node);
            return true;
        }
        if (children == null)
            Subdivide();
        for (int i = 0; i < children.Count; ++i) {
            if (children[i].Insert(node))
                return true;
        }
        return false;
    }

    public bool Remove(OctreeNode<T> node) {
        if (!boundary.Contains(node.position))
            return false;
        if (nodes.Remove(node))
            return true;
        for (int i = 0; i < children.Count; ++i)
            if (children[i].Remove(node))
                return true;
        return false;

    }


    // this is the slow part :(, add parent Octree to OctreeNode to speed it up. Change OctreeNodes to structs
    // can do a more efficient remove and insert, since the insert will be near the remove point.
    public void Update(OctreeNode<T> node, Vector3 newPos) {
        Remove(node);
        node.position = newPos;
        Insert(node);
    }


    // Runs a function on a subset of the elements in the tree, determined by what is inside subsetBounds
    // more efficient than returning a list of elements in range and then applying the function on them
    // use this when you can, no copying
    public void RunActionInRange(Action<T> action, Bounds subsetBounds) {
        if (!boundary.Intersects(subsetBounds))
            return;
        for (int i = 0; i < nodes.Count; ++i)
            if (subsetBounds.Contains(nodes[i].position))
                action(nodes[i].elem);
        if (children == null)
            return;
        for (int i = 0; i < children.Count; ++i)
            RunActionInRange(action, subsetBounds);
    }


    // Gets all the elements within the subsetBounds.
    // prefer RunActionInRange to avoid the copy of elements from the octree to the list
    public void GetElementsInRange(Bounds subsetBounds, ref List<T> elements) {

        if (!boundary.Intersects(subsetBounds))
            return;
        for (int i = 0; i < nodes.Count; ++i)
            if (subsetBounds.Contains(nodes[i].position))
                elements.Add(nodes[i].elem);
        if (children == null)
            return;
        for (int i = 0; i < children.Count; ++i)
            GetElementsInRange(subsetBounds, ref elements);
    }


    void Subdivide() {
        children = new List<Octree<T>>(8); // 8 children for octree

        Vector3 newSize = 0.5f * boundary.size;
        float hw = newSize.x;
        children[0] = new Octree<T>(new Bounds(boundary.center + new Vector3(hw, hw, hw), newSize));
        children[1] = new Octree<T>(new Bounds(boundary.center + new Vector3(hw, -hw, hw), newSize));
        children[2] = new Octree<T>(new Bounds(boundary.center + new Vector3(hw, hw, -hw), newSize));
        children[3] = new Octree<T>(new Bounds(boundary.center + new Vector3(hw, -hw, -hw), newSize));
        children[4] = new Octree<T>(new Bounds(boundary.center + new Vector3(-hw, hw, hw), newSize));
        children[5] = new Octree<T>(new Bounds(boundary.center + new Vector3(-hw, -hw, hw), newSize));
        children[6] = new Octree<T>(new Bounds(boundary.center + new Vector3(-hw, hw, -hw), newSize));
        children[7] = new Octree<T>(new Bounds(boundary.center + new Vector3(-hw, -hw, -hw), newSize));
    }
}
