using UnityEngine;

/// <summary>
/// Este script debe ser añadido al objeto de la espátula que es clickeable.
/// Al hacer clic, notifica al GrillManager para activar el modo espátula.
/// </summary>
public class SpatulaInteraction : MonoBehaviour
{
    void OnMouseDown()
    {
        // Llama al singleton del GrillManager para activar el modo.
        if (GrillManager.Instance != null)
        {
            GrillManager.Instance.ActivateSpatulaMode();
        }
        else
        {
            Debug.LogError("No se encontró una instancia de GrillManager en la escena.");
        }
    }
}
