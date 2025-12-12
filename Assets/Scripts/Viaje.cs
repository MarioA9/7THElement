using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class Viaje : MonoBehaviour
{
    [Header("Referencias")]
    public GameObject textoUI;      // Texto que aparece al estar dentro
    public GameObject pantallaCarga; // Objeto que se activa 5 segundos
    public Transform puntoDestino;  // Lugar al que se teletransporta el jugador

    private bool jugadorDentro = false;
    private GameObject jugador;

    void Start()
    {
        // Asegura que inicien desactivados
        if (textoUI != null)
            textoUI.SetActive(false);

        if (pantallaCarga != null)
            pantallaCarga.SetActive(false);
    }

    void Update()
    {
        if (jugadorDentro && Input.GetKeyDown(KeyCode.F))
        {
            StartCoroutine(Viajar());
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorDentro = true;
            jugador = other.gameObject;

            if (textoUI != null)
                textoUI.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorDentro = false;
            jugador = null;

            if (textoUI != null)
                textoUI.SetActive(false);
        }
    }

    IEnumerator Viajar()
    {
        // Activa el objeto especial
        if (pantallaCarga != null)
            pantallaCarga.SetActive(true);

        // Teletransporta al jugador
        if (jugador != null && puntoDestino != null)
        {
            jugador.transform.position = puntoDestino.position;
        }

        // Espera 5 segundos
        yield return new WaitForSeconds(5f);

        // Desactiva el objeto nuevamente
        if (pantallaCarga != null)
            pantallaCarga.SetActive(false);
    }
}
