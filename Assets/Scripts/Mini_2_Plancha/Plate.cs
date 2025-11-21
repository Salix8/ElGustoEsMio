using UnityEngine;

/// <summary>
/// Este script va en el Plato. Actúa como un trigger para
/// recibir la carne cocinada, calcular su puntuación y destruirla.
/// </summary>
[RequireComponent(typeof(Collider))]
public class Plate : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Intentamos coger el script 'Meat'
        Meat meat = other.GetComponent<Meat>();

        if (meat == null)
        {
            return;
        }

        float finalScore = meat.CalculateFinalScore();

        Debug.Log($"¡Carne entregada! Puntuación: {finalScore.ToString("F1")} / 10.0");
    }
}
