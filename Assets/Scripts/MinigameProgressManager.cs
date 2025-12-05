using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

[System.Serializable]
public class MinigameProgress
{
    public string nombreMinijuego;
    public bool completado;
    public int puntuacion;
    public int intentos;
    
    // Para temporizadores persistentes
    public bool timerActivo;
    public double tiempoFinalizacion; // Timestamp real cuando debe terminar

    public MinigameProgress(string nombre)
    {
        nombreMinijuego = nombre;
        completado = false;
        puntuacion = 0;
        intentos = 0;
        timerActivo = false;
        tiempoFinalizacion = 0;
    }
}

public class MinigameProgressManager : MonoBehaviour
{
    public static MinigameProgressManager Instance { get; private set; }

    [Header("Configuración")]
    [Tooltip("Si está activado, guarda el progreso entre sesiones usando PlayerPrefs")]
    public bool guardarProgreso = true;

    [Header("Lista de Minijuegos")]
    [Tooltip("Lista de nombres de escenas de minijuegos que se gestionarán")]
    public List<string> nombresMinijuegos = new List<string>();

    // Diccionario para acceso rápido por nombre de minijuego
    public Dictionary<string, MinigameProgress> progresoMinijuegos = new Dictionary<string, MinigameProgress>();

    // Minijuego actual en ejecución
    private string minijuegoActual = "";
    
    // Puntuación media global
    public int puntuacionMedia = 0;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        InicializarMinijuegos();

