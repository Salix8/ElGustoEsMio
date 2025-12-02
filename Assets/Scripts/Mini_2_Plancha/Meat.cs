using UnityEngine;
using System.Collections; // Necesario para Corutinas

/// <summary>
/// Este script va en el prefab de la Carne.
/// Gestiona su estado de cocción, cambia de color y calcula la puntuación.
/// </summary>
[RequireComponent(typeof(Renderer))] // Necesitamos un Renderer para cambiar el color
public class Meat : MonoBehaviour
{
    [Header("Estado de Cocción")]
    public float cookingProgressSideA = 0f;
    public float cookingProgressSideB = 0f;
    public bool isSideADown = true;

    [Header("Parámetros de Cocción")]
    [Tooltip("El tiempo/progreso ideal de cocción para un 10/10.")]
    public float idealCookProgress = 5f;
    [Tooltip("El error máximo antes de que la puntuación sea 0 (ej: 5s = 0 error, 10s = 5 error).")]
    public float maxError = 5f;

    [Header("Visuales")]
    public Color rawColor = Color.red;
    public Color perfectColor = new Color(0.6f, 0.2f, 0f); // Marrón
    public Color burntColor = Color.black;

    // Variables privadas
    private bool isCooking = false;
    private float currentGrillPower = 1f;
    private Material meatMaterial;
    private float logTimer = 0f;
    private const float LOG_INTERVAL = 1.0f; // Loguear cada segundo
    private bool isFlipping = false; // Para evitar voltear múltiples veces

    void Awake()
    {
        // Creamos una instancia del material para no cambiar todos los filetes
        meatMaterial = GetComponent<Renderer>().material;
        UpdateColor(); // Empezar con el color de crudo
    }

    void Update()
    {
        if (!isCooking) return;

        float cookAmount = Time.deltaTime * currentGrillPower;

        if (isSideADown)
        {
            cookingProgressSideA += cookAmount;
        }
        else
        {
            cookingProgressSideB += cookAmount;
        }

        UpdateColor();

        logTimer += Time.deltaTime;
        if (logTimer >= LOG_INTERVAL)
        {
            logTimer = 0f;
            float currentSideProgress = isSideADown ? cookingProgressSideA : cookingProgressSideB;
            string currentSide = isSideADown ? "Lado A" : "Lado B";
            float percentage = (currentSideProgress / idealCookProgress) * 100f;
            Debug.Log($"{currentSide} cocinándose. Progreso: {percentage.ToString("F0")}% del punto ideal.");
        }
    }

    public void StartCooking(float power)
    {
        isCooking = true;
        currentGrillPower = power;
    }

    public void StopCooking()
    {
        isCooking = false;
    }

    /// <summary>
    /// Inicia la corutina de la animación de volteo.
    /// </summary>
    public void Flip()
    {
        // No voltear si ya se está volteando.
        if (isFlipping) return;
        StartCoroutine(FlipAnimationCoroutine());
    }

    /// <summary>
    /// Corutina que anima el volteo de la carne.
    /// </summary>
    private IEnumerator FlipAnimationCoroutine()
    {
        isFlipping = true;

        // 1. Preparar variables para la animación (la espera inicial se ha eliminado para sincronizar con la espátula)
        float duration = 0.5f; // Duración total del movimiento de volteo
        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;
        Vector3 peakPosition = startPosition + new Vector3(0, 0.75f, 0); // Altura del salto
        Quaternion endRotation = startRotation * Quaternion.Euler(180, 0, 0); // Rotación de 180 grados en el eje X local

        // 2. Voltear la lógica y actualizar el color INMEDIATAMENTE
        isSideADown = !isSideADown;
        UpdateColor();
        Debug.Log("Carne volteada!");

        // 3. Animar el movimiento y la rotación
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration; // Progreso de la animación (0 a 1)

            // Interpolar la posición en un arco (sube y luego baja)
            if (t < 0.5f)
            {
                // Mitad de subida
                transform.position = Vector3.Lerp(startPosition, peakPosition, t * 2);
            }
            else
            {
                // Mitad de bajada
                transform.position = Vector3.Lerp(peakPosition, startPosition, (t - 0.5f) * 2);
            }

            // Interpolar la rotación suavemente durante toda la animación
            transform.rotation = Quaternion.Slerp(startRotation, endRotation, t);
            
            yield return null; // Esperar al siguiente frame
        }

        // 5. Asegurar el estado final para evitar imprecisiones
        transform.position = startPosition;
        transform.rotation = endRotation;
        
        isFlipping = false; // Terminar el volteo
    }
    
    /// <summary>
    /// Actualiza el color del material basándose en la cocción del lado actual.
    /// </summary>
    void UpdateColor()
    {
        float currentSideProgress = isSideADown ? cookingProgressSideA : cookingProgressSideB;

        if (currentSideProgress < idealCookProgress)
        {
            float t = Mathf.InverseLerp(0, idealCookProgress, currentSideProgress);
            meatMaterial.color = Color.Lerp(rawColor, perfectColor, t);
        }
        else
        {
            float t = Mathf.InverseLerp(idealCookProgress, idealCookProgress + maxError, currentSideProgress);
            meatMaterial.color = Color.Lerp(perfectColor, burntColor, t);
        }
    }

    /// <summary>
    /// Llamado por el plato (Plate.cs) para obtener la puntuación final.
    /// </summary>
    public float CalculateFinalScore()
    {
        // Calculamos la puntuación de cada lado por separado
        float scoreA = CalculateSideScore(cookingProgressSideA);
        float scoreB = CalculateSideScore(cookingProgressSideB);

        // La puntuación final es la media de los dos lados
        float finalScore = (scoreA + scoreB) / 2f;
        return finalScore;
    }

    /// <summary>
    /// Calcula la puntuación (de 2 a 10) para un solo lado.
    /// </summary>
    private float CalculateSideScore(float progress)
    {
        // 1. Calcular el error (cuán lejos estamos del ideal)
        float error = Mathf.Abs(progress - idealCookProgress);

        // 2. Normalizar el error (convertir el error a un valor de 0 a 1)
        float t = Mathf.InverseLerp(0, maxError, error);

        // 3. Interpolar la puntuación
        float score = Mathf.Lerp(10, 2, t);

        return score;
    }
}