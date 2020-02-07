using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// We represent a line as ax + by = d
// The vector (a, b) is perpendicular to the direction of the line
// If (a, b) is normalized, d is the distance to the origin
public class Line
{
    protected Vector2 pointOnLine;
    public Vector2 PointOnLine
    {
        get => pointOnLine;
    }
    protected Vector2 anotherPointOnLine;
    public Vector2 AnotherPointOnLine
    {
        get => anotherPointOnLine;
    }
    protected Vector2 directionNormal;
    public Vector2 DirectionNormal
    {
        get => directionNormal;
    }
    protected float d;
    public float D
    {
        get => d;
    }

    public Line(Vector2 firstPointOnLine, Vector2 secondPointOnLine)
    {
        directionNormal = new Vector2(-(secondPointOnLine.y - firstPointOnLine.y), secondPointOnLine.x - firstPointOnLine.x);
        directionNormal.Normalize();
        pointOnLine = firstPointOnLine;
        anotherPointOnLine = secondPointOnLine;
        d = directionNormal.x * pointOnLine.x + directionNormal.y * pointOnLine.y;
    }

    public Vector2 GetDirection()
    {
        return new Vector2(directionNormal.y, -directionNormal.x);
    }

    public virtual bool ContainsPoint(Vector2 p)
    {
        float cosTheta = Vector2.Dot((p - pointOnLine).normalized, GetDirection().normalized);
        if (Mathf.Approximately(Mathf.Abs(cosTheta), 1f))
        {
            return true;
        }
        if (cosTheta > 0.9f)
        {
            //Debug.LogFormat("Cos(theta) was {0}", cosTheta);
        }
        return false;
    }
}
