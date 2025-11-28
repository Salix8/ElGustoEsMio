using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// El cerebro del minijuego. Gestiona la UI y orquesta la animación de la espátula.
/// </summary>
public class GrillManager : MonoBehaviour
{
    public static GrillManager Instance { get; private set; }

    [Header("Referencias de Escena")]
    [Tooltip("Arrastra aquí el objeto de la plancha que tiene el script 'Grill.cs'.")]
    public Grill mainGrill;
    [Tooltip("Arrastra aquí el objeto visual de la espátula que se animará.")]
    public Transform spatulaTransform;
    [Tooltip("Arrastra aquí un objeto vacío que marca la posición inicial/de reposo de la espátula.")]
    public Transform spatulaStartPosition;

    [Header("Referencias de UI")]
    [Tooltip("Arrastra aquí el botón de 'Repetir' del Canvas.")]
    public Button retryButton;
    [Tooltip("Arrastra aquí el Slider de 'Potencia' del Canvas.")]
    public Slider powerSlider;
    
    [Header("Estado del Juego")]
    public bool isSpatulaModeActive = false;

    private bool isAnimatingSpatula = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    void Start()
    {
        if (retryButton != null)
        {
            retryButton.onClick.AddListener(RetryMinigame);
        }

        if (powerSlider != null)
        {
            powerSlider.onValueChanged.AddListener(SetGrillPower);
            SetGrillPower(powerSlider.value);
        }
    }

    /// <summary>
    /// Activa el modo espátula. El próximo clic en una carne la volteará.
    /// Este método debe ser llamado por el botón de la UI de la espátula.
    /// </summary>
    public void ActivateSpatulaMode()
    {
        // Si ya se está animando, no hacer nada.
        if (isAnimatingSpatula) return;

        // Invertimos el estado actual del modo espátula.
        isSpatulaModeActive = !isSpatulaModeActive;

        if (isSpatulaModeActive)
        {
            Debug.Log("Modo espátula ACTIVADO. Haz clic en una hamburguesa para voltearla.");
            // Aquí se podría cambiar el cursor para dar feedback visual.
        }
        else
        {
            Debug.Log("Modo espátula CANCELADO.");
            // Aquí se podría revertir el cursor.
        }
    }

    /// <summary>
    /// Es llamado por la carne cuando se hace clic sobre ella en modo espátula.
    /// </summary>
    public void FlipMeatWithSpatula(Meat meatToFlip)
    {
        // Solo proceder si no hay otra animación en curso y el modo está activo.
        if (isAnimatingSpatula || !isSpatulaModeActive) return;

        // Desactivar el modo espátula una vez que se ha seleccionado una carne.
        isSpatulaModeActive = false;
        Debug.Log("Carne seleccionada. Modo espátula DESACTIVADO.");

        // Iniciar la corutina de animación.
        StartCoroutine(AnimateSpatulaAndFlipMeat(meatToFlip));
    }


    private IEnumerator AnimateSpatulaAndFlipMeat(Meat targetMeat)
    {
        isAnimatingSpatula = true;

        if (targetMeat == null)
        {
            Debug.LogError("Se intentó voltear una carne nula (targetMeat era null).");
            isAnimatingSpatula = false;
            yield break;
        }

        // Asegurarse de que las referencias de la espátula están asignadas
        if (spatulaTransform == null || spatulaStartPosition == null)
        {
            Debug.LogError("Las referencias de la espátula (Transform o StartPosition) no están asignadas en el GrillManager.");
            targetMeat.Flip(); // Voltear la carne directamente sin animación de la espátula
            isAnimatingSpatula = false;
            yield break;
        }

        Vector3 targetPosition = targetMeat.transform.position;
        float travelDuration = 0.7f;
        float elapsedTime = 0f;

        // 2. Animar la espátula hacia la carne
        Debug.Log("Moviendo espátula hacia la carne...");
        while (elapsedTime < travelDuration)
        {
            spatulaTransform.position = Vector3.Lerp(spatulaStartPosition.position, targetPosition, elapsedTime / travelDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        spatulaTransform.position = targetPosition;

        // 3. Llamar a la animación de volteo de la carne SÓLO DESPUÉS de que la espátula ha llegado
        targetMeat.Flip();

        // 4. Esperar a que la animación de la carne termine (aprox. 1s en total: 0.5s espera + 0.5s anim)
        yield return new WaitForSeconds(1.0f);

        // 5. Animar la espátula de vuelta a su posición inicial
        Debug.Log("Devolviendo espátula a su posición.");
        elapsedTime = 0f;
        Vector3 currentSpatulaPos = spatulaTransform.position; // Usar la posición actual por si la carne se movió
        while (elapsedTime < travelDuration)
        {
            spatulaTransform.position = Vector3.Lerp(currentSpatulaPos, spatulaStartPosition.position, elapsedTime / travelDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        spatulaTransform.position = spatulaStartPosition.position;

        isAnimatingSpatula = false;
    }

    public void SetGrillPower(float newPower)
    {
        if (mainGrill != null)
        {
            mainGrill.currentPower = newPower;
        }
    }

    public void RetryMinigame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}