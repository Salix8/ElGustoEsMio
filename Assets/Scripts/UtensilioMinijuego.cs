using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class UtensilioMinijuego : MonoBehaviour
{
    [Header("Configuración del Minijuego")]
    [Tooltip("Nombre de la escena del minijuego a cargar")]
    public string escenaMinijuego;

    [Header("Ingredientes Requeridos")]
    [Tooltip("Lista de GameObjects (prefabs o de la escena) que deben ser arrastrados aquí")]
    public List<GameObject> ingredientesRequeridos = new List<GameObject>();

    [Header("Feedback Visual")]
    [Tooltip("Color cuando un ingrediente correcto está sobre el utensilio")]
    public Color colorCorrecto = new Color(0.3f, 1f, 0.3f, 0.5f);
    
    [Tooltip("Color cuando un ingrediente incorrecto está sobre el utensilio")]
    public Color colorIncorrecto = new Color(1f, 0.3f, 0.3f, 0.5f);
    
    [Tooltip("Duración del efecto de shake cuando es incorrecto (segundos)")]
    public float duracionShake = 0.3f;
    
    [Tooltip("Intensidad del shake")]
    public float intensidadShake = 10f;

    [Header("Configuración de Detección")]
    [Tooltip("Radio de detección para ingredientes soltados cerca")]
    public float radioDeteccion = 2f;
    
    [Tooltip("Tag que deben tener los objetos arrastrables para ser considerados ingredientes")]
    public string tagIngrediente = "Ingrediente";
    
    [Tooltip("Si está activado, ignora el tag y acepta cualquier objeto de la lista")]
    public bool ignorarTag = false;

    [Header("Audio (Opcional)")]
    [Tooltip("Sonido al colocar ingrediente correcto")]
    public AudioClip sonidoCorrecto;
    
    [Tooltip("Sonido al colocar ingrediente incorrecto")]
    public AudioClip sonidoIncorrecto;

    // Estado interno
    private List<string> ingredientesColocados = new List<string>();
    private Renderer rendererUtensilio;
    private Color colorOriginal;
    private Vector3 posicionOriginal;
    private AudioSource audioSource;
    private bool objetoSobreUtensilio = false;
    private GameObject objetoActual = null;

    void Start()
    {
        // Obtener renderer para feedback visual
        rendererUtensilio = GetComponent<Renderer>();
        if (rendererUtensilio != null)
        {
            colorOriginal = rendererUtensilio.material.color;
        }

        posicionOriginal = transform.position;

        // Configurar audio si existe
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && (sonidoCorrecto != null || sonidoIncorrecto != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        // Validación
        if (string.IsNullOrEmpty(escenaMinijuego))
        {
            Debug.LogWarning($"El utensilio '{gameObject.name}' no tiene escena de minijuego asignada.");
        }
    }

    void Update()
    {
        // Verificar si hay un objeto seleccionado del PrefabManager
        if (PrefabManagerSingleton.Instance != null)
        {
            GameObject objetoSeleccionado = PrefabManagerSingleton.Instance.selectedObject;

            if (objetoSeleccionado != null)
            {
                // IMPORTANTE: Evitar que el utensilio se detecte a sí mismo
                if (objetoSeleccionado == gameObject || objetoSeleccionado.transform.IsChildOf(transform))
                {
                    return;
                }

                // Verificar que el objeto tenga el tag correcto (si no se ignora el tag)
                if (!ignorarTag && !objetoSeleccionado.CompareTag(tagIngrediente))
                {
                    return;
                }

                // Calcular distancia al utensilio
                float distancia = Vector3.Distance(objetoSeleccionado.transform.position, transform.position);

                // Si está dentro del radio de detección
                if (distancia <= radioDeteccion)
                {
                    if (!objetoSobreUtensilio)
                    {
                        objetoSobreUtensilio = true;
                        objetoActual = objetoSeleccionado;
                        MostrarFeedbackHover(objetoSeleccionado);
                    }
                }
                else
                {
                    if (objetoSobreUtensilio)
                    {
                        objetoSobreUtensilio = false;
                        RestaurarColor();
                        objetoActual = null;
                    }
                }
            }
            else
            {
                // El jugador soltó el objeto
                if (objetoSobreUtensilio && objetoActual != null)
                {
                    ProcesarIngrediente(objetoActual);
                    objetoSobreUtensilio = false;
                    objetoActual = null;
                }
                else
                {
                    RestaurarColor();
                }
            }
        }
    }

    void MostrarFeedbackHover(GameObject ingrediente)
    {
        if (rendererUtensilio == null) return;

        // Verificar si el ingrediente es correcto
        bool esCorrecto = EsIngredienteCorrecto(ingrediente.name);

        // Cambiar color según sea correcto o no
        Color colorFeedback = esCorrecto ? colorCorrecto : colorIncorrecto;
        rendererUtensilio.material.color = Color.Lerp(colorOriginal, colorFeedback, 0.7f);
    }

    void RestaurarColor()
    {
        if (rendererUtensilio != null)
        {
            rendererUtensilio.material.color = colorOriginal;
        }
    }

    void ProcesarIngrediente(GameObject ingrediente)
    {
        string nombreIngrediente = ingrediente.name;

        // Verificar si es un ingrediente requerido y no ha sido colocado ya
        if (EsIngredienteCorrecto(nombreIngrediente) && !ingredientesColocados.Contains(nombreIngrediente))
        {
            // ¡CORRECTO!
            ingredientesColocados.Add(nombreIngrediente);
            
            // Feedback visual y audio
            StartCoroutine(FeedbackCorrecto(ingrediente));
            ReproducirSonido(sonidoCorrecto);

            Debug.Log($"Ingrediente '{nombreIngrediente}' añadido correctamente al utensilio '{gameObject.name}'. Progreso: {ingredientesColocados.Count}/{ingredientesRequeridos.Count}");

            // Hacer desaparecer el ingrediente suavemente
            StartCoroutine(DestruirIngredienteSuave(ingrediente));

            // Verificar si ya tenemos todos los ingredientes
            if (ingredientesColocados.Count >= ingredientesRequeridos.Count)
            {
                IniciarMinijuego();
            }
        }
        else
        {
            // INCORRECTO
            if (ingredientesColocados.Contains(nombreIngrediente))
            {
                Debug.Log($"El ingrediente '{nombreIngrediente}' ya ha sido colocado.");
            }
            else
            {
                Debug.Log($"Ingrediente '{nombreIngrediente}' no es necesario para este utensilio.");
            }

            // Feedback de error
            StartCoroutine(FeedbackIncorrecto());
            ReproducirSonido(sonidoIncorrecto);
        }

        RestaurarColor();
    }

    bool EsIngredienteCorrecto(string nombreIngrediente)
    {
        // Buscar en la lista de ingredientes requeridos por nombre
        foreach (GameObject ingredienteRequerido in ingredientesRequeridos)
        {
            if (ingredienteRequerido != null)
            {
                // Comparación flexible (ignora mayúsculas/minúsculas, espacios y sufijo "(Clone)")
                string nombreRequerido = ingredienteRequerido.name.Replace("(Clone)", "").Trim().ToLower();
                string nombreActual = nombreIngrediente.Replace("(Clone)", "").Trim().ToLower();
                
                if (nombreActual == nombreRequerido)
                {
                    return true;
                }
            }
        }
        return false;
    }

    IEnumerator DestruirIngredienteSuave(GameObject ingrediente)
    {
        float duracion = 0.5f;
        float tiempo = 0f;

        Vector3 escalaOriginal = ingrediente.transform.localScale;
        Vector3 posicionInicial = ingrediente.transform.position;
        Vector3 posicionFinal = transform.position; // Mover hacia el utensilio

        // Desactivar física si tiene Rigidbody
        Rigidbody rb = ingrediente.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        // Animación: mover hacia el utensilio mientras se encoge
        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;
            float progreso = tiempo / duracion;

            // Interpolación suave
            float curva = Mathf.Sin(progreso * Mathf.PI * 0.5f); // EaseOut

            ingrediente.transform.position = Vector3.Lerp(posicionInicial, posicionFinal, curva);
            ingrediente.transform.localScale = Vector3.Lerp(escalaOriginal, Vector3.zero, curva);

            // Rotación opcional para efecto visual
            ingrediente.transform.Rotate(Vector3.up, Time.deltaTime * 360f);

            yield return null;
        }

        // Destruir el objeto
        Destroy(ingrediente);
    }

    IEnumerator FeedbackCorrecto(GameObject ingrediente)
    {
        // Pulso de color verde suave en el utensilio
        if (rendererUtensilio != null)
        {
            float duracion = 0.3f;
            float tiempo = 0f;

            while (tiempo < duracion)
            {
                tiempo += Time.deltaTime;
                float alpha = Mathf.PingPong(tiempo * 6f, 1f);
                rendererUtensilio.material.color = Color.Lerp(colorOriginal, colorCorrecto, alpha * 0.5f);
                yield return null;
            }

            rendererUtensilio.material.color = colorOriginal;
        }
    }

    IEnumerator FeedbackIncorrecto()
    {
        // Shake horizontal del utensilio (sin cambiar su posición actual)
        float tiempoTranscurrido = 0f;
        Vector3 posicionActual = transform.position; // Guardar posición actual, no la original

        while (tiempoTranscurrido < duracionShake)
        {
            float offsetX = Mathf.Sin(tiempoTranscurrido * 50f) * intensidadShake * (1f - tiempoTranscurrido / duracionShake);
            transform.position = posicionActual + new Vector3(offsetX * 0.01f, 0, 0);

            // También cambiar color brevemente
            if (rendererUtensilio != null)
            {
                rendererUtensilio.material.color = Color.Lerp(colorOriginal, colorIncorrecto, Mathf.PingPong(tiempoTranscurrido * 10f, 1f));
            }

            tiempoTranscurrido += Time.deltaTime;
            yield return null;
        }

        // Restaurar a la posición actual (donde estaba antes del shake), no a la original
        transform.position = posicionActual;
        RestaurarColor();
    }

    void ReproducirSonido(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    void IniciarMinijuego()
    {
        Debug.Log($"¡Todos los ingredientes colocados! Iniciando minijuego: {escenaMinijuego}");

        if (!string.IsNullOrEmpty(escenaMinijuego))
        {
            // Verificar que la escena existe en Build Settings
            if (Application.CanStreamedLevelBeLoaded(escenaMinijuego))
            {
                StartCoroutine(CargarEscenaConDelay(1f));
            }
            else
            {
                Debug.LogError($"La escena '{escenaMinijuego}' no está en Build Settings. Añádela en File > Build Settings.");
            }
        }
        else
        {
            Debug.LogWarning("No hay escena de minijuego asignada.");
        }
    }

    IEnumerator CargarEscenaConDelay(float delay)
    {
        // Pequeño delay para que el jugador vea el último ingrediente desaparecer
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(escenaMinijuego);
    }

    // Métodos públicos para debugging o control externo
    public void ReiniciarIngredientes()
    {
        ingredientesColocados.Clear();
        Debug.Log($"Ingredientes reiniciados para '{gameObject.name}'");
    }

    public int GetIngredientesColocados()
    {
        return ingredientesColocados.Count;
    }

    public int GetIngredientesRequeridos()
    {
        return ingredientesRequeridos.Count;
    }

    // Visualización en editor (Gizmos)
    void OnDrawGizmosSelected()
    {
        // Dibujar el radio de detección
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radioDeteccion);
    }
}
