using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceIntersection
{
    public Edge e;
    public Vector2 intersection;
    public Vertex vertexAtIntersection;
    public bool makeNewStuff = true;

    public FaceIntersection() { }
    
    public Vertex CreateNewVertex()
    {
        vertexAtIntersection = GameObject.Instantiate(Paper.Instance.VertexPrefab, intersection, Quaternion.identity, Paper.Instance.transform).GetComponent<Vertex>();
        return vertexAtIntersection;
    }

    public Edge CreateNewEdge(Vertex movingVertex)
    {
        return new Edge(movingVertex, vertexAtIntersection);
    }

    public void UpdateExistingEdge(Vertex nonMovingVertex)
    {
        e.UpdateEdge(nonMovingVertex, vertexAtIntersection);
    }
}
