using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Ciclo dia/noche. De dia: sol mas intenso y fondo mas claro, NPCs presentes.
/// De noche: sol tenue, fondo oscuro, NPCs ocultos. El cambio ocurre durante un
/// fade a negro (TransitionManager). Se dispara con boton (ToggleDayNight) o eventos (SetNight).
/// </summary>
public class DayNightManager : MonoBehaviour
{
    public static DayNightManager Instance { get; private set; }

    [Header("Estado inicial")]
    public bool startAtNight = false;

    [Header("Luz (sol)")]
    [Tooltip("Luz direccional. Vacio = se busca 'Directional Light'.")]
    public Light sun;
    [Tooltip("Intensidad del sol de dia.")]
    public float dayIntensity = 1.4f;
    [Tooltip("Intensidad del sol de noche.")]
    public float nightIntensity = 0.2f;

    [Header("Fondo (color de la camara)")]
    [Tooltip("Camara. Vacio = Camera.main.")]
    public Camera cam;
    public Color dayColor = new Color(0.45f, 0.62f, 0.85f);
    public Color nightColor = new Color(0.02f, 0.02f, 0.06f);

    public bool IsNight { get; private set; }

    readonly List<NPC> npcs = new List<NPC>();

    void Awake() { Instance = this; }

    void Start()
    {
        if (sun == null)
        {
            GameObject go = GameObject.Find("Directional Light");
            if (go != null) sun = go.GetComponent<Light>();
        }
        if (cam == null) cam = Camera.main;

        npcs.Clear();
        npcs.AddRange(FindObjectsByType<NPC>(FindObjectsInactive.Include, FindObjectsSortMode.None));

        IsNight = startAtNight;
        ApplyState();
    }

    /// <summary>Alterna dia/noche (para el boton en pantalla).</summary>
    public void ToggleDayNight() => SetNight(!IsNight);

    /// <summary>Cambia a dia/noche con transicion. Llamable desde cualquier evento.</summary>
    public void SetNight(bool night)
    {
        if (TransitionManager.Instance != null)
            TransitionManager.Instance.Play(() => { IsNight = night; ApplyState(); });
        else { IsNight = night; ApplyState(); }
    }

    /// <summary>Marca a los NPC presentes como 'se fueron' y los oculta (usar dentro de un fade).</summary>
    public void MakeNpcsLeave()
    {
        foreach (NPC n in npcs) if (n != null) n.HasLeft = true;
        ApplyState();
    }

    void ApplyState()
    {
        foreach (NPC n in npcs)
            if (n != null) n.gameObject.SetActive(!IsNight && !n.HasLeft);

        if (sun != null) sun.intensity = IsNight ? nightIntensity : dayIntensity;

        if (cam != null)
        {
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = IsNight ? nightColor : dayColor;
        }
    }
}
