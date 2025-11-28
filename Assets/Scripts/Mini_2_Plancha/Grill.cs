using UnityEngine;

/// <summary>
/// Este script va en la Plancha. Actúa como un trigger para
/// decirle a la carne cuándo empezar y parar de cocinarse.
/// También mantiene una referencia a la carne que tiene encima.
/// </summary>
[RequireComponent(typeof(Collider))]
public class Grill : MonoBehaviour
{
    [Tooltip("La potencia actual de la plancha, controlada por el GameManager.")]
    public float currentPower = 1f;

    public Meat currentMeatOnGrill { get; private set; }

    private void OnTriggerEnter(Collider other)
    {
        Meat meat = other.GetComponent<Meat>();
        if (meat != null)
        {
            // Guardar la referencia y empezar a cocinar
            currentMeatOnGrill = meat;
            meat.StartCooking(currentPower);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Meat meat = other.GetComponent<Meat>();
        if (meat != null && meat == currentMeatOnGrill)
        {
            // Limpiar la referencia y parar de cocinar
            meat.StopCooking();
            currentMeatOnGrill = null;
        }
    }
}