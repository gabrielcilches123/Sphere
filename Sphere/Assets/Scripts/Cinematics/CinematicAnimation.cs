using System.Collections;
using UnityEngine;

/// <summary>
/// Base de la ANIMACION especifica de una cinematica. Cada cinematica que necesite
/// animacion tiene su propio script (subclase) colocado EN su CinematicPanel, con
/// sus variables particulares (trayectorias, balanceos, referencias directas a sus
/// elementos...). El reproductor la ejecuta entre el fade de entrada y el de salida.
///
/// Un panel sin CinematicAnimation es valido: se muestra estatico durante la duracion.
/// </summary>
[RequireComponent(typeof(CinematicPanel))]
public abstract class CinematicAnimation : MonoBehaviour
{
    /// <summary>
    /// Programa aqui la animacion. Usa ctx.animationTime como tiempo disponible
    /// (la duracion total menos los fades, que ya los maneja el reproductor).
    /// </summary>
    public abstract IEnumerator Animate(CinematicContext ctx);

    // ---------- Helpers reutilizables ----------

    /// <summary>Mueve un elemento entre dos anclas de viewport (0..1) durante 'time' segundos.</summary>
    protected IEnumerator MoveViewport(RectTransform rt, Vector2 from, Vector2 to, float time)
    {
        if (rt == null) yield break;
        float t = 0f;
        while (t < time)
        {
            t += Time.deltaTime;
            Vector2 p = Vector2.Lerp(from, to, t / time);
            rt.anchorMin = p;
            rt.anchorMax = p;
            yield return null;
        }
        rt.anchorMin = rt.anchorMax = to;
    }
}
