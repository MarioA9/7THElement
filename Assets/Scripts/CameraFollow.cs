using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;   // El jugador
    public float smoothSpeed = 5f; // Velocidad de suavizado (opcional)

    private float fixedZ;

    void Start()
    {
        // Guarda la posición Z original de la cámara
        fixedZ = transform.position.z;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Nueva posición siguiendo solo X e Y
        Vector3 newPosition = new Vector3(
            target.position.x,
            target.position.y,
            fixedZ
        );

        // Movimiento suave
        transform.position = Vector3.Lerp(transform.position, newPosition, smoothSpeed * Time.deltaTime);
    }
}
