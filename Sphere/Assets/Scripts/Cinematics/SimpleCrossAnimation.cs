using System.Collections;
using UnityEngine;

/// <summary>
/// Animacion especifica: UN elemento cruza la vista en linea recta (el cometa/amigo
/// de la cinematica 'viaje'). Va EN el panel de esa cinematica; sus variables se
/// configuran en la escena con referencias directas.
/// </summary>
public class SimpleCrossAnimation : CinematicAnimation
{
    [Tooltip("El elemento que cruza (arrastra aqui el hijo del panel, ej: Comet).")]
    public RectTransform mover;

    [Tooltip("Anclas de viewport (0..1) de inicio y fin del recorrido.")]
    public Vector2 from = new Vector2(-0.15f, 0.75f);
    public Vector2 to = new Vector2(1.15f, 0.45f);

    public override IEnumerator Animate(CinematicContext ctx)
    {
        yield return MoveViewport(mover, from, to, ctx.animationTime);
    }
}
