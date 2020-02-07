using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EdgeComparisonInfo
{
    public Edge e1;
    public Edge e2;
    public float signedAngle = 0f;

    public EdgeComparisonInfo() { }

    public void Report()
    {
        Debug.LogFormat("SignedAngle between {0} and {1} is {2}", e1, e2, signedAngle);
    }
}
