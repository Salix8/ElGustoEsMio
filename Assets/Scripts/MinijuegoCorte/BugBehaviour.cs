using UnityEngine;

public class BugBehaviour : MonoBehaviour
{
    [Header("Movimiento")]
    public float moveAmount = 20f;
    public float moveSpeed = 2f;

    [Header("Refs")]
    public Animator animator;
    public SpriteRenderer spriteRenderer;

    Color originalColor;
    Vector3 startPos;

    PintarSobreCanvas activeCanvas;

    void Start()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

        originalColor = spriteRenderer.color;
        startPos = transform.localPosition;

        FindActiveFoodCanvas();
    }

    void Update()
    {
        MoveSideToSide();
        CheckCutZoneStatus();
    }

    void MoveSideToSide()
    {
        float offset = Mathf.Sin(Time.time * moveSpeed) * moveAmount;
        Vector3 pos = startPos;
        pos.x += offset;
        transform.localPosition = pos;

        animator.SetBool("Walking", true);
    }

    void FindActiveFoodCanvas()
    {
        PintarSobreCanvas[] all = FindObjectsOfType<PintarSobreCanvas>();
        foreach (var p in all)
        {
            if (p.gameObject.activeInHierarchy)
            {
                activeCanvas = p;
                return;
            }
        }
    }

    void CheckCutZoneStatus()
    {
        if (activeCanvas == null)
            FindActiveFoodCanvas();
        if (activeCanvas == null) return;

        // 🔴 ROJO si toca zonas malas
        foreach (bool z in activeCanvas.zoneEntered)
        {
            if (z)
            {
                SetColor(Color.red);
                return;
            }
        }

        // Si no hay zonas malas, volver a color original
        SetColor(originalColor);
    }

    void SetColor(Color c)
    {
        if (spriteRenderer.color != c)
            spriteRenderer.color = c;
    }
}
