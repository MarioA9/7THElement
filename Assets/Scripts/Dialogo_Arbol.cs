using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Dialogo_Arbol : MonoBehaviour
{
    [Header("Activación")]
    public bool activarAutomatico = false;
    public KeyCode teclaActivacion = KeyCode.F;

    [Header("UI")]
    public GameObject panelDialogo;
    public TMP_Text textoDialogo;
    public Button botonSacudir;
    public Button botonSalir;

    [Header("Texto")]
    [TextArea(2, 4)]
    public string textoInicial = "Hmmm, no si se podre conseguir algo en estos arboles, debere intertarlo!";

    [TextArea(2, 4)]
    public string textoManzana = "¡Encontraste una manzana!";
    [TextArea(2, 4)]
    public string textoNaranja = "¡Encontraste una naranja!";
    [TextArea(2, 4)]
    public string textoPera = "¡Encontraste una pera!";
    [TextArea(2, 4)]
    public string textoRama = "¡Una rama cayó sobre ti!";

    [Header("Configuración")]
    public float velocidadEscritura = 0.03f;
    public float tiempoResultado = 1.5f;
    public float dañoRama = 10f;
    public bool destruirAlTerminar = false;

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

        if (botonSacudir != null)
        {
            botonSacudir.gameObject.SetActive(false);
            botonSacudir.onClick.AddListener(OpcionSacudir);
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
        if (botonSacudir != null)
            botonSacudir.gameObject.SetActive(activar);

        if (botonSalir != null)
            botonSalir.gameObject.SetActive(activar);
    }

    public void OpcionSacudir()
    {
        if (!esperandoDecision)
            return;

        esperandoDecision = false;
        ActivarBotones(false);

        int azar = Random.Range(0, 4); // 0,1,2 = fruta / 3 = rama

        switch (azar)
        {
            case 0:
                if (player != null) player.manzana++;
                MostrarResultadoYCerrar(textoManzana);
                break;

            case 1:
                if (player != null) player.naranja++;
                MostrarResultadoYCerrar(textoNaranja);
                break;

            case 2:
                if (player != null) player.pera++;
                MostrarResultadoYCerrar(textoPera);
                break;

            default:
                if (player != null)
                    player.TakeDamage(dañoRama);

                MostrarResultadoYCerrar(textoRama);
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