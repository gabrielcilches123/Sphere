using UnityEngine;

/// <summary>
/// Fuerza a la camara a limpiar con un color solido (negro por defecto), para el
/// fondo espacial. [ExecuteAlways] hace que tambien se vea en modo edicion.
/// </summary>
[ExecuteAlways]
[RequireComponent(typeof(Camera))]
public class SolidBlackBackground : MonoBehaviour
{
    public Color background = Color.black;

    void OnEnable() { Apply(); }
    void Update() { Apply(); }

    void Apply()
    {
        Camera cam = GetComponent<Camera>();
        if (cam == null) return;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = background;
    }
}
