using UnityEngine;

public class BookAnimator : MonoBehaviour
{
    [Header("Objeto a Mover")]
    [Tooltip("Arrastra aquí el RectTransform del panel del libro de recetas.")]
    public RectTransform bookPanel;

    [Header("Posiciones")]
    [Tooltip("La posición X (horizontal) cuando el libro está EN pantalla.")]
    public float shownXPosition = 0; // Por ejemplo, 0 si est� centrado

    [Tooltip("La posición X cuando el libro est� FUERA de pantalla (a la derecha).")]
    public float hiddenXPosition = 2000; // Un valor grande, fuera del borde

    [Header("Vel Animación")]
    [Tooltip("Velocidad del deslizamiento. M�s alto = más rápido.")]
    public float animationSpeed = 10f;

    // --- Variables privadas ---
    private bool isBookShown = false; // Estado actual del libro
    private Vector2 targetPosition;   // Posición a la que nos queremos mover

    void Start()
    {
        if (bookPanel != null)         // Asegurarse de que el libro empiece oculto
            bookPanel.anchoredPosition = new Vector2(hiddenXPosition, bookPanel.anchoredPosition.y);
    }

    void Update()
    {
        if (isBookShown)
            targetPosition = new Vector2(shownXPosition, bookPanel.anchoredPosition.y);
        else
            targetPosition = new Vector2(hiddenXPosition, bookPanel.anchoredPosition.y);

        //bookPanel.anchoredPosition = Vector2.Lerp(bookPanel.anchoredPosition, targetPosition, animationSpeed * Time.deltaTime);
        bookPanel.anchoredPosition = Vector2.Lerp(bookPanel.anchoredPosition, targetPosition, animationSpeed * Time.unscaledDeltaTime);
    }
    public void ToggleBook()
    {
        isBookShown = !isBookShown; // Invierte el estado (si estaba mostrado, se oculta, y viceversa)
        if (isBookShown)
            TimeManager.Instance.PauseGame();
        else
            TimeManager.Instance.ResumeGame();
    }
}
