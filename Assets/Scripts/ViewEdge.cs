using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vectrosity;

public class ViewEdge
{
    private VectorLine vectorLine;

    public ViewEdge()
    {
        vectorLine = new VectorLine("BLA", new List<Vector3>(), 5f);
    }

    public void SetPositions(Vector2 p1, Vector2 p2)
    {
        vectorLine.points3 = new List<Vector3>() { p1, p2 };
        vectorLine.Draw3D();
    }

    public void Destroy()
    {
        VectorLine.Destroy(ref vectorLine);
    }

    public void Show()
    {
        vectorLine.SetWidth(5f);
    }

    public void Hide()
    {
        vectorLine.SetWidth(0f);
    }

    public IEnumerator Highlight(float duration)
    {
        float time = 0f;
        vectorLine.SetColor(Color.red);
        vectorLine.points3 = new List<Vector3>() { vectorLine.points3[0] + Vector3.back, vectorLine.points3[1] + Vector3.back };
        vectorLine.Draw();
        while (time < duration)
        {
            time += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        vectorLine.SetColor(Color.white);
        Vector3 position0 = vectorLine.points3[0], position1 = vectorLine.points3[1];
        position0.z = 0f;
        position1.z = 0f;
        vectorLine.points3 = new List<Vector3>() { position0, position1 };
        vectorLine.Draw();
    }
}
