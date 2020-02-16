using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoldInformation
{
    private FaceIntersection faceIntersection0 = null, faceIntersection1 = null;
    public FaceIntersection FaceIntersection0
    {
        get => faceIntersection0;
    }
    public FaceIntersection FaceIntersection1
    {
        get => faceIntersection1;
    }

    public Edge newEdge;

    public FoldInformation()
    {

    }

    /// <summary>
    /// Each involved face always has two intersections with the foldline
    /// </summary>
    /// <param name="e"></param>
    /// <param name="intersection"></param>
    /// <returns>Returns true if this foldInformation instance is complete; it needs to intersect with 2 edges. If something tries to add a third, an error is thrown.</returns>
    public bool NewFaceIntersection(Edge e, Vector2 intersection)
    {
        if (faceIntersection0 == null)
        {
            faceIntersection0 = new FaceIntersection();
            faceIntersection0.e = e;
            faceIntersection0.intersection = intersection;
            return false;
        }
        else if (faceIntersection1 == null)
        {
            faceIntersection1 = new FaceIntersection();
            faceIntersection1.e = e;
            faceIntersection1.intersection = intersection;
            return true;
        }
        else
        {
            Debug.LogError("Tried to add a third faceIntersection instance to this FoldInformation!");
            return false;
        }
    }

    public bool NewFaceIntersection(Vertex v)
    {
        if (faceIntersection0 == null)
        {
            faceIntersection0 = new FaceIntersection();
            faceIntersection0.vertexAtIntersection = v;
            faceIntersection0.makeNewStuff = false;
            return false;
        }
        else if (faceIntersection1 == null)
        {
            faceIntersection1 = new FaceIntersection();
            faceIntersection1.vertexAtIntersection = v;
            faceIntersection1.makeNewStuff = false;
            return true;
        }
        else
        {
            Debug.LogError("Tried to add a third faceIntersection instance to this FoldInformation!");
            return false;
        }
    }

    public Edge CreateNewEdge()
    {
        newEdge = new Edge(faceIntersection0.vertexAtIntersection, faceIntersection1.vertexAtIntersection);
        return newEdge;
    }
}
