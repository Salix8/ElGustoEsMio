using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections; // Necesario para Corutinas

/// <summary>
/// Este script va en el Plato. Actúa como un trigger para
/// recibir todos los trozos de carne cocinada, calcular su puntuación media y almacenarla.
/// El número de carnes esperadas se define dinámicamente desde el MeatSpawner.
/// </summary>
[RequireComponent(typeof(Collider))]
public class Plate : MonoBehaviour
{
    [Header("Dependencias")]
    [Tooltip("Arrastra aquí el objeto de la escena que contiene el script MeatSpawner.")]
    public MeatSpawner meatSpawner;

    private QuestBookManager questBookManager;
    private int expectedMeats;
    private List<float> collectedScores = new List<float>();
    private List<Meat> collectedMeatObjects = new List<Meat>();
    private float averageScore = 0f;
    private bool isFinalScoreCalculated = false;

    void Start()
    {
        questBookManager = FindObjectOfType<QuestBookManager>();
        if (questBookManager == null)
        {
            Debug.LogError("¡Error en Plate.cs! No se encontró un 'QuestBookManager' en la escena.", this);
        }

        // Asigna dinámicamente el número de carnes esperadas.
        if (meatSpawner != null)
        {
            expectedMeats = meatSpawner.meatSprites.Length;
            Debug.Log($"Plato configurado para esperar {expectedMeats} trozos de carne.");
        }
        else
        {
            Debug.LogError("¡Error en Plate.cs! La referencia a 'meatSpawner' no está asignada en el Inspector.", this);
            expectedMeats = 0; // Evita que el juego espere carnes si hay un error de configuración.
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Si ya hemos calculado la puntuación final, o si no hay carnes esperadas, no hacemos nada más.
        if (isFinalScoreCalculated || expectedMeats <= 0)
        {
            return;
        }

        Meat meat = other.GetComponent<Meat>();

        // Ignora si no es una carne o si ya ha sido contada.
        if (meat == null || collectedMeatObjects.Contains(meat))
        {
            return;
        }

        float meatScore = meat.CalculateFinalScore();
        collectedScores.Add(meatScore);
        collectedMeatObjects.Add(meat);

        Debug.Log($"¡Carne entregada! Puntuación individual: {meatScore.ToString("F1")}. Trozos en el plato: {collectedScores.Count}/{expectedMeats}");

        if (collectedScores.Count >= expectedMeats)
        {
            StartCoroutine(CalculateAndShowScore());
        }
    }

    private IEnumerator CalculateAndShowScore()
    {
        if (collectedScores.Count == 0)
        {
            averageScore = 0f;
        }
        else
        {
            averageScore = collectedScores.Average();
        }

        isFinalScoreCalculated = true;
        Debug.Log($"¡Puntuación final del plato completada! Puntuación media: {averageScore.ToString("F1")} / 10.0");

        // Esperar un poco antes de mostrar el libro
        yield return new WaitForSeconds(0.5f);

        // Llamar al libro para que muestre el resultado
        if (questBookManager != null)
        {
            // Convertimos el score a un entero para mostrarlo, pero puedes cambiarlo si lo necesitas.
            questBookManager.ShowMinigameResult("Objetivo: sofreír", (int)averageScore, "¡Lo has clavado, Chef! Has manejado la plancha con una maestría increíble. El solomillo ha entrado en contacto con el calor extremo de la plancha, logrando ese sellado exterior dorado y crujiente que todo buen cocinero persigue. Has conseguido que las altas temperaturas caramelicen la superficie, creando una costra espectacular que actúa como un escudo. Esta costra sella todos los jugos internos del solomillo, asegurando que cada bocado sea de una ternura y jugosidad que te harán ganar la admiración de cualquier crítico culinario.");
        }
    }

    public float GetScore()
    {
        if (isFinalScoreCalculated)
        {
            return averageScore;
        }
        else
        {
            Debug.LogWarning("Se ha llamado a GetScore() pero la puntuación final aún no ha sido calculada.");
            return 0f;
        }
    }
}