using PrimeTween;
using UnityEngine;

/// <summary>
/// Una estrella que cae desde el cielo hacia el centro del planeta. Al tocar la
/// superficie se detiene (con squash & stretch) y se emparenta al planeta. Al
/// recogerla hace un punch y vuela hacia el contador de la UI.
/// Todo el juice se configura en Resources/JuiceSettings.
/// </summary>
[RequireComponent(typeof(Collider))]
public class FallingStar : MonoBehaviour
{
    Transform planet;
    Vector3 center;
    float surfaceRadius;
    float fallSpeed;
    bool landed;
    bool collected;

    public void Init(Transform planet, Vector3 center, float surfaceRadius, float fallSpeed)
    {
        this.planet = planet;
        this.center = center;
        this.surfaceRadius = surfaceRadius;
        this.fallSpeed = fallSpeed;
    }

    void Update()
    {
        if (landed || collected) return;

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

        // Squash & stretch del impacto. La fuerza es PROPORCIONAL a la escala actual
        // (una fuerza absoluta puede volver la escala negativa y romper el collider).
        JuiceSettings s = Juice.S;
        if (s.landEnabled)
        {
            Vector3 strength = Vector3.Scale(s.landSquash, transform.localScale);
            Tween.PunchScale(transform, strength, s.landSquashDuration);
        }
    }

    /// <summary>Recoge la estrella: punch, vuelo hacia el contador y +1.</summary>
    public void Collect()
    {
        if (collected) return;
        collected = true;

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;
        transform.SetParent(null, true); // que no la arrastre el giro del planeta

        JuiceSettings s = Juice.S;
        if (!s.collectEnabled)
        {
            if (GameManager.Instance != null) GameManager.Instance.AddStar();
            Destroy(gameObject);
            return;
        }

        Tween.StopAll(transform);
        Sequence.Create()
            .Chain(Tween.PunchScale(transform, transform.localScale * s.collectPunch, s.collectPunchDuration))
            .Chain(Tween.Position(transform, CounterWorldPos(), s.collectFlyDuration, s.collectFlyEase))
            .Group(Tween.Scale(transform, 0.1f, s.collectFlyDuration, Ease.InQuad))
            .ChainCallback(() =>
            {
                if (GameManager.Instance != null) GameManager.Instance.AddStar();
                Destroy(gameObject);
            });
    }

    /// <summary>Posicion (mundo) del contador de estrellas de la UI, como destino del vuelo.</summary>
    Vector3 CounterWorldPos()
    {
        Camera cam = Camera.main;
        if (cam == null) return transform.position + Vector3.up * 2f;

        float depth = transform.position.z - cam.transform.position.z;

        GameManager gm = GameManager.Instance;
        if (gm != null && gm.counterText != null)
        {
            // Canvas overlay: la posicion del texto ya esta en pixeles de pantalla.
            Vector3 screen = gm.counterText.transform.position;
            return cam.ScreenToWorldPoint(new Vector3(screen.x, screen.y, depth));
        }

        // Fallback: esquina superior derecha.
        return cam.ViewportToWorldPoint(new Vector3(0.93f, 0.94f, depth));
    }

    void OnDestroy()
    {
        Tween.StopAll(transform);
    }
}
