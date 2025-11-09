using UnityEngine;
using UnityEngine.UI; // Necesario para Toggle
using TMPro; // Necesario para TextMeshProUGUI

/// <summary>
/// Controla la representación visual de UN solo item de la lista de misiones.
/// Este script se coloca en el prefab del item de la misión.
/// </summary>
public class QuestItemUI : MonoBehaviour
{
    [Header("Referencias de UI")]
    [Tooltip("El objeto de texto que mostrará la descripción de la misión.")]
    public TextMeshProUGUI taskText;

    [Tooltip("El componente Toggle que actúa como nuestro checkbox.")]
    public Toggle taskToggle;

    [Header("Estilos Visuales")]
    [Tooltip("Color del texto cuando la misión está PENDIENTE.")]
    public Color pendingColor = Color.orange;

    [Tooltip("Color del texto cuando la misión está COMPLETADA.")]
    public Color completedColor = Color.green;

    // Almacena los datos de esta tarea
    private QuestTask currentTaskData;

    /// <summary>
    /// Configura este item visual con los datos de una QuestTask.
    /// </summary>
    /// <param name="taskToSetup">Los datos de la misión a mostrar.</param>
    public void Setup(QuestTask taskToSetup)
    {
        currentTaskData = taskToSetup;

        if (currentTaskData == null)
        {
            Debug.LogError("Setup fallido: No se proporcionaron datos de la tarea (QuestTask).");
            return;
        }

        taskText.text = currentTaskData.taskDescription;

        taskToggle.isOn = currentTaskData.isCompleted;
        taskToggle.interactable = false; // El jugador no debe poder marcarla

        if (currentTaskData.isCompleted)
        {
            taskText.color = completedColor;
            taskText.fontStyle = FontStyles.Strikethrough;
        }
        else
        {
            taskText.color = pendingColor;
            taskText.fontStyle = FontStyles.Normal;
        }
    }
}