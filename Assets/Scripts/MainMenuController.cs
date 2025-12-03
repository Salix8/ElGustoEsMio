using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class MainMenuController : MonoBehaviour
{
    [Header("Configuración de Niveles")]
    public List<LevelData> levels;
    public string nextComingSoonText = "¡Próximamente!";
    
    [Header("Recursos Gráficos Globales")]
    [Tooltip("Imagen de fondo para todo el menú")]
    public Sprite backgroundImage; 
    [Tooltip("Color de fondo si no hay imagen, o tinte si la hay")]
    public Color backgroundColor = new Color(0.12f, 0.12f, 0.15f); 
    
    [Tooltip("Imagen por defecto para los platos si el nivel no tiene una específica")]
    public Sprite globalPlateSprite; 
    
    [Tooltip("Imagen para el botón de Jugar")]
    public Sprite uiButtonSprite;

    [Tooltip("Fuente para todo el texto")]
    public Font globalFont; 
    
    [Header("Colores")]
    public Color accentColor = new Color(1f, 0.6f, 0.2f); // Color naranja para botones destacados
    public Color textColor = Color.white;

    [Header("Ajustes del Carrusel")]
    public float itemSpacing = 400f; // Separación entre platos
    public float scaleDownFactor = 0.5f; // Tamaño de los platos laterales
    public float swipeSpeed = 15f; 
    public float snapSpeed = 10f;

    [System.Serializable]
    public struct LevelData
    {
        public string displayName;
        public string sceneName;
        public Sprite plateImage; // Imagen específica del nivel (opcional)
        public bool isLocked;
    }

    // --- Referencias Internas (Generadas por Código) ---
    private RectTransform carouselContainer;
    private Text levelTitleText;
    private Button playButton;
    
    // Estado interno
    private List<GameObject> spawnedItems = new List<GameObject>();
    private float currentScroll = 0f;
    private float targetScroll = 0f;
    private bool isDragging = false;
    private int selectedIndex = 0;
    private Sprite defaultRoundSprite; // Sprite generado proceduralmente

    void Awake()
    {
        // 0. Generar recursos básicos en memoria (para no depender de assets externos)
        defaultRoundSprite = CreateCircleSprite();
        
        // 1. CONSTRUIR LA INTERFAZ DE USUARIO DESDE CERO
        SetupUserInterface();
    }

    void Start()
    {
        SpawnLevelItems();
        UpdateSelectionUI();
    }

    void Update()
    {
        HandleInput();
        AnimateCarousel();
    }

    // --- GENERACIÓN PROCEDURAL DE LA UI ---

    void SetupUserInterface()
    {
        // 1. Crear Canvas Principal
        GameObject canvasGO = new GameObject("MainMenuCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        // 2. Fondo (Background)
        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(canvasGO.transform, false);
        Image bgImg = bg.AddComponent<Image>();
        
        // Usar Sprite del inspector si existe, sino color sólido
        if (backgroundImage != null)
        {
            bgImg.sprite = backgroundImage;
            bgImg.color = backgroundColor; // Permite teñir la imagen
        }
        else
        {
            bgImg.color = backgroundColor;
        }

        bg.GetComponent<RectTransform>().anchorMin = Vector2.zero;
        bg.GetComponent<RectTransform>().anchorMax = Vector2.one;

        // 3. Título del Nivel (Arriba)
        GameObject titleGO = new GameObject("LevelTitle");
        titleGO.transform.SetParent(canvasGO.transform, false);
        levelTitleText = titleGO.AddComponent<Text>();
        levelTitleText.font = GetFont();
        levelTitleText.fontSize = 90;
        levelTitleText.alignment = TextAnchor.MiddleCenter;
        levelTitleText.color = textColor;
        levelTitleText.fontStyle = FontStyle.Bold;
        levelTitleText.raycastTarget = false; // IMPORTANTE: No bloquear clics
        
        RectTransform rtTitle = titleGO.GetComponent<RectTransform>();
        rtTitle.anchorMin = new Vector2(0, 0.8f); // Parte superior
        rtTitle.anchorMax = new Vector2(1, 0.95f);
        rtTitle.offsetMin = Vector2.zero;
        rtTitle.offsetMax = Vector2.zero;
        
        // 4. Contenedor del Carrusel (Centro) - Aquí irán los platos
        GameObject containerGO = new GameObject("CarouselContainer");
        containerGO.transform.SetParent(canvasGO.transform, false);
        carouselContainer = containerGO.AddComponent<RectTransform>();
        // Centrado en la pantalla
        carouselContainer.anchorMin = new Vector2(0.5f, 0.5f);
        carouselContainer.anchorMax = new Vector2(0.5f, 0.5f);
        carouselContainer.sizeDelta = Vector2.zero; 

        // 5. Botón Jugar (Abajo)
        GameObject btnGO = new GameObject("PlayButton");
        btnGO.transform.SetParent(canvasGO.transform, false);
        Image btnImg = btnGO.AddComponent<Image>();
        
        // Usar Sprite del inspector si existe
        if (uiButtonSprite != null)
            btnImg.sprite = uiButtonSprite;
        else
            btnImg.sprite = defaultRoundSprite; // Fallback
            
        btnImg.type = Image.Type.Sliced;
        btnImg.color = accentColor;
        
        playButton = btnGO.AddComponent<Button>();
        playButton.onClick.AddListener(OnPlayButtonClicked);
        
        RectTransform rtBtn = btnGO.GetComponent<RectTransform>();
        rtBtn.anchorMin = new Vector2(0.5f, 0.15f); // Parte inferior
        rtBtn.anchorMax = new Vector2(0.5f, 0.15f);
        rtBtn.sizeDelta = new Vector2(400, 100);

        // Texto dentro del botón
        GameObject btnTxtGO = new GameObject("BtnText");
        btnTxtGO.transform.SetParent(btnGO.transform, false);
        Text btnTxt = btnTxtGO.AddComponent<Text>();
        btnTxt.text = "JUGAR NIVEL";
        btnTxt.font = GetFont();
        btnTxt.fontSize = 40;
        btnTxt.alignment = TextAnchor.MiddleCenter;
        btnTxt.color = Color.white; // Texto del botón siempre blanco para contraste
        btnTxt.raycastTarget = false; // IMPORTANTE: No bloquear clics

        RectTransform rtBtnTxt = btnTxtGO.GetComponent<RectTransform>();
        rtBtnTxt.anchorMin = Vector2.zero; rtBtnTxt.anchorMax = Vector2.one;
        rtBtnTxt.offsetMin = Vector2.zero; rtBtnTxt.offsetMax = Vector2.zero;
    }

    // --- GENERACIÓN DE ITEMS DEL CARRUSEL ---

    void SpawnLevelItems()
    {
        // Limpiar si hubiera algo (aunque sea nuevo)
        foreach (Transform child in carouselContainer) Destroy(child.gameObject);
        spawnedItems.Clear();

        // Crear items para cada nivel
        for (int i = 0; i < levels.Count; i++)
        {
            CreateItem(i, levels[i].displayName, levels[i].plateImage, false);
        }

        // Crear el item final "Próximamente"
        CreateItem(levels.Count, nextComingSoonText, null, true);
    }

    void CreateItem(int index, string title, Sprite icon, bool isComingSoon)
    {
        // 1. Objeto Raíz del Item
        GameObject itemObj = new GameObject(isComingSoon ? "Item_Soon" : $"Item_{index}");
        itemObj.transform.SetParent(carouselContainer, false);
        
        RectTransform rt = itemObj.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(400, 400); // Tamaño base del plato

        // 2. Imagen del Plato
        Image img = itemObj.AddComponent<Image>();
        
        // Lógica de sprites: Si tiene uno asignado úsalo, si no usa el global del inspector, si no usa el generado
        if (icon != null) img.sprite = icon;
        else if (globalPlateSprite != null) img.sprite = globalPlateSprite;
        else img.sprite = defaultRoundSprite;

        // Color especial para "Próximamente"
        if (isComingSoon && icon == null) img.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);

        // 3. Botón invisible (para hacer clic en el plato directamente)
        Button btn = itemObj.AddComponent<Button>();
        // CORRECCIÓN: Unity no tiene Selectable.Transition.Scale. Usamos None porque el script controla la escala.
        btn.transition = Selectable.Transition.None; 
        btn.onClick.AddListener(() => OnItemClicked(index, isComingSoon));

        // 4. Número del Nivel (Texto decorativo dentro del plato)
        if (!isComingSoon)
        {
            GameObject numObj = new GameObject("LevelNumber");
            numObj.transform.SetParent(itemObj.transform, false);
            Text numTxt = numObj.AddComponent<Text>();
            numTxt.text = (index + 1).ToString();
            numTxt.font = GetFont();
            numTxt.fontSize = 150;
            numTxt.alignment = TextAnchor.MiddleCenter;
            numTxt.color = new Color(1, 1, 1, 0.8f); // Blanco casi opaco
            numTxt.raycastTarget = false; // IMPORTANTE: No bloquear clics
            
            // Sombra para el texto
            Outline ol = numObj.AddComponent<Outline>();
            ol.effectColor = new Color(0,0,0,0.5f);
            ol.effectDistance = new Vector2(2, -2);

            RectTransform numRT = numObj.GetComponent<RectTransform>();
            numRT.anchorMin = Vector2.zero; numRT.anchorMax = Vector2.one;
            numRT.offsetMin = Vector2.zero; numRT.offsetMax = Vector2.zero;
        }
        else
        {
            // Icono de interrogación o texto para "Soon"
            GameObject soonObj = new GameObject("SoonText");
            soonObj.transform.SetParent(itemObj.transform, false);
            Text soonTxt = soonObj.AddComponent<Text>();
            soonTxt.text = "?";
            soonTxt.font = GetFont();
            soonTxt.fontSize = 150;
            soonTxt.alignment = TextAnchor.MiddleCenter;
            soonTxt.color = new Color(1, 1, 1, 0.3f);
            soonTxt.raycastTarget = false; // IMPORTANTE: No bloquear clics
            
            RectTransform sRT = soonObj.GetComponent<RectTransform>();
            sRT.anchorMin = Vector2.zero; sRT.anchorMax = Vector2.one;
            sRT.offsetMin = Vector2.zero; sRT.offsetMax = Vector2.zero;
        }

        spawnedItems.Add(itemObj);
    }

    // --- LÓGICA DEL CARRUSEL (ANIMACIÓN Y INPUT) ---

    void HandleInput()
    {
        if (spawnedItems.Count == 0) return;

        // Detectar inicio de clic/toque
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
        }

        // Mientras arrastramos
        if (Input.GetMouseButton(0) && isDragging)
        {
            float deltaX = Input.GetAxis("Mouse X");
            targetScroll -= deltaX * swipeSpeed * Time.deltaTime; 
        }

        // Al soltar
        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            // Snapping: Redondear al entero más cercano
            int nearestIndex = Mathf.RoundToInt(targetScroll);
            nearestIndex = Mathf.Clamp(nearestIndex, 0, spawnedItems.Count - 1);
            targetScroll = nearestIndex;
        }
        
        // Límites
        if (!isDragging)
        {
            targetScroll = Mathf.Clamp(targetScroll, 0, spawnedItems.Count - 1);
        }
    }

    void AnimateCarousel()
    {
        if (spawnedItems.Count == 0) return;

        // Suavizado (Lerp)
        float lerpSpeed = isDragging ? 20f : snapSpeed;
        currentScroll = Mathf.Lerp(currentScroll, targetScroll, Time.deltaTime * lerpSpeed);

        for (int i = 0; i < spawnedItems.Count; i++)
        {
            GameObject item = spawnedItems[i];
            RectTransform rt = item.GetComponent<RectTransform>();

            // Distancia del item al centro (0 = centro exacto)
            float distance = currentScroll - i;
            float absDistance = Mathf.Abs(distance);

            // 1. Posición X con espaciado
            float xPos = -distance * itemSpacing; 
            rt.anchoredPosition = new Vector2(xPos, 0);

            // 2. Escala (Efecto 3D de profundidad)
            float scale = Mathf.Clamp(1f - (absDistance * (1f - scaleDownFactor)), scaleDownFactor, 1f);
            rt.localScale = new Vector3(scale, scale, 1f);

            // 3. Opacidad (Los lejanos se desvanecen)
            Image img = item.GetComponent<Image>();
            if (img != null)
            {
                float alpha = Mathf.Clamp(1f - (absDistance * 0.6f), 0.3f, 1f);
                Color c = img.color;
                c.a = alpha;
                img.color = c;
            }

            // 4. Orden de dibujado (El del centro siempre encima)
            if (absDistance < 0.5f)
            {
                rt.SetAsLastSibling();
            }
        }

        // Actualizar UI de texto si cambió el seleccionado
        int newIndex = Mathf.RoundToInt(currentScroll);
        newIndex = Mathf.Clamp(newIndex, 0, spawnedItems.Count - 1);
        
        if (newIndex != selectedIndex)
        {
            selectedIndex = newIndex;
            UpdateSelectionUI();
        }
    }

    // --- INTERACCIÓN ---

    void OnItemClicked(int index, bool isComingSoon)
    {
        // Si pincho uno lateral, muévete hacia él
        if (index != selectedIndex)
        {
            targetScroll = index;
            isDragging = false; 
            return;
        }

        // Si es el central...
        if (isComingSoon)
        {
            // Feedback de no disponible
            StartCoroutine(ShakeObject(levelTitleText.transform));
            Debug.Log("Este nivel no está disponible aún.");
        }
        else
        {
            // Verificamos si está bloqueado antes de cargar
            if (levels[index].isLocked)
            {
                // Feedback de bloqueado al hacer click en el plato
                StartCoroutine(ShakeObject(levelTitleText.transform));
                Debug.Log("Nivel Bloqueado");
            }
            else
            {
                LoadLevel(index);
            }
        }
    }

    void OnPlayButtonClicked()
    {
        if (selectedIndex < levels.Count)
        {
            // Comprobamos si el nivel seleccionado está bloqueado
            if (levels[selectedIndex].isLocked)
            {
                // Feedback visual de error/bloqueo
                StartCoroutine(ShakeObject(playButton.transform));
                Debug.Log("Nivel Bloqueado - No se puede jugar");
            }
            else
            {
                LoadLevel(selectedIndex);
            }
        }
        else
        {
            // Es el item "Coming Soon"
            StartCoroutine(ShakeObject(playButton.transform));
        }
    }

    void LoadLevel(int index)
    {
        if (index >= 0 && index < levels.Count)
        {
            string sceneToLoad = levels[index].sceneName;
            if (!string.IsNullOrEmpty(sceneToLoad))
            {
                // CHECK DE SEGURIDAD: Comprobar si la escena está en los Build Settings
                if (Application.CanStreamedLevelBeLoaded(sceneToLoad))
                {
                    Debug.Log($"Cargando escena: {sceneToLoad}...");
                    SceneManager.LoadScene(sceneToLoad);
                }
                else
                {
                    Debug.LogError($"CRÍTICO: La escena '{sceneToLoad}' no se puede cargar.\n" +
                                   "SOLUCIÓN: Ve a 'File > Build Settings' y arrastra la escena a la lista 'Scenes In Build'.");
                }
            }
            else
            {
                Debug.LogWarning($"El nivel '{levels[index].displayName}' no tiene nombre de escena asignado en el Inspector.");
            }
        }
    }

    void UpdateSelectionUI()
    {
        if (levelTitleText == null) return;

        if (selectedIndex < levels.Count)
        {
            LevelData currentLevel = levels[selectedIndex];
            levelTitleText.text = currentLevel.displayName.ToUpper();

            if (currentLevel.isLocked)
            {
                // Mostramos estado bloqueado, pero dejamos interactable para dar feedback al click
                playButton.interactable = true;
                playButton.GetComponent<Image>().color = Color.gray;
                playButton.GetComponentInChildren<Text>().text = "BLOQUEADO";
            }
            else
            {
                // Estado normal
                playButton.interactable = true;
                playButton.GetComponent<Image>().color = accentColor;
                playButton.GetComponentInChildren<Text>().text = "JUGAR";
            }
        }
        else
        {
            // Item "Próximamente"
            levelTitleText.text = nextComingSoonText.ToUpper();
            playButton.interactable = true; // Interactable para feedback
            playButton.GetComponent<Image>().color = Color.gray;
            playButton.GetComponentInChildren<Text>().text = "BLOQUEADO";
        }
    }

    // --- UTILIDADES ---

    IEnumerator ShakeObject(Transform target)
    {
        Vector3 originalPos = target.localPosition;
        float elapsed = 0f;
        while(elapsed < 0.25f)
        {
            float x = Mathf.Sin(elapsed * 60f) * 15f;
            target.localPosition = originalPos + new Vector3(x, 0, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }
        target.localPosition = originalPos;
    }

    Font GetFont()
    {
        return globalFont != null ? globalFont : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    }

    // Genera un sprite circular suave proceduralmente (para no necesitar PNGs externos)
    Sprite CreateCircleSprite()
    {
        int size = 256;
        Texture2D tex = new Texture2D(size, size);
        Color[] colors = new Color[size * size];
        float center = size / 2f;
        float radius = size / 2f - 2; // Un poco de margen

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                // Antialiasing básico
                float alpha = 1f - Mathf.Clamp01(dist - radius); 
                colors[y * size + x] = new Color(1, 1, 1, alpha);
            }
        }
        tex.SetPixels(colors);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }
}