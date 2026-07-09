using UnityEngine;

/// <summary>
/// Aplica un color propio a este objeto sin tocar el material compartido,
/// usando un MaterialPropertyBlock (cada objeto puede tener un color distinto
/// aunque compartan el mismo material). [ExecuteAlways] para verlo en edicion.
/// </summary>
[ExecuteAlways]
[RequireComponent(typeof(Renderer))]
public class TintColor : MonoBehaviour
{
    public Color color = Color.white;

    void OnEnable() { Apply(); }
    void Update() { Apply(); }

    void Apply()
    {
        Renderer r = GetComponent<Renderer>();
        if (r == null) return;
        MaterialPropertyBlock mpb = new MaterialPropertyBlock();
        r.GetPropertyBlock(mpb);
        mpb.SetColor("_BaseColor", color);
        r.SetPropertyBlock(mpb);
    }
}
