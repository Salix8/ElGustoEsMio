using UnityEngine;

public class SpriteGen : MonoBehaviour
{
    [Header("Parámetros de generación")]
    public GameObject prefab;       // Prefab del sprite u objeto a generar
    public Transform guideObject;   // Objeto guía (por ejemplo, un cubo o plano)
    public int amount = 10;         // Número de objetos a generar

    private Bounds guideBounds;
    private float altura;           // Altura definida por el objeto padre

    void Start()
    {
        if (prefab == null || guideObject == null)
        {
            Debug.LogError("⚠️ Faltan referencias: asigna el prefab y el objeto guía.");
            return;
        }

        // Obtener altura del objeto padre
        altura = transform.position.y;

        // Calcular límites reales del objeto guía
        UpdateGuideBounds();

        // Generar los objetos
        GenerateObjects();
    }

    void UpdateGuideBounds()
    {
        Renderer rend = guideObject.GetComponent<Renderer>();
        if (rend != null)
        {
            guideBounds = rend.bounds; // Tamaño real considerando escala y posición
        }
        else
        {
            // Si el objeto guía no tiene renderer (por ejemplo, es un Empty)
            // usamos localScale como aproximación
            Vector3 center = guideObject.position;
            Vector3 size = guideObject.localScale;
            guideBounds = new Bounds(center, size);
        }
    }

    void GenerateObjects()
    {
        for (int i = 0; i < amount; i++)
        {
            Vector3 randomPos = GetRandomPositionInBounds();
            GameObject obj = Instantiate(prefab, randomPos, Quaternion.identity, transform);
            obj.name = prefab.name + "_" + i;
        }
    }

    Vector3 GetRandomPositionInBounds()
    {
        float x = Random.Range(guideBounds.min.x, guideBounds.max.x);
        float z = Random.Range(guideBounds.min.z, guideBounds.max.z);
        return new Vector3(x, altura, z);
    }
}
