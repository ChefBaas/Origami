using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewVertex : MonoBehaviour
{
    public delegate void StartDrag();
    public event StartDrag OnStartDrag;
    public delegate void StopDrag();
    public event StopDrag OnStopDrag;

    [SerializeField] private MeshRenderer meshRenderer, textRenderer;
    [SerializeField] private BoxCollider boxCollider;
    [SerializeField] private TextMesh text;

    public void SetPosition(Vector2 position)
    {
        transform.position = position;
    }

    public void Show()
    {
        meshRenderer.enabled = true;
        boxCollider.enabled = true;
    }

    public void Hide()
    {
        meshRenderer.enabled = false;
        boxCollider.enabled = false;
    }

    public IEnumerator Highlight(float duration)
    {
        float time = 0f;
        transform.position += Vector3.back;
        meshRenderer.material.color = Color.red;
        while (time < duration)
        {
            time += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        meshRenderer.material.color = Color.white;
        transform.position -= Vector3.back;
    }

    public void SetText(string text)
    {
        this.text.text = text;
    }

    private void OnMouseDown()
    {
        if (OnStartDrag != null)
        {
            OnStartDrag();
        }
    }

    private void OnMouseUp()
    {
        if (OnStopDrag != null)
        {
            OnStopDrag();
        }
    }

    private void OnMouseEnter()
    {
        textRenderer.enabled = true;
    }

    private void OnMouseExit()
    {
        textRenderer.enabled = false;
    }
}