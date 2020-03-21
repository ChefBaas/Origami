using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewVertex
{
    public delegate void StartDrag();
    public event StartDrag OnStartDrag;
    public delegate void StopDrag();
    public event StopDrag OnStopDrag;

    private GameObject cube;

    public ViewVertex()
    {
        cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.localScale = Vector3.one * 0.2f;
        MouseEventResponder mouseEventResponder = cube.AddComponent<MouseEventResponder>();
        mouseEventResponder.mouseDown = MouseDown;
        mouseEventResponder.mouseDrag = MouseDrag;
        mouseEventResponder.mouseUp = MouseUp;
    }

    public void SetPosition(Vector3 position)
    {
        cube.transform.position = position;
    }

    private void MouseDown()
    {
        if (OnStartDrag != null)
        {
            Debug.Log(0);
            OnStartDrag();
        }
    }

    private void MouseDrag()
    {

    }

    private void MouseUp()
    {
        if (OnStopDrag != null)
        {
            OnStopDrag();
        }
    }
}