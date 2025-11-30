using UnityEngine;
/// <summary>
/// Gestiona la generación de carnes con diferentes sprites al inicio de la escena.
/// </summary>
public class MeatSpawner : MonoBehaviour
{
[Header("Configuración de Spawn")]
[Tooltip("El prefab base para cada pieza de carne. Debe tener un SpriteRenderer.")]
    public GameObject baseMeatPrefab;

    [Tooltip("Array con todos los sprites que se usarán para las carnes.")]
    public Sprite[] meatSprites;

    [Header("Área de Destino")]
    [Tooltip("El Collider que define el área donde aparecerán las carnes (ej. el plato).")]
    public Collider spawnArea;

    void Start()
    {
        if (!IsValidConfiguration())
        {
            return;
        }

        SpawnAllMeats();
    }

    /// <summary>
    /// Valida que todos los campos necesarios estén asignados en el Inspector.
    /// </summary>
    /// <returns>True si la configuración es válida, de lo contrario False.</returns>
    private bool IsValidConfiguration()
    {
        if (baseMeatPrefab == null)
        {
            Debug.LogError("Error en MeatSpawner: El campo 'Base Meat Prefab' no está asignado.", this);
            return false;
        }

        if (meatSprites == null || meatSprites.Length == 0)
        {
            Debug.LogError("Error en MeatSpawner: No se han asignado 'Meat Sprites'.", this);
            return false;
        }

        if (spawnArea == null)
        {
            Debug.LogError("Error en MeatSpawner: El campo 'Spawn Area' no está asignado.", this);
            return false;
        }

        return true;
    }

    /// <summary>
    /// Crea una instancia del prefab base para cada sprite en el array,
    /// le asigna el sprite correspondiente y lo posiciona aleatoriamente en el área de spawn.
    /// </summary>
    private void SpawnAllMeats()
    {
        Bounds spawnBounds = spawnArea.bounds;

        foreach (Sprite meatSprite in meatSprites)
        {
            if (meatSprite == null) continue;

            // Calcula una posición aleatoria dentro de los límites del collider.
            // Se añade un pequeño margen para evitar que los objetos aparezcan justo en el borde.
            const float margin = 0.3f; // Aumentado para mayor distribución y evitar bordes
            float randomX = Random.Range(spawnBounds.min.x + margin, spawnBounds.max.x - margin);
            float randomZ = Random.Range(spawnBounds.min.z + margin, spawnBounds.max.z - margin);

            // La altura de spawn se toma de la parte superior del collider.
            float spawnY = spawnBounds.max.y;

            Vector3 spawnPosition = new Vector3(randomX, spawnY, randomZ);

            // Instancia el prefab y lo nombra según su sprite para claridad en la jerarquía.
            GameObject newMeatInstance = Instantiate(baseMeatPrefab, spawnPosition, baseMeatPrefab.transform.rotation);
            newMeatInstance.name = $"Meat_{meatSprite.name}";

            // Asigna el sprite al SpriteRenderer del nuevo objeto.
            SpriteRenderer spriteRenderer = newMeatInstance.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = meatSprite;
            }
            else
            {
                Debug.LogWarning($"El prefab '{baseMeatPrefab.name}' no tiene un componente SpriteRenderer para asignarle el sprite '{meatSprite.name}'.", newMeatInstance);
            }
        }
    }
}