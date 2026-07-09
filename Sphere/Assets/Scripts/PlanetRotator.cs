using UnityEngine;

/// <summary>
/// Rota todo el planeta y su escenario (que cuelgan como hijos de este objeto)
/// sobre su propio eje. El personaje se queda "arriba" y el mundo gira debajo.
///
/// Para la vista lateral 2D (rueda) el eje debe ser (0,0,1): gira hacia la camara.
/// </summary>
public class PlanetRotator : MonoBehaviour
{
    [Tooltip("Velocidad de rotacion en grados por segundo. Negativo invierte el sentido.")]
    public float degreesPerSecond = 20f;

    [Tooltip("Eje sobre el que gira el planeta. Vista lateral 2D (rueda) = (0,0,1).")]
    public Vector3 axis = new Vector3(0f, 0f, 1f);

    void Update()
    {
        if (axis == Vector3.zero) return;
        transform.Rotate(axis.normalized, degreesPerSecond * Time.deltaTime, Space.Self);
    }
}
