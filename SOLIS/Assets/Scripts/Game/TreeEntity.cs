using UnityEngine;

public class TreeEntity : Entity
{
    // Update is called once per frame
    public Color[] colors;
    private void Start()
    {
        SetRandomColors();
    }
    void Update()
    {
        SetChildLayer();
    }
    void SetRandomColors()
    {
        for(int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).GetComponent<SpriteRenderer>().color = colors[i];
        }
    }
}
