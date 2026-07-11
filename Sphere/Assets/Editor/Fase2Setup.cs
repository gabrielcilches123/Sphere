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

        // 3. Los dias de evento ahora se configuran como assets DayConfigSO
        //    (los crea Fase 4 Setup, o a mano: Create > Sphere > Day Config).

        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
        Debug.Log("[Fase2] Setup completo y escena guardada.");
    }

}
#endif
