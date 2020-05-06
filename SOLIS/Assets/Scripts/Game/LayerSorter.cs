using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LayerSorter : MonoBehaviour
{
    [SerializeField]
    private bool runOnlyOnce = false;

    private Renderer myRenderer;
    void Start()
    {
        myRenderer = gameObject.GetComponent<Renderer>();
    }

    void LateUpdate()
    {
        myRenderer.sortingOrder = (int)((transform.position.y - myRenderer.bounds.size.y / 2f)*-1000);
        if (runOnlyOnce)
        {
            Destroy(this);
        }
    }
}
