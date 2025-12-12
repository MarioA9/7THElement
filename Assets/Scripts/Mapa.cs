using UnityEngine;
using UnityEngine.Serialization;

public class Mapa : MonoBehaviour
{
    public GameObject mapaActivate; // Objeto que se activará/desactivará

    void Start()
    {
        // Asegura que comience desactivado
        if (mapaActivate != null)
            mapaActivate.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (mapaActivate != null)
                mapaActivate.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (mapaActivate != null)
                mapaActivate.SetActive(false);
        }
    }
}