using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class BtnNext : MonoBehaviour
{
    public void OnClickNextButton()
    {
        // Verificar si estamos en la escena "Final"
        string escenaActual = SceneManager.GetActiveScene().name;
        
        if (escenaActual == "Final")
        {
            // Finalizar el juego completo
            if (MinigameProgressManager.Instance != null)
            {
                Debug.Log("Escena 'Final' detectada. Finalizando juego completo...");
                MinigameProgressManager.Instance.FinalizarJuego("PantallaFinal");
            }
            else
            {
                Debug.LogError("No se encontró la instancia de MinigameProgressManager.");
            }
            return;
        }
        
        // Lógica normal para otros minijuegos
        MinijuegoFinalizador finalizador = FindObjectOfType<MinijuegoFinalizador>();
        if (finalizador == null)
        {
            Debug.LogError("No se encontró el objeto MinijuegoFinalizador en la escena.");
            return;
        }

        if (QuestBookManager.Instance == null)
        {
            Debug.LogError("No se encontró la instancia de QuestBookManager. ¿Está en la escena?");
            return;
        }
        int puntos = QuestBookManager.Instance.LastShownScore;

        Debug.Log($"Botón 'Siguiente' pulsado. Pasando puntuación: {puntos} al finalizador.");
        finalizador.CompletarMinijuego(puntos);
    }
}
