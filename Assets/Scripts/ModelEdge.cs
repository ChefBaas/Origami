using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelEdge
{
    public ModelVertex v1, v2;

    private Linepiece linepiece;
    private ViewEdge viewEdge;
    //private TextMesh debugText;

    public int number;

    public ModelEdge(ModelVertex v1, ModelVertex v2, ViewEdge viewEdge)
    {
        Debug.LogFormat("Creating new edge between {0} and {1}", v1.number, v2.number);

        this.v1 = v1;
        this.v2 = v2;

        v1.AddEdge(this);
        v2.AddEdge(this);

        //viewEdge = new ViewEdge();
        this.viewEdge = viewEdge;
        UpdateViewEdge();
        linepiece = CalculateLinepieceValues();

        /*debugText = lineObject.AddComponent<TextMesh>();
        debugText.color = Color.black;
        debugText.characterSize = 0.2f;*/

        Paper.Instance.NewEdge(this, out number);
    }

    public ModelVertex GetOther(ModelVertex v)
    {
        if (v == v1)
        {
            return v2;
        }
        else if (v == v2)
        {
            return v1;
        }
        else
        {
            Debug.LogErrorFormat("Edge {0} does not contain vertex {1}", number, v.number);
            return null;
        }
    }

    public bool HasVertex(ModelVertex v)
    {
        return v == v1 || v == v2;
    }

    public void Show()
    {
        viewEdge.Show();
    }

    public void Hide()
    {
        viewEdge.Hide();
    }

    public void Highlight(float duration)
    {
        Debug.LogFormat("Highlighting edge from {0} at {2} to {1} at {3}", v1.number, v2.number, v1.GetModelPosition(), v2.GetModelPosition());
        CoroutineStarter.Instance.StartCoroutine(viewEdge.Highlight(duration));
    }

    public ViewEdge GetViewEdge()
    {
        //ViewEdge viewEdge = new ViewEdge();

        return viewEdge;
    }

    public void UpdateLinepiece()
    {
        linepiece = CalculateLinepieceValues();
    }

    public void UpdateViewEdge()
    {
        viewEdge.SetPositions(v1.GetModelPosition(), v2.GetModelPosition());
    }

    /*public void UpdateText()
    {
        debugText.text = string.Format("{0}={1}:{2}", number, v1.number, v2.number);
    }*/

    public void UpdateEdge(ModelVertex v1, ModelVertex v2)
    {
        //Debug.LogFormat("Edge {0} contains vertices {1} and {2}, checking for {3} and {4}", number, this.v1.number, this.v2.number, v1.number, v2.number);
        if (v1 != this.v1 && v2 != this.v1)
        {
            this.v1.RemoveEdge(this);
        }
        if (v1 != this.v2 && v2 != this.v2)
        {
            this.v2.RemoveEdge(this);
        }
        this.v1 = v1;
        this.v2 = v2;
        v1.AddEdge(this);
        v2.AddEdge(this);

        UpdateLinepiece();
        UpdateViewEdge();
    }

    public Linepiece GetLinepiece()
    {
        return linepiece;
    }

    private Linepiece CalculateLinepieceValues()
    {
        // maybe not create a new instance every time, but update the existing one
        return new Linepiece(v1.GetModelPosition(), v2.GetModelPosition());
    }
}
