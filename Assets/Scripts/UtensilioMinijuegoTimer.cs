using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class UtensilioMinijuegoTimer : MonoBehaviour
{
    [Header("Configuración del Minijuego")]
    [Tooltip("Nombre único del minijuego para el sistema de progreso")]
    public string nombreMinijuego = "MinijuegoTimer";

    [Header("Ingredientes")]
    [Tooltip("Ingrediente que debe arrastrarse para iniciar el minijuego")]
    public GameObject ingredienteRequerido;
    
    [Tooltip("Ingrediente que aparece si GANAS el minijuego")]
    public GameObject ingredienteSiGanas;
    
    [Tooltip("Ingrediente que aparece si PIERDES el minijuego")]
    public GameObject ingredienteSiPierdes;

    [Header("Configuración del Timer")]
    [Tooltip("Tiempo de espera en segundos antes de poder extraer")]
    public float tiempoEspera = 10f;
    
    [Tooltip("Margen de tiempo (en segundos) después de llegar a 0 para ganar")]
    public float margenExito = 3f;

    [Header("Configuración de Detección")]
    [Tooltip("Radio de detección para ingredientes")]
    public float radioDeteccion = 2f;
    
    [Tooltip("Tag del ingrediente")]
    public string tagIngrediente = "Ingrediente";
    
    public bool ignorarTag = false;

    [Header("UI del Timer")]
    [Tooltip("Prefab del Canvas con el timer (se creará automáticamente si es null)")]
    public GameObject timerCanvasPrefab;
    
    [Tooltip("Offset del timer respecto al utensilio (usa Z positivo para poner delante)")]
    public Vector3 timerOffset = new Vector3(0, 0, 2f);
    
    [Tooltip("Fuente para el texto del timer")]
    public Font fuenteTimer;

    [Header("Feedback Visual")]
    public Color colorNormal = Color.white;
    public Color colorProcesando = Color.yellow;
    public Color colorListo = Color.green;
    public Color colorFallido = Color.red;

    [Header("Audio (Opcional)")]
    public AudioClip sonidoIniciar;
    public AudioClip sonidoCompletado;
    public AudioClip sonidoFallado;

    // Estado interno
    private GameObject ingredienteActual;
    private GameObject timerUI;
    private Text timerText;
    private Image timerBackground;
    private Canvas timerCanvas;
    
    private float tiempoRestante;
    private bool timerActivo = false;
    private bool dentroMargenExito = false;
    private bool minijuegoCompletado = false;
    
    private Renderer rendererUtensilio;
    private Color colorOriginal;
    private AudioSource audioSource;

    void Start()
    {
        rendererUtensilio = GetComponent<Renderer>();
        if (rendererUtensilio != null)
        {
            colorOriginal = rendererUtensilio.material.color;
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && (sonidoIniciar != null || sonidoCompletado != null || sonidoFallado != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        // Verificar si el minijuego ya fue completado
        VerificarProgresoYActualizarIngredientes();
        
        // Verificar si hay un timer activo desde antes
        RestaurarTimerSiExiste();
    }

    void Update()
    {
        if (PrefabManagerSingleton.Instance == null) return;

        GameObject objetoSeleccionado = PrefabManagerSingleton.Instance.selectedObject;

        // Detección de ingrediente mientras se arrastra
        if (!timerActivo && !minijuegoCompletado && objetoSeleccionado != null)
        {
            if (objetoSeleccionado == gameObject || objetoSeleccionado.transform.IsChildOf(transform))
                return;

            if (!ignorarTag && !objetoSeleccionado.CompareTag(tagIngrediente))
                return;

            float distancia = Vector3.Distance(objetoSeleccionado.transform.position, transform.position);

            if (distancia <= radioDeteccion)
            {
                if (ingredienteActual != objetoSeleccionado)
                {
                    ingredienteActual = objetoSeleccionado;
                    MostrarFeedbackHover(objetoSeleccionado);
                    Debug.Log($"Ingrediente detectado cerca del utensilio: {objetoSeleccionado.name}, distancia: {distancia}");
                }
            }
            else
            {
                // Restaurar color cuando se aleja
                if (ingredienteActual != null)
                {
                    RestaurarColor();
                    ingredienteActual = null;
                }
            }
        }

        // Al soltar el ingrediente (cuando ya no hay objeto seleccionado)
        if (!timerActivo && !minijuegoCompletado && objetoSeleccionado == null && ingredienteActual != null)
        {
            float distancia = Vector3.Distance(ingredienteActual.transform.position, transform.position);
            Debug.Log($"Ingrediente soltado: {ingredienteActual.name}. Distancia al utensilio: {distancia}, Radio: {radioDeteccion}");
            
            if (distancia <= radioDeteccion)
            {
                if (EsIngredienteCorrecto(ingredienteActual))
                {
                    Debug.Log("Ingrediente correcto colocado. Iniciando minijuego...");
                    IniciarMinijuego(ingredienteActual);
                }
                else
                {
                    Debug.Log("Ingrediente incorrecto. Mostrando feedback...");
                    StartCoroutine(FeedbackIngredienteIncorrecto());
                }
            }
            else
            {
                Debug.Log("Ingrediente soltado fuera del radio de detección.");
            }
            
            RestaurarColor();
            ingredienteActual = null;
        }

        // Actualizar timer
        if (timerActivo)
        {
            ActualizarTimer();
        }

        // Detectar click en el timer para extraer
        if (timerActivo && Input.GetMouseButtonDown(0))
        {
            DetectarClickEnTimer();
        }
    }

    void VerificarProgresoYActualizarIngredientes()
    {
        if (MinigameProgressManager.Instance == null)
        {
            // Si no hay manager, mostrar ingrediente requerido por defecto
            if (ingredienteRequerido != null)
                ingredienteRequerido.SetActive(true);
            if (ingredienteSiGanas != null)
                ingredienteSiGanas.SetActive(false);
            if (ingredienteSiPierdes != null)
                ingredienteSiPierdes.SetActive(false);
            return;
        }

        bool completado = MinigameProgressManager.Instance.EstaCompletado(nombreMinijuego);
        
        // Mostrar/ocultar ingredientes según progreso
        if (ingredienteRequerido != null)
            ingredienteRequerido.SetActive(!completado);
        
        if (ingredienteSiGanas != null)
            ingredienteSiGanas.SetActive(completado);
        
        if (ingredienteSiPierdes != null)
            ingredienteSiPierdes.SetActive(false); // Solo se muestra temporalmente al fallar
    }

    void RestaurarTimerSiExiste()
    {
        if (MinigameProgressManager.Instance == null) return;

        // Verificar si hay un timer activo guardado
        if (MinigameProgressManager.Instance.TieneTimerActivo(nombreMinijuego))
        {
            float tiempoRestanteGuardado = MinigameProgressManager.Instance.ObtenerTiempoRestante(nombreMinijuego);

            if (tiempoRestanteGuardado > -margenExito)
            {
                // Restaurar el timer
                timerActivo = true;
                minijuegoCompletado = false;

                // Ocultar ingrediente requerido
                if (ingredienteRequerido != null)
                {
                    ingredienteRequerido.SetActive(false);
                }

                // Crear UI del timer
                CrearTimerUI();

                // Cambiar color según estado
                if (tiempoRestanteGuardado > 0)
                {
                    if (rendererUtensilio != null)
                    {
                        rendererUtensilio.material.color = colorProcesando;
                    }
                }
                else
                {
                    dentroMargenExito = true;
                    if (rendererUtensilio != null)
                    {
                        rendererUtensilio.material.color = colorListo;
                    }
                }

                Debug.Log($"Timer restaurado para '{nombreMinijuego}'. Tiempo restante: {tiempoRestanteGuardado}s");
            }
            else
            {
                // El timer expiró mientras estabas fuera - FALLO AUTOMÁTICO
                Debug.Log($"Timer de '{nombreMinijuego}' expiró mientras estabas en otra escena. Auto-fallo.");
                FallarMinijuego();
            }
        }
    }

    void MostrarFeedbackHover(GameObject ingrediente)
    {
        if (rendererUtensilio == null) return;

        // Verificar si el ingrediente es correcto
        bool esCorrecto = EsIngredienteCorrecto(ingrediente);

        // Cambiar color según sea correcto o no
        Color colorFeedback = esCorrecto ? colorListo : colorFallido;
        rendererUtensilio.material.color = Color.Lerp(colorOriginal, colorFeedback, 0.7f);
    }

    void RestaurarColor()
    {
        if (rendererUtensilio != null)
        {
            rendererUtensilio.material.color = colorOriginal;
        }
    }

    bool EsIngredienteCorrecto(GameObject ingrediente)
    {
        if (ingredienteRequerido == null) return false;
        
        string nombreRequerido = ingredienteRequerido.name.Replace("(Clone)", "").Trim().ToLower();
        string nombreActual = ingrediente.name.Replace("(Clone)", "").Trim().ToLower();
        
        return nombreActual == nombreRequerido;
    }

    void IniciarMinijuego(GameObject ingrediente)
    {
        timerActivo = true;
        dentroMargenExito = false;
        minijuegoCompletado = false;

        // Ocultar el ingrediente
        ingrediente.SetActive(false);

        // Registrar inicio en el sistema de progreso con timer persistente
        if (MinigameProgressManager.Instance != null)
        {
            MinigameProgressManager.Instance.IniciarMinijuego(nombreMinijuego);
            MinigameProgressManager.Instance.IniciarTimer(nombreMinijuego, tiempoEspera);
        }

        // Crear UI del timer
        CrearTimerUI();

        // Cambiar color del utensilio con feedback
        if (rendererUtensilio != null)
        {
            rendererUtensilio.material.color = colorProcesando;
            StartCoroutine(FeedbackCorrectoIngrediente());
        }

        // Reproducir sonido
        //ReproducirSonido(sonidoIniciar);

        Debug.Log($"Minijuego '{nombreMinijuego}' iniciado. Tiempo de espera: {tiempoEspera}s");
    }

    void ActualizarTimer()
    {
        // Obtener tiempo restante del sistema persistente
        if (MinigameProgressManager.Instance != null)
        {
            tiempoRestante = MinigameProgressManager.Instance.ObtenerTiempoRestante(nombreMinijuego);
        }
        else
        {
            tiempoRestante -= Time.deltaTime;
        }

        // Actualizar UI
        if (timerText != null)
        {
            if (tiempoRestante > 0)
            {
                // Procesando
                timerText.text = Mathf.CeilToInt(tiempoRestante).ToString();
                timerText.color = Color.white;
                
                if (timerBackground != null)
                {
                    timerBackground.color = new Color(0, 0, 0, 0.7f);
                }
            }
            else if (tiempoRestante > -margenExito)
            {
                // Dentro del margen de éxito - VERDE con baja opacidad
                if (!dentroMargenExito)
                {
                    dentroMargenExito = true;
                    //ReproducirSonido(sonidoCompletado);
                }
                
                timerText.text = "¡LISTO!";
                timerText.color = Color.white;
                timerText.fontSize = 35;
                
                if (timerBackground != null)
                {
                    timerBackground.color = new Color(colorListo.r, colorListo.g, colorListo.b, 0.3f);
                }
                
                if (rendererUtensilio != null)
                {
                    rendererUtensilio.material.color = colorListo;
                }
            }
            else
            {
                // Fuera del margen - ROJO con baja opacidad hasta que pulse
                timerText.text = "¡TARDE!";
                timerText.color = Color.white;
                timerText.fontSize = 35;
                
                if (timerBackground != null)
                {
                    timerBackground.color = new Color(colorFallido.r, colorFallido.g, colorFallido.b, 0.3f);
                }
                
                // No llamar FallarMinijuego aquí, esperar a que haga click
            }
        }
    }

    void CrearTimerUI()
    {
        if (timerCanvasPrefab != null)
        {
            timerUI = Instantiate(timerCanvasPrefab, transform);
            timerUI.transform.localPosition = timerOffset;
            timerCanvas = timerUI.GetComponent<Canvas>();
            timerText = timerUI.GetComponentInChildren<Text>();
            timerBackground = timerUI.GetComponentInChildren<Image>();
        }
        else
        {
            // Crear UI proceduralmente
            GameObject canvasGO = new GameObject("TimerCanvas");
            canvasGO.transform.SetParent(transform, false);
            canvasGO.transform.localPosition = timerOffset;

            timerCanvas = canvasGO.AddComponent<Canvas>();
            timerCanvas.renderMode = RenderMode.WorldSpace;
            
            // Calcular tamaño basado en el renderer del utensilio
            float tamañoUtensilio = 1f;
            if (rendererUtensilio != null)
            {
                Bounds bounds = rendererUtensilio.bounds;
                tamañoUtensilio = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
            }
            
            // El canvas será lo suficientemente grande para verse (mínimo 3 unidades)
            //float tamañoCanvas = Mathf.Max(tamañoUtensilio, 3f);
            
            RectTransform canvasRect = canvasGO.GetComponent<RectTransform>();
            canvasRect.sizeDelta = new Vector2(14, 14);
            
            CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.dynamicPixelsPerUnit = 100;
            
            canvasGO.AddComponent<GraphicRaycaster>();

            // Fondo
            /*
            GameObject bgGO = new GameObject("Background");
            bgGO.transform.SetParent(canvasGO.transform, false);
            timerBackground = bgGO.AddComponent<Image>();
            timerBackground.color = new Color(1f, 1f, 1f, 0.0f);
            RectTransform bgRect = bgGO.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
*/
            // Texto
            GameObject textGO = new GameObject("TimerText");
            textGO.transform.SetParent(canvasGO.transform, false);
            timerText = textGO.AddComponent<Text>();
            
            // Usar fuente del inspector o fuente por defecto
            if (fuenteTimer != null)
                timerText.font = fuenteTimer;
            else
                timerText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            
            timerText.fontSize = 60;
            timerText.alignment = TextAnchor.MiddleCenter;
            timerText.color = Color.white;
            timerText.fontStyle = FontStyle.Bold;
            timerText.text = "0";
            
            RectTransform textRect = textGO.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            timerUI = canvasGO;
        }

        // Hacer que el canvas mire siempre a la cámara
        if (Camera.main != null)
        {
            StartCoroutine(ActualizarRotacionCanvas());
        }
    }

    IEnumerator ActualizarRotacionCanvas()
    {
        while (timerCanvas != null && Camera.main != null)
        {
            timerCanvas.transform.LookAt(Camera.main.transform);
            timerCanvas.transform.Rotate(0, 180, 0); // Corregir orientación
            yield return null;
        }
    }

    void DetectarClickEnTimer()
    {
        if (timerUI == null || Camera.main == null) return;

        // Usar GraphicRaycaster para detectar clicks en UI
        UnityEngine.EventSystems.PointerEventData pointerData = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current)
        {
            position = Input.mousePosition
        };

        List<UnityEngine.EventSystems.RaycastResult> results = new List<UnityEngine.EventSystems.RaycastResult>();
        if (timerCanvas != null)
        {
            GraphicRaycaster raycaster = timerCanvas.GetComponent<GraphicRaycaster>();
            if (raycaster != null)
            {
                raycaster.Raycast(pointerData, results);
                
                if (results.Count > 0)
                {
                    ExtraerIngrediente();
                }
            }
        }
    }

    void ExtraerIngrediente()
    {
        if (minijuegoCompletado) return;

        if (tiempoRestante > 0)
        {
            // Extracción prematura - reiniciar
            ReiniciarTimer();
        }
        else if (dentroMargenExito)
        {
            // ¡ÉXITO!
            CompletarMinijuego();
        }
        else
        {
            // Click después del margen - FALLO
            FallarMinijuego();
        }
    }

    void ReiniciarTimer()
    {
        dentroMargenExito = false;
        timerActivo = false;

        // Feedback de error por extracción prematura
        StartCoroutine(FeedbackShakeIncorrecto());

        // Destruir el timer UI actual
        if (timerUI != null)
        {
            Destroy(timerUI);
            timerUI = null;
        }

        // Devolver el ingrediente inmediatamente
        if (ingredienteRequerido != null)
        {
            ingredienteRequerido.SetActive(true);
        }

        // Detener el timer en el manager
        if (MinigameProgressManager.Instance != null)
        {
            MinigameProgressManager.Instance.DetenerTimer(nombreMinijuego);
        }

        // Restaurar color
        if (rendererUtensilio != null)
        {
            StartCoroutine(RestaurarColorDespues(0.5f));
        }

        Debug.Log("Timer reiniciado. Ingrediente devuelto. Puedes reintentar.");
    }

    void CompletarMinijuego()
    {
        minijuegoCompletado = true;
        timerActivo = false;

        // Detener timer persistente y registrar victoria
        if (MinigameProgressManager.Instance != null)
        {
            MinigameProgressManager.Instance.DetenerTimer(nombreMinijuego);
            MinigameProgressManager.Instance.CompletarMinijuego(100);
        }

        // Destruir timer UI
        if (timerUI != null)
        {
            Destroy(timerUI);
        }

        // Cambiar color
        if (rendererUtensilio != null)
        {
            rendererUtensilio.material.color = colorListo;
            StartCoroutine(RestaurarColorDespues(2f));
        }

        // Reproducir sonido
        //ReproducirSonido(sonidoCompletado);

        // Actualizar ingredientes condicionales primero
        //ActualizarIngredientesCondicionales();

        // Esperar un frame y luego actualizar ingredientes locales
        StartCoroutine(ActualizarIngredientesDespuesDeCompletar());

        Debug.Log($"¡Minijuego '{nombreMinijuego}' completado con éxito! Puntuación: 100");
    }

    void FallarMinijuego()
    {
        // NO marcar como completado para permitir reintento
        minijuegoCompletado = false;
        timerActivo = false;

        // Detener timer persistente
        if (MinigameProgressManager.Instance != null)
        {
            MinigameProgressManager.Instance.DetenerTimer(nombreMinijuego);
            MinigameProgressManager.Instance.FallarMinijuego();
        }

        // Destruir timer UI
        if (timerUI != null)
        {
            Destroy(timerUI);
        }

        // Cambiar color
        if (rendererUtensilio != null)
        {
            rendererUtensilio.material.color = colorFallido;
            StartCoroutine(RestaurarColorDespues(2f));
        }

        // Reproducir sonido
        //ReproducirSonido(sonidoFallado);

        // Devolver ingrediente requerido inmediatamente (permitir reintento)
        if (ingredienteRequerido != null)
        {
            ingredienteRequerido.SetActive(true);
            Debug.Log($"Devolviendo ingrediente requerido: {ingredienteRequerido.name}");
        }
        
        // Mostrar temporalmente ingrediente de fallo
        if (ingredienteSiPierdes != null)
        {
            ingredienteSiPierdes.SetActive(true);
            StartCoroutine(OcultarIngredienteDespues(ingredienteSiPierdes, 3f));
        }
        
        // Asegurar que ingrediente de victoria está oculto
        if (ingredienteSiGanas != null)
        {
            ingredienteSiGanas.SetActive(false);
        }

        Debug.Log($"Minijuego '{nombreMinijuego}' fallado. Puntuación: 0. Puedes reintentar.");
    }

    IEnumerator RestaurarColorDespues(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (rendererUtensilio != null)
        {
            rendererUtensilio.material.color = colorOriginal;
        }
    }

    IEnumerator OcultarIngredienteDespues(GameObject ingrediente, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (ingrediente != null)
        {
            ingrediente.SetActive(false);
        }
    }

    IEnumerator ActualizarIngredientesDespuesDeCompletar()
    {
        // Esperar dos frames para asegurar que todo se actualice
        yield return null;
        yield return null;
        
        Debug.Log("Actualizando ingredientes después de completar minijuego...");
        
        // Ocultar ingrediente requerido
        if (ingredienteRequerido != null)
        {
            ingredienteRequerido.SetActive(false);
            Debug.Log($"Ocultando ingrediente requerido: {ingredienteRequerido.name}");
        }
        
        // Asegurar que ingrediente de fallo está oculto
        if (ingredienteSiPierdes != null)
        {
            ingredienteSiPierdes.SetActive(false);
        }
        
        // Mostrar ingrediente de victoria
        if (ingredienteSiGanas != null)
        {
            ingredienteSiGanas.SetActive(true);
            Debug.Log($"Mostrando ingrediente de victoria: {ingredienteSiGanas.name}, IsActive: {ingredienteSiGanas.activeSelf}");
        }
        else
        {
            Debug.LogWarning("ingredienteSiGanas es NULL!");
        }
        
        // Actualizar ingredientes condicionales
        ActualizarIngredientesCondicionales();
    }

    void ActualizarIngredientesCondicionales()
    {
        IngredienteCondicional[] ingredientes = FindObjectsOfType<IngredienteCondicional>();
        foreach (var ing in ingredientes)
        {
            ing.ActualizarVisibilidad();
        }
    }

    void ReproducirSonido(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    IEnumerator FeedbackCorrectoIngrediente()
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
                rendererUtensilio.material.color = Color.Lerp(colorOriginal, colorProcesando, alpha * 0.5f);
                yield return null;
            }
        }
    }

    IEnumerator FeedbackShakeIncorrecto()
    {
        // Shake horizontal del utensilio cuando extrae antes de tiempo
        float tiempoTranscurrido = 0f;
        Vector3 posicionActual = transform.position;
        float duracionShake = 0.3f;
        float intensidad = 0.05f;

        while (tiempoTranscurrido < duracionShake)
        {
            float offsetX = Mathf.Sin(tiempoTranscurrido * 50f) * intensidad * (1f - tiempoTranscurrido / duracionShake);
            transform.position = posicionActual + new Vector3(offsetX, 0, 0);

            // También cambiar color brevemente
            if (rendererUtensilio != null && timerBackground != null)
            {
                timerBackground.color = Color.Lerp(new Color(0, 0, 0, 0.7f), new Color(1, 0, 0, 0.7f), Mathf.PingPong(tiempoTranscurrido * 10f, 1f));
            }

            tiempoTranscurrido += Time.deltaTime;
            yield return null;
        }

        transform.position = posicionActual;
        
        if (timerBackground != null)
        {
            timerBackground.color = new Color(0, 0, 0, 0.7f);
        }
    }

    IEnumerator FeedbackIngredienteIncorrecto()
    {
        // Shake horizontal cuando se coloca ingrediente incorrecto
        float tiempoTranscurrido = 0f;
        Vector3 posicionActual = transform.position;
        float duracionShake = 0.3f;
        float intensidad = 0.05f;

        while (tiempoTranscurrido < duracionShake)
        {
            float offsetX = Mathf.Sin(tiempoTranscurrido * 50f) * intensidad * (1f - tiempoTranscurrido / duracionShake);
            transform.position = posicionActual + new Vector3(offsetX, 0, 0);

            // También cambiar color brevemente
            if (rendererUtensilio != null)
            {
                rendererUtensilio.material.color = Color.Lerp(colorOriginal, colorFallido, Mathf.PingPong(tiempoTranscurrido * 10f, 1f));
            }

            tiempoTranscurrido += Time.deltaTime;
            yield return null;
        }

        transform.position = posicionActual;
        RestaurarColor();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radioDeteccion);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position + timerOffset, new Vector3(2, 2, 0.1f));
    }

    // Método público para reiniciar manualmente
    [ContextMenu("Reiniciar Minijuego")]
    public void ReiniciarMinijuegoManual()
    {
        if (MinigameProgressManager.Instance != null)
        {
            MinigameProgressManager.Instance.ReiniciarMinijuego(nombreMinijuego);
        }

        if (timerUI != null)
        {
            Destroy(timerUI);
        }

        timerActivo = false;
        minijuegoCompletado = false;
        
        VerificarProgresoYActualizarIngredientes();
    }
}
