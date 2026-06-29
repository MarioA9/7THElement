using UnityEngine;

public class FlechaRitmo : MonoBehaviour
{
    public int direccion;
    public Transform hitLine;

    public MiniJuego_Ritmo juego;

    void Update()
    {
        transform.Translate(Vector3.down * juego.velocidad * Time.deltaTime);

        if (transform.position.y < hitLine.position.y - 200f)
        {
            Destroy(gameObject);
        }
    }
}