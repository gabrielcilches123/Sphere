using UnityEngine;

/// <summary>
/// Plantilla GENERICA de una cinematica del telescopio (GDD 7.3). Un asset por
/// cinematica (Create > Sphere > Cinematic). ES la identidad de la cinematica:
/// - Los Day Config la referencian arrastrando este asset (telescopeCinematic).
/// - Su CinematicPanel de la escena (el arte) tambien la referencia arrastrandola.
/// - El mensaje y la duracion los controla ESTE asset (no el dia).
///
/// La animacion especifica (programar los objetos/personajes que se ven) va en un
/// script propio (subclase de CinematicAnimation) colocado en el panel, porque
/// necesita referencias directas a los elementos de escena que anima.
/// </summary>
[CreateAssetMenu(fileName = "Cinematic_", menuName = "Sphere/Cinematic")]
public class CinematicSO : ScriptableObject
{
    [Tooltip("Nombre descriptivo de la cinematica (solo contexto, sin logica). " +
             "Ej: 'El amigo viaja por la galaxia'.")]
    public string nombre = "";

    [TextArea]
    [Tooltip("Mensaje que se muestra durante la cinematica.")]
    public string message = "";

    [Tooltip("Duracion total (segundos).")]
    public float duration = 5f;

    [Tooltip("Fundido de entrada/salida (segundos). Lo aplica el reproductor.")]
    public float fadeTime = 0.5f;
}

/// <summary>Datos resueltos que el reproductor pasa a la animacion.</summary>
public class CinematicContext
{
    public CinematicPanel panel;   // el arte en la escena
    public MonoBehaviour host;     // para corrutinas anidadas si hacen falta
    public string message;         // mensaje de la cinematica
    public float duration;         // duracion total
    public float animationTime;    // tiempo disponible para animar (total - fades)
}
