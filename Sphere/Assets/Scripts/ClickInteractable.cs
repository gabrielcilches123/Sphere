using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Objeto interactuable con un click simple (telescopio, banco de crafteo, etc.).
/// PlanetController lo detecta en su raycast y llama a Click().
/// </summary>
[RequireComponent(typeof(Collider))]
public class ClickInteractable : MonoBehaviour
{
    [Tooltip("Accion al hacer click sobre el objeto.")]
    public UnityEvent onClick;

    public void Click() => onClick?.Invoke();
}