        if (guardarProgreso)
        {
            CargarProgreso();
        }
    }

    /// <summary>
    /// Inicializa todos los minijuegos de la lista
    /// </summary>
    private void InicializarMinijuegos()
    {
        foreach (string nombreMinijuego in nombresMinijuegos)
        {
            if (!string.IsNullOrEmpty(nombreMinijuego) && !progresoMinijuegos.ContainsKey(nombreMinijuego))
            {
                progresoMinijuegos[nombreMinijuego] = new MinigameProgress(nombreMinijuego);
            }
        }

        Debug.Log($"MinigameProgressManager inicializado con {progresoMinijuegos.Count} minijuegos.");
    }

    // ========== MÉTODOS PÚBLICOS PRINCIPALES ==========

    /// <summary>
    /// Establece qué minijuego se está jugando actualmente
    /// </summary>
    public void IniciarMinijuego(string nombreMinijuego)
    {
        minijuegoActual = nombreMinijuego;

        if (!progresoMinijuegos.ContainsKey(nombreMinijuego))
        {
            progresoMinijuegos[nombreMinijuego] = new MinigameProgress(nombreMinijuego);
        }

        progresoMinijuegos[nombreMinijuego].intentos++;
        Debug.Log($"Iniciando minijuego: {nombreMinijuego} (Intento #{progresoMinijuegos[nombreMinijuego].intentos})");
    }

    /// <summary>
    /// Marca el minijuego actual como completado con una puntuación
    /// </summary>
    public void CompletarMinijuego(int puntuacion)
    {
        if (string.IsNullOrEmpty(minijuegoActual))
        {
            Debug.LogWarning("No hay minijuego activo para completar.");
            return;
        }

        if (!progresoMinijuegos.ContainsKey(minijuegoActual))
        {
            progresoMinijuegos[minijuegoActual] = new MinigameProgress(minijuegoActual);
        }

        MinigameProgress progreso = progresoMinijuegos[minijuegoActual];
        progreso.completado = true;

        // Actualizar puntuación solo si es mayor
        if (puntuacion > progreso.puntuacion)
        {
            progreso.puntuacion = puntuacion;
            Debug.Log($"¡Nueva mejor puntuación para {minijuegoActual}: {puntuacion}!");
        }

        Debug.Log($"Minijuego '{minijuegoActual}' completado con {puntuacion} puntos.");

        if (guardarProgreso)
        {
            GuardarProgreso();
        }
    }

    /// <summary>
    /// Marca el minijuego actual como fallado
    /// </summary>
    public void FallarMinijuego()
    {
        if (string.IsNullOrEmpty(minijuegoActual))
        {
            Debug.LogWarning("No hay minijuego activo para fallar.");
            return;
        }

        Debug.Log($"Minijuego '{minijuegoActual}' fallado.");
        // No cambia el estado de completado, solo registra el intento
    }

    /// <summary>
    /// Verifica si un minijuego específico ha sido completado
    /// </summary>
    public bool EstaCompletado(string nombreMinijuego)
    {
        if (progresoMinijuegos.ContainsKey(nombreMinijuego))
        {
            return progresoMinijuegos[nombreMinijuego].completado;
        }
        return false;
    }

    /// <summary>
    /// Obtiene la puntuación de un minijuego
    /// </summary>
    public int ObtenerPuntuacion(string nombreMinijuego)
    {
        if (progresoMinijuegos.ContainsKey(nombreMinijuego))
        {
            return progresoMinijuegos[nombreMinijuego].puntuacion;
        }
        return 0;
    }

    /// <summary>
    /// Obtiene el número de intentos de un minijuego
    /// </summary>
    public int ObtenerIntentos(string nombreMinijuego)
    {
        if (progresoMinijuegos.ContainsKey(nombreMinijuego))
        {
            return progresoMinijuegos[nombreMinijuego].intentos;
        }
        return 0;
    }

    /// <summary>
    /// Reinicia el progreso de un minijuego específico
    /// </summary>
    public void ReiniciarMinijuego(string nombreMinijuego)
    {
        if (progresoMinijuegos.ContainsKey(nombreMinijuego))
        {
            progresoMinijuegos[nombreMinijuego] = new MinigameProgress(nombreMinijuego);
            Debug.Log($"Progreso de '{nombreMinijuego}' reiniciado.");

            if (guardarProgreso)
            {
                GuardarProgreso();
            }
        }
    }

    /// <summary>
    /// Reinicia TODO el progreso del juego
    /// </summary>
    public void ReiniciarTodoElProgreso()
    {
        progresoMinijuegos.Clear();
        minijuegoActual = "";

        if (guardarProgreso)
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
        }

        Debug.Log("Todo el progreso ha sido reiniciado.");
    }

    /// <summary>
    /// Obtiene un resumen de todos los minijuegos
    /// </summary>
    public Dictionary<string, MinigameProgress> ObtenerTodoElProgreso()
    {
        return new Dictionary<string, MinigameProgress>(progresoMinijuegos);
    }

    // ========== PERSISTENCIA (GUARDAR/CARGAR) ==========

    private void GuardarProgreso()
    {
        foreach (var kvp in progresoMinijuegos)
        {
            string key = kvp.Key;
            MinigameProgress progreso = kvp.Value;

            PlayerPrefs.SetInt($"{key}_completado", progreso.completado ? 1 : 0);
            PlayerPrefs.SetInt($"{key}_puntuacion", progreso.puntuacion);
            PlayerPrefs.SetInt($"{key}_intentos", progreso.intentos);
            PlayerPrefs.SetInt($"{key}_timerActivo", progreso.timerActivo ? 1 : 0);
            PlayerPrefs.SetString($"{key}_tiempoFinalizacion", progreso.tiempoFinalizacion.ToString());
        }

        PlayerPrefs.Save();
        Debug.Log("Progreso guardado.");
    }

    private void CargarProgreso()
    {
        foreach (string nombreMinijuego in nombresMinijuegos)
        {
            if (!string.IsNullOrEmpty(nombreMinijuego))
            {
                CargarProgresoMinijuego(nombreMinijuego);
            }
        }

        Debug.Log($"Progreso cargado para {progresoMinijuegos.Count} minijuegos.");
    }

    /// <summary>
    /// Carga el progreso de un minijuego específico desde PlayerPrefs
    /// </summary>
    private void CargarProgresoMinijuego(string nombreMinijuego)
    {
        if (PlayerPrefs.HasKey($"{nombreMinijuego}_completado"))
        {
            MinigameProgress progreso = new MinigameProgress(nombreMinijuego)
            {
                completado = PlayerPrefs.GetInt($"{nombreMinijuego}_completado") == 1,
                puntuacion = PlayerPrefs.GetInt($"{nombreMinijuego}_puntuacion", 0),
                intentos = PlayerPrefs.GetInt($"{nombreMinijuego}_intentos", 0),
                timerActivo = PlayerPrefs.GetInt($"{nombreMinijuego}_timerActivo", 0) == 1,
                tiempoFinalizacion = 0
            };

            // Cargar timestamp del timer si existe
            if (PlayerPrefs.HasKey($"{nombreMinijuego}_tiempoFinalizacion"))
            {
                string timestampStr = PlayerPrefs.GetString($"{nombreMinijuego}_tiempoFinalizacion");
                if (double.TryParse(timestampStr, out double timestamp))
                {
                    progreso.tiempoFinalizacion = timestamp;
                }
            }

            progresoMinijuegos[nombreMinijuego] = progreso;
            Debug.Log($"Progreso de '{nombreMinijuego}' cargado: Completado={progreso.completado}, Puntuación={progreso.puntuacion}, TimerActivo={progreso.timerActivo}");
        }
    }

    // ========== MÉTODOS DE UTILIDAD ==========

    /// <summary>
    /// Obtiene el nombre del minijuego actual
    /// </summary>
    public string ObtenerMinijuegoActual()
    {
        return minijuegoActual;
    }

    /// <summary>
    /// Calcula la puntuación total de todos los minijuegos
    /// </summary>
    public int ObtenerPuntuacionTotal()
    {
        int total = 0;
        foreach (var progreso in progresoMinijuegos.Values)
        {
            total += progreso.puntuacion;
        }
        return total;
    }

    /// <summary>
    /// Cuenta cuántos minijuegos han sido completados
    /// </summary>
    public int ContarMinijuegosCompletados()
    {
        int count = 0;
        foreach (var progreso in progresoMinijuegos.Values)
        {
            if (progreso.completado) count++;
        }
        return count;
    }

    // ========== MÉTODOS PARA TEMPORIZADORES PERSISTENTES ==========

    /// <summary>
    /// Inicia un temporizador persistente para un minijuego
    /// </summary>
    public void IniciarTimer(string nombreMinijuego, float duracionSegundos)
    {
        if (!progresoMinijuegos.ContainsKey(nombreMinijuego))
        {
            progresoMinijuegos[nombreMinijuego] = new MinigameProgress(nombreMinijuego);
        }

        MinigameProgress progreso = progresoMinijuegos[nombreMinijuego];
        progreso.timerActivo = true;
        progreso.tiempoFinalizacion = GetTiempoActual() + duracionSegundos;

        if (guardarProgreso)
        {
            GuardarProgreso();
        }

        Debug.Log($"Timer iniciado para '{nombreMinijuego}'. Finalizará en {duracionSegundos}s (timestamp: {progreso.tiempoFinalizacion})");
    }

    /// <summary>
    /// Detiene el temporizador de un minijuego
    /// </summary>
    public void DetenerTimer(string nombreMinijuego)
    {
        if (progresoMinijuegos.ContainsKey(nombreMinijuego))
        {
            MinigameProgress progreso = progresoMinijuegos[nombreMinijuego];
            progreso.timerActivo = false;
            progreso.tiempoFinalizacion = 0;

            if (guardarProgreso)
            {
                GuardarProgreso();
            }

            Debug.Log($"Timer detenido para '{nombreMinijuego}'");
        }
    }

    /// <summary>
    /// Obtiene el tiempo restante de un temporizador (en segundos)
    /// Devuelve -1 si no hay timer activo
    /// </summary>
    public float ObtenerTiempoRestante(string nombreMinijuego)
    {
        if (progresoMinijuegos.ContainsKey(nombreMinijuego))
        {
            MinigameProgress progreso = progresoMinijuegos[nombreMinijuego];
            
            if (!progreso.timerActivo)
                return -1f;

            double tiempoRestante = progreso.tiempoFinalizacion - GetTiempoActual();
            return (float)tiempoRestante;
        }

        return -1f;
    }

    /// <summary>
    /// Verifica si un temporizador está activo
    /// </summary>
    public bool TieneTimerActivo(string nombreMinijuego)
    {
        if (progresoMinijuegos.ContainsKey(nombreMinijuego))
        {
            return progresoMinijuegos[nombreMinijuego].timerActivo;
        }
        return false;
    }

    /// <summary>
    /// Obtiene el timestamp actual en segundos desde epoch
    /// </summary>
    private double GetTiempoActual()
    {
        return (System.DateTime.UtcNow - new System.DateTime(1970, 1, 1)).TotalSeconds;
    }

    // ========== FINALIZACIÓN DEL JUEGO ==========

    /// <summary>
    /// Finaliza el juego completo, calcula la puntuación media y carga la escena final
    /// </summary>
    /// <param name="escenaFinal">Nombre de la escena de finalización (ej: "PantallaFinal", "Resultados")</param>
    public void FinalizarJuego(string escenaFinal = "PantallaFinal")
    {
        // Calcular y guardar puntuación media global
        puntuacionMedia = CalcularPuntuacionMedia();
        
        // Guardar puntuación final en PlayerPrefs
        PlayerPrefs.SetInt("PuntuacionFinal", puntuacionMedia);
        PlayerPrefs.SetInt("MinijuegosCompletados", ContarMinijuegosCompletados());
        PlayerPrefs.SetInt("MinijuegosTotales", nombresMinijuegos.Count);
        PlayerPrefs.Save();

        Debug.Log($"Juego finalizado. Puntuación media: {puntuacionMedia}. Minijuegos completados: {ContarMinijuegosCompletados()}/{nombresMinijuegos.Count}");

        // Cargar escena final
        if (!string.IsNullOrEmpty(escenaFinal))
        {
            if (Application.CanStreamedLevelBeLoaded(escenaFinal))
            {
                SceneManager.LoadScene(escenaFinal);
            }
            else
            {
                Debug.LogError($"La escena '{escenaFinal}' no está en Build Settings. Añádela en File > Build Settings.");
            }
        }
    }

    /// <summary>
    /// Calcula la puntuación media de todos los minijuegos completados
    /// </summary>
    /// <returns>Puntuación media (0-100)</returns>
    public int CalcularPuntuacionMedia()
    {
        if (progresoMinijuegos.Count == 0)
            return 0;

        int totalPuntuacion = 0;
        int minijuegosContados = 0;

        foreach (var progreso in progresoMinijuegos.Values)
        {
            // Solo contar minijuegos que tienen puntuación (completados o intentados)
            if (progreso.intentos > 0)
            {
                totalPuntuacion += progreso.puntuacion;
                minijuegosContados++;
            }
        }

        if (minijuegosContados == 0)
            return 0;

        return totalPuntuacion / minijuegosContados;
    }

    /// <summary>
    /// Verifica si todos los minijuegos obligatorios han sido completados
    /// </summary>
    /// <returns>True si todos están completados</returns>
    public bool TodosLosMinijuegosCompletados()
    {
        if (nombresMinijuegos.Count == 0)
            return false;

        foreach (string nombreMinijuego in nombresMinijuegos)
        {
            if (!EstaCompletado(nombreMinijuego))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Obtiene el porcentaje de progreso del juego (0-100)
    /// </summary>
    /// <returns>Porcentaje de minijuegos completados</returns>
    public float ObtenerPorcentajeProgreso()
    {
        if (nombresMinijuegos.Count == 0)
            return 0f;

        int completados = ContarMinijuegosCompletados();
        return (float)completados / nombresMinijuegos.Count * 100f;
    }

    void OnApplicationQuit()
    {
        if (guardarProgreso)
        {
            GuardarProgreso();
        }
    }
}
