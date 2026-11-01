using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(SpriteRenderer))]
public class ScriptInSpritesFolder : MonoBehaviour
{
    static readonly int UVRectId = Shader.PropertyToID("_UVRect");
    SpriteRenderer sr;
    MaterialPropertyBlock mpb;

    void OnEnable()  { Apply(); }
    void OnValidate() { Apply(); }

    public void Apply()
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        if (sr.sprite == null) return;
        if (mpb == null) mpb = new MaterialPropertyBlock();

        var sprite = sr.sprite;
        var tex = sprite.texture;          // при упаковке в атлас вернёт текстуру атласа
        Rect r = sprite.textureRect;       // Rect в пикселях (без учёта Extrude)

        Vector4 uvRect = new Vector4(
            r.xMin / tex.width,
            r.yMin / tex.height,
            r.xMax / tex.width,
            r.yMax / tex.height);

        sr.GetPropertyBlock(mpb);
        mpb.SetVector(UVRectId, uvRect);
        sr.SetPropertyBlock(mpb);
    }
}