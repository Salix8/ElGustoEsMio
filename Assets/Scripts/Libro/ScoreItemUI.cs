using UnityEngine;
using TMPro;

/// <summary>
/// Controla la representación visual de la pantalla de resultados de un minijuego.
/// Este script se colocará en el prefab de la pantalla de puntuación.
/// </summary>
public class ScoreItemUI : MonoBehaviour
{
    [Header("Referencias de UI")]
    [Tooltip("El objeto de texto que mostrará el título del resultado.")]
    public TextMeshProUGUI titleText;

    [Tooltip("El objeto de texto que mostrará la descripción.")]
    public TextMeshProUGUI puntuacionText;

    [Tooltip("El objeto de texto que mostrará la descripción.")]
    public TextMeshProUGUI descriptionText;


    public void Setup(string title, string puntuacion, string description)
    {
        if (titleText != null)
        {
            titleText.text = title;
        }
        else
        {
            Debug.LogError("No se ha asignado 'titleText' en el prefab de ScoreItemUI.");
        }

        if (puntuacionText != null) // Corrected: was titleText != null
        {
            puntuacionText.text = puntuacion;
        }
        else
        {
            Debug.LogError("No se ha asignado 'puntuacionText' en el prefab de ScoreItemUI.");
        }

        if (descriptionText != null)
        {
            descriptionText.text = description;
        }
        else
        {
            Debug.LogError("No se ha asignado 'descriptionText' en el prefab de ScoreItemUI.");
        }
    }
}
