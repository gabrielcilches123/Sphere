using UnityEngine;

/// <summary>
/// Genera un campo de estrellas como fondo espacial, ADAPTANDOSE a la camara principal
/// (funciona con camara ortografica o en perspectiva).
///
/// Las estrellas se crean como hijas de la camara, a una distancia dentro del Far Clip
/// Plane, y repartidas para cubrir todo el encuadre. Asi nunca quedan recortadas ni fuera
/// de cuadro aunque muevas o cambies la camara. Se generan al iniciar Play.
/// </summary>
public class Starfield : MonoBehaviour
{
    [Tooltip("Cantidad de estrellas.")]
    public int starCount = 200;

    [Tooltip("Margen extra fuera del borde de la pantalla (1 = justo al borde).")]
    public float coverage = 1.15f;

    [Tooltip("Tamano de cada estrella (unidades de mundo en ortografica).")]
    public float minSize = 0.03f;
    public float maxSize = 0.09f;

    [Tooltip("Distancia delante de la camara. 0 = automatico (dentro del Far Clip).")]
    public float distance = 0f;

    void Start()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogWarning("Starfield: no encontre Camera.main (la camara necesita el tag 'MainCamera').");
            return;
        }

        // Distancia segura: dentro del far clip y delante del near clip.
        float dist = distance > 0f
            ? distance
            : Mathf.Clamp(cam.farClipPlane * 0.6f, cam.nearClipPlane + 0.5f, cam.farClipPlane - 0.5f);

        // Medio-alto y medio-ancho del encuadre a esa distancia.
        float halfH = cam.orthographic
            ? cam.orthographicSize
            : Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad) * dist;
        float halfW = halfH * cam.aspect;

        Material mat = MakeUnlitWhite();

        for (int i = 0; i < starCount; i++)
        {
            GameObject star = GameObject.CreatePrimitive(PrimitiveType.Quad);
            star.name = "Star";
            star.transform.SetParent(cam.transform, false); // relativo a la camara

            float x = Random.Range(-halfW, halfW) * coverage;
            float y = Random.Range(-halfH, halfH) * coverage;
            star.transform.localPosition = new Vector3(x, y, dist);
            star.transform.localRotation = Quaternion.identity; // la cara del Quad mira a la camara

            // En perspectiva escalamos con la distancia para tamano aparente uniforme.
            float sizeFactor = cam.orthographic ? 1f : dist;
            float s = Random.Range(minSize, maxSize) * sizeFactor;
            star.transform.localScale = new Vector3(s, s, s);

            Collider col = star.GetComponent<Collider>();
            if (col != null) Destroy(col);

            star.GetComponent<Renderer>().sharedMaterial = mat;
        }
    }

    static Material MakeUnlitWhite()
    {
        Shader sh = Shader.Find("Universal Render Pipeline/Unlit");
        if (sh == null) sh = Shader.Find("Unlit/Color");
        if (sh == null) sh = Shader.Find("Sprites/Default");

        Material mat = new Material(sh);
        mat.color = Color.white;
        mat.SetColor("_BaseColor", Color.white); // URP Unlit usa _BaseColor
        return mat;
    }
}
