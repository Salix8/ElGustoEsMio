using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class IngredienteCondicional : MonoBehaviour
{
    [Header("Condiciones de Visibilidad")]
    [Tooltip("Nombre del minijuego que debe estar completado para mostrar estos ingredientes. Debe coincidir con el nombre de la escena del minijuego.")]
    public string minijuegoRequerido;

    [Tooltip("Si está marcado, estos ingredientes se muestran ANTES de completar el minijuego")]
    public bool mostrarAntesDeCompletar = true;

    [Tooltip("Si está marcado, estos ingredientes se muestran DESPUÉS de completar el minijuego")]
    public bool mostrarDespuesDeCompletar = false;

    [Header("Referencias")]
    [Tooltip("Lista de GameObjects que serán mostrados/ocultados según las condiciones")]
    public List<GameObject> ingredientes = new List<GameObject>();

    [Header("Ayuda (Solo lectura)")]
    [Tooltip("Esta es una lista de los minijuegos registrados en el MinigameProgressManager")]
    [TextArea(3, 10)]
    public string minijuegosDisponibles = "Ejecuta el juego para ver la lista de minijuegos disponibles aquí.";

    void Start()
    {
        ActualizarVisibilidad();
    }

    void OnValidate()
    {
        ActualizarListaMinijuegos();
    }

    void ActualizarListaMinijuegos()
    {
        // Buscar el MinigameProgressManager en la escena si no lo encontramos como Instance
        MinigameProgressManager manager = FindObjectOfType<MinigameProgressManager>();
        
        if (manager != null)
        {
            // Acceder a la lista pública de minijuegos
            if (manager.nombresMinijuegos != null && manager.nombresMinijuegos.Count > 0)
            {
                minijuegosDisponibles = "Minijuegos disponibles:\n";
                foreach (string nombre in manager.nombresMinijuegos)
                {
                    minijuegosDisponibles += $"- {nombre}\n";
                }
            }
            else
            {
                minijuegosDisponibles = "No hay minijuegos registrados en el MinigameProgressManager.\nAñádelos en la lista 'Nombres Minijuegos'.";
            }
        }
        else
        {
            minijuegosDisponibles = "MinigameProgressManager no encontrado en la escena.\nAsegúrate de tenerlo en el nivel principal.";
        }
    }

    /// <summary>
    /// Actualiza la visibilidad de los ingredientes según el progreso
    /// </summary>
    public void ActualizarVisibilidad()
    {
        if (MinigameProgressManager.Instance == null)
        {
            Debug.LogWarning("MinigameProgressManager no está en la escena. Los ingredientes se mostrarán por defecto.");
            MostrarIngredientes(true);
            return;
        }

        bool minijuegoCompletado = MinigameProgressManager.Instance.EstaCompletado(minijuegoRequerido);
        bool debenMostrarse = false;

        if (minijuegoCompletado)
        {
            // El minijuego está completado
            debenMostrarse = mostrarDespuesDeCompletar;
        }
        else
        {
            // El minijuego NO está completado
            debenMostrarse = mostrarAntesDeCompletar;
        }

        MostrarIngredientes(debenMostrarse);

        Debug.Log($"Ingredientes del grupo '{gameObject.name}': {(debenMostrarse ? "VISIBLES" : "OCULTOS")} " +
                  $"(Minijuego '{minijuegoRequerido}' completado: {minijuegoCompletado})");
    }

    /// <summary>
    /// Muestra u oculta los ingredientes
    /// </summary>
    private void MostrarIngredientes(bool mostrar)
    {
        foreach (GameObject ingrediente in ingredientes)
        {
            if (ingrediente != null)
            {
                ingrediente.SetActive(mostrar);
            }
        }
    }

    /// <summary>
    /// Añade un ingrediente a la lista
    /// </summary>
    public void AñadirIngrediente(GameObject ingrediente)
    {
        if (ingrediente != null && !ingredientes.Contains(ingrediente))
        {
            ingredientes.Add(ingrediente);
        }
    }

    /// <summary>
    /// Remueve un ingrediente de la lista
    /// </summary>
    public void RemoverIngrediente(GameObject ingrediente)
    {
        if (ingredientes.Contains(ingrediente))
        {
            ingredientes.Remove(ingrediente);
        }
    }

    // Método para debugging en el editor
    [ContextMenu("Actualizar Visibilidad Ahora")]
    void ActualizarVisibilidadManual()
    {
        ActualizarVisibilidad();
    }

    [ContextMenu("Mostrar Todos")]
    void MostrarTodos()
    {
        MostrarIngredientes(true);
    }

    [ContextMenu("Ocultar Todos")]
    void OcultarTodos()
    {
        MostrarIngredientes(false);
    }
}
