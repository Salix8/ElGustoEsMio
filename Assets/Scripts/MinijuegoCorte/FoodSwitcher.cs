using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FoodSwitcher : MonoBehaviour
{
    [Header("Referencias")]
    public GameObject[] foodImages;   // Contiene Image + PintarSobreCanvas
    public GameObject[] drawCanvases; // Los RawImage asociados

    [Header("Animación")]
    public float slideDuration = 0.35f;   // Velocidad del deslizamiento
    public float canvasWidth = 2000f;     // Ancho aproximado del canvas (ajústalo)

    private int currentIndex = 0;
    private bool isAnimating = false;

    void Start()
    {
        for (int i = 0; i < foodImages.Length; i++)
        {
            bool active = i == 0;
            foodImages[i].SetActive(active);
            drawCanvases[i].SetActive(active);

            // Asegura que están en el centro
            if (active)
            {
                foodImages[i].GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                drawCanvases[i].GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            }
        }
    }

    public void NextFood()
    {
        if (isAnimating) return;
        if (foodImages.Length == 0) return;

        int nextIndex = currentIndex + 1;
        if (nextIndex >= foodImages.Length)
            nextIndex = 0;

        StartCoroutine(SwitchFoodAnimated(currentIndex, nextIndex));

        currentIndex = nextIndex;
    }

    private IEnumerator SwitchFoodAnimated(int oldIndex, int newIndex)
    {
        isAnimating = true;

        GameObject oldFood = foodImages[oldIndex];
        GameObject oldDraw = drawCanvases[oldIndex];

        GameObject newFood = foodImages[newIndex];
        GameObject newDraw = drawCanvases[newIndex];

        RectTransform oldRT = oldFood.GetComponent<RectTransform>();
        RectTransform oldDrawRT = oldDraw.GetComponent<RectTransform>();

        RectTransform newRT = newFood.GetComponent<RectTransform>();
        RectTransform newDrawRT = newDraw.GetComponent<RectTransform>();

        // Posicionar el nuevo fuera de pantalla, por la derecha
        Vector2 rightPos = new Vector2(canvasWidth, 0);
        newRT.anchoredPosition = rightPos;
        newDrawRT.anchoredPosition = rightPos;

        newFood.SetActive(true);
        newDraw.SetActive(true);

        float t = 0f;

        Vector2 startOld = Vector2.zero;
        Vector2 endOld = new Vector2(-canvasWidth, 0);

        Vector2 startNew = rightPos;
        Vector2 endNew = Vector2.zero;

        while (t < 1f)
        {
            t += Time.deltaTime / slideDuration;
            float smooth = Mathf.SmoothStep(0, 1, t);

            oldRT.anchoredPosition = Vector2.Lerp(startOld, endOld, smooth);
            oldDrawRT.anchoredPosition = Vector2.Lerp(startOld, endOld, smooth);

            newRT.anchoredPosition = Vector2.Lerp(startNew, endNew, smooth);
            newDrawRT.anchoredPosition = Vector2.Lerp(startNew, endNew, smooth);

            yield return null;
        }

        // Asegurar posiciones finales
        oldRT.anchoredPosition = endOld;
        oldDrawRT.anchoredPosition = endOld;

        newRT.anchoredPosition = endNew;
        newDrawRT.anchoredPosition = endNew;

        oldFood.SetActive(false);
        oldDraw.SetActive(false);

        isAnimating = false;
    }
}
