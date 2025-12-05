using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Script de ejemplo para finalizar un minijuego.
/// Coloca este script en tu escena de minijuego y llama a sus métodos cuando termine.
/// </summary>
public class MinijuegoFinalizador : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("Nombre de la escena del nivel principal a la que volver")]
    public string escenaNivelPrincipal = "NivelPrincipal";

    [Tooltip("Delay antes de volver al nivel principal (segundos)")]
    public float delayAntesDeVolver = 2f;

    /// <summary>
    /// Llama este método cuando el jugador complete exitosamente el minijuego
    /// </summary>
    public void CompletarMinijuego(int puntuacion)
    {
        if (MinigameProgressManager.Instance != null)
        {
            MinigameProgressManager.Instance.CompletarMinijuego(puntuacion);
            Debug.Log($"¡Minijuego completado con {puntuacion} puntos!");
        }
        else
        {
            Debug.LogWarning("MinigameProgressManager no encontrado.");
        }
        Time.timeScale = 1f;
        VolverAlNivelPrincipal();
    }

    /// <summary>
    /// Llama este método cuando el jugador falle el minijuego
    /// </summary>
    public void FallarMinijuego()
    {
        if (MinigameProgressManager.Instance != null)
        {
            MinigameProgressManager.Instance.FallarMinijuego();
            Debug.Log("Minijuego fallado.");
        }
        else
        {
            Debug.LogWarning("MinigameProgressManager no encontrado.");
        }

        VolverAlNivelPrincipal();
    }

    /// <summary>
    /// Vuelve al nivel principal después de un delay
    /// </summary>
    private void VolverAlNivelPrincipal()
    {
        if (!string.IsNullOrEmpty(escenaNivelPrincipal))
        {
            // Actualizar ingredientes visibles antes de volver
            ActualizarIngredientesCondicionales();

            SceneManager.LoadScene(escenaNivelPrincipal);
        }
        else
        {
            Debug.LogError("No se ha asignado la escena del nivel principal.");
        }
    }

    /// <summary>
    /// Actualiza todos los IngredienteCondicional en el nivel principal
    /// </summary>
    private void ActualizarIngredientesCondicionales()
    {
        // Esta búsqueda se hará cuando volvamos al nivel principal
        // Por ahora solo registramos que necesitamos actualizar
        Debug.Log("Los ingredientes se actualizarán al volver al nivel principal.");
    }

    /// <summary>
    /// Método de ejemplo para calcular puntuación basada en tiempo
    /// </summary>
    public int CalcularPuntuacionPorTiempo(float tiempoTranscurrido, float tiempoMaximo = 60f)
    {
        // Puntuación máxima 100, disminuye según el tiempo usado
        float porcentaje = Mathf.Clamp01(1f - (tiempoTranscurrido / tiempoMaximo));
        return Mathf.RoundToInt(porcentaje * 100);
    }

    /// <summary>
    /// Método de ejemplo para calcular puntuación basada en errores
    /// </summary>
    public int CalcularPuntuacionPorErrores(int errores, int maxErrores = 3)
    {
        // Puntuación máxima 100, pierde 25 puntos por error
        int puntos = 100 - (errores * 25);
        return Mathf.Max(0, puntos);
    }

    /// <summary>
    /// Método de ejemplo para calcular puntuación combinada
    /// </summary>
    public int CalcularPuntuacionCombinada(float tiempo, float tiempoMax, int errores, int maxErrores)
    {
        int puntosTiempo = CalcularPuntuacionPorTiempo(tiempo, tiempoMax);
        int puntosErrores = CalcularPuntuacionPorErrores(errores, maxErrores);
        return (puntosTiempo + puntosErrores) / 2;
    }
}
