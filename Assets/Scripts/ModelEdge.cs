using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelEdge
{
    public ModelVertex v1, v2;

    private Linepiece linepiece;
    private LineRenderer lineRenderer;
    private TextMesh debugText;
    private GameObject lineObject;
    private bool visible = false;

    public int number;

    public ModelEdge(ModelVertex v1, ModelVertex v2)
    {
        Debug.LogFormat("Creating new edge between {0} and {1}", v1.number, v2.number);

        this.v1 = v1;
        this.v2 = v2;

        v1.AddEdge(this);
        v2.AddEdge(this);

        linepiece = CalculateLinepieceValues();
        lineObject = new GameObject();

        debugText = lineObject.AddComponent<TextMesh>();
        debugText.color = Color.black;
        debugText.characterSize = 0.2f;

        lineObject.name = "EdgeDebugObject";
        lineRenderer = lineObject.AddComponent<LineRenderer>();
        lineRenderer.material = Paper.Instance.LineMaterial;
        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.05f;
        UpdateLineRenderer(null);
        Hide();

        //lineObject.transform.position = (v1.transform.position + v2.transform.position) / 2f;
        lineObject.transform.position = (v1.GetModelPosition() + v2.GetModelPosition()) / 2f;

        Paper.Instance.NewEdge(this, out number);
    }

    ~ModelEdge()
    {
        GameObject.Destroy(lineObject);
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
        visible = true;
        lineRenderer.enabled = true;
        debugText.gameObject.SetActive(true);
    }

    public void Hide()
    {
        visible = false;
        lineRenderer.enabled = false;
        debugText.gameObject.SetActive(false);
    }

    public ViewEdge GetViewEdge()
    {
        ViewEdge viewEdge = new ViewEdge();

        return viewEdge;
    }

    public void UpdateLineRenderer(ModelVertex source)
    {
        if (source != null)
        {
            if (source != v1 && source != v2)
            {
                Debug.LogErrorFormat("Vertex with number {0} tried to update edge's renderer with number {1}", source.number, number);
            }
        }
        lineRenderer.SetPositions(new Vector3[2] { v1.GetModelPosition(), v2.GetModelPosition() });
        UpdateLinepiece();
    }

    public void GiveColor(Color color)
    {
        lineRenderer.material.color = color;
    }

    public void UpdateLinepiece()
    {
        linepiece = CalculateLinepieceValues();
    }

    public void UpdateText()
    {
        debugText.text = string.Format("{0}={1}:{2}", number, v1.number, v2.number);
    }

    public void UpdateEdge(ModelVertex v1, ModelVertex v2)
    {
        Debug.LogFormat("Edge {0} contains vertices {1} and {2}, checking for {3} and {4}", number, this.v1.number, this.v2.number, v1.number, v2.number);
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
        
        UpdateLineRenderer(null);
        lineObject.transform.position = (v1.GetModelPosition() + v2.GetModelPosition()) / 2f;
    }

    public bool IsMainVertex(ModelVertex v)
    {
        return v == v1;
    }

    public Linepiece GetLinepiece()
    {
        return linepiece;
    }

    public IEnumerator Flash()
    {
        int flashes = 8;
        while(flashes > 0)
        {
            if (flashes%2==0)
            {
                lineRenderer.material.color = Color.white;
            }
            else
            {
                lineRenderer.material.color = Color.red;
            }
            yield return new WaitForSeconds(0.5f);
            flashes--;
        }
        lineRenderer.material.color = Color.white;
    }

    private Linepiece CalculateLinepieceValues()
    {
        return new Linepiece(v1.GetModelPosition(), v2.GetModelPosition());
    }
}
