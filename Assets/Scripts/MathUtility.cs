using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MathUtility
{
    public static bool LinesIntersect(Line l1, Line l2, out Vector2 intersection)
    {
        // Two (infinite) lines always intersect when their directions are not equal
        if (!Equals(l1.GetDirection(), l2.GetDirection()) && !Equals(l1.GetDirection(), -l2.GetDirection())) 
        {
            Vector2 dn1 = l1.DirectionNormal;
            Vector2 dn2 = l2.DirectionNormal;
            float d1 = l1.D;
            float d2 = l2.D;

            float x = (dn2.y * d1 - dn1.y * d2) / (dn1.x * dn2.y - dn2.x * dn1.y);
            float y = (dn1.x * d2 - dn2.x * d1) / (dn1.x * dn2.y - dn2.x * dn1.y);

            intersection = new Vector2(x, y);
            return true;
        }
        // If their directions are equal they could still overlap, in which case there are an infinite number of intersections
        if (l1.ContainsPoint(l2.PointOnLine))
        {
            intersection = l2.PointOnLine;
            return true;
        }

        intersection = Vector2.zero;
        return false;
    }

    public static bool LinepiecesIntersect(Linepiece l1, Linepiece l2, out Vector2 intersection)
    {
        intersection = Vector2.zero;
        // There is one (unlikely) edge case: two equidirectional linepieces that overlap
        // LinesIntersect will return a random point on either line as the intersection point
        // This point might not lie on both linepieces
        // This could result in a false evaluation, while the linepieces do overlap
        if (LinesIntersect(l1, l2, out intersection))
        {
            return l1.ContainsPoint(intersection) && l2.ContainsPoint(intersection);
        }
        return false;
    }

    public static bool LinepieceIntersectWithLine(Linepiece l1, Line l2, out Vector2 intersection)
    {
        intersection = Vector2.zero;
        if (LinesIntersect(l1, l2, out intersection))
        {
            return l1.ContainsPoint(intersection);
        }
        return false;
    }

    public static Vector2 ClosestPointOnLinepiece(Linepiece linepiece, Vector2 point)
    {
        Vector2 ab = linepiece.End - linepiece.Start;
        Vector2 ap = point - linepiece.Start;
        float abSquared = ab.x * ab.x + ab.y * ab.y;
        float dotABAP = ab.x * ap.x + ab.y * ap.y;
        // Make sure that we don't divide by zero here
        // If so, just return the original point
        if (Mathf.Approximately(abSquared, 0f))
        {
            return linepiece.Start;
        }
        float ratio = dotABAP / abSquared;
        if (ratio < 0f)
        {
            return linepiece.Start;
        }
        else if (ratio > 1f)
        {
            return linepiece.End;
        }
        Vector2 closestPoint = linepiece.Start + ab * ratio;
        return closestPoint;
    }

    public static float DistancePointToLinepiece(Linepiece linepiece, Vector2 point)
    {
        Vector2 closestPoint = ClosestPointOnLinepiece(linepiece, point);
        return Vector2.Distance(closestPoint, point);
    }

    public static Vector2 MirrorPointInLinepiece(Linepiece linepiece, Vector2 point)
    {
        Vector2 closestPointOnLine = ClosestPointOnLinepiece(linepiece, point);
        Vector2 mirroredPoint = point + 2f * (closestPointOnLine - point);
        return mirroredPoint;
    }

    /// <summary>
    /// Checks whether the number n is between a and b
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="n"></param>
    /// <returns></returns>
    public static bool IsBetween(float a, float b, float n)
    {
        return (b >= n && n >= a) || (a >= n && n >= b);
    }
}
