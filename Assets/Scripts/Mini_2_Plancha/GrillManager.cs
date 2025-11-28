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

    [Header("Animación Espátula")]
    [Tooltip("Distancia de offset para que la punta de la espátula (y no su centro) apunte a la carne. Ajústalo según el tamaño de tu espátula.")]
    public float spatulaOffset = 0.5f;
    [Tooltip("Ángulo en grados que se levantará la espátula para 'empujar' la carne.")]
    public float spatulaLiftAngle = 15.0f;
    [Tooltip("Duración en segundos de la animación de levantar y bajar la espátula.")]
    public float spatulaLiftDuration = 0.2f;


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

        // --- 1. Calcular la posición objetivo con el offset ---
        Vector3 offset = -spatulaTransform.right * spatulaOffset;
        Vector3 targetPosition = targetMeat.transform.position + offset;
        
        float travelDuration = 0.7f;
        float elapsedTime = 0f;

        // --- 2. Animar la espátula hacia la carne ---
        Debug.Log("Moviendo espátula hacia la carne...");
        Vector3 initialSpatulaPos = spatulaTransform.position;
        while (elapsedTime < travelDuration)
        {
            spatulaTransform.position = Vector3.Lerp(initialSpatulaPos, targetPosition, elapsedTime / travelDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        spatulaTransform.position = targetPosition;

        // --- 3. Llamar a la animación de volteo de la carne ---
        targetMeat.Flip();

        // --- 4. Animación de "levantar" la espátula ---
        Quaternion originalRotation = spatulaTransform.rotation;
        // Rota sobre el eje Z local para dar un efecto de "cabeceo". Puedes cambiar el eje (X, Y, Z) y el signo del ángulo.
        Quaternion liftedRotation = originalRotation * Quaternion.Euler(originalRotation.x, -spatulaLiftAngle, originalRotation.z);

        // 4a. Animar hacia arriba
        float liftAnimTime = 0f;
        while (liftAnimTime < spatulaLiftDuration)
        {
            spatulaTransform.rotation = Quaternion.Slerp(originalRotation, liftedRotation, liftAnimTime / spatulaLiftDuration);
            liftAnimTime += Time.deltaTime;
            yield return null;
        }

        // 4b. Esperar a que la carne casi termine de girar (la anim de la carne dura ~1s)
        yield return new WaitForSeconds(0.6f); 

        // 4c. Animar hacia abajo
        liftAnimTime = 0f;
        while (liftAnimTime < spatulaLiftDuration)
        {
            spatulaTransform.rotation = Quaternion.Slerp(liftedRotation, originalRotation, liftAnimTime / spatulaLiftDuration);
            liftAnimTime += Time.deltaTime;
            yield return null;
        }
        spatulaTransform.rotation = originalRotation; // Asegurar la rotación final

        // --- 5. Animar la espátula de vuelta a su posición inicial ---
        Debug.Log("Devolviendo espátula a su posición.");
        elapsedTime = 0f;
        Vector3 currentSpatulaPos = spatulaTransform.position;
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