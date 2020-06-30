using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LayerSorter : MonoBehaviour
{
    [SerializeField]
    private bool runOnlyOnce = true;

    private Renderer myRenderer;
    void Start()
    {
        myRenderer = gameObject.GetComponent<Renderer>();
    }

    void LateUpdate()
    {
        myRenderer.sortingOrder = (int)((transform.position.y - myRenderer.bounds.size.y / 2f)*-100);
        if (runOnlyOnce)
        {
            Destroy(this);
        }
    }
}
