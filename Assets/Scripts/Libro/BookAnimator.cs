using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class BookAnimator : MonoBehaviour
{
    [Header("Objeto a Mover")]
    [Tooltip("Arrastra aquí el RectTransform del panel del libro de recetas.")]
    public RectTransform bookPanel;

    [Header("Posiciones")]
    [Tooltip("La posición X (horizontal) cuando el libro está EN pantalla.")]
    public float shownXPosition = 0;

    [Tooltip("La posición X cuando el libro está FUERA de pantalla (a la derecha).")]
    public float hiddenXPosition = 2000;

    [Header("Vel Animación")]
    [Tooltip("Velocidad del deslizamiento. Más alto = más rápido.")]
    public float animationSpeed = 10f;
    
    [Header("Animación de Páginas")]
    [Tooltip("La imagen UI que mostrará los sprites de las páginas pasando.")]
    public Image animatedPageImage;

    [Tooltip("Los sprites para la animación de pasar página, en orden.")]
    public Sprite[] pageTurnSprites;

    [Tooltip("El sprite base del libro abierto (cuando no hay animación).")]
    public Sprite bookBaseSprite;

    [Tooltip("Tiempo en segundos entre cada frame de la animación de página.")]
    public float pageTurnSpeed = 0.1f;
    
    // --- Variables privadas ---
    private bool isAnimating = false; // Para controlar la corutina activa
    private bool isBookShown = false; // Estado lógico del libro (abierto/cerrado)

    void Start()
    {
        if (bookPanel != null)
        {
            // Asegurarse de que el libro empiece oculto
            bookPanel.anchoredPosition = new Vector2(hiddenXPosition, bookPanel.anchoredPosition.y);
        }
        if (animatedPageImage != null)
        {
            animatedPageImage.gameObject.SetActive(false); // La página animada empieza oculta
        }
    }

    void Update()
    {
        if (bookPanel == null) return;

        // Determinar la posición objetivo basado en el estado
        Vector2 targetPosition = isBookShown 
            ? new Vector2(shownXPosition, bookPanel.anchoredPosition.y) 
            : new Vector2(hiddenXPosition, bookPanel.anchoredPosition.y);

        // Mover suavemente hacia el objetivo
        bookPanel.anchoredPosition = Vector2.Lerp(bookPanel.anchoredPosition, targetPosition, animationSpeed * Time.unscaledDeltaTime);
    }

    /// <summary>
    /// Inicia la animación para abrir o cerrar el libro.
    /// No controla el contenido, solo la animación.
    /// </summary>
    public void ToggleBook()
    {
        // Si ya hay una animación en curso, no hacer nada para evitar solapamientos.
        if (isAnimating)
        {
            Debug.LogWarning("Se ha ignorado la acción: la animación del libro ya está en curso.");
            return;
        }

        // El estado lógico cambia, y la animación correspondiente se dispara.
        if (!isBookShown)
        {
            StartCoroutine(OpenBookSequence());
        }
        else
        {
            StartCoroutine(CloseBookSequence());
        }
    }

    public bool IsBookOpen()
    {
        return isBookShown;
    }

    private IEnumerator OpenBookSequence()
    {
        isAnimating = true;
        Debug.Log("Iniciando secuencia de apertura de libro...");
        TimeManager.Instance.PauseGame();
        
        // 1. Indicar que el libro debe mostrarse para que Update empiece a moverlo
        isBookShown = true;

        // 2. Esperar a que el libro casi llegue a su posición
        yield return new WaitUntil(() => Mathf.Abs(bookPanel.anchoredPosition.x - shownXPosition) < 1f);
        Debug.Log("Libro en posición. Iniciando animación de páginas.");

        // 3. Animar las páginas pasando hacia adelante
        if (animatedPageImage != null && pageTurnSprites != null && pageTurnSprites.Length > 0)
        {
            animatedPageImage.gameObject.SetActive(true);

            foreach (var pageSprite in pageTurnSprites)
            {
                animatedPageImage.sprite = pageSprite;
                yield return new WaitForSecondsRealtime(pageTurnSpeed);
            }
            
            // Opcional: mostrar el libro base después de la animación
            if (bookBaseSprite != null)
            {
                animatedPageImage.sprite = bookBaseSprite;
            }
            else
            {
                animatedPageImage.gameObject.SetActive(false);
            }
        }
        
        Debug.Log("Animación de páginas finalizada.");
        isAnimating = false; // Liberar el bloqueo
    }

    private IEnumerator CloseBookSequence()
    {
        isAnimating = true;
        Debug.Log("Iniciando secuencia de cierre de libro...");
        
        if (animatedPageImage != null && pageTurnSprites != null && pageTurnSprites.Length > 0)
        {
            animatedPageImage.gameObject.SetActive(true);
            if (bookBaseSprite != null)
            {
                animatedPageImage.sprite = bookBaseSprite;
                yield return new WaitForSecondsRealtime(pageTurnSpeed); // Pequeña pausa
            }

            for (int i = pageTurnSprites.Length - 1; i >= 0; i--)
            {
                animatedPageImage.sprite = pageTurnSprites[i];
                yield return new WaitForSecondsRealtime(pageTurnSpeed);
            }
        }
        
        Debug.Log("Animación de páginas invertida finalizada. Ocultando libro.");
        
        // Ocultar la imagen de animación y reanudar el juego
        if (animatedPageImage != null)
        {
            animatedPageImage.gameObject.SetActive(false);
        }
        TimeManager.Instance.ResumeGame();

        // Indicar que el libro debe ocultarse para que Update empiece a moverlo
        isBookShown = false;
        
        // Esperar un poco a que el libro se vaya para liberar el bloqueo
        yield return new WaitForSecondsRealtime(1.0f); // Darle tiempo a que se vaya de la pantalla
        isAnimating = false; // Liberar el bloqueo
    }
}