using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controla la rotacion del planeta segun el input del jugador.
///
/// - Click / toque en la MITAD IZQUIERDA de la pantalla  -> gira en sentido POSITIVO.
/// - Click / toque en la MITAD DERECHA  de la pantalla    -> gira en sentido NEGATIVO.
///
/// Mantener presionado hace que el planeta gire de forma continua mientras se sostiene.
/// Todo el escenario (hijos de este objeto) gira como un bloque; el personaje se queda arriba.
///
/// Usa el nuevo Input System (Pointer.current), valido para mouse, touch y web.
/// </summary>
public class PlanetController : MonoBehaviour
{
    [Tooltip("Velocidad de rotacion en grados por segundo mientras se mantiene presionado.")]
    public float degreesPerSecond = 60f;

    [Tooltip("Eje de rotacion. Vista lateral 2D (rueda) = (0,0,1).")]
    public Vector3 axis = new Vector3(0f, 0f, 1f);

    void Update()
    {
        Pointer pointer = Pointer.current;
        if (pointer == null || !pointer.press.isPressed) return;
        if (axis == Vector3.zero) return;

        float x = pointer.position.ReadValue().x;

        // Izquierda = positivo, Derecha = negativo.
        float dir = (x < Screen.width * 0.5f) ? 1f : -1f;

        transform.Rotate(axis.normalized, dir * degreesPerSecond * Time.deltaTime, Space.Self);
    }
}
