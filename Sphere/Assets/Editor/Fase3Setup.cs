#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

/// <summary>
/// FASE 3 del plan (docs/PLAN-implementacion.md): banco de crafteo + cohete.
/// - Crea el BANCO DE CRAFTEO placeholder (cubo mostrador) sobre el planeta,
///   con ClickInteractable + CraftingBank y un catalogo inicial de 3 cohetes.
/// - Crea el SITIO DE CONSTRUCCION (pad plano) en su posicion predefinida,
///   con ClickInteractable + RocketBuildSite.
///
/// Ejecutar con: Tools > Sphere > Run Fase 3 Setup  (idempotente: no duplica)
/// </summary>
public static class Fase3Setup
{
    [MenuItem("Tools/Sphere/Run Fase 3 Setup")]
    public static void Run()
    {
        GameObject planet = GameObject.Find("Planet");
        if (planet == null) { Debug.LogError("[Fase3] No encontre Planet."); return; }
        Vector3 center = planet.transform.position;

        // 1. Banco de crafteo: mostrador en angulo 0 (derecha, horizonte).
        if (GameObject.Find("CraftingBank") == null)
        {
            GameObject bank = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bank.name = "CraftingBank";
            bank.transform.position = PosOnSurface(center, 0f, 7.9f);
            bank.transform.rotation = Quaternion.Euler(0f, 0f, 0f - 90f); // de pie, radial
            bank.transform.localScale = new Vector3(1.3f, 1.0f, 1.0f);
            bank.transform.SetParent(planet.transform, true);

            bank.AddComponent<ClickInteractable>();
            CraftingBank cb = bank.AddComponent<CraftingBank>();
            // Catalogo inicial editable (GDD 7.2: visuales, sin stats).
            cb.catalog.Add(new RocketData { rocketName = "Cohete de chatarra", starCost = 10, height = 2.2f, color = new Color(0.55f, 0.5f, 0.45f) });
            cb.catalog.Add(new RocketData { rocketName = "Cohete grande", starCost = 25, height = 3.2f, color = new Color(0.75f, 0.75f, 0.8f) });
            cb.catalog.Add(new RocketData { rocketName = "El definitivo", starCost = 50, height = 4.2f, color = new Color(0.9f, 0.55f, 0.25f) });

            Debug.Log("[Fase3] CraftingBank creado con catalogo de 3 cohetes.");
        }

        // 2. Sitio de construccion: pad en angulo 180 (izquierda, horizonte).
        if (GameObject.Find("RocketBuildSite") == null)
        {
            GameObject site = GameObject.CreatePrimitive(PrimitiveType.Cube);
            site.name = "RocketBuildSite";
            site.transform.position = PosOnSurface(center, 180f, 7.55f);
            site.transform.rotation = Quaternion.Euler(0f, 0f, 180f - 90f); // pad radial
            site.transform.localScale = new Vector3(2.0f, 0.35f, 1.4f);
            site.transform.SetParent(planet.transform, true);

            site.AddComponent<ClickInteractable>();
            site.AddComponent<RocketBuildSite>();

            Debug.Log("[Fase3] RocketBuildSite creado (posicion predefinida).");
        }

        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
        Debug.Log("[Fase3] Setup completo y escena guardada.");
    }

    static Vector3 PosOnSurface(Vector3 center, float angleDeg, float radius)
    {
        float a = angleDeg * Mathf.Deg2Rad;
        return new Vector3(
            center.x + Mathf.Cos(a) * radius,
            center.y + Mathf.Sin(a) * radius,
            -5f); // plano frontal (clickeable, delante del planeta)
    }
}
#endif
