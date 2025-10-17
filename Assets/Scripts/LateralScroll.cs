using UnityEngine;

public class LateralScroll : MonoBehaviour
{
    [Header("Configuracion del Scroll")]
    [Tooltip("Arrastra aqui el objeto (Panel, Imagen, etc.) que quieres desplazar. Debe tener un RectTransform.")]
    public RectTransform contentToScroll;

    [Tooltip("La posicion X manima hasta la que se puede desplazar el contenido.")]
    public float minXPosition;

    [Tooltip("La posicion X maxima hasta la que se puede desplazar el contenido.")]
    public float maxXPosition;

    [Tooltip("Ajusta la velocidad del desplazamiento. 1 es normal, >1 mas rapido, <1 mas lento.")]
    public float scrollSensitivity = 1.0f;

    // Variables internas
    private bool isDragging = false;
    private Vector3 lastTouchPosition;

    void Start()
    {
        CalculateBounds();
    }

    void Update()
    {
        // --- GESTION DE INPUT PARA MoVIL Y EDITOR ---
        if (Input.GetMouseButtonDown(0)) // Si se pulsa la pantalla (o el boton izquierdo del raton)
        {
            isDragging = true;
            lastTouchPosition = Input.mousePosition; // Guardamos la posicion inicial del toque
        }
        else if (Input.GetMouseButtonUp(0)) // Si se levanta el dedo de la pantalla
        {
            isDragging = false;
        }

        // --- LaGICA DE DESPLAZAMIENTO ---

        if (isDragging)
        {
            if (contentToScroll == null)
            {
                Debug.LogError("No has asignado el 'Content To Scroll' en el Inspector.");
                isDragging = false; // Detenemos el arrastre si no hay contenido
                return;
            }

            // Cuanto se ha movido
            Vector3 currentTouchPosition = Input.mousePosition;
            float differenceX = (currentTouchPosition.x - lastTouchPosition.x) * scrollSensitivity;

            // Posicion actual
            Vector2 newPosition = contentToScroll.anchoredPosition;
            newPosition.x += differenceX;

            newPosition.x = Mathf.Clamp(newPosition.x, minXPosition, maxXPosition);

            contentToScroll.anchoredPosition = newPosition;

            lastTouchPosition = currentTouchPosition;
        }
    }
    
     [ContextMenu("Calcular límites")]
    public void CalculateBounds()
    {
        if (contentToScroll == null)
        {
            Debug.LogWarning("CalculateBounds: contentToScroll no asignado.");
            minXPosition = -500;
            maxXPosition = 500;
            return;
        }

        float contentWidth = contentToScroll.rect.width;
        float viewportWidth = contentToScroll.rect.width;

        if (contentWidth <= viewportWidth)
        {
            minXPosition = -contentWidth;
            maxXPosition = contentWidth;
        }
        else
        {
            float extra = contentWidth - viewportWidth; // cuanto "desborda"
            maxXPosition = contentWidth;
            minXPosition = -(contentWidth) - extra;
        }
    }
}
