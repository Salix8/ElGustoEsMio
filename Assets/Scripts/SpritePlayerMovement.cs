using UnityEngine;
using System.Collections;
using System.Linq;

public class SpritePlayerMovement : MonoBehaviour
{
    [Header("Referencias")]
    public Transform guia;
    public Camera camara;
    public LayerMask layerMask = ~0;
    public float maxRayDistance = 1000f;

    [Header("Feedback al arrastrar")]
    [SerializeField] private float liftHeight = 0.3f;
    [SerializeField] private float hoverShakeAmplitude = 0.05f;
    [SerializeField] private float hoverShakeFrequency = 12f;
    [SerializeField] private float tiltAmplitude = 6f;
    [SerializeField] private float tiltFrequency = 8f;
    [SerializeField] private float tiltLerpSpeed = 12f;
    [SerializeField] private float dropDuration = 0.18f;
    [SerializeField] private ParticleSystem dropParticlesPrefab;

    private Transform spriteSeleccionado;
    private Vector3 offset;
    private Bounds limites;
    private bool guiaTieneBounds3D = false;
    private bool guiaTieneBounds2D = false;
    private Bounds guiaBounds2D;

    private Quaternion rotationAntesDeArrastrar;
    private float shakeSeed;
    private Coroutine dropCoroutine;
    private Vector3 ultimoPuntoPlano;

    void Start()
    {
        if (camara == null) camara = Camera.main;
        if (camara == null) Debug.LogError("No has asignado la cámara y no hay Main Camera en la escena.");

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
                    Transform seleccionado = hit2d.transform;
                    Vector3 contacto = new Vector3(hit2d.point.x, transform.position.y, seleccionado.position.z);
                    ConfigurarSpriteSeleccionado(seleccionado, contacto);
                    Debug.Log($"Hit 2D: {seleccionado.name}");
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

                ultimoPuntoPlano = punto;
                spriteSeleccionado.position = punto + CalcularOffsetHover();
                spriteSeleccionado.rotation = Quaternion.Slerp(
                    spriteSeleccionado.rotation,
                    CalcularRotacionHover(),
                    tiltLerpSpeed * Time.deltaTime
                );
            }
        }

        // UP: soltar
        if (Input.GetMouseButtonUp(0) && spriteSeleccionado != null)
        {
            Debug.Log($"Soltado: {spriteSeleccionado.name}");
            Transform soltado = spriteSeleccionado;
            Vector3 destino = ultimoPuntoPlano;
            destino.y = transform.position.y;
            Quaternion rotDestino = rotationAntesDeArrastrar;

            spriteSeleccionado = null;
            dropCoroutine = StartCoroutine(DropAndImpact(soltado, destino, rotDestino));
        }
    }

    private void SeleccionarDesdeHit3D(RaycastHit hit)
    {
        ConfigurarSpriteSeleccionado(hit.transform, hit.point);
        Debug.Log($"Sprite seleccionado: {spriteSeleccionado.name} | offset {offset}");
        if(PrefabManagerSingleton.Instance != null)
            PrefabManagerSingleton.Instance.SetSeleccionado(spriteSeleccionado.gameObject);
    }

    private void ConfigurarSpriteSeleccionado(Transform sprite, Vector3 contacto)
    {
        if (dropCoroutine != null)
        {
            StopCoroutine(dropCoroutine);
            dropCoroutine = null;
        }

        spriteSeleccionado = sprite;
        offset = sprite.position - contacto;
        offset.y = 0f;

        rotationAntesDeArrastrar = sprite.rotation;
        shakeSeed = Random.value * Mathf.PI * 2f;
        ultimoPuntoPlano = new Vector3(sprite.position.x, transform.position.y, sprite.position.z);
    }

    private Vector3 CalcularOffsetHover()
    {
        float tiempo = Time.time + shakeSeed;
        float hoverY = liftHeight + Mathf.Sin(tiempo * hoverShakeFrequency) * hoverShakeAmplitude;
        float hoverX = Mathf.Sin(tiempo * hoverShakeFrequency * 0.9f) * hoverShakeAmplitude * 0.5f;
        float hoverZ = Mathf.Cos(tiempo * hoverShakeFrequency * 0.7f) * hoverShakeAmplitude * 0.5f;

        return new Vector3(hoverX, hoverY, hoverZ);
    }

    private Quaternion CalcularRotacionHover()
    {
        float tiempo = Time.time + shakeSeed;
        float tiltX = Mathf.Sin(tiempo * tiltFrequency) * tiltAmplitude;
        float tiltZ = Mathf.Cos(tiempo * tiltFrequency * 0.8f) * tiltAmplitude * 0.5f;

        return rotationAntesDeArrastrar * Quaternion.Euler(tiltX, 0f, tiltZ);
    }

    private IEnumerator DropAndImpact(Transform sprite, Vector3 destino, Quaternion rotacionDestino)
    {
        Vector3 inicioPos = sprite.position;
        Quaternion inicioRot = sprite.rotation;
        float duracion = Mathf.Max(0.01f, dropDuration);
        float tiempo = 0f;

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;
            float t = Mathf.Clamp01(tiempo / duracion);
            float eased = 1f - Mathf.Pow(1f - t, 3f);

            Vector3 nuevaPos = Vector3.Lerp(inicioPos, destino, eased);
            nuevaPos.y -= Mathf.Sin(t * Mathf.PI) * hoverShakeAmplitude * 0.5f;

            sprite.position = nuevaPos;
            sprite.rotation = Quaternion.Slerp(inicioRot, rotacionDestino, eased);
            yield return null;
        }

        sprite.position = destino;
        sprite.rotation = rotacionDestino;

        if (dropParticlesPrefab != null)
        {
            ParticleSystem ps = Instantiate(dropParticlesPrefab, destino, Quaternion.identity);
            ps.Play();
            var main = ps.main;
            float lifetime = main.duration + main.startLifetime.constantMax;
            Destroy(ps.gameObject, lifetime);
        }

        dropCoroutine = null;
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