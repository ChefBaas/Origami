using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseDraggable : MonoBehaviour
{
    public delegate void MoveFinished();
    public event MoveFinished OnMoveFinished;

    private Vector3 lastPosition;
    private float speed = 0.01f;
    private bool isMoving = false;

    private void OnMouseDown()
    {
        lastPosition = Input.mousePosition;
    }

    private void OnMouseDrag()
    {
        isMoving = true;
        transform.position += (Input.mousePosition - lastPosition) * speed;
        lastPosition = Input.mousePosition;
    }

    private void OnMouseUp()
    {
        if (!isMoving)
        {
            if (OnMoveFinished != null)
            {
                OnMoveFinished();
            }
            isMoving = false;
        }
    }
}
