using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class minijuegofinal : MonoBehaviour
{
    [Header("Configuración del Milhojas")]
    public int targetStackHeight = 10;
    public float tableWidth = 10f;
    public float minDropHeight = 0.5f;

    [Header("Dificultad")]
    public float plateMoveSpeed = 1.5f; // Velocidad del plato
    public float plateMoveRange = 2.5f; // Rango de movimiento

    [Header("Arte y Decoración")]
    public Sprite backgroundImage;
    
    [Header("Personalización Platos y UI")]
    [Tooltip("Sprite para los platos (Origen y Objetivo). Si vacío, usa placeholder.")]
    public Sprite plateSprite;
    [Tooltip("Multiplicador de tamaño para el sprite del plato objetivo (base).")]
    public float plateScaleMultiplier = 1f;
    [Tooltip("Multiplicador de tamaño para los platos de origen (lateral).")]
    public float sourcePlateScaleMultiplier = 0.5f;
    [Tooltip("Posición X de los platos de origen (lateral derecha).")]
    public float sourcePlatesXPosition = 6.5f;
    [Tooltip("Sprite para el fondo de los paneles de UI. Si vacío, se genera proceduralmente.")]
    public Sprite uiPanelSprite;
    [Tooltip("Fuente personalizada para toda la UI. Si vacío, usa fuente por defecto.")]
    public Font customFont;
    [Tooltip("Rotación Z de los ingredientes en los platos de origen (en grados).")]
    public float ingredientRotation = 0f;

    [System.Serializable]
    public struct IngredientType {
        public int id;
        public string name;
        [Tooltip("Imagen del ingrediente. Si se asigna, usará su tamaño nativo.")]
        public Sprite sprite; 
        [Tooltip("Escala para ajustar el tamaño del sprite (1 = tamaño original).")]
        public float scaleMultiplier; 
        public Color color;
        [Tooltip("Tamaño usado SOLO si no hay sprite.")]
        public Vector2 sizeIfNoSprite; 
        public float mass;
        public float bounciness;
        [Tooltip("Rotación Z del ingrediente cuando cae (en grados).")]
        public float dropRotation;

        public IngredientType(int i, string n, Color c, float scale, float m, float b) {
            id = i; name = n; color = c; scaleMultiplier = scale; sizeIfNoSprite = new Vector2(3, 0.3f); mass = m; bounciness = b; sprite = null; dropRotation = 0f;
        }
    }

    public List<IngredientType> ingredients; 

    // --- Referencias ---
    private GameObject currentHeldObject;
    private Rigidbody2D currentRB;
    private Transform visualChild; 
    private int currentHeldTypeID = -1;
    
    private Camera mainCam;
    private Transform targetPlateTransform;
    private Rigidbody2D targetPlateRB; // Referencia para mover el plato
    private GameObject dropLineVisual;
    
    // Estado
    private bool isDragging = false;
    private bool gameOver = false;
    private int stackCount = 0;
    private float currentRawScore = 0;
    private float highestPoint = -2.0f;
    private int droppedPieces = 0; // Nuevo contador de fallos
    
    private int lastIngredientID = -1;
    private HashSet<int> usedIngredientTypes = new HashSet<int>();
    private List<GameObject> activeIngredients = new List<GameObject>();
    private List<GameObject> sourcePlates = new List<GameObject>();

    // UI
    private Text scoreText;
    private Text feedbackText;
    private Text varietyText;
    private GameObject restartButton;
    private Canvas mainCanvas;

    void Awake()
    {
        // REBOTE REDUCIDO: Valores bajados de (0.5-0.7) a (0.2-0.4) para facilitar el juego
        if (ingredients == null || ingredients.Count == 0)
        {
            ingredients = new List<IngredientType>() {
                new IngredientType(0, "Berenjena", new Color(0.5f, 0.2f, 0.6f), 1.0f, 1.0f, 0.3f),
                new IngredientType(1, "Calabacín", new Color(0.6f, 0.8f, 0.4f), 1.0f, 0.9f, 0.4f),
                new IngredientType(2, "Tomate", new Color(0.9f, 0.4f, 0.4f), 1.0f, 1.2f, 0.3f),
                new IngredientType(3, "Patata", new Color(0.9f, 0.8f, 0.6f), 1.0f, 1.5f, 0.2f),
                new IngredientType(4, "Queso", new Color(1.0f, 0.9f, 0.4f), 1.0f, 0.8f, 0.1f)
            };
        }

        SetupScene();
        SetupUI();
    }

    void Update()
    {
        if (gameOver) return;
        HandleInput();
        CheckWinCondition();
        UpdateDropLine();
    }

    void FixedUpdate()
    {
        // CHALLENGE: Mover el plato base con física (Kinematic)
        if (!gameOver && targetPlateRB != null)
        {
            float newX = Mathf.Sin(Time.time * plateMoveSpeed) * plateMoveRange;
            targetPlateRB.MovePosition(new Vector2(newX, targetPlateRB.position.y));
        }
    }

    // --- 1. Input y Control ---
    void HandleInput()
    {
        if (mainCam == null) return;

        Vector3 inputPos = Vector3.zero;
        bool inputActive = false;
        bool inputDown = false;
        bool inputUp = false;

        if (Input.touchCount > 0)
        {
            Touch t = Input.GetTouch(0);
            inputPos = mainCam.ScreenToWorldPoint(t.position);
            inputActive = true;
            inputDown = t.phase == TouchPhase.Began;
            inputUp = t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled;
        }
        else if (Input.GetMouseButton(0) || Input.GetMouseButtonUp(0))
        {
            inputPos = mainCam.ScreenToWorldPoint(Input.mousePosition);
            inputActive = true;
            inputDown = Input.GetMouseButtonDown(0);
            inputUp = Input.GetMouseButtonUp(0);
        }

        inputPos.z = 0;

        if (inputActive)
        {
            if (inputDown && !isDragging)
            {
                Collider2D hit = Physics2D.OverlapPoint(inputPos);
                if (hit != null)
                {
                    for(int i=0; i<sourcePlates.Count; i++)
                    {
                        if(hit.gameObject == sourcePlates[i])
                        {
                            SpawnIngredient(i, inputPos);
                            isDragging = true;
                            break;
                        }
                    }
                }
            }

            if (isDragging && currentHeldObject != null)
            {
                Vector3 targetPos = new Vector3(inputPos.x, inputPos.y + 0.5f, 0);
                currentHeldObject.transform.position = Vector3.Lerp(currentHeldObject.transform.position, targetPos, Time.deltaTime * 25f);
                currentRB.linearVelocity = Vector2.zero;
                currentRB.angularVelocity = 0;
                
                float limitY = highestPoint + minDropHeight;
                SpriteRenderer sr = visualChild.GetComponent<SpriteRenderer>();
                if (currentHeldObject.transform.position.y < limitY)
                    sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 0.4f);
                else
                    sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 1.0f);
            }

            if (inputUp && isDragging)
            {
                float limitY = highestPoint + minDropHeight;
                if (currentHeldObject.transform.position.y >= limitY)
                {
                    ReleaseIngredient();
                }
                else
                {
                    CancelDrop();
                    ShowFloatingText(currentHeldObject.transform.position, "¡Muy bajo!", Color.red);
                }
                isDragging = false;
            }
        }
    }

    void UpdateDropLine()
    {
        if (dropLineVisual != null)
        {
            RecalculateTowerHeight();
            float y = highestPoint + minDropHeight;
            dropLineVisual.transform.position = new Vector3(0, y, 1);
            dropLineVisual.SetActive(isDragging);
        }
    }

    // --- 2. Gestión de Objetos ---

    void SpawnIngredient(int index, Vector3 pos)
    {
        IngredientType type = ingredients[index];
        currentHeldTypeID = type.id;

        currentHeldObject = new GameObject(type.name);
        currentHeldObject.transform.position = pos;

        // Primero crear collider en el padre (sin rotación)
        BoxCollider2D col = currentHeldObject.AddComponent<BoxCollider2D>();
        
        // Determinar tamaño del collider según sprite o tamaño manual
        Vector2 colliderSize;
        if (type.sprite != null)
        {
            float scale = (type.scaleMultiplier > 0) ? type.scaleMultiplier : 1f;
            colliderSize = type.sprite.bounds.size * scale;
        }
        else
        {
            colliderSize = (type.sizeIfNoSprite != Vector2.zero) ? type.sizeIfNoSprite : new Vector2(3, 0.3f);
        }
        col.size = colliderSize;

        // Ahora crear hijo visual con rotación
        GameObject vChild = new GameObject("Visuals");
        vChild.transform.parent = currentHeldObject.transform;
        vChild.transform.localPosition = Vector3.zero;
        
        // Aplicar rotación específica del tipo de ingrediente (solo al visual)
        vChild.transform.localRotation = Quaternion.Euler(0, 0, type.dropRotation);
        
        visualChild = vChild.transform;

        SpriteRenderer sr = vChild.AddComponent<SpriteRenderer>();
        
        if (type.sprite != null)
        {
            sr.sprite = type.sprite;
            float scale = (type.scaleMultiplier > 0) ? type.scaleMultiplier : 1f;
            vChild.transform.localScale = new Vector3(scale, scale, 1);
        }
        else
        {
            Texture2D tex = new Texture2D(1, 1); tex.SetPixel(0,0,Color.white); tex.Apply();
            sr.sprite = Sprite.Create(tex, new Rect(0,0,1,1), new Vector2(0.5f,0.5f), 1);
            
            Vector2 size = (type.sizeIfNoSprite != Vector2.zero) ? type.sizeIfNoSprite : new Vector2(3, 0.3f);
            vChild.transform.localScale = new Vector3(size.x, size.y, 1);

            GameObject border = new GameObject("Border");
            border.transform.parent = vChild.transform;
            border.transform.localPosition = new Vector3(0,0,0.01f);
            border.transform.localScale = new Vector3(1.05f, 1.1f, 1);
            SpriteRenderer bs = border.AddComponent<SpriteRenderer>();
            bs.sprite = sr.sprite;
            bs.color = new Color(0,0,0,0.5f);
        }
        sr.color = type.color; 

        currentRB = currentHeldObject.AddComponent<Rigidbody2D>();
        currentRB.mass = type.mass;
        currentRB.gravityScale = 0; 
        currentRB.freezeRotation = true;
        currentRB.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        PhysicsMaterial2D mat = new PhysicsMaterial2D();
        mat.friction = 0.6f; 
        mat.bounciness = type.bounciness;
        col.sharedMaterial = mat;

        IngredientBehavior beh = currentHeldObject.AddComponent<IngredientBehavior>();
        beh.gameManager = this;
        beh.visualTransform = visualChild;
        beh.typeID = type.id;
    }

    void ReleaseIngredient()
    {
        if (currentHeldObject == null) return;
        currentRB.gravityScale = 1.2f;
        currentRB.freezeRotation = false;
        
        SpriteRenderer sr = visualChild.GetComponent<SpriteRenderer>();
        sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 1f);

        activeIngredients.Add(currentHeldObject);
        currentHeldObject = null;
        visualChild = null;
    }

    void CancelDrop()
    {
        if (currentHeldObject == null) return;
        Destroy(currentHeldObject);
        currentHeldObject = null;
    }

    void RecalculateTowerHeight()
    {
        // Se usa la posición del plato como base mínima
        float maxY = targetPlateTransform != null ? targetPlateTransform.position.y : -2.9f;
        
        activeIngredients.RemoveAll(item => item == null);

        foreach(GameObject obj in activeIngredients)
        {
            Collider2D c = obj.GetComponent<Collider2D>();
            if(c != null)
            {
                if (c.bounds.max.y > maxY) maxY = c.bounds.max.y;
            }
        }
        highestPoint = maxY;
    }

    // --- 3. Eventos y Puntuación ---

    public void OnIngredientLanded(GameObject obj, int typeID, float impactForce, float xPos)
    {
        RecalculateTowerHeight();

        // Calcular puntuación basada en qué tan centrado está respecto al PLATO (que ahora se mueve)
        float offset = Mathf.Abs(xPos - targetPlateTransform.position.x);
        float alignmentScore = Mathf.Max(0, (100 - (offset * 40)));
        
        float multiplier = 1.0f;
        if (typeID == lastIngredientID) multiplier = 0.5f;
        else if (lastIngredientID != -1) multiplier = 1.2f;

        if (!usedIngredientTypes.Contains(typeID))
        {
            usedIngredientTypes.Add(typeID);
            if (usedIngredientTypes.Count == ingredients.Count)
                ShowFloatingText(new Vector3(0, 3, 0), "¡VARIEDAD\nCOMPLETA!", Color.cyan);
        }

        float finalPoints = alignmentScore * multiplier;
        currentRawScore += finalPoints;
        stackCount = activeIngredients.Count; 
        lastIngredientID = typeID;

        UpdateUI();

        if (multiplier > 1f && finalPoints > 50)
            ShowFloatingText(obj.transform.position, "+" + (int)finalPoints, Color.yellow);
    }

    public void OnIngredientFell(GameObject obj)
    {
        // PENALIZACIÓN SEVERA
        droppedPieces++; 

        // --- CORRECCIÓN DE VARIEDAD ---
        // Si se cae, verificar si quedan otros ingredientes de este tipo.
        // Si no quedan, eliminarlo de la lista de tipos usados.
        IngredientBehavior beh = obj.GetComponent<IngredientBehavior>();
        if (beh != null)
        {
            int fallenID = beh.typeID;
            
            // Filtramos la lista eliminando el objeto actual (que está a punto de destruirse o salir)
            // y buscamos si queda alguno más con el mismo ID
            bool typeStillExists = false;
            foreach(var item in activeIngredients)
            {
                if (item != null && item != obj)
                {
                    IngredientBehavior b = item.GetComponent<IngredientBehavior>();
                    if (b != null && b.typeID == fallenID)
                    {
                        typeStillExists = true;
                        break;
                    }
                }
            }

            if (!typeStillExists)
            {
                usedIngredientTypes.Remove(fallenID);
            }
        }
        
        activeIngredients.Remove(obj);
        RecalculateTowerHeight();
        
        UpdateUI();
        ShowFloatingText(Vector3.zero, "¡CAÍDA! (-10 pts)", Color.red);
        Destroy(obj);
    }

    void CheckWinCondition()
    {
        if (activeIngredients.Count >= targetStackHeight && !gameOver)
        {
            StartCoroutine(WaitAndWin());
        }
    }

    IEnumerator WaitAndWin()
    {
        yield return new WaitForSeconds(1.0f);
        if (activeIngredients.Count >= targetStackHeight && !gameOver)
            GameOver(true);
    }

    void GameOver(bool win)
    {
        gameOver = true;
        restartButton.SetActive(true);
        
        float averageScore = stackCount > 0 ? currentRawScore / stackCount : 0;
        if (usedIngredientTypes.Count == ingredients.Count) averageScore += 20;
        
        // --- APLICAR PENALIZACIÓN FINAL ---
        float penalty = droppedPieces * 10f; // 10 puntos menos de la nota por pieza caída
        float finalScoreCalc = averageScore - penalty;
        
        int finalGrade = Mathf.Clamp(Mathf.RoundToInt(finalScoreCalc), 0, 100);

        string title = "RESULTADO";
        string subtitle = "";
        Color titleColor = Color.white;

        if (finalGrade < 50) { title = "SUSPENSO"; subtitle = $"Se cayeron {droppedPieces} piezas"; titleColor = new Color(1f, 0.4f, 0.4f); }
        else if (finalGrade < 80) { title = "ACEPTABLE"; subtitle = "Buen trabajo"; titleColor = new Color(1f, 0.9f, 0.4f); }
        else { title = "¡EXCELENTE!"; subtitle = "Chef Maestro"; titleColor = new Color(0.4f, 1f, 0.4f); }

        feedbackText.text = $"<size=50>{title}</size>\n<size=30>{subtitle}</size>\n\nNota: {finalGrade}/100";
        feedbackText.color = titleColor;
        feedbackText.transform.parent.gameObject.SetActive(true);
    }

    public void RestartGame()
    {
        foreach(var go in activeIngredients) Destroy(go);
        activeIngredients.Clear();
        if(currentHeldObject) Destroy(currentHeldObject);

        currentRawScore = 0;
        stackCount = 0;
        droppedPieces = 0; // Resetear caídas
        highestPoint = targetPlateTransform.position.y;
        lastIngredientID = -1;
        usedIngredientTypes.Clear();
        gameOver = false;
        
        restartButton.SetActive(false);
        feedbackText.transform.parent.gameObject.SetActive(false);
        UpdateUI();
    }

    // --- 4. Escena ---

    void SetupScene()
    {
        mainCam = Camera.main != null ? Camera.main : gameObject.AddComponent<Camera>();
        mainCam.transform.position = new Vector3(0, 2, -10);
        mainCam.orthographic = true;
        mainCam.orthographicSize = 6;
        mainCam.backgroundColor = new Color(0.92f, 0.92f, 0.92f);

        if (backgroundImage != null)
        {
            GameObject bg = new GameObject("Background");
            SpriteRenderer bgSR = bg.AddComponent<SpriteRenderer>();
            bgSR.sprite = backgroundImage;
            bgSR.sortingOrder = -100;
            bg.transform.position = new Vector3(0, 0, 10); 
            float scale = Mathf.Max((mainCam.orthographicSize * 2 * mainCam.aspect) / bgSR.sprite.bounds.size.x, (mainCam.orthographicSize * 2) / bgSR.sprite.bounds.size.y);
            bg.transform.localScale = new Vector3(scale, scale, 1);
        }

        dropLineVisual = new GameObject("DropLine");
        SpriteRenderer lsr = dropLineVisual.AddComponent<SpriteRenderer>();
        Texture2D lineTex = new Texture2D(1,1); lineTex.SetPixel(0,0,Color.white); lineTex.Apply();
        lsr.sprite = Sprite.Create(lineTex, new Rect(0,0,1,1), new Vector2(0.5f,0.5f), 1);
        lsr.color = new Color(1, 0, 0, 0.3f);
        dropLineVisual.transform.localScale = new Vector3(tableWidth, 0.05f, 1);
        dropLineVisual.SetActive(false);

        // Suelo (Muerte)
        GameObject deathZone = new GameObject("Suelo");
        deathZone.transform.position = new Vector3(0, -7f, 0);
        BoxCollider2D dzCol = deathZone.AddComponent<BoxCollider2D>();
        dzCol.size = new Vector2(30, 1);
        dzCol.isTrigger = true;
        deathZone.AddComponent<DeathZone>().gameManager = this;

        // Plato Objetivo (Ahora con Rigidbody Kinematic para moverse)
        GameObject plate = new GameObject("PlatoObjetivo");
        plate.transform.position = new Vector3(0, -2.9f, 0);
        SpriteRenderer psr = plate.AddComponent<SpriteRenderer>();
        
        // --- PERSONALIZACIÓN PLATO OBJETIVO ---
        if (plateSprite != null) {
            psr.sprite = plateSprite;
            // Aplicar escala y respetar la escala base del transform
            float finalScale = plateScaleMultiplier;
            plate.transform.localScale = new Vector3(finalScale, finalScale, 1);
        } else {
            psr.sprite = lsr.sprite; psr.color = Color.white;
            plate.transform.localScale = new Vector3(3.5f, 0.2f, 1);
        }
        
        // Si usamos sprite custom, el collider debe adaptarse
        BoxCollider2D pCol = plate.AddComponent<BoxCollider2D>(); 
        if(plateSprite != null) pCol.size = plateSprite.bounds.size;
        
        // Ajustar offset del collider para mejor detección
        pCol.offset = new Vector2(0, -0.48f);

        targetPlateRB = plate.AddComponent<Rigidbody2D>(); 
        targetPlateRB.bodyType = RigidbodyType2D.Kinematic; 
        
        targetPlateTransform = plate.transform;
        highestPoint = plate.transform.position.y;

        SetupSourcePlates();
    }

    void SetupSourcePlates()
    {
        float startY = 3.5f;
        float gapY = 1.2f;
        float xPos = sourcePlatesXPosition;

        for (int i = 0; i < ingredients.Count; i++)
        {
            IngredientType ing = ingredients[i];
            
            GameObject source = new GameObject($"Plato_{ing.name}");
            source.transform.position = new Vector3(xPos, startY - (i * gapY), 0);
            
            GameObject plateBg = new GameObject("PlateBg");
            plateBg.transform.parent = source.transform;
            plateBg.transform.localPosition = Vector3.zero;
            SpriteRenderer pbs = plateBg.AddComponent<SpriteRenderer>();

            // --- PERSONALIZACIÓN PLATOS ORIGEN ---
            if (plateSprite != null)
            {
                pbs.sprite = plateSprite;
                // Usar multiplicador específico para platos laterales
                float lateralScale = sourcePlateScaleMultiplier; 
                plateBg.transform.localScale = new Vector3(lateralScale, lateralScale, 1);
            }
            else
            {
                Texture2D tex = new Texture2D(1,1); tex.SetPixel(0,0,Color.white); tex.Apply();
                pbs.sprite = Sprite.Create(tex, new Rect(0,0,1,1), new Vector2(0.5f,0.5f), 1);
                pbs.color = new Color(0.9f, 0.9f, 0.9f);
                plateBg.transform.localScale = new Vector3(2f, 1f, 1);
            }

            GameObject sample = new GameObject("Muestra");
            sample.transform.parent = source.transform;
            sample.transform.localPosition = Vector3.zero;
            
            // Aplicar rotación configurada
            sample.transform.localRotation = Quaternion.Euler(0, 0, ingredientRotation);
            
            SpriteRenderer ssr = sample.AddComponent<SpriteRenderer>();
            
            if(ing.sprite != null) {
                ssr.sprite = ing.sprite;
                float scale = (ing.scaleMultiplier > 0) ? ing.scaleMultiplier : 1f;
                // Ajustar al plato lateral (más pequeño)
                float maxPlateW = plateSprite != null ? (plateSprite.bounds.size.x * plateBg.transform.localScale.x * 0.8f) : 1.8f;
                float currentW = ing.sprite.bounds.size.x * scale;
                if(currentW > maxPlateW) scale *= (maxPlateW / currentW);
                
                sample.transform.localScale = new Vector3(scale, scale, 1);
            } else {
                ssr.sprite = pbs.sprite; // Fallback al mismo sprite del fondo o blanco
                if (plateSprite == null) {
                    Vector2 sz = (ing.sizeIfNoSprite != Vector2.zero) ? ing.sizeIfNoSprite : new Vector2(3, 0.3f);
                    sample.transform.localScale = new Vector3(sz.x * 0.5f, sz.y, 1);
                } else {
                    sample.transform.localScale = new Vector3(0.5f, 0.1f, 1);
                }
            }
            ssr.color = ing.color;

            BoxCollider2D col = source.AddComponent<BoxCollider2D>();
            col.size = new Vector2(2f, 1f);
            col.isTrigger = true;
            sourcePlates.Add(source);

            GameObject textObj = new GameObject("Label");
            textObj.transform.parent = source.transform;
            textObj.transform.localPosition = new Vector3(0, -0.4f, -1);
            TextMesh tm = textObj.AddComponent<TextMesh>();
            tm.text = ing.name;
            tm.characterSize = 0.15f;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.color = Color.black;
        }
    }

    // --- 5. UI ---

    void SetupUI()
    {
        GameObject canvasGO = new GameObject("Canvas");
        mainCanvas = canvasGO.AddComponent<Canvas>();
        mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasGO.AddComponent<GraphicRaycaster>();

        // Usar Sprite Custom para UI si existe, sino procedural
        Sprite panelSprite = (uiPanelSprite != null) ? uiPanelSprite : CreateRoundedRectSprite(Color.white, 256, 256, 40);

        GameObject scorePanel = CreateStylishPanel(mainCanvas.transform, panelSprite, new Vector2(300, 100), new Vector2(20, -20), new Vector2(0, 1));
        scoreText = CreateText(scorePanel.transform, "0", 35, FontStyle.Bold);
        
        GameObject varietyPanel = CreateStylishPanel(mainCanvas.transform, panelSprite, new Vector2(240, 60), new Vector2(20, -130), new Vector2(0, 1));
        varietyText = CreateText(varietyPanel.transform, "Variedad: 0/5", 22, FontStyle.Normal);

        GameObject feedPanel = CreateStylishPanel(mainCanvas.transform, panelSprite, new Vector2(500, 300), Vector2.zero, new Vector2(0.5f, 0.5f));
        feedPanel.GetComponent<Image>().color = new Color(0.15f, 0.15f, 0.2f, 0.95f); 
        feedbackText = CreateText(feedPanel.transform, "", 40, FontStyle.Bold);
        feedbackText.rectTransform.anchoredPosition = new Vector2(0, 50);
        feedPanel.SetActive(false);

        restartButton = new GameObject("RestartButton");
        restartButton.transform.SetParent(feedPanel.transform, false);
        Image btnImg = restartButton.AddComponent<Image>();
        btnImg.sprite = panelSprite;
        btnImg.color = new Color(0.2f, 0.8f, 0.4f);
        Button btn = restartButton.AddComponent<Button>();
        btn.onClick.AddListener(RestartGame);
        RectTransform rt = restartButton.GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(0, -80);
        rt.sizeDelta = new Vector2(220, 60);
        
        Text btnText = CreateText(restartButton.transform, "JUGAR DE NUEVO", 24, FontStyle.Bold);
        btnText.color = Color.white;

        UpdateUI();
    }

    GameObject CreateStylishPanel(Transform parent, Sprite sprite, Vector2 size, Vector2 pos, Vector2 anchor)
    {
        GameObject p = new GameObject("Panel");
        p.transform.SetParent(parent, false);
        Image img = p.AddComponent<Image>();
        img.sprite = sprite;
        // Si usamos sprite custom, usar Simple o Sliced según convenga. Default a Sliced.
        img.type = Image.Type.Sliced;
        img.color = new Color(1f, 1f, 1f, 0.9f); 
        
        // Sombra (solo si es el procedural, si es custom puede quedar raro duplicarlo)
        if (uiPanelSprite == null)
        {
            GameObject shadow = new GameObject("Shadow");
            shadow.transform.SetParent(p.transform, false);
            shadow.transform.SetAsFirstSibling();
            Image sImg = shadow.AddComponent<Image>();
            sImg.sprite = sprite;
            sImg.color = new Color(0,0,0,0.3f);
            RectTransform srt = shadow.GetComponent<RectTransform>();
            srt.anchorMin = Vector2.zero; srt.anchorMax = Vector2.one;
            srt.offsetMin = new Vector2(4, -4); srt.offsetMax = new Vector2(4, -4);
        }

        RectTransform rt = p.GetComponent<RectTransform>();
        rt.anchorMin = anchor; rt.anchorMax = anchor; rt.pivot = anchor;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        return p;
    }

    Text CreateText(Transform parent, string content, int size, FontStyle style)
    {
        GameObject obj = new GameObject("Txt");
        obj.transform.SetParent(parent, false);
        Text t = obj.AddComponent<Text>();
        t.text = content;
        
        // Usar fuente personalizada si está asignada, sino usar la por defecto
        if (customFont != null)
            t.font = customFont;
        else
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        
        t.fontSize = size;
        t.fontStyle = style;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = new Color(0.2f, 0.2f, 0.2f);
        
        RectTransform rt = t.rectTransform;
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        return t;
    }

    Sprite CreateRoundedRectSprite(Color c, int w, int h, int r)
    {
        Texture2D tex = new Texture2D(w, h);
        Color[] colors = new Color[w * h];
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                bool corner = (x < r && y < r) || (x > w-r && y < r) || (x < r && y > h-r) || (x > w-r && y > h-r);
                if (corner)
                {
                    float dx = (x < r) ? r - x - 1 : x - (w - r);
                    float dy = (y < r) ? r - y - 1 : y - (h - r);
                    if (dx*dx + dy*dy > r*r) colors[y*w+x] = Color.clear;
                    else colors[y*w+x] = c;
                }
                else colors[y*w+x] = c;
            }
        }
        tex.SetPixels(colors);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100, 0, SpriteMeshType.FullRect, new Vector4(r,r,r,r));
    }

    void UpdateUI()
    {
        scoreText.text = $"<size=20>SCORE</size>\n{(int)currentRawScore}\n<size=16>Fallos: {droppedPieces}</size>";
        varietyText.text = $"Ingredientes: {usedIngredientTypes.Count}/{ingredients.Count}";
    }

    void ShowFloatingText(Vector3 pos, string msg, Color c)
    {
        if (mainCanvas == null || mainCam == null) return;

        GameObject txtObj = new GameObject("FloatTxt");
        txtObj.transform.SetParent(mainCanvas.transform);
        Text txt = txtObj.AddComponent<Text>();
        txt.text = msg;
        
        // Usar fuente personalizada si está asignada
        if (customFont != null)
            txt.font = customFont;
        else
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        
        txt.fontSize = 32;
        txt.fontStyle = FontStyle.Bold;
        txt.color = c;
        txt.alignment = TextAnchor.MiddleCenter;
        
        Outline ol = txtObj.AddComponent<Outline>();
        ol.effectDistance = new Vector2(2, -2);
        ol.effectColor = new Color(0,0,0,0.5f);
        
        if (mainCam != null)
            txt.rectTransform.position = mainCam.WorldToScreenPoint(pos);
        else
            txt.rectTransform.position = new Vector3(Screen.width/2, Screen.height/2, 0);
        
        StartCoroutine(AnimateFloatingText(txtObj));
    }

    IEnumerator AnimateFloatingText(GameObject obj)
    {
        float t = 0;
        Vector3 startPos = obj.transform.position;
        while(t < 1.0f)
        {
            t += Time.deltaTime;
            if (obj != null) 
            {
                obj.transform.position = startPos + Vector3.up * (t * 150);
                Text txt = obj.GetComponent<Text>();
                if(txt) txt.color = new Color(txt.color.r, txt.color.g, txt.color.b, 1f - t);
            }
            yield return null;
        }
        if (obj != null) Destroy(obj);
    }
}

