using System.Collections;
using UnityEngine;
using TMPro;

public class NPC : MonoBehaviour
{
    [Header("Jugador")]
    public Transform player;

    [Header("Movimiento NPC")]
    public bool puedeMoverse = false;
    public Transform[] puntosMovimiento;
    public float velocidad = 2f;
    public float tiempoEsperaEntreMovimientos = 4f;

    private int puntoActual = 0;
    private bool moviendose = false;

    [Header("Detección del Player")]
    public float distanciaMinima = 1.5f;  // demasiado cerca
    public float distanciaObservacion = 4f; // distancia donde el NPC sospecha
    public float distanciaMaxima = 7f; // demasiado lejos

    [Header("Tiempo observando")]
    public float tiempoQuieto = 2f;

    [Header("Texto flotante")]
    public GameObject cuadroTexto;
    public TextMeshPro textoNPC;

    [TextArea]
    public string[] pensamientos;

    private bool pensando = false;

    void Start()
    {
        if (cuadroTexto != null)
            cuadroTexto.SetActive(false);

        if (puedeMoverse)
            StartCoroutine(RutinaMovimiento());
    }

    void Update()
    {
        if (player == null) return;

        float distancia = Vector2.Distance(transform.position, player.position);

        // Si el jugador está demasiado cerca
        if (distancia < distanciaMinima)
        {
            OcultarTexto();
            return;
        }

        // Si el jugador está demasiado lejos
        if (distancia > distanciaMaxima)
        {
            OcultarTexto();
            return;
        }

        // Distancia de observación
        if (distancia <= distanciaObservacion && !pensando)
        {
            StartCoroutine(ObservarJugador());
        }
    }

    IEnumerator ObservarJugador()
    {
        pensando = true;

        // NPC se queda quieto
        bool estadoMovimiento = moviendose;
        moviendose = false;

        yield return new WaitForSeconds(tiempoQuieto);

        MostrarTexto();

        yield return new WaitForSeconds(3f);

        OcultarTexto();

        moviendose = estadoMovimiento;
        pensando = false;
    }

    void MostrarTexto()
    {
        if (cuadroTexto == null || pensamientos.Length == 0) return;

        cuadroTexto.SetActive(true);

        int random = Random.Range(0, pensamientos.Length);
        textoNPC.text = pensamientos[random];
    }

    void OcultarTexto()
    {
        if (cuadroTexto != null)
            cuadroTexto.SetActive(false);
    }

    IEnumerator RutinaMovimiento()
    {
        while (true)
        {
            yield return new WaitForSeconds(tiempoEsperaEntreMovimientos);

            if (!puedeMoverse || puntosMovimiento.Length == 0) continue;

            moviendose = true;

            Transform destino = puntosMovimiento[puntoActual];

            while (Vector2.Distance(transform.position, destino.position) > 0.1f && moviendose)
            {
                transform.position = Vector2.MoveTowards(
                    transform.position,
                    destino.position,
                    velocidad * Time.deltaTime
                );

                yield return null;
            }

            moviendose = false;

            puntoActual++;

            if (puntoActual >= puntosMovimiento.Length)
                puntoActual = 0;
        }
    }
}