using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseEventResponder : MonoBehaviour
{
    public System.Action mouseDown, mouseDrag, mouseUp;

    private void OnMouseDown()
    {
        mouseDown();
    }

    private void OnMouseDrag()
    {
        mouseDrag();
    }

    private void OnMouseUp()
    {
        mouseUp();
    }
}
