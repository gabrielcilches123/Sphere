using System;
using UnityEngine;

/// <summary>
/// Un cohete del catalogo del banco de crafteo (GDD 7.2). Meramente visual:
/// sin estadisticas reales, porque ninguno va a funcionar (tema del juego).
/// </summary>
[Serializable]
public class RocketData
{
    [Tooltip("Nombre que se muestra en el panfleto.")]
    public string rocketName = "Cohete";

    [Tooltip("Estrellas necesarias para completarlo.")]
    public int starCost = 10;

    [Tooltip("Icono del cohete en el panfleto (opcional; vacio = placeholder).")]
    public Sprite icon;

    [Tooltip("Color placeholder del cohete.")]
    public Color color = new Color(0.7f, 0.7f, 0.75f);

    [Tooltip("Altura del cohete completo (placeholder).")]
    public float height = 2.5f;
}
