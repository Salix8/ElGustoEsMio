using UnityEngine;

public class KnifeFollowCursor : MonoBehaviour
{
    [Header("Referencia al cuchillo")]
    public RectTransform knifeRect; // RectTransform del Image del cuchillo
    public Canvas canvas;           // Canvas donde está el cuchillo

    [Header("Offset en píxeles desde la punta")]
    public float offsetX = 30f;     // Ajusta horizontalmente
    public float offsetY = -25f;     // Ajusta verticalmente

    void Update()
    {
        Vector2 screenPos;

        // Obtener input del ratón o del primer toque
        if (Input.touchCount > 0)
            screenPos = Input.GetTouch(0).position;
        else
            screenPos = Input.mousePosition;

        // Convertir la posición de pantalla a posición local del canvas
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            screenPos,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
            out Vector2 localPoint
        );

        // Aplicar offset hardcodeado para que la punta coincida
        knifeRect.localPosition = localPoint + new Vector2(offsetX, offsetY);
    }
}
