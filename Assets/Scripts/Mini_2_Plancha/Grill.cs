using UnityEngine;

/// <summary>
/// Este script va en la Plancha. Actúa como un trigger para
/// decirle a la carne cuándo empezar y parar de cocinarse.
/// </summary>
[RequireComponent(typeof(Collider))] // Asegura que tengamos un collider
public class Grill : MonoBehaviour
{
    [Tooltip("La potencia actual de la plancha, controlada por el GameManager.")]
    public float currentPower = 1f;

    void Start()
    {
        // Asegurarse de que el collider sea un Trigger
        GetComponent<Collider>().isTrigger = true;
    }

    // Cuando la carne entra en la plancha
    private void OnTriggerEnter(Collider other)
    {
        // Intentamos coger el script 'Meat' del objeto que ha entrado
        Meat meat = other.GetComponent<Meat>();

        // Si es un trozo de carne...
        if (meat != null)
        {
            // ...le decimos que empiece a cocinarse con nuestra potencia actual
            meat.StartCooking(currentPower);
        }
    }

    // Cuando la carne sale de la plancha
    private void OnTriggerExit(Collider other)
    {
        // Intentamos coger el script 'Meat' del objeto que ha salido
        Meat meat = other.GetComponent<Meat>();

        // Si es un trozo de carne...
        if (meat != null)
        {
            // ...le decimos que deje de cocinarse
            meat.StopCooking();
        }
    }
}