using UnityEngine;
using UnityEngine.UI;

public class FoodSwitcher : MonoBehaviour
{
    [Header("Referencias")]
    public GameObject[] foodImages;   // GameObjects que contienen Image + PintarSobreCanvas
    public GameObject[] drawCanvases; // RawImages para pintar (uno por cada foodImage)

    private int currentIndex = 0;

    void Start()
    {
        // Activar solo el primero y desactivar los demás
        for (int i = 0; i < foodImages.Length; i++)
        {
            bool active = i == 0;
            foodImages[i].SetActive(active);
            drawCanvases[i].SetActive(active);
        }
    }

    // Esta función se asigna al botón
    public void NextFood()
    {
        if (foodImages.Length == 0 || drawCanvases.Length == 0) return;

        // Desactivar el actual
        foodImages[currentIndex].SetActive(false);
        drawCanvases[currentIndex].SetActive(false);

        // Avanzar índice
        currentIndex++;
        if (currentIndex >= foodImages.Length)
            currentIndex = 0; // o poner currentIndex--; si quieres detenerse al final

        // Activar el siguiente
        foodImages[currentIndex].SetActive(true);
        drawCanvases[currentIndex].SetActive(true);
    }
}
