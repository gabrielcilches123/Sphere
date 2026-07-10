using UnityEngine;

/// <summary>
/// Una estrella que cae desde el cielo hacia el centro del planeta. Al tocar la
/// superficie se detiene y se emparenta al planeta (para girar con el). Se puede
/// recoger con click en cualquier momento (cayendo o ya aterrizada).
/// </summary>
[RequireComponent(typeof(Collider))]
public class FallingStar : MonoBehaviour
{
    Transform planet;
    Vector3 center;
    float surfaceRadius;
    float fallSpeed;
    bool landed;

    public void Init(Transform planet, Vector3 center, float surfaceRadius, float fallSpeed)
    {
        this.planet = planet;
        this.center = center;
        this.surfaceRadius = surfaceRadius;
        this.fallSpeed = fallSpeed;
    }

    void Update()
    {
        if (landed) return;

        Vector3 toCenter = center - transform.position;
        float dist = toCenter.magnitude;
        float targetDist = surfaceRadius + transform.localScale.x * 0.5f;

        float step = fallSpeed * Time.deltaTime;
        if (dist - step <= targetDist)
        {
            Land();
        }
        else
        {
            transform.position += toCenter.normalized * step;
        }
    }

    void Land()
    {
        landed = true;

        // Apoyar exactamente sobre la superficie (en el plano z=0 de la rueda 2D).
        Vector3 dirOut = transform.position - center;
        dirOut.z = 0f;
        if (dirOut.sqrMagnitude < 0.0001f) dirOut = Vector3.up;
        dirOut.Normalize();
        transform.position = center + dirOut * (surfaceRadius + transform.localScale.x * 0.5f);

        // Colgar del planeta para que gire junto con la superficie y los cactus.
        if (planet != null) transform.SetParent(planet, true);
    }

    /// <summary>Recoge la estrella: suma al contador y se destruye.</summary>
    public void Collect()
    {
        if (GameManager.Instance != null) GameManager.Instance.AddStar();
        Destroy(gameObject);
    }
}
