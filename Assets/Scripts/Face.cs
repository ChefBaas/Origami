using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Face : MonoBehaviour
{
    private int height = 0;
    public int Height
    {
        get => height;
        set => height = value;
    }

    public List<Vertex> vertices = new List<Vertex>();
    public List<Edge> edges = new List<Edge>();

    private bool performingFold = false;

    [HideInInspector]
    public int number = -1;

    private void Awake()
    {
        Debug.Log("HALO");
        Paper.Instance.NewFace(this, out number);
    }

    public IEnumerator PerformFold(Vertex v)
    {
        performingFold = true;

        // We need to collect some information each frame, which we later use to calculate the result of the fold
        List<Vertex> verticesToMove = new List<Vertex>() { v };
        List<FoldInformation> foldInfo = new List<FoldInformation>();
        List<GameObject> snappingGhosts = new List<GameObject>() { Instantiate(Paper.Instance.SnappingGhostVertexPrefab, transform) };
        List<GameObject> supportGhosts = new List<GameObject>();
        List<Vertex> verticesOnFoldline = new List<Vertex>();
        List<Edge> intersectedEdges = new List<Edge>();
        List<Edge> outerEdges = new List<Edge>();


        LineRenderer lineRenderer = new GameObject("LineRenderer", typeof(LineRenderer)).GetComponent<LineRenderer>();
        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.05f;

        while (true)
        {
            // Before doing anything else, we check whether the vertex can be moved in the direction the user attempted
            bool moveIsLegal = true;// v.MoveIsLegal(movingVertex.transform.position);

            if (moveIsLegal)
            {
                snappingGhosts[0].transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition) + Vector3.back * Camera.main.transform.position.z;

                // Calculate where the foldLine is
                // Later we should make sure the snapping works correctly with the representation of the line (so don't use the mousePosition, but the vertex' position)
                //Linepiece foldLine = GetFoldLine(v.transform.position, Camera.main.ScreenToWorldPoint(Input.mousePosition));
                Linepiece foldLine = GetFoldLine(v.transform.position, snappingGhosts[0].GetComponent<GhostVertex>().GetPosition());
                lineRenderer.SetPositions(new Vector3[] { foldLine.Start, foldLine.End });

                // Find which vertices lie on the same side of the foldLine as v
                // These, including v, are moving and will be mirrored in the foldLine
                verticesToMove = FindVerticesToMove(foldLine, v);

                // Save all vertices that intersect with the foldLine
                // Edges that contain those are not eligible to form new vertices
                verticesOnFoldline = GetVerticesOnFold(foldLine);

                // Save all edges that intersect with the foldLine, as well as the position they intersect
                // We want to create new vertices here when the fold is completed
                foldInfo = GetIntersections(foldLine, verticesToMove, verticesOnFoldline);

                // For now, show all new positions of new and moved vertices using transparent 'Ghost' vertices
                // Later replace this by live updating the paper (FANCY AF)
                CreateGhosts(verticesToMove, foldInfo, ref snappingGhosts, ref supportGhosts, foldLine);


                // When the user completes the fold, create new vertices and edges
                if (!performingFold)
                {
                    /*// Mirror existing vertices
                    for (int i = 0; i < verticesToMove.Count; i++)
                    {
                        verticesToMove[i].transform.position = snappingGhosts[i].GetComponent<GhostVertex>().GetPosition();
                        // Increase height
                    }
                    // Create the new vertices
                    List<Vertex> vertices = new List<Vertex>();
                    vertices.AddRange(verticesOnFoldline);
                    for (int i = 0; i < foldInfo.Count; i++)
                    {
                        vertices.Add(foldInfo[i].CreateNewVertex());
                    }
                    // Create new edges
                    for (int i = 0; i < foldInfo.Count; i++)
                    {
                        foldInfo[i].CreateNewEdge();
                    }
                    // Specifically the edge that lies along the foldline
                    List<Vertex> outerVertices = GetOuterVertices(foldLine, vertices);
                    new Edge(outerVertices[0], outerVertices[1]);
                    // Update existing edges with new information
                    for (int i = 0; i < foldInfo.Count; i++)
                    {
                        foldInfo[i].UpdateExistingEdge();
                    }
                    v.DetermineVertexState();
                    break;*/
                    NewFace(verticesToMove, snappingGhosts, foldInfo, verticesOnFoldline, foldLine, v);
                    break;
                }
            }
            else
            {
                if (!performingFold)
                {
                    break;
                }
            }
            yield return new WaitForEndOfFrame();
        }

        // Clean up everything
        for (int i = 0; i < snappingGhosts.Count; i++)
        {
            Destroy(snappingGhosts[i]);
        }
        snappingGhosts.Clear();
        for (int i = 0; i < supportGhosts.Count; i++)
        {
            Destroy(supportGhosts[i]);
        }
        supportGhosts.Clear();
        Destroy(lineRenderer.gameObject);
    }

    public void AddVertex(Vertex v)
    {
        if (!vertices.Contains(v))
        {
            vertices.Add(v);
            v.AddFace(this);
        }
    }

    public void RemoveVertex(Vertex v)
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

    public void AddEdge(Edge e)
    {
        if (!edges.Contains(e))
        {
            edges.Add(e);
        }
    }

    public void RemoveEdge(Edge e)
    {
        if (edges.Contains(e))
        {
            edges.Remove(e);
        }
    }

    public void EndFold()
    {
        performingFold = false;
    }

    public void Flash()
    {
        StartCoroutine(ShowComponents());
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

    private IEnumerator ShowComponents()
    {
        for (int i = 0; i < vertices.Count; i++)
        {
            vertices[i].GiveColor(Color.red);
        }
        for (int i = 0; i < edges.Count; i++)
        {
            edges[i].GiveColor(Color.red);
        }
        yield return new WaitForSeconds(1f);
        for (int i = 0; i < vertices.Count; i++)
        {
            vertices[i].GiveColor(Color.white);
        }
        for (int i = 0; i < edges.Count; i++)
        {
            edges[i].GiveColor(Color.white);
        }
    }

    private Linepiece GetFoldLine(Vector2 from, Vector2 to)
    {
        // calculate point in between the ghost and the ogVertex
        //Vector2 middlePoint = ((Vector2)v.transform.position + movingVertexGhost.GetPosition()) / 2f;
        Vector2 middlePoint = (from + to) / 2f;
        // then construct a line through that point, perpendicular to the linepiece going from the ghost to the ogVertex
        //Vector2 direction = movingVertexGhost.GetPosition() - (Vector2)v.transform.position;
        Vector2 direction = to - from;
        Vector2 perpendicularDirection = new Vector2(direction.y, -direction.x).normalized;
        // the result is the line along which the fold is made
        Linepiece foldLine = new Linepiece(middlePoint + perpendicularDirection * 100f, middlePoint - perpendicularDirection * 100f);
        return foldLine;
    }

    private List<Vertex> FindVerticesToMove(Linepiece foldLine, Vertex movingVertex)
    {
        // Add the vertex the user is dragging to the list; it of course always has to move
        List<Vertex> result = new List<Vertex>() { movingVertex };
        // Then loop through each vertex and check whether any of them lie on the same side of the foldLine
        for (int i = 0; i < vertices.Count; i++)
        {
            if (vertices[i] != movingVertex)
            {
                // We need this to supply to the intersection function, but we don't use it
                Vector2 intersection;
                Linepiece lineFromMovingVertexToInspectedVertex = new Linepiece(movingVertex.transform.position, vertices[i].transform.position);
                if (!MathUtility.LinepiecesIntersect(foldLine, lineFromMovingVertexToInspectedVertex, out intersection))
                {
                    result.Add(vertices[i]);
                }
            }
        }
        return result;
    }

    private List<Vertex> GetVerticesOnFold(Linepiece foldLine)
    {
        List<Vertex> result = new List<Vertex>();
        for (int i = 0; i < vertices.Count; i++)
        {
            float distanceToFoldLine = MathUtility.DistancePointToLinepiece(foldLine, vertices[i].transform.position);
            if (distanceToFoldLine < 0.1f)
            {
                result.Add(vertices[i]);
            }
        }
        return result;
    }

    private List<FoldInformation> GetIntersections(Linepiece foldLine, List<Vertex> movingVertices, List<Vertex> verticesOnFold)
    {
        List<FoldInformation> foldInfo = new List<FoldInformation>();
        for (int i = 0; i < edges.Count; i++)
        {
            // Check first whether any edges contain vertices that intersect with the foldLine
            // If so, the edge intersects at that vertex, so a fold would just use the existing vertex
            // So don't check for an intersection
            bool result = true;
            for (int j = 0; j < verticesOnFold.Count; j++)
            {
                if (edges[i].HasVertex(verticesOnFold[j]))
                {
                    result = false;
                    break;
                }
            }
            if (result)
            {
                Vector2 intersection;
                if (MathUtility.LinepiecesIntersect(edges[i].GetLinepiece(), foldLine, out intersection))
                {
                    Vertex movingVertexOnThisEdge = null;
                    for (int j = 0; j < movingVertices.Count; j++)
                    {
                        if (edges[i].HasVertex(movingVertices[j]))
                        {
                            movingVertexOnThisEdge = movingVertices[j];
                            break;
                        }
                    }
                    FoldInformation fi = new FoldInformation();
                    /*fi.edge = edges[i];
                    fi.intersection = intersection;
                    fi.movingVertex = movingVertexOnThisEdge;
                    foldInfo.Add(fi);*/
                }
            }
        }
        return foldInfo;
    }

    private void CreateGhosts(List<Vertex> verticesToMove, List<FoldInformation> foldInfo, ref List<GameObject> snappingGhosts, ref List<GameObject> supportGhosts, Linepiece foldLine)
    {
        while (verticesToMove.Count > snappingGhosts.Count)
        {
            snappingGhosts.Add(Instantiate(Paper.Instance.SnappingGhostVertexPrefab, transform));
        }
        while (verticesToMove.Count < snappingGhosts.Count)
        {
            GameObject ghost = snappingGhosts[snappingGhosts.Count - 1];
            snappingGhosts.RemoveAt(snappingGhosts.Count - 1);
            Destroy(ghost);
        }
        // Start at 1, because the first one should already be taken care of; it is used to calculate the foldLine
        for (int i = 1; i < snappingGhosts.Count; i++)
        {
            snappingGhosts[i].transform.position = MathUtility.MirrorPointInLinepiece(foldLine, verticesToMove[i].transform.position);
        }

        while (foldInfo.Count > supportGhosts.Count)
        {
            supportGhosts.Add(Instantiate(Paper.Instance.SupportGhostVertexPrefab, transform));
        }
        while (foldInfo.Count < supportGhosts.Count)
        {
            GameObject ghost = supportGhosts[supportGhosts.Count - 1];
            supportGhosts.RemoveAt(supportGhosts.Count - 1);
            Destroy(ghost);
        }
        for (int i = 0; i < supportGhosts.Count; i++)
        {
            //supportGhosts[i].transform.position = foldInfo[i].intersection;
        }
    }

    private List<Vertex> GetOuterVertices(Linepiece foldLine, List<Vertex> vertices)
    {
        List<Vertex> result = new List<Vertex>() { null, null };
        float smallestDistance = float.PositiveInfinity;
        float biggestDistance = 0f;
        for (int i = 0; i < vertices.Count; i++)
        {
            float distance = Vector2.Distance(vertices[i].transform.position, foldLine.Start);
            if (distance < smallestDistance)
            {
                smallestDistance = distance;
                result[0] = vertices[i];
            }
            if (distance > biggestDistance)
            {
                biggestDistance = distance;
                result[1] = vertices[i];
            }
        }

        if (result[0] == null || result[1] == null || result[0] == result[1])
        {
            Debug.LogErrorFormat("GetOuterVertices failed! Closest vertex is {0}, farthest vertex is {1}. They are{2} equal", result[0], result[1], result[0] == result[1] ? "" : " not");
        }
        return result;
    }

    private void NewFace(List<Vertex> verticesToMove, List<GameObject> snappingGhosts, List<FoldInformation> foldInfo, List<Vertex> verticesOnFoldline, Linepiece foldLine, Vertex draggedVertex)
    {
        // Mirror existing vertices
        for (int i = 0; i < verticesToMove.Count; i++)
        {
            verticesToMove[i].transform.position = snappingGhosts[i].GetComponent<GhostVertex>().GetPosition();
            // Increase height
        }
        // Create the new vertices
        List<Vertex> vertices = new List<Vertex>();
        vertices.AddRange(verticesOnFoldline);
        for (int i = 0; i < foldInfo.Count; i++)
        {
            //vertices.Add(foldInfo[i].CreateNewVertex());
        }
        // Create new edges
        for (int i = 0; i < foldInfo.Count; i++)
        {
            //foldInfo[i].CreateNewEdge();
        }
        // Specifically the edge that lies along the foldline
        List<Vertex> outerVertices = GetOuterVertices(foldLine, vertices);
        new Edge(outerVertices[0], outerVertices[1]);
        // Update existing edges with new information
        for (int i = 0; i < foldInfo.Count; i++)
        {
            //foldInfo[i].UpdateExistingEdge();
        }
        draggedVertex.DetermineVertexState();

        // Create the new face, assign vertices to it and this one
        Face newFace = Instantiate(Paper.Instance.FacePrefab).GetComponent<Face>();
        AddVertex(outerVertices[0]);
        AddVertex(outerVertices[1]);
        newFace.AddVertex(outerVertices[0]);
        newFace.AddVertex(outerVertices[1]);
        for (int i = 0; i < verticesToMove.Count; i++)
        {
            newFace.AddVertex(verticesToMove[i]);
        }

        // Remove vertices from this face and self-destruct if it is no longer a face
        for (int i = 0; i < verticesToMove.Count; i++)
        {
            RemoveVertex(verticesToMove[i]);
        }

        // Have each face check whether is needs to add or delete edges
        UpdateEdges();
        newFace.UpdateEdges();
    }
}
