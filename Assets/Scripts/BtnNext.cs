using UnityEngine;
using UnityEngine.UI;

public class BtnNext : MonoBehaviour
{
    public void OnClickNextButton()
    {
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
