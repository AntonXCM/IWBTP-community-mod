using UnityEngine;

public class DistanceFade : ObjectVisual
{
    [SerializeField] private Vector2 distances;
    [SerializeField] private Transform light;
    private SpriteRenderer sprite;
    Color colorAlphed, colorUnalphed;
    protected override void Start()
    {
        base.Start();
        sprite = GetComponent<SpriteRenderer>();
    }

    protected override void Render()
    {
        Color baseColor = sprite.color;
        if (baseColor != colorAlphed) { colorUnalphed = baseColor; }
        float lighta = 1f - Mathf.Clamp01(((transform.position - light.position).magnitude - distances.x) / (distances.y - distances.x));
        
        baseColor = colorUnalphed;
        baseColor.a *= lighta;
        colorAlphed = baseColor;
        sprite.color = baseColor;
    }
}
