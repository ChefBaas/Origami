using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelVertex
{
    /*[SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private BoxCollider boxCollider;
    [SerializeField] private GameObject ghostVertexPrefab;
    [SerializeField] private TextMesh debugText;*/

    public List<ModelEdge> edges = new List<ModelEdge>();
    public List<ModelFace> faces = new List<ModelFace>();
    private bool isOuterVertex = true;
    public bool IsOuterVertex
    {
        get => isOuterVertex;
    }
    /*private List<Edge> outerEdges;
    public List<Edge> OuterEdges
    {
        get => outerEdges;
    }*/

    /*private Vector3 lastPosition;
    private bool moving = false;
    private Coroutine fancyStuffCoroutine;
    private bool visible = false;*/

    private ViewVertex viewVertex;
    private Vector3 position;

    [HideInInspector]
    public int number = -1;

    public ModelVertex(Vector3 position)
    {
        this.position = position;
        //Hide();

        viewVertex = new ViewVertex();
        viewVertex.SetPosition(position);
        viewVertex.OnStartDrag += OnStartFold;
        viewVertex.OnStopDrag += OnStopFold;

        //lastPosition = transform.position;
        Paper.Instance.NewVertex(this, out number);
        //debugText.text = number.ToString();
        /*for (int i = 0; i < edges.Count; i++)
        {
            edges[i].UpdateText();
        }*/
    }
    
    /*void LateUpdate()
    {
        if (lastPosition != transform.position && visible)
        {
            for (int i = 0; i < edges.Count; i++)
            {
                edges[i].UpdateLineRenderer(this);
            }
        }
        lastPosition = transform.position;
    }*/

    /*public List<ModelVertex> GetAllNeighbours()
    {
        List<ModelVertex> neighbours = new List<ModelVertex>();
        for (int i = 0; i < edges.Count; i++)
        {
            neighbours.Add(edges[i].GetOther(this));
        }
        return neighbours;
    }*/

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

    public void AddFace(ModelFace f)
    {
        if (!faces.Contains(f))
        {
            faces.Add(f);
        }
    }

    public void RemoveFace(ModelFace f)
    {
        if (faces.Contains(f))
        {
            faces.Remove(f);
        }
    }

    /*public void Show()
    {
        visible = true;
        meshRenderer.enabled = true;
        boxCollider.enabled = true;
        debugText.gameObject.SetActive(true);
    }

    public void Hide()
    {
        visible = false;
        meshRenderer.enabled = false;
        boxCollider.enabled = false;
        debugText.gameObject.SetActive(false);
    }*/

    public ViewVertex GetViewVertex()
    {
        return viewVertex;
    }

    public Vector3 GetModelPosition()
    {
        return position;
    }

    public void UpdatePosition(Vector3 position)
    {
        this.position = position;
        viewVertex.SetPosition(position);
    }

    private void OnStartFold()
    {
        Debug.Log(1);
        CoroutineStarter.Instance.StartCoroutine(Paper.Instance.PerformFold(this));
    }

    private void OnStopFold()
    {
        Paper.Instance.EndFold();
    }

    /*public void DetermineVertexState()
    {
        Edge randomEdge = edges[Random.Range(0, edges.Count)];
        EdgeComparisonInfo minEdge = new EdgeComparisonInfo();
        minEdge.e1 = randomEdge;
        minEdge.e2 = randomEdge;
        EdgeComparisonInfo maxEdge = new EdgeComparisonInfo();
        maxEdge.e1 = randomEdge;
        maxEdge.e2 = randomEdge;

        // we compare a random edge against all other and save the smallest and largest angle we find
        for (int i = 0; i < edges.Count; i++)
        {
            Vector2 v1 = randomEdge.GetOther(this).transform.position - transform.position;
            Vector2 v2 = edges[i].GetOther(this).transform.position - transform.position;
            float signedAngle = Vector2.SignedAngle(v1, v2);
            if (signedAngle < minEdge.signedAngle)
            {
                minEdge.e2 = edges[i];
                minEdge.signedAngle = signedAngle;
            }
            else if (signedAngle > maxEdge.signedAngle)
            {
                maxEdge.e2 = edges[i];
                maxEdge.signedAngle = signedAngle;
            }
        }

        // if the difference between the largest and smallest angle is bigger than 180 degrees, this vertex cannot be moved
        if (maxEdge.signedAngle - minEdge.signedAngle > 180f)
        {
            isOuterVertex = false;
        }
        else
        {
            isOuterVertex = true;
        }
        SetOuterEdges(new List<Edge>() { minEdge.e2, maxEdge.e2 });
    }*/

    /*public bool MoveIsLegal(Vector3 newPosition)
    {
        float currentAngle = Vector2.SignedAngle(outerEdges[0].GetOther(this).transform.position - transform.position, outerEdges[1].GetOther(this).transform.position - transform.position);
        float movedAngle = Vector2.SignedAngle(outerEdges[0].GetOther(this).transform.position - newPosition, outerEdges[1].GetOther(this).transform.position - newPosition);
        if (Mathf.Abs(movedAngle) <= Mathf.Abs(currentAngle))
        {
            return false;
        }

        float distance = Vector2.Distance(transform.position, newPosition);
        return distance > 0.1f;
    }*/

    /*public void SetOuterEdges(List<Edge> outerEdges)
    {
        if (outerEdges.Count != 2)
        {
            Debug.LogErrorFormat("List of outerEdges has to contain exactly 2 elements. Supplied list contains {0}", outerEdges.Count);
            return;
        }
        if (Equals(outerEdges[0], outerEdges[1]))
        {
            Debug.LogError("Elements in the supplied list are equal");
            return;
        }

        this.outerEdges = outerEdges;
    }*/

    /*public void GiveColor(Color color)
    {
        GetComponent<MeshRenderer>().material.color = color;
    }

    public void MoveToFront()
    {
        Vector3 frontPosition = transform.position + Vector3.back;
        transform.position = frontPosition;
    }

    public void MoveToBack()
    {
        Vector3 backPosition = transform.position + Vector3.forward;
        transform.position = backPosition;
    }

    private void OnMouseDown()
    {
        if (Paper.Instance.debugMode)
        {
            for (int i = 0; i < edges.Count; i++)
            {
                StartCoroutine(edges[i].Flash());
            }
        }
    }

    private void OnMouseDrag()
    {
        if (!moving && !Paper.Instance.debugMode)
        {
            if (isOuterVertex)
            {
                moving = true;
                StartCoroutine(Paper.Instance.PerformFold(this));
            }
            else
            {
                Debug.LogError("This vertex cannot be moved!");
            }
        }
    }

    private void OnMouseUp()
    {
        if (moving && !Paper.Instance.debugMode)
        {
            moving = false;
            Paper.Instance.EndFold();
        }
    }*/
}
