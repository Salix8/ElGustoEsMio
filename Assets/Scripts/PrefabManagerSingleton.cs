using UnityEngine;

public class PrefabManagerSingleton : MonoBehaviour
{
    public static PrefabManagerSingleton Instance { get; private set; }

    [Header("Objeto seleccionado")]
    [Tooltip("Referencia al objeto actualmente seleccionado (se asigna mediante SetSeleccionado).")]
    public GameObject selectedObject;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        /*if (Input.GetMouseButtonDown(0))
        {
            Ray rayo = camara.ScreenPointToRay(Input.mousePosition);

        }*/
        if (Input.GetMouseButtonUp(0) && selectedObject != null)
        {
            selectedObject = null;
        }
    }

    public void SetSeleccionado(GameObject obj)
    {
        selectedObject = obj;
    }

    public bool HayObjetoSeleccionado()
    {
        return selectedObject != null;
    }

    public void ClearSeleccionado()
    {
        selectedObject = null;
    }
}