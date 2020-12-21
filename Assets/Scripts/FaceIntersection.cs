using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceIntersection
{
    public Vector2 intersection;
    public ModelVertex modelVertexAtIntersection;
    public ModelEdge modelEdge;
    public ViewVertex viewVertexAtIntersection;
    public bool makeNewStuff = true;

    public FaceIntersection() { }
    
    public ModelVertex CreateNewModelVertex()
    {
        //vertexAtIntersection = GameObject.Instantiate(Paper.Instance.VertexPrefab, intersection, Quaternion.identity, Paper.Instance.transform).GetComponent<ModelVertex>();
        modelVertexAtIntersection = new ModelVertex(intersection, GameObject.Instantiate(Paper.Instance.ViewVertexPrefab, Vector3.zero, Quaternion.identity, Paper.Instance.transform).GetComponent<ViewVertex>());
        return modelVertexAtIntersection;
    }

    public ViewVertex CreateNewViewVertex()
    {
        viewVertexAtIntersection = GameObject.Instantiate(Paper.Instance.ViewVertexPrefab, intersection, Quaternion.identity, Paper.Instance.transform).GetComponent<ViewVertex>();
        viewVertexAtIntersection.SetPosition(intersection);
        return viewVertexAtIntersection;
    }

    public ModelEdge CreateNewModelEdge(ModelVertex movingVertex)
    {
        return new ModelEdge(movingVertex, modelVertexAtIntersection, new ViewEdge());
    }

    public void UpdateExistingEdge(ModelVertex nonMovingModelVertex)
    {
        modelEdge.UpdateEdge(nonMovingModelVertex, modelVertexAtIntersection);
    }

    public ViewEdge CreateNewViewEdge(ViewVertex viewVertex)
    {
        ViewEdge newViewEdge = new ViewEdge();
        newViewEdge.SetPositions(viewVertex.transform.position, viewVertexAtIntersection.transform.position);
        return newViewEdge;
    }

    public void UpdateExistingViewEdge(ViewVertex nonMovingViewVertex)
    {
        modelEdge.GetViewEdge().SetPositions(nonMovingViewVertex.transform.position, viewVertexAtIntersection.transform.position);
    }
}
