using UnityEngine;

/// <summary>
/// Genera estrellas caidas (prefab sprite 2D) cada cierto intervalo, en un angulo
/// aleatorio alrededor del planeta. Caen en un plano ADELANTADO hacia la camara
/// (frontOffset) para que, en vista ortografica, se vean sobre la misma linea que
/// las estructuras pero SIEMPRE por delante -> siempre clickeables, nunca detras.
/// </summary>
public class StarSpawner : MonoBehaviour
{
    [Header("Planeta")]
    [Tooltip("El planeta (centro y padre al aterrizar). Si esta vacio se busca 'Planet'.")]
    public Transform planet;

    [Tooltip("Radio de la superficie del planeta donde aterrizan las estrellas.")]
    public float surfaceRadius = 7.5f;

    [Header("Prefab de estrella (sprite 2D)")]
    [Tooltip("Prefab a instanciar. Si esta vacio se carga Resources/Star.")]
    public GameObject starPrefab;

    [Header("Spawn")]
    [Tooltip("Distancia desde el centro donde nacen las estrellas (debe ser > surfaceRadius).")]
    public float spawnRadius = 11f;

    [Tooltip("Segundos entre cada estrella.")]
    public float spawnInterval = 1.5f;

    [Tooltip("Velocidad de caida (unidades por segundo).")]
    public float fallSpeed = 6f;

    [Tooltip("Tamano de cada estrella.")]
    public float starSize = 0.5f;

    [Tooltip("Cuanto se adelantan las estrellas hacia la camara para que nunca queden " +
             "detras del planeta. Debe superar el radio en Z del planeta.")]
    public float frontOffset = 5f;

    float timer;

    void Awake()
    {
        if (planet == null)
        {
            GameObject p = GameObject.Find("Planet");
            if (p != null) planet = p.transform;
        }
        if (starPrefab == null)
            starPrefab = Resources.Load<GameObject>("Star");
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsBusy) return;
        if (planet == null || starPrefab == null) return;

        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer -= spawnInterval;
            SpawnStar();
        }
    }

    void SpawnStar()
    {
        // Centro de caida en el plano frontal (adelantado hacia la camara, -Z).
        Vector3 center = planet.position;
        center.z -= frontOffset;

        float ang = Random.Range(0f, Mathf.PI * 2f);
        Vector3 dir = new Vector3(Mathf.Cos(ang), Mathf.Sin(ang), 0f);
        Vector3 spawnPos = center + dir * spawnRadius;

        GameObject go = Instantiate(starPrefab, spawnPos, Quaternion.identity);
        go.transform.localScale = Vector3.one * starSize;

        FallingStar fs = go.GetComponent<FallingStar>();
        if (fs == null) fs = go.AddComponent<FallingStar>();
        fs.Init(planet, center, surfaceRadius, fallSpeed);
    }
}
