using UnityEngine;

public class SpriteRotator : MonoBehaviour
{
    private Camera mainCamera;
    private Transform[] childSprites;

    private bool asignado = false;

    void Start()
    {
        Asignaciones();
    }

    private void Asignaciones(){
        mainCamera = Camera.main;

        int childCount = transform.childCount;
        childSprites = new Transform[childCount];
        for (int i = 0; i < childCount; i++)
            childSprites[i] = transform.GetChild(i);
        
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

        foreach (Transform sprite in childSprites)
        {
            if (sprite == null) continue;

            Vector3 lookDir = mainCamera.transform.position - sprite.position;
            lookDir.y = 0; // No rotar en el eje vertical
            sprite.rotation = Quaternion.LookRotation(-lookDir);
        }
    }
}
