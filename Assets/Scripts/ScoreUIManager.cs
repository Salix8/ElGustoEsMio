using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScoreUIManager : MonoBehaviour
{

    public MinigameProgressManager minigameProgressManager;

    [Header("Estrellas Doradas (activas)")]
    public Image[] estrellasDoradas = new Image[5];

    [Header("Estrellas Negras (inactivas)")]
    public Image[] estrellasNegras = new Image[5];

    [Header("Platos")]
    public GameObject plato1;
    public GameObject plato2;
    public GameObject plato3;

    [Header("Textos (5 niveles)")]
    public GameObject[] textos = new GameObject[5];
    public float score;

    void Start()
    {
        if(minigameProgressManager == null)
        {
            minigameProgressManager = FindObjectOfType<MinigameProgressManager>();
        }
        score = minigameProgressManager.puntuacionMedia;
        SetScore();
    }
    // Llama a esta función con tu puntuación
    public void SetScore()
    {
        UpdateStars();
        UpdatePlate();
        UpdateText();
    }

    void UpdateStars()
    {
        int estrellas = GetStarCount();

        for (int i = 0; i < 5; i++)
        {
            bool activarDorada = i < estrellas;

            estrellasDoradas[i].gameObject.SetActive(activarDorada);
            estrellasNegras[i].gameObject.SetActive(!activarDorada);

            if (activarDorada)
                StartCoroutine(AnimarEstrella(estrellasDoradas[i].transform));
        }
    }

    int GetStarCount()
    {
        if (score < 2) return 1;
        if (score < 4) return 2;
        if (score < 6) return 3;
        if (score < 8) return 4;
        return 5;
    }

    IEnumerator AnimarEstrella(Transform t)
    {
        float tiempo = 0;
        while (true)
        {
            tiempo += Time.deltaTime * 2f;
            float rot = Mathf.Sin(tiempo) * 8f; // Oscila -8º a +8º
            t.localRotation = Quaternion.Euler(0, 0, rot);
            yield return null;
        }
    }

    void UpdatePlate()
    {
        plato1.SetActive(false);
        plato2.SetActive(false);
        plato3.SetActive(false);

        if (score < 4) plato1.SetActive(true);
        else if (score < 8) plato2.SetActive(true);
        else plato3.SetActive(true);
    }

    void UpdateText()
    {
        for (int i = 0; i < textos.Length; i++)
            textos[i].SetActive(false);

        int index = GetStarCount() - 1; // 0–4
        textos[index].SetActive(true);
    }
}
