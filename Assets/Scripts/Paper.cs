using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Paper : MonoBehaviour
{
    private static Paper instance;
    public static Paper Instance
    {
        get => instance;
    }

    [SerializeField] PaperView view;
    [SerializeField] private Material lineMaterial;
    public Material LineMaterial
    {
        get => lineMaterial;
    }
    [SerializeField] private GameObject vertexPrefab, snappingGhostVertexPrefab, supportGhostVertexPrefab;//, facePrefab;
    public GameObject VertexPrefab
    {
        get => vertexPrefab;
    }
    public GameObject SnappingGhostVertexPrefab
    {
        get => snappingGhostVertexPrefab;
    }
    public GameObject SupportGhostVertexPrefab
    {
        get => supportGhostVertexPrefab;
    }
    /*public GameObject FacePrefab
    {
        get => facePrefab;
    }*/
    private List<Vertex> vertices = new List<Vertex>();
    private List<Edge> edges = new List<Edge>();
    private List<Face> faces = new List<Face>();

    [HideInInspector]
    public bool debugMode = false;
    private bool performingFold = false;

    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        SummonPaper();
        //SummonTripod();

        for (int i = 0; i < vertices.Count; i++)
        {
            vertices[i].DetermineVertexState();
        }
    }

    private int faceIndex = 0;
    private void Update()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            debugMode = true;
        }
        else
        {
            debugMode = false;
        }

        if (Input.GetKeyUp(KeyCode.F))
        {
            faces[faceIndex].Flash();
            faceIndex++;
            if (faceIndex >= faces.Count)
            {
                faceIndex = 0;
            }
        }
    }

    public IEnumerator PerformFold(Vertex v)
    {
        performingFold = true;
        List<Vertex> verticesToMove = new List<Vertex>() { v };
        List<Face> facesInvolved = new List<Face>();
        Dictionary<Face, FoldInformation> foldInfo = new Dictionary<Face, FoldInformation>();
        List<Vertex> verticesOnFold = new List<Vertex>();
        List<GameObject> snappingGhosts = new List<GameObject>() { Instantiate(snappingGhostVertexPrefab, transform) };
        List<GameObject> supportGhosts = new List<GameObject>();

        LineRenderer lineRenderer = new GameObject("LineRenderer", typeof(LineRenderer)).GetComponent<LineRenderer>();
        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.05f;

        while(true)
        {
            // Before anything else, check whether the vertex can actually be moved where the user tries to move it
            bool moveIsLegal = true; //v.MoveIsLegal(v.transform.position);

            if (moveIsLegal)
            {
                // Move a ghost of v
                snappingGhosts[0].transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition) + Vector3.back * Camera.main.transform.position.z;

                // Calculate where the foldline is
                Linepiece foldLine = GetFoldLine(v.transform.position, snappingGhosts[0].GetComponent<GhostVertex>().GetPosition());
                lineRenderer.SetPositions(new Vector3[] { foldLine.Start, foldLine.End });

                // Find which vertices (roughly) lie on the foldline
                // Those are also intersections, but do not require a new vertex
                verticesOnFold = GetVerticesOnFold(foldLine);

                // Find which vertices lie on the same side of the foldLine as v
                // These, including v, are moving and will be mirrored in the foldLine
                // Also record all faces that the movingVertices are a part of
                facesInvolved.Clear();
                verticesToMove.Clear();
                FindVerticesToMove(foldLine, v, ref verticesToMove, ref facesInvolved);

                // Find all intersections
                foldInfo.Clear();
                GetFoldInformation(foldLine, facesInvolved, ref foldInfo, verticesOnFold);

                // Create ghost vertices to indicate where vertices will end up
                CreateGhosts(foldLine, verticesToMove, foldInfo, ref snappingGhosts, ref supportGhosts);


                // When the user releases the mouse button, calculate all the new stuff
                if (!performingFold)
                {
                    // Mirror existing vertices
                    for (int i = 0; i < verticesToMove.Count; i++)
                    {
                        verticesToMove[i].transform.position = snappingGhosts[i].GetComponent<GhostVertex>().GetPosition();
                    }

                    // Create new vertices
                    List<Vertex> newVertices = new List<Vertex>();
                    List<FaceIntersection> uniqueFaceIntersections = new List<FaceIntersection>();
                    // For each FaceIntersection, check whether a vertex was created on its edge
                    // If not, create one, if so, assign the already created vertex
                    foreach (FoldInformation fi in foldInfo.Values)
                    {
                        //if (fi.FaceIntersection0 != null)
                        if (fi.FaceIntersection0.makeNewStuff)
                        {
                            if (!uniqueFaceIntersections.Any(x => x.e == fi.FaceIntersection0.e))
                            {
                                uniqueFaceIntersections.Add(fi.FaceIntersection0);
                                newVertices.Add(fi.FaceIntersection0.CreateNewVertex());
                            }
                            else
                            {
                                fi.FaceIntersection0.vertexAtIntersection = uniqueFaceIntersections.First(x => x.e == fi.FaceIntersection0.e).vertexAtIntersection;
                            }
                        }
                        //if (fi.FaceIntersection1 != null)
                        if (fi.FaceIntersection1.makeNewStuff)
                        {
                            if (!uniqueFaceIntersections.Any(x => x.e == fi.FaceIntersection1.e))
                            {
                                uniqueFaceIntersections.Add(fi.FaceIntersection1);
                                newVertices.Add(fi.FaceIntersection1.CreateNewVertex());
                            }
                            else
                            {
                                fi.FaceIntersection1.vertexAtIntersection = uniqueFaceIntersections.First(x => x.e == fi.FaceIntersection1.e).vertexAtIntersection;
                            }
                        }
                    }

                    // For each intersected edge, two new ones are needed
                    // Create a new one between the new vertex and the moved vertex of the intersected edges
                    // Update the edge to be between the new vertex and the other (non-moved) vertex
                    for (int i = 0; i < uniqueFaceIntersections.Count; i++)
                    {
                        if (uniqueFaceIntersections[i].makeNewStuff)
                        {
                            uniqueFaceIntersections[i].CreateNewEdge(verticesToMove.First(x => uniqueFaceIntersections[i].e.HasVertex(x)));
                        }
                    }
                    for (int i = 0; i < uniqueFaceIntersections.Count; i++)
                    {
                        if (uniqueFaceIntersections[i].makeNewStuff)
                        {
                            // unreadable as fuck
                            uniqueFaceIntersections[i].UpdateExistingEdge(uniqueFaceIntersections[i].e.GetOther(verticesToMove.First(x => uniqueFaceIntersections[i].e.HasVertex(x))));
                        }
                    }
                    // Create a new edge along the foldline for each face that was involved
                    foreach(FoldInformation fi in foldInfo.Values)
                    {
                        fi.CreateNewEdge();
                    }

                    // Finally, update the face information
                    facesInvolved = facesInvolved.OrderByDescending(x => x.Height).ToList();
                    int heightIncrease = 1;
                    for (int i = 0; i < facesInvolved.Count; i++)
                    {
                        Debug.Log(facesInvolved[i].Height);
                        Face face = new Face();
                        //Face face = Instantiate(facePrefab, transform).GetComponent<Face>();
                        face.Height = facesInvolved[i].Height + heightIncrease;
                        for (int j = 0; j < verticesToMove.Count; j++)
                        {
                            if (facesInvolved[i].vertices.Contains(verticesToMove[j]))
                            {
                                face.AddVertex(verticesToMove[j]);
                                //verticesToMove[j].transform.parent = face.transform;
                                facesInvolved[i].RemoveVertex(verticesToMove[j]);
                            }
                        }
                        facesInvolved[i].AddVertex(foldInfo[facesInvolved[i]].FaceIntersection0.vertexAtIntersection);
                        facesInvolved[i].AddVertex(foldInfo[facesInvolved[i]].FaceIntersection1.vertexAtIntersection);
                        face.AddVertex(foldInfo[facesInvolved[i]].FaceIntersection0.vertexAtIntersection);
                        face.AddVertex(foldInfo[facesInvolved[i]].FaceIntersection1.vertexAtIntersection);
                        //foldInfo[facesInvolved[i]].FaceIntersection0.vertexAtIntersection.transform.parent = face.transform;
                        //foldInfo[facesInvolved[i]].FaceIntersection1.vertexAtIntersection.transform.parent = face.transform;

                        facesInvolved[i].UpdateEdges();
                        face.UpdateEdges();

                        heightIncrease += 2;
                    }

                    break;
                }
            }

            yield return new WaitForEndOfFrame();
        }

        for (int i = 0; i < faces.Count; i++)
        {
            Debug.Log(faces[i].Height);
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

    public void EndFold()
    {
        performingFold = false;
    }

    public void NewVertex(Vertex v, out int number)
    {
        if (!vertices.Contains(v))
        {
            vertices.Add(v);
            number = vertices.Count;
            //v.Show();
            //v.DetermineVertexState();
        }
        else
        {
            number = -1;
            Debug.LogError("Tried to add new vertex, but was already in the list");
        }
    }

    public void NewEdge(Edge e, out int number)
    {
        if (!edges.Contains(e))
        {
            edges.Add(e);
            //e.Show();
            number = edges.Count;
        }
        else
        {
            number = -1;
            Debug.LogError("Tried to add new edge, but was already in the list");
        }
    }

    public void NewFace(Face f, out int number)
    {
        Debug.Log("New FAce!");
        if (!faces.Contains(f))
        {
            faces.Add(f);
            number = faces.Count;
        }
        else
        {
            number = -1;
            Debug.LogError("Tried to add new face, but was already in the list");
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

    private void FindVerticesToMove(Linepiece foldLine, Vertex movingVertex, ref List<Vertex> verticesToMove, ref List<Face> facesInvolved)
    {
        // Make sure the movingVertex is added to the list
        if (!verticesToMove.Contains(movingVertex))
        {
            verticesToMove.Add(movingVertex);
        }
        // Find which face(s) the movingVertex belong(s) to
        for (int i = 0; i < faces.Count; i++)
        {
            // Only check the vertices part of thie face
            List<Vertex> vertices = faces[i].vertices;
            if (vertices.Contains(movingVertex))
            {
                // Record all faces involved (a face is involved if it contains a moving vertex)
                if (!facesInvolved.Contains(faces[i]))
                {
                    facesInvolved.Add(faces[i]);
                }
                for (int j = 0; j < vertices.Count; j++)
                {
                    if (!verticesToMove.Contains(vertices[j]))
                    {
                        // We need this to supply to the intersection function, but we don't use it
                        Vector2 intersection;
                        Linepiece lp = new Linepiece(movingVertex.transform.position, vertices[j].transform.position);
                        if (!MathUtility.LinepiecesIntersect(foldLine, lp, out intersection))
                        {
                            verticesToMove.Add(vertices[j]);
                            // It might be that the original movingVertex is not part of a face that vertices[j] is a part of.
                            // If vertices[j] moves as well, we should check all its faces as well
                            FindVerticesToMove(foldLine, vertices[j], ref verticesToMove, ref facesInvolved);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Each face is either intersected twice or not at all. Figure that out and save some info in case of an intersection
    /// </summary>
    /// <param name="foldLine"></param>
    /// <param name="facesInvolved"></param>
    /// <param name="foldInfo"></param>
    private void GetFoldInformation(Linepiece foldLine, List<Face> facesInvolved, ref Dictionary<Face, FoldInformation> foldInfo, List<Vertex> verticesOnFold)
    {
        for (int i = 0; i < facesInvolved.Count; i++)
        {
            FoldInformation fi = new FoldInformation();
            for (int j = 0; j < facesInvolved[i].edges.Count; j++)
            {
                Vector2 intersection;
                if (MathUtility.LinepiecesIntersect(foldLine, facesInvolved[i].edges[j].GetLinepiece(), out intersection))
                {
                    if (!foldInfo.ContainsKey(facesInvolved[i]))
                    {
                        foldInfo.Add(facesInvolved[i], fi);
                    }
                    // Check whether the fold goes through a vertex on the intersected edge
                    // If so, no new vertex needs to be created, so send different info to the foldInformation instance
                    Vertex vertexOnFold = null;
                    for (int k = 0; k < verticesOnFold.Count; k++)
                    {
                        if (facesInvolved[i].edges[j].HasVertex(verticesOnFold[k])) 
                        {
                            vertexOnFold = verticesOnFold[k];
                            break;
                        }
                    }
                    if (vertexOnFold == null)
                    {
                        // Returns true if both intersections with the face have been found
                        if (fi.NewFaceIntersection(facesInvolved[i].edges[j], intersection))
                        {
                            break;
                        }
                    }
                    else
                    {
                        if (fi.NewFaceIntersection(vertexOnFold))
                        {
                            break;
                        }
                    }
                }
            }
        }
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
    
    private Dictionary<Face, List<FoldInformation>> GetIntersections(Linepiece foldLine, List<Vertex> movingVertices, List<Vertex> verticesOnFold, List<Face> facesInvolved)
    {
        Dictionary<Face, List<FoldInformation>> foldInfo = new Dictionary<Face, List<FoldInformation>>();
        for (int i = 0; i < facesInvolved.Count; i++)
        {
            foldInfo.Add(facesInvolved[i], new List<FoldInformation>());
            List<Edge> edges = facesInvolved[i].edges;
            for (int j = 0; j < edges.Count; j++)
            {
                // Check first whether any edges contain vertices that intersect with the foldLine
                // If so, the edge intersects at that vertex, so a fold would just use the existing vertex
                // So don't check for an intersection
                bool result = true;
                for (int k = 0; k < verticesOnFold.Count; k++)
                {
                    if (edges[j].HasVertex(verticesOnFold[k]))
                    {
                        result = false;
                        break;
                    }
                }
                if (result)
                {
                    Vector2 intersection;
                    if (MathUtility.LinepiecesIntersect(edges[j].GetLinepiece(), foldLine, out intersection))
                    {
                        Vertex movingVertexOnThisEdge = null;
                        for (int k = 0; k < movingVertices.Count; k++)
                        {
                            if (edges[j].HasVertex(movingVertices[k]))
                            {
                                movingVertexOnThisEdge = movingVertices[k];
                                break;
                            }
                        }
                    }
                }
            }
        }
        return foldInfo;
    }

    private void CreateGhosts(Linepiece foldLine, List<Vertex> verticesToMove, Dictionary<Face, FoldInformation> foldInfo, ref List<GameObject> snappingGhosts, ref List<GameObject> supportGhosts)
    {
        while(verticesToMove.Count > snappingGhosts.Count)
        {
            snappingGhosts.Add(Instantiate(snappingGhostVertexPrefab, transform));
        }
        while(verticesToMove.Count < snappingGhosts.Count)
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

        // Find all unique intersected edges with the intersection point
        List<FaceIntersection> faceIntersections = new List<FaceIntersection>();
        foreach(FoldInformation fi in foldInfo.Values)
        {
            if (fi.FaceIntersection0 != null)
            {
                if (!faceIntersections.Any(x => x.e == fi.FaceIntersection0.e))
                {
                    faceIntersections.Add(fi.FaceIntersection0);
                }
            }
            if (fi.FaceIntersection1 != null)
            {
                if (!faceIntersections.Any(x => x.e == fi.FaceIntersection1.e))
                {
                    faceIntersections.Add(fi.FaceIntersection1);
                }
            }
        }
        while (faceIntersections.Count > supportGhosts.Count)
        {
            supportGhosts.Add(Instantiate(supportGhostVertexPrefab, transform));
        }
        while (faceIntersections.Count < supportGhosts.Count)
        {
            GameObject ghost = supportGhosts[supportGhosts.Count - 1];
            supportGhosts.RemoveAt(supportGhosts.Count - 1);
            Destroy(ghost);
        }
        for (int i = 0; i < supportGhosts.Count; i++)
        {
            supportGhosts[i].transform.position = faceIntersections[i].intersection;
        }
    }

    private void SummonPaper()
    {
        //Face f1 = Instantiate(facePrefab, transform).GetComponent<Face>();
        Face f1 = new Face();
        f1.Height = 0;

        Vertex v1 = Instantiate(vertexPrefab, -Vector2.one * 3f, Quaternion.identity).GetComponent<Vertex>();
        Vertex v2 = Instantiate(vertexPrefab, new Vector3(3, -3, 0), Quaternion.identity).GetComponent<Vertex>();
        //Vertex v3 = Instantiate(vertexPrefab, Vector2.zero, Quaternion.identity, transform).GetComponent<Vertex>();
        Vertex v4 = Instantiate(vertexPrefab, new Vector3(-3, 3, 0), Quaternion.identity).GetComponent<Vertex>();
        Vertex v5 = Instantiate(vertexPrefab, Vector2.one * 3f, Quaternion.identity).GetComponent<Vertex>();

        Edge e1 = new Edge(v2, v1);
        Edge e2 = new Edge(v1, v4);
        Edge e3 = new Edge(v4, v5);
        Edge e4 = new Edge(v5, v2);
        /*Edge e5 = new Edge(v1, v3);
        Edge e6 = new Edge(v4, v3);
        Edge e7 = new Edge(v3, v5);
        Edge e8 = new Edge(v3, v2);*/

        f1.AddVertex(v1);
        f1.AddVertex(v2);
        f1.AddVertex(v4);
        f1.AddVertex(v5);
        f1.AddEdge(e1);
        f1.AddEdge(e2);
        f1.AddEdge(e3);
        f1.AddEdge(e4);
    }

    private void SummonTripod()
    {
        Face f1 = new Face();
        f1.Height = 0;
        Vertex v1 = Instantiate(vertexPrefab, Vector2.zero, Quaternion.identity, transform).GetComponent<Vertex>();
        vertices.Add(v1);
        Vertex v2 = Instantiate(vertexPrefab, Vector2.up, Quaternion.identity, transform).GetComponent<Vertex>();
        vertices.Add(v2);
        Vertex v3 = Instantiate(vertexPrefab, Vector2.down + Vector2.right, Quaternion.identity, transform).GetComponent<Vertex>();
        vertices.Add(v3);
        Vertex v4 = Instantiate(vertexPrefab, Vector2.down + Vector2.left, Quaternion.identity, transform).GetComponent<Vertex>();
        vertices.Add(v4);

        Edge e1 = new Edge(v1, v2);
        edges.Add(e1);
        Edge e2 = new Edge(v1, v3);
        edges.Add(e2);
        Edge e3 = new Edge(v1, v4);
        edges.Add(e3);

        f1.AddVertex(v1);
        f1.AddVertex(v2);
        f1.AddVertex(v3);
        f1.AddVertex(v4);
        f1.AddEdge(e1);
        f1.AddEdge(e2);
        f1.AddEdge(e3);
    }
}
