using System.Collections;
using UnityEngine;

/// <summary>
/// EJEMPLO de animacion especifica de una cinematica: el amigo/npc dentro del cohete
/// se mueve hacia la DERECHA con un pequeno shake erratico (simula el traqueteo del
/// cohete). Copia este script como plantilla para cada cinematica nueva: va EN el
/// panel de esa cinematica y sus variables se configuran en la escena con referencias
/// directas a los elementos que anima.
/// </summary>
public class RocketTravelAnimation : CinematicAnimation
{
    [Tooltip("El cohete (con el pinguino dentro como hijo). Arrastra el elemento del panel.")]
    public RectTransform rocket;

    [Header("Trayectoria (anclas de viewport 0..1)")]
    public Vector2 from = new Vector2(-0.15f, 0.5f);
    public Vector2 to = new Vector2(1.15f, 0.55f);

    [Header("Shake erratico (traqueteo del cohete)")]
    [Tooltip("Amplitud del temblor en pixeles.")]
    public float shakeAmount = 6f;
    [Tooltip("Velocidad del temblor.")]
    public float shakeSpeed = 18f;

    [Header("Balanceo (rotacion)")]
    public float wobbleAngle = 6f;
    public float wobbleSpeed = 5f;

    public override IEnumerator Animate(CinematicContext ctx)
    {
        if (rocket == null) yield break;

        float t = 0f;
        while (t < ctx.animationTime)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / ctx.animationTime);

            // Avanza hacia la derecha por la trayectoria.
            Vector2 p = Vector2.Lerp(from, to, k);
            rocket.anchorMin = rocket.anchorMax = p;

            // Shake erratico con ruido Perlin (organico, no repetitivo).
            float jx = (Mathf.PerlinNoise(t * shakeSpeed, 0.3f) - 0.5f) * 2f * shakeAmount;
            float jy = (Mathf.PerlinNoise(0.7f, t * shakeSpeed) - 0.5f) * 2f * shakeAmount;
            rocket.anchoredPosition = new Vector2(jx, jy);

            // Balanceo suave del morro.
            rocket.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(t * wobbleSpeed) * wobbleAngle);

            yield return null;
        }

        // Reset para la proxima reproduccion.
        rocket.anchoredPosition = Vector2.zero;
        rocket.localRotation = Quaternion.identity;
    }
}
