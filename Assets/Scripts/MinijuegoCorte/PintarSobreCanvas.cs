using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PintarSobreCanvas : MonoBehaviour
{
    [Header("Refs")]
    public RawImage drawRawImage;     // RawImage donde pintamos
    public Image foodImage;          // Image del alimento (sprite)

    [Header("Ajustes")]
    public Color drawColor = Color.red;
    [Tooltip("Resolución del canvas de dibujo (cuadrado). Más alto = más detalle pero más memoria")]
    public int textureResolution = 1024;
    public int brushSize = 12;
    [Tooltip("Cuántos frames esperar entre Apply() para rendimiento (1 = cada frame)")]
    public int applyEveryNFrames = 2;

    Texture2D drawTexture;           // textura donde dibujamos (alpha sobre transparente)
    RectTransform drawRect;
    Sprite foodSprite;
    Texture2D foodTexture;           // textura original del sprite (debe ser readable)
    Rect spriteRect;                 // rect del sprite dentro de la textura (pixels)
    Vector2 spritePivot;             // pivot del sprite (0..1)
    int frameCounter = 0;

    Vector2 lastLocalPos;
    bool drawing = false;

    void Start()
    {
        if (drawRawImage == null || foodImage == null)
        {
            Debug.LogError("Asignar drawRawImage y foodImage en el inspector.");
            enabled = false;
            return;
        }

        drawRect = drawRawImage.rectTransform;

        // Crear textura transparente para pintar
        drawTexture = new Texture2D(textureResolution, textureResolution, TextureFormat.RGBA32, false);
        ClearTexture();
        drawRawImage.texture = drawTexture;

        // Preparar datos del sprite (para comprobación de alpha)
        foodSprite = foodImage.sprite;
        if (foodSprite == null)
        {
            Debug.LogError("Food Image no tiene sprite asignada.");
            enabled = false;
            return;
        }

        // Obtener la textura de la sprite (debe tener Read/Write enabled)
        foodTexture = foodSprite.texture;
        spriteRect = foodSprite.textureRect; // rect dentro de la textura (en píxeles)
        spritePivot = foodSprite.pivot;       // pivot en pixeles relativos al rect
    }

    void Update()
    {
        HandleInput();

        // Aplicar la textura cada N frames para mejor rendimiento
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
            Vector2 screenPos = touch.position;

            if (PointOverDrawRect(screenPos, out local))
            {
                if (IsInsideFoodAlpha(local))
                {
                    DrawBetween(lastLocalPos, local);
                }

                // lastLocalPos SIEMPRE se actualiza
                lastLocalPos = local;
            }
        }
        else
        {
            // MOUSE
            Vector2 mousePos = Input.mousePosition;

            if (PointOverDrawRect(mousePos, out local))
            {
                if (IsInsideFoodAlpha(local))
                {
                    DrawBetween(lastLocalPos, local);
                }

                // lastLocalPos SIEMPRE se actualiza
                lastLocalPos = local;
            }
        }
    }


    bool PointOverDrawRect(Vector2 screenPoint, out Vector2 localPoint)
    {
        // Convierte screenPoint a punto local en rectTransform (coordenadas centradas)
        return RectTransformUtility.ScreenPointToLocalPointInRectangle(drawRect, screenPoint, null, out localPoint);
    }

    // Comprueba si el punto local (en rectTransform coords) cae sobre el sprite (alpha > umbral)
    bool IsInsideFoodAlpha(Vector2 localPoint)
    {
        // Convertimos punto local (-w/2..w/2, -h/2..h/2) a UV (0..1)
        Vector2 uv = LocalToUV(localPoint);

        // Si el punto está fuera del RawImage, no pintamos
        if (uv.x < 0 || uv.x > 1 || uv.y < 0 || uv.y > 1)
            return false;

        // Convertimos UV dentro del rectTransform a UV dentro del sprite
        // foodSprite.textureRect = rectángulo que ocupa el sprite en la textura real
        Rect r = foodSprite.textureRect;

        float texX = r.x + uv.x * r.width;
        float texY = r.y + uv.y * r.height;

        int px = Mathf.FloorToInt(texX);
        int py = Mathf.FloorToInt(texY);

        // Seguridad
        if (px < 0 || py < 0 || px >= foodTexture.width || py >= foodTexture.height)
            return false;

        // Leer el pixel del sprite original
        Color c = foodTexture.GetPixel(px, py);

        // Solo pintar en zonas con alpha real del PNG
        return c.a > 0.1f;
    }

    // Convierte punto local (-w/2..w/2, -h/2..h/2) a UV 0..1 dentro del RawImage
    Vector2 LocalToUV(Vector2 local)
    {
        float w = drawRect.rect.width;
        float h = drawRect.rect.height;
        float u = (local.x + w * 0.5f) / w;
        float v = (local.y + h * 0.5f) / h;
        return new Vector2(u, v);
    }

    // Dibuja una línea entre dos puntos locales (en coords de rectTransform)
    void DrawBetween(Vector2 localStart, Vector2 localEnd)
    {
        Vector2 uvStart = LocalToUV(localStart);
        Vector2 uvEnd = LocalToUV(localEnd);

        Vector2 pxStart = new Vector2(uvStart.x * drawTexture.width, uvStart.y * drawTexture.height);
        Vector2 pxEnd = new Vector2(uvEnd.x * drawTexture.width, uvEnd.y * drawTexture.height);

        DrawLine((int)pxStart.x, (int)pxStart.y, (int)pxEnd.x, (int)pxEnd.y);
    }

    // Dibuja línea con algoritmo simple y pinta círculos (grosor)
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
        {
            for (int y = y0; y <= y1; y++)
            {
                int dx = x - cx;
                int dy = y - cy;
                if (dx * dx + dy * dy <= sq)
                {
                    drawTexture.SetPixel(x, y, drawColor);
                }
            }
        }
    }

    public void ClearTexture()
    {
        Color clear = new Color(0, 0, 0, 0);
        Color[] arr = new Color[drawTexture.width * drawTexture.height];
        for (int i = 0; i < arr.Length; i++) arr[i] = clear;
        drawTexture.SetPixels(arr);
        drawTexture.Apply();
    }
}