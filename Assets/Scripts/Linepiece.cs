using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Linepiece : Line
{
    private Vector2 start, end;
    public Vector2 Start
    {
        get => start;
    }
    public Vector2 End
    {
        get => end;
    }

    public Linepiece(Vector2 start, Vector2 end)
        :base(start, end)
    {
        this.start = start;
        this.end = end;
    }

    public override bool ContainsPoint(Vector2 p)
    {
        if (base.ContainsPoint(p))
        {
            if (MathUtility.IsBetween(start.x, end.x, p.x) && MathUtility.IsBetween(start.y, end.y, p.y))
            {
                return true;
            }
            return false;
        }
        return false;
    }
}
