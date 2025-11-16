using UnityEngine;

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

    void Awake()
    {
        // Creamos una instancia del material para no cambiar todos los filetes
        meatMaterial = GetComponent<Renderer>().material;
        UpdateColor(); // Empezar con el color de crudo
    }

    void Update()
    {
        // Si no estamos en la plancha, no hacemos nada
        if (!isCooking) return;

        // Tu lógica: DeltaTime * Potencia
        float cookAmount = Time.deltaTime * currentGrillPower;

        // Cocinar el lado que esté boca abajo
        if (isSideADown)
        {
            cookingProgressSideA += cookAmount;
        }
        else
        {
            cookingProgressSideB += cookAmount;
        }

        // Actualizar el color en tiempo real
        UpdateColor();
    }

    /// <summary>
    /// Llamado por la plancha (Grill.cs)
    /// </summary>
    public void StartCooking(float power)
    {
        isCooking = true;
        currentGrillPower = power;
    }

    /// <summary>
    /// Llamado por la plancha (Grill.cs)
    /// </summary>
    public void StopCooking()
    {
        isCooking = false;
    }

    /// <summary>
    /// Esta función pública será llamada por el script Tappable.
    /// </summary>
    public void Flip()
    {
        // Solo podemos dar la vuelta si estamos cocinando
        if (!isCooking) return;

        isSideADown = !isSideADown;
        Debug.Log("Carne volteada!");

        // Al voltear, actualizamos el color al del nuevo lado
        UpdateColor();
    }

    /// <summary>
    /// Actualiza el color del material basándose en la cocción del lado actual.
    /// Esta es una alternativa más simple que un shader.
    /// </summary>
    void UpdateColor()
    {
        // Coger el progreso del lado que se está cocinando actualmente
        float currentSideProgress = isSideADown ? cookingProgressSideA : cookingProgressSideB;

        if (currentSideProgress < idealCookProgress)
        {
            // Lerp de Crudo a Perfecto
            float t = Mathf.InverseLerp(0, idealCookProgress, currentSideProgress);
            meatMaterial.color = Color.Lerp(rawColor, perfectColor, t);
        }
        else
        {
            // Lerp de Perfecto a Quemado
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
        // Ej: Si progress=5 (ideal), error=0. Si progress=1, error=4. Si progress=10, error=5.
        float error = Mathf.Abs(progress - idealCookProgress);

        // 2. Normalizar el error (convertir el error a un valor de 0 a 1)
        // Si error=0, t=0. Si error=5 (maxError), t=1.
        float t = Mathf.InverseLerp(0, maxError, error);

        // 3. Interpolar la puntuación
        // Si t=0 (sin error), puntuación=10. Si t=1 (error máximo), puntuación=2.
        float score = Mathf.Lerp(10, 2, t);

        return score;
    }
}