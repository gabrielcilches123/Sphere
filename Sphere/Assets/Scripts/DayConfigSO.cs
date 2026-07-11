using UnityEngine;

/// <summary>
/// Configuracion de UN dia como asset (GDD 6/8): crea uno por dia con
/// "Assets > Create > Sphere > Day Config" y anadelo a DayManager > Day Configs.
/// Los dias sin asset usan los valores por defecto (dia normal).
/// </summary>
[CreateAssetMenu(fileName = "Day_01", menuName = "Sphere/Day Config")]
public class DayConfigSO : ScriptableObject
{
    [Tooltip("Numero de dia al que aplica esta configuracion.")]
    public int day = 1;

    [Header("Estrellas")]
    [Tooltip("Estrellas que caen este dia. -1 = usar el starsPerDay del DayManager.")]
    public int starsOverride = -1;

    [Header("Evento de telescopio")]
    [Tooltip("Si este dia tiene evento de telescopio (cinematica al mirar).")]
    public bool telescopeEvent = false;

    [TextArea]
    [Tooltip("Texto de la cinematica de ESTE dia. Vacio = mensaje por defecto.")]
    public string telescopeMessage = "";

    [Tooltip("Duracion de la cinematica en segundos. -1 = duracion por defecto.")]
    public float telescopeDuration = -1f;

    [Header("Guion del dia (StoryDirector)")]
    [TextArea]
    [Tooltip("Lineas que dice el PLAYER al despertar este dia (vacio = nada).")]
    public string[] wakeUpLines;

    [TextArea]
    [Tooltip("Lineas del AMIGO este dia (si no esta vacio, reemplaza su dialogo).")]
    public string[] friendLines;

    [Tooltip("Este dia el amigo se despide y se va en el cohete (GDD dia 2).")]
    public bool friendLeaves = false;

    [Tooltip("Al despertar, el cohete en obra se destruye (auto-sabotaje, GDD dia 10).")]
    public bool demolishRocket = false;

    [Tooltip("Desde este dia se APAGA la recoleccion de estrellas (queda apagada " +
             "los dias siguientes; GDD dia 12).")]
    public bool starCollectionDisabled = false;
}
