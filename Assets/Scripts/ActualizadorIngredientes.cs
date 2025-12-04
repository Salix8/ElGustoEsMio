using UnityEngine;

/// <summary>
/// Coloca este script en el nivel principal para actualizar automáticamente
/// la visibilidad de ingredientes cuando vuelves de un minijuego.
/// </summary>
public class ActualizadorIngredientes : MonoBehaviour
{
    void Start()
    {
        ActualizarTodosLosIngredientes();
    }

    /// <summary>
    /// Busca todos los IngredienteCondicional en la escena y actualiza su visibilidad
    /// </summary>
    public void ActualizarTodosLosIngredientes()
    {
        IngredienteCondicional[] todosLosIngredientes = FindObjectsOfType<IngredienteCondicional>();

        if (todosLosIngredientes.Length == 0)
        {
            Debug.LogWarning("No se encontraron IngredienteCondicional en la escena.");
            return;
        }

        foreach (IngredienteCondicional ingrediente in todosLosIngredientes)
        {
            ingrediente.ActualizarVisibilidad();
        }

        Debug.Log($"Se actualizaron {todosLosIngredientes.Length} grupos de ingredientes condicionales.");
    }

    /// <summary>
    /// Llama este método manualmente si necesitas actualizar durante el gameplay
    /// </summary>
    [ContextMenu("Actualizar Ingredientes Ahora")]
    public void ActualizarManualmente()
    {
        ActualizarTodosLosIngredientes();
    }
}
