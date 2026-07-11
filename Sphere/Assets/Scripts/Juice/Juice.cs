using PrimeTween;
using UnityEngine;

/// <summary>
/// Fachada de acceso al juice: Juice.S entrega el JuiceSettings de Resources
/// (o uno con valores por defecto si no existe, para que nada rompa).
/// Tambien centraliza efectos compartidos (ej: sacudida de camara).
/// </summary>
public static class Juice
{
    static JuiceSettings cached;

    /// <summary>Configuracion global de juice (Resources/JuiceSettings).</summary>
    public static JuiceSettings S
    {
        get
        {
            if (cached == null) cached = Resources.Load<JuiceSettings>("JuiceSettings");
            if (cached == null) cached = ScriptableObject.CreateInstance<JuiceSettings>();
            return cached;
        }
    }

    /// <summary>Sacudida de la camara principal (impactos: demolicion, etc.).</summary>
    public static void ShakeCamera()
    {
        Camera cam = Camera.main;
        if (cam == null || !S.sabotageEnabled) return;
        Tween.ShakeLocalPosition(cam.transform,
            Vector3.one * S.cameraShakeStrength, S.cameraShakeDuration, frequency: 10f);
    }
}
