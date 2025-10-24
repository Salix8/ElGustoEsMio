using UnityEngine;
using System.Collections;

public class SpritePatrol : MonoBehaviour
{
    [Header("Zona de patrulla")]
    public Transform patrolArea; // Objeto 3D que define los límites (por ejemplo, un cubo o plano)
    public float moveSpeed = 2f; // Velocidad de movimiento
    public float waitTime = 1f;  // Tiempo que esperan antes de elegir nuevo destino

    private Transform[] sprites;
    private Vector3[] targets;
    private bool[] waiting;
    private Vector3[] originalScales;
    private float altura;
    private Bounds patrolBounds;
    private bool asignado = false;

    void Start()
    {
        Asigaciones();
    }

    void Update()
    {
        if (!asignado){
            Asigaciones();
        }
        // Actualizar límites si el objeto cambia de tamaño o posición
        UpdatePatrolBounds();

        for (int i = 0; i < sprites.Length; i++)
        {
            if (sprites[i] == null || waiting[i]) continue;

            Transform sprite = sprites[i];
            Vector3 target = targets[i];

            // Mover sprite hacia el destino
            sprite.position = Vector3.MoveTowards(sprite.position, target, moveSpeed * Time.deltaTime);

            float deltaX = target.x - sprite.position.x;
            if (deltaX > -0.01f)
            {
                sprite.localScale = new Vector3(-Mathf.Abs(originalScales[i].x), originalScales[i].y, originalScales[i].z);
            }
            else if (deltaX < 0.01f)
            {
                sprite.localScale = new Vector3(Mathf.Abs(originalScales[i].x), originalScales[i].y, originalScales[i].z);
            }

            // Si ha llegado, esperar y elegir nuevo destino
            if (Vector3.Distance(sprite.position, target) < 0.1f)
            {
                StartCoroutine(SetNewTargetAfterDelay(i));
            }
        }
    }

    private void Asigaciones(){
        int count = transform.childCount;
        sprites = new Transform[count];
        targets = new Vector3[count];
        waiting = new bool[count];
        originalScales = new Vector3[count];

        for (int i = 0; i < count; i++)
        {
            sprites[i] = transform.GetChild(i);
            originalScales[i] = sprites[i].localScale;
        }

        // Tomar la altura del primer sprite
        altura = (count > 0) ? sprites[0].position.y : transform.position.y;

        // Obtener los límites reales del objeto de patrulla
        UpdatePatrolBounds();

        // Asignar destinos iniciales
        for (int i = 0; i < count; i++)
        {
            targets[i] = GetRandomPointInArea();
        }
        
        if(count != 0){
            asignado = true;
        }
    }

    IEnumerator SetNewTargetAfterDelay(int index)
    {
        waiting[index] = true;
        yield return new WaitForSeconds(waitTime);
        targets[index] = GetRandomPointInArea();
        waiting[index] = false;
    }

    void UpdatePatrolBounds()
    {
        if (patrolArea == null)
        {
            Debug.LogWarning("⚠️ No se asignó un objeto de patrulla.");
            return;
        }

        Renderer rend = patrolArea.GetComponent<Renderer>();
        if (rend != null)
        {
            patrolBounds = rend.bounds; // Esto usa la escala y posición reales
        }
        else
        {
            // Si no hay renderer (por ejemplo, un Empty con colliders), usar localScale como aproximación
            Vector3 center = patrolArea.position;
            Vector3 size = patrolArea.localScale;
            patrolBounds = new Bounds(center, size);
        }
    }

    Vector3 GetRandomPointInArea()
    {
        if (patrolArea == null) return transform.position;

        float x = Random.Range(patrolBounds.min.x, patrolBounds.max.x);
        float z = Random.Range(patrolBounds.min.z, patrolBounds.max.z);

        return new Vector3(x, altura, z);
    }
}
