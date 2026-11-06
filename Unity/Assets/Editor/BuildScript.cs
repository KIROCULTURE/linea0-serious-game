using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class BuildScript
{
    /// <summary>
    /// Build de Windows en Builds/&lt;hito&gt;/Linea0.exe (relativo a la raíz del repo).
    /// El hito se pasa con "-hito M1_Nucleo"; si no, usa la variable LINEA0_HITO o "Dev".
    /// </summary>
    public static void BuildWindows()
    {
        string hito = Argumento("-hito") ?? Environment.GetEnvironmentVariable("LINEA0_HITO") ?? "Dev";
        bool ok = Construir(hito);
        if (Application.isBatchMode) EditorApplication.Exit(ok ? 0 : 1);
    }

    [MenuItem("Tools/Línea 0/Build Windows")]
    public static void BuildDesdeMenu() => Construir("Dev");

    public static bool Construir(string hito)
    {
        try
        {
            if (!File.Exists(GeneradorEscena.RutaEscena) || EditorBuildSettings.scenes.Length == 0) GeneradorEscena.Generar();
            GeneradorEscena.ConfigurarPlayer();
            string raiz = Path.GetFullPath(Path.Combine(Application.dataPath, "..", ".."));
            string carpeta = Path.Combine(raiz, "Builds", hito);
            Directory.CreateDirectory(carpeta);
            var op = new BuildPlayerOptions
            {
                scenes = new[] { GeneradorEscena.RutaEscena },
                locationPathName = Path.Combine(carpeta, "Linea0.exe"),
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };
            var rep = BuildPipeline.BuildPlayer(op);
            Debug.Log("Build " + hito + ": " + rep.summary.result + " (" + rep.summary.totalErrors + " errores) → " + op.locationPathName);
            return rep.summary.result == BuildResult.Succeeded;
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            return false;
        }
    }

    static string Argumento(string nombre)
    {
        var a = Environment.GetCommandLineArgs();
        for (int i = 0; i < a.Length - 1; i++) if (a[i] == nombre) return a[i + 1];
        return null;
    }
}
