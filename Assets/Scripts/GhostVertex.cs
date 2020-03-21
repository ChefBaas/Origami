using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostVertex : MonoBehaviour
{
    public delegate void FoundAnchor(ModelVertex vertex);
    public event FoundAnchor OnFoundAnchor;
    public delegate void LostAnchor();
    public event LostAnchor OnLostAnchor;

    [SerializeField]
    private GameObject child;
    private ModelVertex anchor;

    private void LateUpdate()
    {
        if (anchor != null)
        {
            child.transform.position = anchor.GetModelPosition();
        }
    }

    // TODO: MAKE THIS WORK AGAIN
    /*private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<ModelVertex>())
        {
            anchor = other.GetComponent<ModelVertex>();
            if (OnFoundAnchor != null)
            {
                OnFoundAnchor(anchor);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<ModelVertex>())
        {
            anchor = null;
            child.transform.position = transform.position;
            if (OnLostAnchor != null)
            {
                OnLostAnchor();
            }
        }
    }*/

    public Vector2 GetPosition()
    {
        if (anchor != null)
        {
            return anchor.GetModelPosition();
        }
        return transform.position;
    }
}


