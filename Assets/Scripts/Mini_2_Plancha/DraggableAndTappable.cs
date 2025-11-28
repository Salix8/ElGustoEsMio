using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Un script reutilizable que permite que un objeto con Rigidbody sea arrastrado.
/// También gestiona la interacción de volteo cuando el modo espátula está activo.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Meat))] // Aseguramos que siempre haya un componente Meat
public class DraggableAndTappable : MonoBehaviour
{
	[Header("Configuración")]
	[Tooltip("Cuánto se eleva el objeto al ser arrastrado.")]
	public Vector3 liftOffset = new Vector3(0, 0.2f, 0);

	// Variables privadas de control
	private Rigidbody rb;
	private Camera mainCamera;
	private float zCoord;
	private bool isDragging = false;
	private Vector3 originalPosition;
	private Meat meatComponent;
	private bool flipWasInitiated = false; // BUGFIX: Para no arrastrar si se ha iniciado un volteo.

	void Start()
	{
		rb = GetComponent<Rigidbody>();
		mainCamera = Camera.main;
		meatComponent = GetComponent<Meat>();
	}

	void OnMouseDown()
	{
		// Por defecto, no se ha iniciado un volteo.
		flipWasInitiated = false;

		// Primero, comprobar si estamos en modo espátula.
		if (GrillManager.Instance != null && GrillManager.Instance.isSpatulaModeActive)
		{
			// Si es así, marcamos que el volteo se ha iniciado.
			flipWasInitiated = true;
			// Llamamos al GrillManager para que se encargue de la lógica de volteo.
			GrillManager.Instance.FlipMeatWithSpatula(meatComponent);
			return; // Salimos para no iniciar el arrastre.
		}

		// --- Si no estamos en modo espátula, procedemos con el arrastre normal ---
		isDragging = false;
		originalPosition = transform.position;
		transform.position += liftOffset;
		zCoord = mainCamera.WorldToScreenPoint(transform.position).z;
	}

	void OnMouseDrag()
	{
		// Solo permitir el arrastre si NO se inició un volteo en este clic.
		if (!flipWasInitiated)
		{
			isDragging = true;
			rb.isKinematic = true;

			Vector3 mousePos = Input.mousePosition;
			mousePos.z = zCoord;
			transform.position = mainCamera.ScreenToWorldPoint(mousePos);
		}
	}

	void OnMouseUp()
	{
		// Si se inició un volteo, no hacemos nada aquí.
		if (flipWasInitiated)
		{
			flipWasInitiated = false; // Reseteamos para el próximo clic.
			return;
		}

		// Si no, procedemos con la lógica normal de soltar el objeto.
		if (isDragging)
		{
			rb.isKinematic = false;
		}
		else
		{
			// Si no hubo arrastre, es un 'tap'.
			// Revertimos el pequeño salto inicial.
			transform.position = originalPosition;
		}
		
		// Se restaura el estado de 'no arrastre' al final de cualquier interacción.
		isDragging = false;
	}
}