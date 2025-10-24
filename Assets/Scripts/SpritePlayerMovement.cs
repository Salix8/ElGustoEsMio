using UnityEngine;
using System.Linq;

public class SpritePlayerMovement : MonoBehaviour
{
    [Header("Referencias")]
    public Transform guia;             // Objeto guía que define límites (Collider o Collider2D)
    public Camera camara;              // Cámara usada (si no, toma Camera.main)
    public LayerMask layerMask = ~0;   // Capas a considerar en el raycast (por defecto todo)
    public float maxRayDistance = 1000f;

    private Transform spriteSeleccionado;
    private Vector3 offset;
    private Bounds limites;
    private bool guiaTieneBounds3D = false;
    private bool guiaTieneBounds2D = false;
    private Bounds guiaBounds2D;

    void Start()
    {
        if (camara == null) camara = Camera.main;
        if (camara == null) Debug.LogError("No has asignado la cámara y no hay Main Camera en la escena.");

        // Preparar bounds (3D o 2D)
        if (guia != null)
        {
            Collider col3 = guia.GetComponent<Collider>();
            if (col3 != null)
            {
                limites = col3.bounds;
                guiaTieneBounds3D = true;
            }
            else
            {
                Collider2D col2 = guia.GetComponent<Collider2D>();
                if (col2 != null)
                {
                    guiaBounds2D = col2.bounds;
                    guiaTieneBounds2D = true;
                }
                else
                {
                    Debug.LogWarning("El objeto 'guia' no tiene Collider ni Collider2D. No se aplicarán límites.");
                }
            }
        }
    }

    void Update()
    {
        // DOWN: intentamos seleccionar
        if (Input.GetMouseButtonDown(0))
        {
            if (camara == null) return;

            Ray rayo = camara.ScreenPointToRay(Input.mousePosition);
            Debug.DrawRay(rayo.origin, rayo.direction * 10f, Color.green, 2f);
            Debug.Log($"Ray origin {rayo.origin} dir {rayo.direction}");

            // 1) RaycastAll (3D) - buscar el primer hit que sea hijo del controller
            RaycastHit[] hits = Physics.RaycastAll(rayo, maxRayDistance, layerMask, QueryTriggerInteraction.Ignore);
            if (hits.Length > 0)
            {
                // Ordenar por distancia
                var ordered = hits.OrderBy(h => h.distance);
                foreach (var h in ordered)
                {
                    if (h.transform.IsChildOf(transform))
                    {
                        Debug.Log($"Hit 3D (IgnoreTriggers): {h.transform.name}");
                        SeleccionarDesdeHit3D(h);
                        break;
                    }
                }
            }

            // 2) Si no seleccionamos nada, reintentar incluyendo triggers
            if (spriteSeleccionado == null)
            {
                RaycastHit[] hitsTriggers = Physics.RaycastAll(rayo, maxRayDistance, layerMask, QueryTriggerInteraction.Collide);
                if (hitsTriggers.Length > 0)
                {
                    var ordered2 = hitsTriggers.OrderBy(h => h.distance);
                    foreach (var h in ordered2)
                    {
                        if (h.transform.IsChildOf(transform))
                        {
                            Debug.Log($"Hit 3D (IncludeTriggers): {h.transform.name} (trigger?)");
                            SeleccionarDesdeHit3D(h);
                            break;
                        }
                    }
                }
            }

            // 3) Fallback 2D (si usas sprites con Collider2D) - raycast en punto
            if (spriteSeleccionado == null)
            {
                Vector3 mouseWorld = camara.ScreenToWorldPoint(Input.mousePosition);
                // Asumimos z del mundo = 0; si trabajas en X-Z adapta esto.
                Vector2 mouseWorld2D = new Vector2(mouseWorld.x, mouseWorld.y);
                RaycastHit2D hit2d = Physics2D.Raycast(mouseWorld2D, Vector2.zero, 0f, layerMask);
                if (hit2d.collider != null && hit2d.transform.IsChildOf(transform))
                {
                    Debug.Log($"Hit 2D: {hit2d.transform.name}");
                    spriteSeleccionado = hit2d.transform;
                    offset = spriteSeleccionado.position - (Vector3)mouseWorld2D;
                }
            }

            if (spriteSeleccionado == null)
            {
                Debug.Log("No se seleccionó ningún hijo. Revisa: colliders, capas (layerMask) o si son 2D/3D.");
            }
        }

        // DRAG: actualizar la posición si hay seleccionado
        if (Input.GetMouseButton(0) && spriteSeleccionado != null)
        {
            // Mover en plano horizontal a la altura del padre (X-Z) por defecto
            Ray rayo = camara.ScreenPointToRay(Input.mousePosition);

            // Usamos el plano horizontal a la altura del padre
            Plane planoMovimiento = new Plane(Vector3.up, new Vector3(0, transform.position.y, 0));
            if (planoMovimiento.Raycast(rayo, out float distancia))
            {
                Vector3 punto = rayo.GetPoint(distancia) + offset;
                punto.y = transform.position.y;

                // Aplicar limites 3D (X-Z)
                if (guiaTieneBounds3D)
                {
                    punto.x = Mathf.Clamp(punto.x, limites.min.x, limites.max.x);
                    punto.z = Mathf.Clamp(punto.z, limites.min.z, limites.max.z);
                }
                // Si guía es 2D (X-Y), lo mapeamos a X-Z manteniendo Z del padre
                else if (guiaTieneBounds2D)
                {
                    punto.x = Mathf.Clamp(punto.x, guiaBounds2D.min.x, guiaBounds2D.max.x);
                    float yClamped = Mathf.Clamp(punto.y, guiaBounds2D.min.y, guiaBounds2D.max.y);
                    // si usas X-Y, yClamped debería asignarse a punto.y en lugar de Y del padre;
                    // aquí mantenemos la Y del padre como pediste originalmente:
                    // punto.y = transform.position.y;
                    // si en cambio quieres mover en X-Y cambia la línea anterior.
                }

                spriteSeleccionado.position = punto;
            }
        }

        // UP: soltar
        if (Input.GetMouseButtonUp(0) && spriteSeleccionado != null)
        {
            Debug.Log($"Soltado: {spriteSeleccionado.name}");
            spriteSeleccionado = null;
        }
    }

    private void SeleccionarDesdeHit3D(RaycastHit hit)
    {
        spriteSeleccionado = hit.transform;
        offset = spriteSeleccionado.position - hit.point;
        Debug.Log($"Sprite seleccionado: {spriteSeleccionado.name} | offset {offset}");
    }

    void OnDrawGizmosSelected()
    {
        if (guia == null) return;
        Gizmos.color = Color.cyan;
        Collider col = guia.GetComponent<Collider>();
        if (col != null)
        {
            Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
        }
        else
        {
            Collider2D col2 = guia.GetComponent<Collider2D>();
            if (col2 != null)
                Gizmos.DrawWireCube(col2.bounds.center, col2.bounds.size);
        }
    }
}