using Linea0.Juego;
using Linea0.Vista;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class GeneradorEscena
{
    public const string RutaEscena = "Assets/Scenes/Main.unity";

    [MenuItem("Tools/Línea 0/Generar escena")]
    public static void Generar()
    {
        ConfigurarPlayer();
        var escena = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var cam = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
        cam.tag = "MainCamera";
        var c = cam.GetComponent<Camera>();
        c.orthographic = true;
        c.orthographicSize = 5;
        c.clearFlags = CameraClearFlags.SolidColor;
        c.backgroundColor = Paleta.Cielo;
        cam.transform.position = new Vector3(0, 0, -10);

        new GameObject("Bootstrap", typeof(Bootstrap));

        System.IO.Directory.CreateDirectory("Assets/Scenes");
        EditorSceneManager.SaveScene(escena, RutaEscena);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(RutaEscena, true) };
        AssetDatabase.SaveAssets();
        Debug.Log("Escena generada: " + RutaEscena);
    }

    public static void ConfigurarPlayer()
    {
        PlayerSettings.productName = "Línea 0";
        PlayerSettings.companyName = "KIRO UGM";
        PlayerSettings.fullScreenMode = FullScreenMode.FullScreenWindow;
        PlayerSettings.defaultScreenWidth = 1920;
        PlayerSettings.defaultScreenHeight = 1080;
        PlayerSettings.resizableWindow = true;
        PlayerSettings.allowFullscreenSwitch = true;
        PlayerSettings.runInBackground = true;
        PlayerSettings.visibleInBackground = true;
        PlayerSettings.SplashScreen.show = false;
    }

    /// <summary>Para batch: -executeMethod GeneradorEscena.GenerarBatch</summary>
    public static void GenerarBatch()
    {
        try { Generar(); EditorApplication.Exit(0); }
        catch (System.Exception e) { Debug.LogException(e); EditorApplication.Exit(1); }
    }
}
