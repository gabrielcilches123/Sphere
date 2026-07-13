using UnityEngine;
using TMPro;

/// <summary>
/// El ARTE de una cinematica: un panel de la escena (bajo el Canvas principal) con
/// CanvasGroup + los elementos visuales. Se vincula a su cinematica ARRASTRANDO el
/// asset CinematicSO al campo 'cinematic' (sin ids).
///
/// La animacion especifica de sus objetos/personajes va en un script propio
/// (subclase de CinematicAnimation) en este mismo objeto.
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class CinematicPanel : MonoBehaviour
{
    [Tooltip("La cinematica de este panel (arrastra aqui su asset CinematicSO).")]
    public CinematicSO cinematic;

    [Tooltip("Texto donde se muestra el mensaje de la cinematica (opcional).")]
    public TMP_Text message;

    public CanvasGroup Group => GetComponent<CanvasGroup>();
}
