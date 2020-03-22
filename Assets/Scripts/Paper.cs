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
    [SerializeField] private GameObject snappingGhostVertexPrefab, supportGhostVertexPrefab, viewVertexPrefab;
    public GameObject SnappingGhostVertexPrefab
    {
        get => snappingGhostVertexPrefab;
    }
    public GameObject SupportGhostVertexPrefab
    {
        get => supportGhostVertexPrefab;
    }
    public GameObject ViewVertexPrefab
    {
        get => viewVertexPrefab;
    }
    private List<ModelVertex> vertices = new List<ModelVertex>();
    private List<ModelEdge> edges = new List<ModelEdge>();
    private List<ModelFace> faces = new List<ModelFace>();

    [HideInInspector]
    public bool debugMode = false;
    private bool performingFold = false;
    private bool isFirstUpdate = true;

    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        SummonPaper();
        //SummonTripod();
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
            faces[faceIndex].Highlight(0.5f);
            faceIndex++;
            if (faceIndex >= faces.Count)
            {
                faceIndex = 0;
            }
        }
    }

    private void LateUpdate()
    {
        if (isFirstUpdate)
        {
            isFirstUpdate = false;
            //view.SetStartState(vertices, edges);

            /*Debug.Log("HALLO!");
            for (int i = 0; i < vertices.Count; i++)
            {
                Debug.Log("JA HOOR");
                vertices[i].DetermineVertexState();
            }*/
        }
    }

    public IEnumerator PerformFold(ModelVertex v)
    {
        performingFold = true;
        List<ModelVertex> verticesToMove = new List<ModelVertex>() { v };
        List<ModelFace> facesInvolved = new List<ModelFace>();
        Dictionary<ModelFace, FoldInformation> foldInfo = new Dictionary<ModelFace, FoldInformation>();
        List<ModelVertex> verticesOnFold = new List<ModelVertex>();
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
                //Linepiece foldLine = GetFoldLine(v.transform.position, snappingGhosts[0].GetComponent<GhostVertex>().GetPosition());
                Linepiece foldLine = GetFoldLine(v.GetModelPosition(), snappingGhosts[0].GetComponent<GhostVertex>().GetPosition());
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

                //view.UpdateView(vertices, edges, faces, foldInfo, verticesToMove, facesInvolved, snappingGhosts);
                // When the user releases the mouse button, calculate all the new stuff
                if (!performingFold)
                {
                    // Mirror existing vertices
                    for (int i = 0; i < verticesToMove.Count; i++)
                    {
                        //verticesToMove[i].transform.position = snappingGhosts[i].GetComponent<GhostVertex>().GetPosition();
                        verticesToMove[i].UpdatePosition(snappingGhosts[i].GetComponent<GhostVertex>().GetPosition());
                    }

                    // Create new vertices
                    List<ModelVertex> newVertices = new List<ModelVertex>();
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
                        ModelFace face = new ModelFace();
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

        /*for (int i = 0; i < faces.Count; i++)
        {
            Debug.Log(faces[i].Height);
        }*/

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

    public void NewVertex(ModelVertex v, out int number)
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

    public void NewEdge(ModelEdge e, out int number)
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

    public void NewFace(ModelFace f, out int number)
    {
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

    private void FindVerticesToMove(Linepiece foldLine, ModelVertex movingVertex, ref List<ModelVertex> verticesToMove, ref List<ModelFace> facesInvolved)
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
            List<ModelVertex> vertices = faces[i].vertices;
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
                        // Check whether vertices[j] lies on the same side of the foldline as movingVertex
                        //Linepiece lp = new Linepiece(movingVertex.transform.position, vertices[j].transform.position);
                        Linepiece lp = new Linepiece(movingVertex.GetModelPosition(), vertices[j].GetModelPosition());
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
    private void GetFoldInformation(Linepiece foldLine, List<ModelFace> facesInvolved, ref Dictionary<ModelFace, FoldInformation> foldInfo, List<ModelVertex> verticesOnFold)
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
                    ModelVertex vertexOnFold = null;
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

    private List<ModelVertex> GetVerticesOnFold(Linepiece foldLine)
    {
        List<ModelVertex> result = new List<ModelVertex>();
        for (int i = 0; i < vertices.Count; i++)
        {
            float distanceToFoldLine = MathUtility.DistancePointToLinepiece(foldLine, vertices[i].GetModelPosition());
            if (distanceToFoldLine < 0.1f)
            {
                result.Add(vertices[i]);
            }
        }
        return result;
    }
    
    private Dictionary<ModelFace, List<FoldInformation>> GetIntersections(Linepiece foldLine, List<ModelVertex> movingVertices, List<ModelVertex> verticesOnFold, List<ModelFace> facesInvolved)
    {
        Dictionary<ModelFace, List<FoldInformation>> foldInfo = new Dictionary<ModelFace, List<FoldInformation>>();
        for (int i = 0; i < facesInvolved.Count; i++)
        {
            foldInfo.Add(facesInvolved[i], new List<FoldInformation>());
            List<ModelEdge> edges = facesInvolved[i].edges;
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
                        ModelVertex movingVertexOnThisEdge = null;
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

    private void CreateGhosts(Linepiece foldLine, List<ModelVertex> verticesToMove, Dictionary<ModelFace, FoldInformation> foldInfo, ref List<GameObject> snappingGhosts, ref List<GameObject> supportGhosts)
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
            snappingGhosts[i].transform.position = MathUtility.MirrorPointInLinepiece(foldLine, verticesToMove[i].GetModelPosition());
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
        ModelFace f1 = new ModelFace();
        f1.Height = 0;

        ModelVertex v1 = new ModelVertex(-Vector2.one * 3f);
        ModelVertex v2 = new ModelVertex(new Vector3(3, -3, 0));
        //ModelVertex v3 = new ModelVertex(Vector2.zero);
        ModelVertex v4 = new ModelVertex(new Vector3(-3, 3, 0));
        ModelVertex v5 = new ModelVertex(Vector3.one * 3f);

        /*ModelVertex v1 = Instantiate(vertexPrefab, -Vector2.one * 3f, Quaternion.identity).GetComponent<ModelVertex>();
        ModelVertex v2 = Instantiate(vertexPrefab, new Vector3(3, -3, 0), Quaternion.identity).GetComponent<ModelVertex>();
        //Vertex v3 = Instantiate(vertexPrefab, Vector2.zero, Quaternion.identity, transform).GetComponent<Vertex>();
        ModelVertex v4 = Instantiate(vertexPrefab, new Vector3(-3, 3, 0), Quaternion.identity).GetComponent<ModelVertex>();
        ModelVertex v5 = Instantiate(vertexPrefab, Vector2.one * 3f, Quaternion.identity).GetComponent<ModelVertex>();*/

        ModelEdge e1 = new ModelEdge(v2, v1);
        ModelEdge e2 = new ModelEdge(v1, v4);
        ModelEdge e3 = new ModelEdge(v4, v5);
        ModelEdge e4 = new ModelEdge(v5, v2);
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

    /*private void SummonTripod()
    {
        ModelFace f1 = new ModelFace();
        f1.Height = 0;
        ModelVertex v1 = Instantiate(vertexPrefab, Vector2.zero, Quaternion.identity, transform).GetComponent<ModelVertex>();
        vertices.Add(v1);
        ModelVertex v2 = Instantiate(vertexPrefab, Vector2.up, Quaternion.identity, transform).GetComponent<ModelVertex>();
        vertices.Add(v2);
        ModelVertex v3 = Instantiate(vertexPrefab, Vector2.down + Vector2.right, Quaternion.identity, transform).GetComponent<ModelVertex>();
        vertices.Add(v3);
        ModelVertex v4 = Instantiate(vertexPrefab, Vector2.down + Vector2.left, Quaternion.identity, transform).GetComponent<ModelVertex>();
        vertices.Add(v4);

        ModelEdge e1 = new ModelEdge(v1, v2);
        edges.Add(e1);
        ModelEdge e2 = new ModelEdge(v1, v3);
        edges.Add(e2);
        ModelEdge e3 = new ModelEdge(v1, v4);
        edges.Add(e3);

        f1.AddVertex(v1);
        f1.AddVertex(v2);
        f1.AddVertex(v3);
        f1.AddVertex(v4);
        f1.AddEdge(e1);
        f1.AddEdge(e2);
        f1.AddEdge(e3);
    }*/
}
