using UnityEngine;

/// <summary>
/// Este script va en el Plato. Actúa como un trigger para
/// recibir la carne cocinada, calcular su puntuación y destruirla.
/// </summary>
[RequireComponent(typeof(Collider))]
public class Plate : MonoBehaviour
{
    void Start()
    {
        // Asegurarse de que el collider sea un Trigger
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Intentamos coger el script 'Meat'
        Meat meat = other.GetComponent<Meat>();

        // Si no es carne, o si la carne no ha sido cocinada (sigue cruda)
        // (puedes quitar esta segunda comprobación si quieres poder puntuar carne cruda)
        if (meat == null || (meat.cookingProgressSideA == 0 && meat.cookingProgressSideB == 0))
        {
            return;
        }

        // 1. Obtener la puntuación
        float finalScore = meat.CalculateFinalScore();

        // 2. Mostrar la puntuación en la consola
        Debug.Log($"¡Carne entregada! Puntuación: {finalScore.ToString("F1")} / 10.0");

        // 3. Destruir el objeto de la carne
        Destroy(other.gameObject);
    }
}