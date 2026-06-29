using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Dialogo_MuñecoNieve : MonoBehaviour
{
    [Header("Activación")]
    public bool activarAutomatico = false;
    public KeyCode teclaActivacion = KeyCode.F;

    [Header("UI")]
    public GameObject panelDialogo;
    public TMP_Text textoDialogo;
    public Button botonInteractuar;
    public Button botonSalir;

    [Header("Texto")]
    [TextArea(2, 4)]
    public string textoInicial = "Que extraño, parece que alguien hizo un muñeco de nieve. Vere si puedo conseguir algo util aqui.";

    [TextArea(2, 4)]
    public string textoPocionCura = "¡Encontraste una poción de cura!";
    [TextArea(2, 4)]
    public string textoPocionFuerza = "¡Encontraste una poción de fuerza!";
    [TextArea(2, 4)]
    public string textoPocionDefensa = "¡Encontraste una poción de defensa!";
    [TextArea(2, 4)]
    public string textoGolpeNieve = "Ay! Pero que- Ash... muñeco estupido...";

    [Header("Configuración")]
    public float velocidadEscritura = 0.03f;
    public float tiempoResultado = 1.5f;
    public float dañoBolaNieve = 10f;
    public bool destruirAlTerminar = false;

    [Header("Pociones obtenidas por este objeto")]
    public int pocionCura = 0;
    public int pocionFuerza = 0;
    public int pocionDefensa = 0;

    private Player player;
    private float velocidadOriginal;
    private bool jugadorDentro = false;
    private bool dialogoAbierto = false;
    private bool esperandoDecision = false;
    private bool escribiendoInicial = false;

    private Coroutine rutinaEscritura;
    private Coroutine rutinaCierre;

    void Start()
    {
        if (panelDialogo != null)
            panelDialogo.SetActive(false);

        if (botonInteractuar != null)
        {
            botonInteractuar.gameObject.SetActive(false);
            botonInteractuar.onClick.AddListener(OpcionInteractuar);
        }

        if (botonSalir != null)
        {
            botonSalir.gameObject.SetActive(false);
            botonSalir.onClick.AddListener(OpcionSalir);
        }

        player = FindObjectOfType<Player>();
        if (player != null)
            velocidadOriginal = player.moveSpeed;
    }

    void Update()
    {
        if (jugadorDentro && !dialogoAbierto && Input.GetKeyDown(teclaActivacion))
        {
            IniciarDialogo();
        }

        if (dialogoAbierto && escribiendoInicial && Input.GetMouseButtonDown(0))
        {
            MostrarTextoInicialCompleto();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorDentro = true;

            if (activarAutomatico && !dialogoAbierto)
                IniciarDialogo();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            jugadorDentro = false;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            jugadorDentro = true;

            if (activarAutomatico && !dialogoAbierto)
                IniciarDialogo();
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
            jugadorDentro = false;
    }

    void IniciarDialogo()
    {
        if (panelDialogo == null || textoDialogo == null)
            return;

        dialogoAbierto = true;
        esperandoDecision = false;
        escribiendoInicial = true;

        BloquearJugador(true);

        panelDialogo.SetActive(true);
        ActivarBotones(false);

        if (rutinaCierre != null)
        {
            StopCoroutine(rutinaCierre);
            rutinaCierre = null;
        }

        if (rutinaEscritura != null)
            StopCoroutine(rutinaEscritura);

        rutinaEscritura = StartCoroutine(EscribirTextoInicial());
    }

    IEnumerator EscribirTextoInicial()
    {
        textoDialogo.text = "";

        foreach (char c in textoInicial)
        {
            textoDialogo.text += c;
            yield return new WaitForSeconds(velocidadEscritura);
        }

        escribiendoInicial = false;
        esperandoDecision = true;
        ActivarBotones(true);
    }

    void MostrarTextoInicialCompleto()
    {
        if (rutinaEscritura != null)
            StopCoroutine(rutinaEscritura);

        textoDialogo.text = textoInicial;
        escribiendoInicial = false;
        esperandoDecision = true;
        ActivarBotones(true);
    }

    void ActivarBotones(bool activar)
    {
        if (botonInteractuar != null)
            botonInteractuar.gameObject.SetActive(activar);

        if (botonSalir != null)
            botonSalir.gameObject.SetActive(activar);
    }

    public void OpcionInteractuar()
    {
        if (!esperandoDecision)
            return;

        esperandoDecision = false;
        ActivarBotones(false);

        int azar = Random.Range(0, 4); // 0,1,2 = pociones / 3 = bola de nieve

        switch (azar)
        {
            case 0:
                pocionCura++;
                MostrarResultadoYCerrar(textoPocionCura);
                break;

            case 1:
                pocionFuerza++;
                MostrarResultadoYCerrar(textoPocionFuerza);
                break;

            case 2:
                pocionDefensa++;
                MostrarResultadoYCerrar(textoPocionDefensa);
                break;

            default:
                if (player != null)
                    player.TakeDamage(dañoBolaNieve);

                MostrarResultadoYCerrar(textoGolpeNieve);
                break;
        }
    }

    public void OpcionSalir()
    {
        if (!dialogoAbierto)
            return;

        CerrarDialogo();
    }

    void MostrarResultadoYCerrar(string mensaje)
    {
        if (rutinaCierre != null)
            StopCoroutine(rutinaCierre);

        rutinaCierre = StartCoroutine(EsperarYCerrar(mensaje));
    }

    IEnumerator EsperarYCerrar(string mensaje)
    {
        textoDialogo.text = mensaje;
        yield return new WaitForSeconds(tiempoResultado);
        CerrarDialogo();
    }

    void CerrarDialogo()
    {
        dialogoAbierto = false;
        esperandoDecision = false;
        escribiendoInicial = false;

        ActivarBotones(false);
        BloquearJugador(false);

        if (panelDialogo != null)
            panelDialogo.SetActive(false);

        if (destruirAlTerminar)
            Destroy(gameObject);
    }

    void BloquearJugador(bool bloquear)
    {
        if (player == null)
            return;

        if (bloquear)
        {
            player.enabled = false;
            player.moveSpeed = 0f;
        }
        else
        {
            player.enabled = true;
            player.moveSpeed = velocidadOriginal;
        }
    }
}