using UnityEngine;

public class BugBehaviour : MonoBehaviour
{
    [Header("Movimiento")]
    public float moveAmount = 20f;
    public float moveSpeed = 2f;

    [Header("Refs")]
    public Animator animator;
    public SpriteRenderer spriteRenderer;

    Material mat;
    Vector3 startPos;

    PintarSobreCanvas activeCanvas;

    void Start()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

        mat = spriteRenderer.material;
        startPos = transform.localPosition;

        FindActiveFoodCanvas();
    }

    void Update()
    {
        MoveSideToSide();
        CheckCutZoneStatus();
        UpdateAnimator();
        //Debug.Log("Anim params: Enfadado=" + animator.GetBool("Enfadado") + " velocidad=" + animator.GetFloat("velocidad"));

    }

    // ---------------------------------------------------------
    // Movimiento lateral (idle si moveAmount = 0)
    // ---------------------------------------------------------
    void MoveSideToSide()
    {
        if (moveAmount == 0)
        {
            // No moverse
            animator.SetFloat("Velocidad", 0f);
            transform.localPosition = startPos;
            return;
        }

        float offset = Mathf.Sin(Time.time * moveSpeed) * moveAmount;
        Vector3 pos = startPos;
        pos.x += offset;
        transform.localPosition = pos;

        animator.SetFloat("Velocidad", moveAmount);
    }

    // ---------------------------------------------------------
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

    // ---------------------------------------------------------
    void CheckCutZoneStatus()
    {
        if (activeCanvas == null)
            FindActiveFoodCanvas();
        if (activeCanvas == null) return;

        // Si toca zona mala → enfadado
        foreach (bool z in activeCanvas.zoneEntered)
        {
            if (z)
            {
                SetEnfadado(true);
                return;
            }
        }

        // Si no toca zona mala → no enfadado
        SetEnfadado(false);
    }

    // ---------------------------------------------------------
    void SetEnfadado(bool isAngry)
    {
        animator.SetBool("Enfadado", isAngry);

        if (isAngry)
        {
            mat.SetFloat("_Hue_Shift", 300f);
            mat.SetFloat("_Saturation", 1f);
            mat.SetFloat("_Contrast", 1f);
        }
        else
        {
            mat.SetFloat("_Hue_Shift", 0f);
            mat.SetFloat("_Saturation", 1f);
            mat.SetFloat("_Contrast", 1f);
        }
    }

    // ---------------------------------------------------------
    void UpdateAnimator()
    {
        // Aquí NO tocamos Enfadado porque ya lo maneja CutZoneStatus

        if (moveAmount == 0)
            animator.SetFloat("Velocidad", 0f);
        else
            animator.SetFloat("Velocidad", moveAmount);
    }
}
