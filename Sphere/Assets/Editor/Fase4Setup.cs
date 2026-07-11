#if UNITY_EDITOR
using System.IO;
using UnityEngine;
using UnityEditor;

/// <summary>
/// FASE 4 del plan (docs/PLAN-implementacion.md): guion de dias con ScriptableObjects.
/// - Anade StoryDirector al GameSystem y lo conecta (amigo, player).
/// - Crea un asset DayConfigSO por dia del guion (Assets/GameData/Days/Day_XX.asset)
///   con el timeline del GDD seccion 8, y los asigna a DayManager > Day Configs.
///
/// Cada dia se edita luego en su propio asset (inspector), o se crean nuevos con
/// Create > Sphere > Day Config. Ejecutar con: Tools > Sphere > Run Fase 4 Setup
/// </summary>
public static class Fase4Setup
{
    const string Dir = "Assets/GameData/Days";

    [MenuItem("Tools/Sphere/Run Fase 4 Setup")]
    public static void Run()
    {
        GameObject gs = GameObject.Find("GameSystem");
        if (gs == null) { Debug.LogError("[Fase4] No encontre GameSystem."); return; }

        DayManager dm = gs.GetComponent<DayManager>();
        if (dm == null) { Debug.LogError("[Fase4] GameSystem no tiene DayManager (corre Fase 1)."); return; }

        // 1. StoryDirector.
        StoryDirector sd = gs.GetComponent<StoryDirector>();
        if (sd == null) { sd = gs.AddComponent<StoryDirector>(); Debug.Log("[Fase4] StoryDirector anadido."); }

        GameObject npc = GameObject.Find("NPC_Penguin");
        if (npc != null) sd.friend = npc.GetComponent<NPC>();
        GameObject player = GameObject.Find("Player");
        if (player != null) sd.player = player.transform;

        // 2. Timeline GDD seccion 8, un asset por dia.
        Directory.CreateDirectory(Dir);

        var d1 = Cfg(dm, 1);
        d1.friendLines = new[]
        {
            "¡Ya casi esta listo nuestro cohete!",
            "Recoge las estrellas que caigan y ponlas en el cohete.",
            "¡Y cuando este listo... nos vamos de viaje!"
        };
        d1.starsOverride = 12; // el primer cohete cuesta 8

        var d2 = Cfg(dm, 2);
        d2.friendLeaves = true;
        d2.friendLines = new[]
        {
            "¡Esta listo! Hoy nos vamos de este lugar.",
            "Sube, hay un asiento para ti.",
            "...",
            "¿No vienes?",
            "...Entiendo. Te estare esperando, alla arriba."
        };
        d2.wakeUpLines = new[] { "Hoy es el dia. El cohete esta listo..." };

        Cfg(dm, 3).wakeUpLines = new[]
        {
            "Ese cohete era pura chatarra de todos modos.",
            "Yo puedo construir uno mejor. Mucho mejor."
        };

        Cfg(dm, 4).wakeUpLines = new[] { "Hoy elijo MI cohete en el banco." };

        var d5 = Cfg(dm, 5);
        d5.telescopeEvent = true;
        if (string.IsNullOrEmpty(d5.telescopeMessage))
            d5.telescopeMessage = "Tu amigo aterriza en un planeta lejano. Se ve feliz.";

        var d8 = Cfg(dm, 8);
        d8.telescopeEvent = true;
        if (string.IsNullOrEmpty(d8.telescopeMessage))
            d8.telescopeMessage = "Tu amigo salta entre asteroides. Parece divertirse mucho.";

        var d10 = Cfg(dm, 10);
        d10.demolishRocket = true;
        d10.wakeUpLines = new[]
        {
            "No. Este cohete no va a funcionar.",
            "Tengo que hacerlo mejor. Otra vez."
        };

        Cfg(dm, 11).wakeUpLines = new[] { "El plan no esta yendo bien..." };

        var d12 = Cfg(dm, 12);
        d12.starCollectionDisabled = true;
        d12.wakeUpLines = new[] { "¿Para que seguir recogiendo estrellas...?" };

        Cfg(dm, 13).wakeUpLines = new[] { "Las estrellas se acumulan ahi fuera. Ya no importa." };

        // 3. Guardar assets + escena.
        foreach (DayConfigSO so in dm.dayConfigs)
            if (so != null) EditorUtility.SetDirty(so);
        AssetDatabase.SaveAssets();

        EditorUtility.SetDirty(dm);
        EditorUtility.SetDirty(sd);
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
        Debug.Log("[Fase4] Guion de dias 1-13 en Assets/GameData/Days y escena guardada.");
    }

    /// <summary>Carga o crea el asset DayConfigSO del dia y garantiza que este en la lista.</summary>
    static DayConfigSO Cfg(DayManager dm, int day)
    {
        string path = $"{Dir}/Day_{day:00}.asset";
        DayConfigSO so = AssetDatabase.LoadAssetAtPath<DayConfigSO>(path);
        if (so == null)
        {
            so = ScriptableObject.CreateInstance<DayConfigSO>();
            so.day = day;
            AssetDatabase.CreateAsset(so, path);
            Debug.Log($"[Fase4] Creado {path}");
        }
        if (!dm.dayConfigs.Contains(so)) dm.dayConfigs.Add(so);
        return so;
    }
}
#endif