// --- CLASES AUXILIARES ---

public class IngredientBehavior : MonoBehaviour
{
    public minijuegofinal gameManager;
    public Transform visualTransform; 
    public int typeID;
    private bool hasLanded = false;

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!hasLanded && collision.relativeVelocity.magnitude > 0.5f)
        {
            if (collision.gameObject.name.Contains("PlatoObjetivo") || collision.gameObject.GetComponent<IngredientBehavior>())
            {
                hasLanded = true;
                if(gameManager != null)
                    gameManager.OnIngredientLanded(this.gameObject, typeID, collision.relativeVelocity.magnitude, transform.position.x);
                
                if(visualTransform != null)
                    StartCoroutine(SquashAnimation());
            }
        }
    }

    IEnumerator SquashAnimation()
    {
        Vector3 originalScale = visualTransform.localScale;
        Vector3 squashScale = new Vector3(originalScale.x * 1.3f, originalScale.y * 0.6f, 1);
        
        float duration = 0.08f;
        float t = 0;
        
        while(t < duration) {
            if(visualTransform == null) yield break;
            visualTransform.localScale = Vector3.Lerp(originalScale, squashScale, t/duration);
            t += Time.deltaTime;
            yield return null;
        }
        
        t = 0;
        Vector3 stretchScale = new Vector3(originalScale.x * 0.9f, originalScale.y * 1.1f, 1);
        while(t < duration) {
            if(visualTransform == null) yield break;
            visualTransform.localScale = Vector3.Lerp(squashScale, stretchScale, t/duration);
            t += Time.deltaTime;
            yield return null;
        }

        t = 0;
        while(t < duration) {
            if(visualTransform == null) yield break;
            visualTransform.localScale = Vector3.Lerp(stretchScale, originalScale, t/duration);
            t += Time.deltaTime;
            yield return null;
        }
        if(visualTransform != null) visualTransform.localScale = originalScale;
    }
}

public class DeathZone : MonoBehaviour
{
    public minijuegofinal gameManager;
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<IngredientBehavior>())
        {
            gameManager.OnIngredientFell(other.gameObject);
        }
    }
}