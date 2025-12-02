using UnityEngine;

/// <summary>
/// Este script debe ser añadido al GameObject de la espátula que es clickeable.
/// Asegúrate de que este GameObject tenga un componente Collider (ej. BoxCollider, MeshCollider).
/// Al hacer clic, notifica al GrillManager para activar el modo espátula.
/// </summary>
public class SpatulaInteraction : MonoBehaviour
{
    void OnMouseDown()
    {
        Debug.Log("A ver si va?.");
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
