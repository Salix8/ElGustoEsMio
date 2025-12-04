using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class PintarSobreCanvas : MonoBehaviour
{
    [Header("Refs")]
    public RawImage drawRawImage;    // RawImage donde pintamos
    public Image foodImage;          // Image del alimento (sprite)

    [Header("Ajustes")]
    public Color drawColor = Color.red;
    [Tooltip("Resolución del canvas de dibujo (cuadrado). Más alto = más detalle pero más memoria")]
    public int textureResolution = 1024;
    public int brushSize = 12;
    [Tooltip("Cuántos frames esperar entre Apply() para rendimiento (1 = cada frame)")]
    public int applyEveryNFrames = 2;

    Texture2D drawTexture;
    RectTransform drawRect;
    Sprite foodSprite;
    Texture2D foodTexture;
    Rect spriteRect;
    Vector2 spritePivot;
    int frameCounter = 0;

    Vector2 lastLocalPos;
    bool drawing = false;
    bool cortadoIzquierda = false;
    bool cortadoDerecha = false;
    [Header("Zonas de corte")]
    public RectTransform[] cutZones;
    public RectTransform[] cutZonesIzq;
    public RectTransform[] cutZonesDch;
    public bool[] zoneEntered;
    public bool[] zoneEnteredIzq;
    public bool[] zoneEnteredDch;

    [Header("Estados del alimento")]
    public List<Sprite> foodStates;  // Sprites del mismo alimento
    int currentStateIndex = 0;
    void Start()
    {
        if (drawRawImage == null || foodImage == null)
        {
            Debug.LogError("Asignar drawRawImage y foodImage en el inspector.");
            enabled = false;
            return;
        }

        drawRect = drawRawImage.rectTransform;

        drawTexture = new Texture2D(textureResolution, textureResolution, TextureFormat.RGBA32, false);
        ClearTexture();
        drawRawImage.texture = drawTexture;

        foodSprite = foodImage.sprite;
        if (foodSprite == null)
        {
            Debug.LogError("Food Image no tiene sprite asignada.");
            enabled = false;
            return;
        }

        foodTexture = foodSprite.texture;
        spriteRect = foodSprite.textureRect;
        spritePivot = foodSprite.pivot;

        zoneEntered = new bool[cutZones.Length];
        zoneEnteredIzq = new bool[cutZonesIzq.Length];
        zoneEnteredDch = new bool[cutZonesDch.Length];
    }

    void Update()
    {
        HandleInput();

        frameCounter++;
        if (frameCounter >= applyEveryNFrames)
        {
            drawTexture.Apply();
            frameCounter = 0;
        }
    }

    void HandleInput()
    {
        Vector2 local;

        // TOUCH
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (PointOverDrawRect(touch.position, out local))
            {
                CheckCutZones(local);

                if (touch.phase == TouchPhase.Began)
                {
                    lastLocalPos = local;
                    drawing = true;

                    if (IsInsideFoodAlpha(local))
                        DrawAtLocal(local);
                }
                else if ((touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary) && drawing)
                {
                    if (IsInsideFoodAlpha(local))
                        DrawBetween(lastLocalPos, local);
                    lastLocalPos = local;
                }
                else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                    drawing = false;
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                drawing = false;

            return; // no procesar ratón si hay touch
        }

        // MOUSE
        if (PointOverDrawRect(Input.mousePosition, out local))
        {
            CheckCutZones(local);

            if (Input.GetMouseButtonDown(0))
            {
                lastLocalPos = local;
                drawing = true;
                if (IsInsideFoodAlpha(local))
                    DrawAtLocal(local);
            }
            else if (Input.GetMouseButton(0) && drawing)
            {
                if (IsInsideFoodAlpha(local))
                    DrawBetween(lastLocalPos, local);
                lastLocalPos = local;
            }
            else if (Input.GetMouseButtonUp(0))
                drawing = false;
        }
        else if (Input.GetMouseButtonUp(0))
            drawing = false;
    }

void CheckCutZones(Vector2 localPoint)
{
    // ===============================
    // 1) ZONAS NORMALES → solo debug
    // ===============================
    for (int i = 0; i < cutZones.Length; i++)
    {
        Vector2 zoneLocal;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            cutZones[i],
            RectTransformUtility.WorldToScreenPoint(null, drawRect.TransformPoint(localPoint)),
            null,
            out zoneLocal
        );

        if (cutZones[i].rect.Contains(zoneLocal))
        {
            if (!zoneEntered[i])
            {
                zoneEntered[i] = true;
                Debug.Log("Entró en zona normal: " + cutZones[i].name);
            }
        }
        else
        {
            zoneEntered[i] = false;
        }
    }

    // ==================================
    // 2) ZONAS IZQUIERDA → cambian sprite
    // ==================================
    bool allLeft = true;
    for (int i = 0; i < cutZonesIzq.Length; i++)
    {
        Vector2 zoneLocal;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            cutZonesIzq[i],
            RectTransformUtility.WorldToScreenPoint(null, drawRect.TransformPoint(localPoint)),
            null,
            out zoneLocal
        );

        if (cutZonesIzq[i].rect.Contains(zoneLocal))
            zoneEnteredIzq[i] = true;

        if (!zoneEnteredIzq[i]) allLeft = false;
    }
    if (allLeft && cutZonesIzq.Length > 0 && !cortadoIzquierda)
    {
        cortadoIzquierda = true;
        NextFoodState();
        ResetCutZonesFlags();
    }

    // ==================================
    // 3) ZONAS DERECHA → cambian sprite
    // ==================================
    bool allRight = true;
    for (int i = 0; i < cutZonesDch.Length; i++)
    {
        Vector2 zoneLocal;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            cutZonesDch[i],
            RectTransformUtility.WorldToScreenPoint(null, drawRect.TransformPoint(localPoint)),
            null,
            out zoneLocal
        );

        if (cutZonesDch[i].rect.Contains(zoneLocal))
            zoneEnteredDch[i] = true;

        if (!zoneEnteredDch[i]) allRight = false;
    }
    if (allRight && cutZonesDch.Length > 0 && !cortadoDerecha)
    {
        cortadoDerecha = true;
        NextFoodState();
        ResetCutZonesFlags();
    }
}

    void ResetCutZonesFlags()
    {
        for (int i = 0; i < zoneEnteredIzq.Length; i++)
            zoneEnteredIzq[i] = false;
        for (int i = 0; i < zoneEnteredDch.Length; i++)
            zoneEnteredDch[i] = false;
    }

    bool PointOverDrawRect(Vector2 screenPoint, out Vector2 localPoint)
    {
        return RectTransformUtility.ScreenPointToLocalPointInRectangle(drawRect, screenPoint, null, out localPoint);
    }

    bool IsInsideFoodAlpha(Vector2 localPoint)
    {
        Vector2 uv = LocalToUV(localPoint);
        if (uv.x < 0 || uv.x > 1 || uv.y < 0 || uv.y > 1)
            return false;

        Rect r = foodSprite.textureRect;
        int px = Mathf.FloorToInt(r.x + uv.x * r.width);
        int py = Mathf.FloorToInt(r.y + uv.y * r.height);

        if (px < 0 || py < 0 || px >= foodTexture.width || py >= foodTexture.height)
            return false;

        Color c = foodTexture.GetPixel(px, py);
        return c.a > 0.1f;
    }

    Vector2 LocalToUV(Vector2 local)
    {
        float w = drawRect.rect.width;
        float h = drawRect.rect.height;
        return new Vector2((local.x + w * 0.5f) / w, (local.y + h * 0.5f) / h);
    }

    void DrawBetween(Vector2 start, Vector2 end)
    {
        Vector2 pxStart = LocalToUV(start) * drawTexture.width;
        Vector2 pxEnd = LocalToUV(end) * drawTexture.width;
        DrawLine((int)pxStart.x, (int)pxStart.y, (int)pxEnd.x, (int)pxEnd.y);
    }

    void DrawAtLocal(Vector2 local)
    {
        Vector2 uv = LocalToUV(local);
        DrawBrushAt((int)(uv.x * drawTexture.width), (int)(uv.y * drawTexture.height));
    }

    void DrawLine(int x0, int y0, int x1, int y1)
    {
        int dx = Mathf.Abs(x1 - x0);
        int dy = Mathf.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;

        while (true)
        {
            DrawBrushAt(x0, y0);
            if (x0 == x1 && y0 == y1) break;
            int e2 = 2 * err;
            if (e2 > -dy) { err -= dy; x0 += sx; }
            if (e2 < dx) { err += dx; y0 += sy; }
        }
    }

    void DrawBrushAt(int cx, int cy)
    {
        int r = brushSize;
        int sq = r * r;
        int x0 = Mathf.Clamp(cx - r, 0, drawTexture.width - 1);
        int x1 = Mathf.Clamp(cx + r, 0, drawTexture.width - 1);
        int y0 = Mathf.Clamp(cy - r, 0, drawTexture.height - 1);
        int y1 = Mathf.Clamp(cy + r, 0, drawTexture.height - 1);

        for (int x = x0; x <= x1; x++)
            for (int y = y0; y <= y1; y++)
                if ((x - cx) * (x - cx) + (y - cy) * (y - cy) <= sq)
                    drawTexture.SetPixel(x, y, drawColor);
    }

    public void ClearTexture()
    {
        Color clear = new Color(0, 0, 0, 0);
        Color[] arr = new Color[drawTexture.width * drawTexture.height];
        for (int i = 0; i < arr.Length; i++) arr[i] = clear;
        drawTexture.SetPixels(arr);
        drawTexture.Apply();
    }

    public void NextFoodState()
    {
        if (foodStates == null || foodStates.Count == 0)
            return;

        currentStateIndex++;
        if (currentStateIndex >= foodStates.Count)
        {
            Debug.Log("Alimento COMPLETADO");
            currentStateIndex = 0;
            ClearTexture(); // reset canvas para siguiente uso si se reactiva
            return;
        }

        foodSprite = foodStates[currentStateIndex];
        foodImage.sprite = foodSprite;

        foodTexture = foodSprite.texture;
        spriteRect = foodSprite.textureRect;
        spritePivot = foodSprite.pivot;

        Debug.Log("Cambiado al estado: " + currentStateIndex);
    }
}
