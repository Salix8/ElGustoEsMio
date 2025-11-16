using UnityEngine;
using UnityEngine.Events; // Necesario para UnityEvent

/// <summary>
/// Un script reutilizable que permite que un objeto con Rigidbody
/// sea arrastrado con el ratón y que detecte un "Tap".
/// Distingue entre un clic corto (Tap) y un clic largo (Drag).
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class DraggableAndTappable : MonoBehaviour
{
	[Header("Eventos")]
	[Tooltip("Este evento se disparará cuando se haga un 'Tap' (clic corto).")]
	public UnityEvent OnTap;

	[Header("Configuración")]
	[Tooltip("El tiempo máximo (en segundos) para que un clic sea considerado 'Tap'.")]
	public float tapThreshold = 0.25f;

	// Variables privadas de control
	private Rigidbody rb;
	private Camera mainCamera;
	private float zCoord;
	private float mouseDownTime;
	private bool isDragging = false;
	private Vector3 mouseStartPos;
	private Vector3 objStartPos;

	void Start()
	{
		rb = GetComponent<Rigidbody>();
		mainCamera = Camera.main; // Cachear la cámara principal
	}

	void OnMouseDown()
	{
		// Guardar el momento y la posición del clic
		mouseDownTime = Time.time;
		isDragging = false;

		// Calcular la coordenada Z para el movimiento del ratón
		zCoord = mainCamera.WorldToScreenPoint(transform.position).z;
	}

	void OnMouseDrag()
	{
		// Si se mueve el ratón, es un 'Drag'
		isDragging = true;

		// Hacemos el Rigidbody kinemático para que no choque mientras lo movemos
		rb.isKinematic = true;

		// Convertir la posición del ratón en pantalla a posición en el mundo
		Vector3 mousePos = Input.mousePosition;
		mousePos.z = zCoord;

		transform.position = mainCamera.ScreenToWorldPoint(mousePos);
	}

	void OnMouseUp()
	{
		if (isDragging)
		// Si estábamos arrastrando, devolvemos el Rigidbody a la normalidad
		{
			rb.isKinematic = false;
		}

		else
		{
			// Si NO estábamos arrastrando (fue un clic estático)...
			// ...comprobamos si fue un 'Tap' (suficientemente rápido)
			if (Time.time - mouseDownTime <= tapThreshold)
			{
				// ¡Fue un Tap! Disparamos el evento.
				if (OnTap != null)
				{
					OnTap.Invoke();
				}
			}
		}
	}
}