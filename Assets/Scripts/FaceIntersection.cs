using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceIntersection
{
    public ModelEdge e;
    public Vector2 intersection;
    public ModelVertex vertexAtIntersection;
    public bool makeNewStuff = true;

    public FaceIntersection() { }
    
    public ModelVertex CreateNewVertex()
    {
        //vertexAtIntersection = GameObject.Instantiate(Paper.Instance.VertexPrefab, intersection, Quaternion.identity, Paper.Instance.transform).GetComponent<ModelVertex>();
        vertexAtIntersection = new ModelVertex(intersection);
        return vertexAtIntersection;
    }

    public ModelEdge CreateNewEdge(ModelVertex movingVertex)
    {
        return new ModelEdge(movingVertex, vertexAtIntersection);
    }

    public void UpdateExistingEdge(ModelVertex nonMovingVertex)
    {
        e.UpdateEdge(nonMovingVertex, vertexAtIntersection);
    }
}
