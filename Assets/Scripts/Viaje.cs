using System.Collections;
using UnityEngine;

public class Viaje : MonoBehaviour
{
    [Header("Referencias")]
    public GameObject textoUI;
    public GameObject pantallaCarga;
    public Transform puntoDestino;

    private bool jugadorDentro = false;
    private bool viajando = false;

    private GameObject jugador;
    private Rigidbody2D jugadorRB;

    void Start()
    {
        if (textoUI != null)
            textoUI.SetActive(false);

        if (pantallaCarga != null)
            pantallaCarga.SetActive(false);
    }

    void Update()
    {
        if (jugadorDentro && !viajando && Input.GetKeyDown(KeyCode.F))
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
            jugadorRB = jugador.GetComponent<Rigidbody2D>();

            if (textoUI != null)
                textoUI.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorDentro = false;

            if (textoUI != null)
                textoUI.SetActive(false);
        }
    }

    IEnumerator Viajar()
    {
        viajando = true;

        if (pantallaCarga != null)
            pantallaCarga.SetActive(true);

        yield return new WaitForSeconds(0.1f);

        if (jugador != null && puntoDestino != null)
        {
            if (jugadorRB != null)
                jugadorRB.position = puntoDestino.position;
            else
                jugador.transform.position = puntoDestino.position;
        }

        yield return new WaitForSeconds(5f);

        if (pantallaCarga != null)
            pantallaCarga.SetActive(false);

        viajando = false;
    }
}