using UnityEngine;
using UnityEngine.Events; // Necesario para UnityEvent

/// <summary>
/// Un script reutilizable que permite que un objeto con Rigidbody
/// sea arrastrado con el ratón. La acción de 'Tap' se gestiona
/// a través del GrillManager en "modo espátula".
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class DraggableAndTappable : MonoBehaviour
{
	[Header("Eventos")]
	[Tooltip("Este evento se disparará cuando el objeto sea 'flipeado' con la espátula.")]
	public UnityEvent OnTap;

	[Header("Configuración")]
	[Tooltip("Cuánto se eleva el objeto al ser arrastrado.")]
	public Vector3 liftOffset = new Vector3(0, 0.2f, 0);

	// Variables privadas de control
	private Rigidbody rb;
	private Camera mainCamera;
	private float zCoord;
	private bool isDragging = false;
	private Vector3 originalPosition;
	private bool didFlipThisClick = false; // Flag para solucionar el bug

	void Start()
	{
		rb = GetComponent<Rigidbody>();
		mainCamera = Camera.main;
	}

	void OnMouseDown()
	{
		didFlipThisClick = false; // Reiniciar el flag en cada clic

		// Primero, comprobar si estamos en modo espátula
		if (GrillManager.Instance != null && GrillManager.Instance.isSpatulaModeActive)
		{
			didFlipThisClick = true; // Marcamos que este clic fue para voltear

			if (OnTap != null)
			{
				OnTap.Invoke(); // Llama a Meat.Flip()
			}

			GrillManager.Instance.isSpatulaModeActive = false;
			Debug.Log("Hamburguesa volteada. Modo Espátula DESACTIVADO.");
			return; 
		}

		// --- Si NO estamos en modo espátula, procedemos con el arrastre normal ---
		isDragging = false;
		originalPosition = transform.position;
		transform.position += liftOffset;
		zCoord = mainCamera.WorldToScreenPoint(transform.position).z;
	}

	void OnMouseDrag()
	{
		if (didFlipThisClick) return; // Si hemos flipeado, no arrastrar

		isDragging = true;
		rb.isKinematic = true;

		Vector3 mousePos = Input.mousePosition;
		mousePos.z = zCoord;
		transform.position = mainCamera.ScreenToWorldPoint(mousePos);
	}

	void OnMouseUp()
	{
		// Si este clic fue para voltear, no hacemos nada al levantar el ratón.
		if (didFlipThisClick)
		{
			return;
		}

		if (isDragging)
		{
			rb.isKinematic = false;
		}
		else
		{
			// Si no hubo arrastre, devolvemos el objeto a su posición.
			transform.position = originalPosition;
		}

		isDragging = false;
	}
}