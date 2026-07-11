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

    [Tooltip("Conversacion del dia al clickear al amigo (si no esta vacia, reemplaza " +
             "su dialogo). Cada linea define QUIEN habla (Amigo/Player) y el orden.")]
    public DialogueLine[] dialogueLines;

    [Tooltip("Variante de la conversacion cuando el cohete en obra esta COMPLETO " +
             "(vacio = usar siempre la normal).")]
    public DialogueLine[] dialogueLinesRocketComplete;

    [Tooltip("Este dia el amigo se despide y se va en el cohete (GDD dia 2). La secuencia " +
             "espera a que el PLAYER clickee al amigo para empezar.")]
    public bool friendLeaves = false;

    [TextArea]
    [Tooltip("Lineas del PLAYER despues de que el amigo se va (tras el fade).")]
    public string[] afterFriendLeavesLines;

    [Tooltip("Desde este dia se activa el AUTO-SABOTAJE (GDD dia 10, queda activo): " +
             "cuando al cohete le falte UNA estrella, el player duda y lo destruye.")]
    public bool sabotageRocket = false;

    [TextArea]
    [Tooltip("Lineas del PLAYER en el momento del sabotaje (vacio = las por defecto " +
             "del StoryDirector).")]
    public string[] sabotageLines;

    [Tooltip("Desde este dia se APAGA la recoleccion de estrellas (queda apagada " +
             "los dias siguientes; GDD dia 12).")]
    public bool starCollectionDisabled = false;

    [Header("Dormir")]
    [Tooltip("Este dia NO se puede dormir hasta completar el cohete en obra (GDD dia 1).")]
    public bool requireRocketCompleteToSleep = false;

    [TextArea]
    [Tooltip("Linea del player si intenta dormir bloqueado.")]
    public string cantSleepLine = "Todavia no... el cohete no esta terminado.";
}
