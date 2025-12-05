using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// El "cerebro" que gestiona el contenido del libro.
/// Decide qué se muestra (misiones o puntuación) y le dice al BookAnimator cuándo animarse.
/// </summary>
public class QuestBookManager : MonoBehaviour
{
    [Header("Componentes Principales")]
    [Tooltip("Arrastra aquí el objeto que tiene el script BookAnimator.")]
    public BookAnimator bookAnimator;

    [Header("UI Misiones")]
    [Tooltip("Arrastra aquí el *Prefab* de 'QuestItem_Template'.")]
    public GameObject questItemPrefab;
    [Tooltip("Arrastra aquí el CONTENEDOR de la lista de misiones.")]
    public GameObject taskListContainer;

    [Header("UI Puntuación")]
    [Tooltip("Arrastra aquí el *Prefab* de la pantalla de puntuación.")]
    public GameObject scoreUIPrefab;
    [Tooltip("Arrastra aquí el CONTENEDOR de la puntuación.")]
    public GameObject scoreUIContainer;

    [Header("Datos de Misiones (Test)")]
    [Tooltip("La lista de todas las misiones del jugador. Rellénala desde el inspector para probar.")]
        public List<QuestTask> allQuests;

        public int LastShownScore { get; private set; } // Variable para guardar la última puntuación

        public static QuestBookManager Instance { get; private set; } // Propiedad para el Singleton

        void Awake()
        {
            // Configuración del patrón Singleton
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        void Start()
        {
            // Asegurarse de que ambos contenedores están en un estado inicial correcto
            if(taskListContainer != null) taskListContainer.SetActive(true);
            if(scoreUIContainer != null) scoreUIContainer.SetActive(false);
        }

        void Update()
        {
            // Para pruebas rápidas: abrir/cerrar el libro con la tecla B
            if (Input.GetKeyDown(KeyCode.B))
            {
                ToggleBook();
            }

            // Para pruebas rápidas: mostrar puntuación con la tecla P
            if (Input.GetKeyDown(KeyCode.P))
            {
                LastShownScore = Random.Range(50, 500);
                ShowMinigameResult("Minijuego de Prueba", LastShownScore, "Descripción de prueba");
            }
        }

        /// <summary>
        /// Alterna la visibilidad del libro, mostrando SIEMPRE la lista de misiones (vista por defecto).
        /// Esta es la función que debe llamar tu botón de la UI.
        /// </summary>
        public void ToggleBook()
        {
            if (bookAnimator == null)
            {
                Debug.LogError("No has asignado el BookAnimator en el QuestBookManager.");
                return;
            }

            // Si el libro se va a abrir, preparamos la vista de misiones
            if (!bookAnimator.IsBookOpen())
            {
                PrepareQuestListView();
            }

            // Le decimos al animador que se mueva
            bookAnimator.ToggleBook();
        }

        /// <summary>
        /// Muestra el resultado de un minijuego en el libro.
        /// Si el libro está cerrado, lo abre. Si ya está abierto, solo actualiza el contenido.
        /// </summary>
        public void ShowMinigameResult(string minigameName, int score, string description)
        {
            if (bookAnimator == null)
            {
                Debug.LogError("No has asignado el BookAnimator en el QuestBookManager.");
                return;
            }

            // 1. Preparar la vista de puntuación
            PrepareScoreView(minigameName, score, description);
            // 2. Si el libro está cerrado, lo abrimos.
            if (!bookAnimator.IsBookOpen())
            {
                bookAnimator.ToggleBook();
            }
        }

        /// <summary>
        /// Prepara el libro para mostrar la lista de misiones.
        /// </summary>
        private void PrepareQuestListView()
        {
            if (questItemPrefab == null || taskListContainer == null)
            {
                Debug.LogError("No has asignado 'questItemPrefab' o 'taskListContainer' en el QuestBookManager.");
                return;
            }

            // 1. Activar el contenedor de misiones y desactivar el de puntuación
            if(scoreUIContainer != null) scoreUIContainer.SetActive(false);
            if(taskListContainer != null) taskListContainer.SetActive(true);

            // 2. Limpiar la lista de misiones anterior
            foreach (Transform child in taskListContainer.transform)
            {
                Destroy(child.gameObject);
            }

            // --- (TEST) ---
            if (allQuests == null || allQuests.Count == 0)
            {
                allQuests = new List<QuestTask> { new QuestTask("No hay misiones de prueba.") };
            }
            // --- (TEST) ---

            // 3. Crear los items de la lista de misiones actualizada
            foreach (QuestTask taskData in allQuests)
            {
                GameObject questItemObject = Instantiate(questItemPrefab, taskListContainer.transform);
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

        /// <summary>
        /// Prepara el libro para mostrar la puntuación de un minijuego.
        /// </summary>
        private void PrepareScoreView(string minigameName, int score, string description)
        {
            if (scoreUIPrefab == null || scoreUIContainer == null)
            {
                Debug.LogError("El 'scoreUIPrefab' o 'scoreUIContainer' no están asignados en el QuestBookManager.");
                return;
            }

            // Guardamos la puntuación para que otros scripts puedan acceder a ella
            LastShownScore = score;

            // 1. Activar el contenedor de puntuación y desactivar el de misiones
            if(taskListContainer != null) taskListContainer.SetActive(false);
            if(scoreUIContainer != null) scoreUIContainer.SetActive(true);

            // 2. Limpiar resultados anteriores
            foreach (Transform child in scoreUIContainer.transform)
            {
                Destroy(child.gameObject);
            }

            // 3. Crear el nuevo objeto de puntuación
            GameObject scoreObject = Instantiate(scoreUIPrefab, scoreUIContainer.transform);
            ScoreItemUI scoreUI = scoreObject.GetComponent<ScoreItemUI>();

            if (scoreUI != null)
            {
                scoreUI.Setup($"¡{minigameName} completado!", $"Puntuación final: {score}", $"{description}");
            }
            else
            {
                Debug.LogError("El prefab 'scoreUIPrefab' no tiene el script 'ScoreItemUI'.");
            }

        }
    }
