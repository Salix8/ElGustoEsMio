using UnityEngine;

/// <summary>
/// Este script va en un objeto en la escena (ej. un cubo "Carne Cruda").
/// Cuando se le hace clic, instancia un nuevo prefab de carne.
/// </summary>
public class MeatSpawner : MonoBehaviour
{
    [Header("Prefab")]
    [Tooltip("Arrastra aquí el *PREFAB* de la carne (el que tiene los scripts Meat y Draggable).")]
    public GameObject meatPrefab;

    [Tooltip("Un pequeño offset para que la carne no aparezca exactamente en el mismo sitio.")]
    public Vector3 spawnOffset = new Vector3(0, 0.5f, 0);

    void OnMouseDown()
    {
        if (meatPrefab == null)
        {
            Debug.LogError("No hay prefab de carne asignado en el Spawner!");
            return;
        }

        // Instanciar un nuevo trozo de carne en la posición del spawner + offset
        Instantiate(meatPrefab, transform.position + spawnOffset, Quaternion.Euler(90f, 0f, 0f));
    }
}