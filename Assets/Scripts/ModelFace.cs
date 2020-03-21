using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelFace
{
    private int height = 0;
    public int Height
    {
        get => height;
        set => height = value;
    }

    public List<ModelVertex> vertices = new List<ModelVertex>();
    public List<ModelEdge> edges = new List<ModelEdge>();

    private bool performingFold = false;

    [HideInInspector]
    public int number = -1;

    public ModelFace()
    {
        Paper.Instance.NewFace(this, out number);
    }

    public ViewFace GetViewFace()
    {
        ViewFace viewFace = new ViewFace();

        return viewFace;
    }

    public void AddVertex(ModelVertex v)
    {
        if (!vertices.Contains(v))
        {
            vertices.Add(v);
            v.AddFace(this);
        }
    }

    public void RemoveVertex(ModelVertex v)
    {
        if (vertices.Contains(v))
        {
            vertices.Remove(v);
            v.RemoveFace(this);
            for (int i = 0; i < v.edges.Count; i++)
            {
                RemoveEdge(v.edges[i]);
            }
        }
    }

    public void AddEdge(ModelEdge e)
    {
        if (!edges.Contains(e))
        {
            edges.Add(e);
        }
    }

    public void RemoveEdge(ModelEdge e)
    {
        if (edges.Contains(e))
        {
            edges.Remove(e);
        }
    }

    public void Flash()
    {
        // TODO: MAKE THIS WORK AGAIN
        //CoroutineStarter.Instance.StartCoroutine(ShowComponents());
    }

    public void UpdateEdges()
    {
        for (int i = 0; i < vertices.Count; i++)
        {
            for (int j = 0; j < vertices[i].edges.Count; j++)
            {
                if (vertices.Contains(vertices[i].edges[j].GetOther(vertices[i])))
                {
                    AddEdge(vertices[i].edges[j]);
                }
            }
        }
    }

    /*private IEnumerator ShowComponents()
    {
        for (int i = 0; i < vertices.Count; i++)
        {
            vertices[i].GiveColor(Color.red);
            vertices[i].MoveToFront();
        }
        for (int i = 0; i < edges.Count; i++)
        {
            edges[i].GiveColor(Color.red);
            edges[i].UpdateLineRenderer(null);
        }
        yield return new WaitForSeconds(1f);
        for (int i = 0; i < vertices.Count; i++)
        {
            vertices[i].GiveColor(Color.white);
            vertices[i].MoveToBack();
        }
        for (int i = 0; i < edges.Count; i++)
        {
            edges[i].GiveColor(Color.white);
            edges[i].UpdateLineRenderer(null);
        }
    }*/
}
