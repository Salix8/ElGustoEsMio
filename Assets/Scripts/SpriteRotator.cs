using UnityEngine;

public class SpriteRotator : MonoBehaviour
{
    private Camera mainCamera;
    private Transform[] childSprites;
    private float[] noiseSeeds;
    private bool asignado = false;

    [SerializeField] private float rotationLerpSpeed = 5f;
    [SerializeField] private float maxYawOffset = 12f;
    [SerializeField] private float noiseSpeed = 0.8f;

    void Start()
    {
        Asignaciones();
    }

    private void Asignaciones(){
        mainCamera = Camera.main;

        int childCount = transform.childCount;
        childSprites = new Transform[childCount];
        noiseSeeds = new float[childCount];
        for (int i = 0; i < childCount; i++)
        {
            childSprites[i] = transform.GetChild(i);
            noiseSeeds[i] = Random.value * 100f;
        }
        
        if (childCount != 0){
            asignado = true;
        }
    }

    void LateUpdate()
    {
        if(!asignado){
            Asignaciones();
        }
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null) return;
        }

        for (int i = 0; i < childSprites.Length; i++)
        {
            Transform sprite = childSprites[i];
            if (sprite == null) continue;

            Vector3 lookDir = mainCamera.transform.position - sprite.position;
            lookDir.y = 0f;
            if (lookDir.sqrMagnitude < Mathf.Epsilon) continue;

            lookDir.Normalize();

            float yawNoise = Mathf.Sin(Time.time * noiseSpeed + noiseSeeds[i]) * maxYawOffset;
            Quaternion noiseRotation = Quaternion.AngleAxis(yawNoise, Vector3.up);
            Vector3 noisyForward = noiseRotation * (-lookDir);

            Quaternion targetRotation = Quaternion.LookRotation(noisyForward);
            sprite.rotation = Quaternion.Slerp(sprite.rotation, targetRotation, rotationLerpSpeed * Time.deltaTime);
        }
    }
}
