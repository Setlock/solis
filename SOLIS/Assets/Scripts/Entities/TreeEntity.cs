using UnityEngine;

public class TreeEntity : Entity
{
    // Update is called once per frame
    public Color leafColor, baseColor;
    private void Start()
    {
        SetColors();
    }
    void SetColors()
    {
        transform.GetChild(0).GetComponent<SpriteRenderer>().color = baseColor;
        transform.GetChild(1).GetComponent<SpriteRenderer>().color = leafColor;
    }
}
