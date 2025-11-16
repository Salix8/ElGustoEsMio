using UnityEngine;
using UnityEngine.SceneManagement; // Necesario para recargar la escena
using UnityEngine.UI; // Necesario para Button y Slider

/// <summary>
/// El cerebro del minijuego. Conecta la UI (Slider, Botón)
/// con los objetos del juego (Plancha).
/// </summary>
public class GrillManager : MonoBehaviour
{
    [Header("Referencias de Escena")]
    [Tooltip("Arrastra aquí el objeto de la plancha que tiene el script 'Grill.cs'.")]
    public Grill mainGrill;

    [Header("Referencias de UI")]
    [Tooltip("Arrastra aquí el botón de 'Repetir' del Canvas.")]
    public Button retryButton;

    [Tooltip("Arrastra aquí el Slider de 'Potencia' del Canvas.")]
    public Slider powerSlider;

    void Start()
    {
        // 1. Conectar los listeners de la UI
        if (retryButton != null)
        {
            // Cuando se haga clic en el botón, llama a la función 'RetryMinigame'
            retryButton.onClick.AddListener(RetryMinigame);
        }

        if (powerSlider != null)
        {
            // Cuando el valor del slider cambie, llama a 'SetGrillPower'
            powerSlider.onValueChanged.AddListener(SetGrillPower);

            // 2. Establecer la potencia inicial
            SetGrillPower(powerSlider.value);
        }
    }

    /// <summary>
    /// Esta función es llamada por el Slider de la UI.
    /// Comunica la nueva potencia a la plancha.
    /// </summary>
    public void SetGrillPower(float newPower)
    {
        if (mainGrill != null)
        {
            mainGrill.currentPower = newPower;
        }
    }

    /// <summary>
    /// Esta función es llamada por el Botón de Repetir.
    /// Recarga la escena actual.
    /// </summary>
    public void RetryMinigame()
    {
        // Recarga la escena en la que estás actualmente
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}