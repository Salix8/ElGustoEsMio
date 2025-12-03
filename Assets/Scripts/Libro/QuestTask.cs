using System;
using UnityEngine;

[System.Serializable]
public class QuestTask
{
    [Tooltip("El texto de la misión (ej: 'Recoge 5 manzanas')")]
    public string taskDescription;

    [Tooltip("Estado actual de la misión (completada o no)")]
    public bool isCompleted;

    /// <summary>
    /// Constructor para crear una nueva tarea fácilmente desde código.
    /// Las tareas siempre empiezan como 'no completadas'.
    /// </summary>
    /// <param name="description">El texto de la tarea.</param>
    public QuestTask(string description)
    {
        taskDescription = description;
        isCompleted = false;
    }
}
