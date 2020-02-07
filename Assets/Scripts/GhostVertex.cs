using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostVertex : MonoBehaviour
{
    public delegate void FoundAnchor(Vertex vertex);
    public event FoundAnchor OnFoundAnchor;
    public delegate void LostAnchor();
    public event LostAnchor OnLostAnchor;

    [SerializeField]
    private GameObject child;
    private Vertex anchor;

    private void LateUpdate()
    {
        if (anchor != null)
        {
            child.transform.position = anchor.transform.position;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Vertex>())
        {
            anchor = other.GetComponent<Vertex>();
            if (OnFoundAnchor != null)
            {
                OnFoundAnchor(anchor);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<Vertex>())
        {
            anchor = null;
            child.transform.position = transform.position;
            if (OnLostAnchor != null)
            {
                OnLostAnchor();
            }
        }
    }

    public Vector2 GetPosition()
    {
        if (anchor != null)
        {
            return anchor.transform.position;
        }
        return transform.position;
    }
}


