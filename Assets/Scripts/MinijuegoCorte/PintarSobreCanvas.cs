using UnityEngine;
using UnityEngine.UI;

public class PintarSobreCanvas : MonoBehaviour
{
    public RawImage rawImage;
    Texture2D tex;

    void Start()
    {
        tex = new Texture2D(1024, 1024);
        Clear(Color.clear);
        rawImage.texture = tex;
    }

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rawImage.rectTransform,
                Input.mousePosition,
                null,
                out localPoint
            );

            // Convertir a píxeles de textura
            Vector2 pivot = rawImage.rectTransform.pivot;
            float width = rawImage.rectTransform.rect.width;
            float height = rawImage.rectTransform.rect.height;

            int x = (int)((localPoint.x + width * pivot.x) * tex.width / width);
            int y = (int)((localPoint.y + height * pivot.y) * tex.height / height);

            PintarPixel(x, y, Color.red);
        }
    }

    void PintarPixel(int x, int y, Color color)
    {
        if (x < 0 || x >= tex.width || y < 0 || y >= tex.height) return;

        tex.SetPixel(x, y, color);
        tex.Apply();
    }

    void Clear(Color c)
    {
        Color[] fill = tex.GetPixels();

        for (int i = 0; i < fill.Length; i++)
            fill[i] = c;

        tex.SetPixels(fill);
        tex.Apply();
    }
}
