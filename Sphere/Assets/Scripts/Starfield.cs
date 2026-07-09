using UnityEngine;

/// <summary>
/// Genera un campo de estrellas simple: muchos puntos blancos pequenos repartidos
/// en un plano lejano detras del planeta, para dar el fondo espacial estilo Machinarium.
/// Se generan una sola vez al iniciar (Start).
/// </summary>
public class Starfield : MonoBehaviour
{
    [Tooltip("Cantidad de estrellas.")]
    public int starCount = 200;

    [Tooltip("Ancho/alto de la zona donde se reparten (unidades de mundo).")]
    public float areaWidth = 70f;
    public float areaHeight = 45f;

    [Tooltip("Profundidad (Z) donde viven las estrellas, detras del planeta.")]
    public float minDepth = 25f;
    public float maxDepth = 55f;

    [Tooltip("Tamano de cada estrella.")]
    public float minSize = 0.06f;
    public float maxSize = 0.22f;

    void Start()
    {
        Material mat = MakeUnlitWhite();

        for (int i = 0; i < starCount; i++)
        {
            GameObject star = GameObject.CreatePrimitive(PrimitiveType.Quad);
            star.name = "Star";
            star.transform.SetParent(transform, false);

            float x = Random.Range(-areaWidth, areaWidth);
            float y = Random.Range(-areaHeight, areaHeight);
            float z = Random.Range(minDepth, maxDepth);
            star.transform.localPosition = new Vector3(x, y, z);

            float s = Random.Range(minSize, maxSize);
            star.transform.localScale = new Vector3(s, s, s);

            // La cara frontal del Quad mira a -Z, justo hacia la camara (que esta en -Z
            // mirando a +Z). Rotacion identidad = visible. No hay que girarlo.
            star.transform.localRotation = Quaternion.identity;

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
