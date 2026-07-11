#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

/// <summary>
/// FASE 2 del plan (docs/PLAN-implementacion.md): Telescopio.
/// - Anade TelescopeCinematic al GameSystem.
/// - Crea el TELESCOPIO placeholder (cilindro inclinado) sobre el planeta, con
///   ClickInteractable + Telescope.
/// - Marca el dia 5 como dia de evento de telescopio (GDD) y el dia 2 para probar rapido.
///
/// Ejecutar con: Tools > Sphere > Run Fase 2 Setup  (idempotente: no duplica)
/// </summary>
public static class Fase2Setup
{
    [MenuItem("Tools/Sphere/Run Fase 2 Setup")]
    public static void Run()
    {
        GameObject gs = GameObject.Find("GameSystem");
        if (gs == null) { Debug.LogError("[Fase2] No encontre GameSystem."); return; }

        // 1. Cinematica placeholder.
        if (gs.GetComponent<TelescopeCinematic>() == null)
        {
            gs.AddComponent<TelescopeCinematic>();
            Debug.Log("[Fase2] TelescopeCinematic anadido al GameSystem.");
        }

        // 2. Telescopio placeholder.
        GameObject planet = GameObject.Find("Planet");
        if (GameObject.Find("Telescope") == null && planet != null)
        {
            GameObject telescope = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            telescope.name = "Telescope";

            // Posicion: angulo 45 (arriba-derecha), superficie r=7.5, plano frontal z=-5.
            Vector3 center = planet.transform.position;
            float ang = 45f * Mathf.Deg2Rad;
            Vector3 dir = new Vector3(Mathf.Cos(ang), Mathf.Sin(ang), 0f);
            telescope.transform.position = new Vector3(
                center.x + dir.x * 7.9f,
                center.y + dir.y * 7.9f,
                -5f);

            // De pie sobre la superficie (radial) + tubo inclinado hacia el cielo.
            telescope.transform.rotation = Quaternion.Euler(0f, 0f, 45f - 90f) * Quaternion.Euler(0f, 0f, 30f);
            telescope.transform.localScale = new Vector3(0.35f, 1.1f, 0.35f);
            telescope.transform.SetParent(planet.transform, true);

            telescope.AddComponent<ClickInteractable>();
            telescope.AddComponent<Telescope>();

            Debug.Log("[Fase2] Telescopio creado y colgado del planeta.");
        }

        // 3. Dias de evento de telescopio: dia 5 (GDD) + dia 2 (para probar rapido).
        DayManager dm = gs.GetComponent<DayManager>();
        if (dm != null)
        {
            // Cada dia de evento con SU cinematica (mensaje propio, GDD 7.3).
            EnsureTelescopeDay(dm, 2, "Ves a tu amigo surcando la galaxia en su cohete..."); // test rapido
            EnsureTelescopeDay(dm, 5, "Tu amigo aterriza en un planeta lejano. Se ve feliz."); // GDD dia 5
            EditorUtility.SetDirty(dm);
        }

        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
        Debug.Log("[Fase2] Setup completo y escena guardada.");
    }

    static void EnsureTelescopeDay(DayManager dm, int day, string message)
    {
        DayManager.DayConfig cfg = dm.dayConfigs.Find(c => c.day == day);
        if (cfg == null)
        {
            cfg = new DayManager.DayConfig { day = day, starsOverride = -1 };
            dm.dayConfigs.Add(cfg);
        }
        if (!cfg.telescopeEvent)
        {
            cfg.telescopeEvent = true;
            Debug.Log($"[Fase2] Dia {day} marcado como evento de telescopio.");
        }
        if (string.IsNullOrEmpty(cfg.telescopeMessage))
        {
            cfg.telescopeMessage = message;
            Debug.Log($"[Fase2] Dia {day}: mensaje de cinematica configurado.");
        }
    }
}
#endif
