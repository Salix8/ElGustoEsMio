using UnityEngine;

public class TimeManager : MonoBehaviour
{

    public static TimeManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    [Tooltip("El juego está pausado actualmente")]
    private bool isPaused = false;

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
        Debug.Log("Juego Pausado");
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        Debug.Log("Juego Reanudado");
    }

    public bool IsPaused()
    {
        return isPaused;
    }
}