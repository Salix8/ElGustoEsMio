using UnityEngine;
using UnityEngine.SceneManagement; // Necesario para recargar la escena
using UnityEngine.UI; // Necesario para Button y Slider

/// <summary>
/// El cerebro del minijuego. Conecta la UI (Slider, Botón)
/// con los objetos del juego (Plancha) y gestiona estados globales como el "modo espátula".
/// </summary>
public class GrillManager : MonoBehaviour
{
    // --- SINGLETON ---
    public static GrillManager Instance { get; private set; }

    [Header("Referencias de Escena")]
    [Tooltip("Arrastra aquí el objeto de la plancha que tiene el script 'Grill.cs'.")]
    public Grill mainGrill;

    [Header("Referencias de UI")]
    [Tooltip("Arrastra aquí el botón de 'Repetir' del Canvas.")]
    public Button retryButton;
    [Tooltip("Arrastra aquí el Slider de 'Potencia' del Canvas.")]
    public Slider powerSlider;
    [Tooltip("Arrastra aquí el nuevo botón 'Espátula' del Canvas.")]
    public Button spatulaButton; // Nuevo botón para la espátula

    [Header("Estado del Juego")]
    public bool isSpatulaModeActive = false;

    void Awake()
    {
        // Lógica del Singleton
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
        // 1. Conectar los listeners de la UI
        if (retryButton != null)
        {
            retryButton.onClick.AddListener(RetryMinigame);
        }

        if (powerSlider != null)
        {
            powerSlider.onValueChanged.AddListener(SetGrillPower);
            SetGrillPower(powerSlider.value); // Establecer la potencia inicial
        }

        if (spatulaButton != null)
        {
            spatulaButton.onClick.AddListener(EnterSpatulaMode);
        }
    }

    /// <summary>
    /// Activa el modo espátula. El próximo clic en una carne la volteará.
    /// </summary>
    public void EnterSpatulaMode()
    {
        isSpatulaModeActive = true;
        Debug.Log("Modo Espátula ACTIVADO. Haz clic en una hamburguesa para voltearla.");
        // Aquí se podría cambiar el cursor o dar feedback visual
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
