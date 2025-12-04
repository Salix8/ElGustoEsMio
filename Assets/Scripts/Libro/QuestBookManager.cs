using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// El "cerebro" que gestiona todo el libro de misiones.
/// Mantiene la lista de misiones, las "dibuja" en la UI
/// y le dice al BookAnimator cu�ndo debe abrirse o cerrarse.
/// </summary>
public class QuestBookManager : MonoBehaviour
{
    [Header("Componentes Principales")]
    [Tooltip("Arrastra aquí el objeto que tiene el script BookAnimator.")]
    public BookAnimator bookAnimator;

    [Header("Datos de Misiones")]
    [Tooltip("La lista de todas las misiones del jugador. Rellénala desde el inspector para probar.")]
    public List<QuestTask> allQuests;

    [Header("Prefab y Contenedor de UI")]
    [Tooltip("Arrastra aquí el *Prefab* de 'QuestItem_Template' desde tu ventana de Project.")]
    public GameObject questItemPrefab;

    [Tooltip("Arrastra aquí el panel (con Vertical Layout Group) donde se crearán los items de la lista.")]
    public Transform taskListContainer;

    public MinigameProgressManager minigameProgressManager;

    private bool isBookOpen = false;

    // --- (TEST) ---
    void Start()
    {
        // Si la lista está vacía, añadimos datos de prueba
        if (allQuests == null || allQuests.Count == 0)
        {
            Debug.Log("No hay misiones. Creando misiones de prueba.");
            allQuests = new List<QuestTask>();
            allQuests.Add(new QuestTask("Coloca las verduras en la tabla de cortar"));
            allQuests.Add(new QuestTask("Cocina la verdura cortada en el horno"));
            allQuests.Add(new QuestTask("Cocina el secreto en la plancha"));
            allQuests.Add(new QuestTask("Emplata el milhojas de  verduras"));
        }
        if (minigameProgressManager == null)
        {
            minigameProgressManager = FindObjectOfType<MinigameProgressManager>();
        }
        if (minigameProgressManager != null)
        {
            for (int i = 0; i < minigameProgressManager.nombresMinijuegos.Count; i++)
            {
                if (minigameProgressManager.EstadoMinijuego(minigameProgressManager.nombresMinijuegos[i]) == MinigameProgressManager.MinijuegoEstado.Completado)
                {
                    Debug.Log($"Minijuego '{minigameProgressManager.nombresMinijuegos[i]}' completado. Marcando misión como completada.");
                    // Marcar la misión correspondiente como completada
                    quest.isCompleted = true;
                      
                }
            }
        }
    }
    // --- (TEST) ---

    void Update()
    {
        // Para pruebas rápidas: abrir/cerrar el libro con la tecla B
        if (Input.GetKeyDown(KeyCode.B))
        {
            RefreshBookUI();
            ToggleBook();
        }
    }


    /// <summary>
    /// Esta es la función pública que debe llamar tu botón del HUD.
    /// </summary>
    public void ToggleBook()
    {
        isBookOpen = !isBookOpen;

        if (isBookOpen)
            RefreshBookUI();

        if (bookAnimator != null)
            bookAnimator.ToggleBook();
        else
            Debug.LogError("No has asignado el BookAnimator en el QuestBookManager.");
    }

    /// <summary>
    /// Borra la lista visual actual y la vuelve a crear desde cero
    /// basándose en la lista de datos 'allQuests'.
    /// </summary>
    private void RefreshBookUI()
    {
        if (questItemPrefab == null)
        {
            Debug.LogError("No has asignado el 'questItemPrefab' en el QuestBookManager.");
            return;
        }
        if (taskListContainer == null)
        {
            Debug.LogError("No has asignado el 'taskListContainer' en el QuestBookManager.");
            return;
        }

        // Reset
        foreach (Transform child in taskListContainer)
            Destroy(child.gameObject);


        foreach (QuestTask taskData in allQuests)
        {
            GameObject questItemObject = Instantiate(questItemPrefab, taskListContainer);
            QuestItemUI uiItem = questItemObject.GetComponent<QuestItemUI>();

            if (uiItem != null)
            {
                uiItem.Setup(taskData);
            }
            else
            {
                Debug.LogError("El prefab 'questItemPrefab' NO tiene el script QuestItemUI!");
            }
        }
    }
}