using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Dialogo_Ritmo : MonoBehaviour
{
    [Header("Activación del diálogo")]
    public bool activarAutomatico = false;
    public KeyCode teclaActivacion = KeyCode.F;

    [Header("Activación Elemental Opcional")]
    public bool usarElemental = false;
    public GameObject objetoElemental;
    private bool elementalYaUsado = false;

    [Header("UI del diálogo")]
    public GameObject panelDialogo;
    public TMP_Text textoDialogo;
    public Image imagenDialogo;

    [Header("Configuración de escritura")]
    public float velocidadEscritura = 0.03f;

    // ==========================================================
    // NUEVOS CAMPOS
    // ==========================================================

    [Header("¿No destruir este objeto al terminar?")]
    public bool noDestruirAlTerminar = false;

    [Header("Este diálogo ya fue activado antes (solo si Voz = TRUE)")]
    public bool dialogoYaActivado = false;

    [Header("Diálogo si Voz = TRUE (primera vez)")]
    public Dialogo[] dialogosVozOn;

    [Header("Diálogo si Voz = TRUE (ya activado previamente)")]
    public Dialogo[] dialogosVozOn_Repetido;

    [Header("Diálogo si Voz = FALSE")]
    public Dialogo[] dialogosVozOff;

    private Dialogo[] dialogosActuales;


    [System.Serializable]
    public class Dialogo
    {
        [TextArea] public string texto;
        public Sprite imagen;
        public bool usarImagen;

        public bool activarMiniJuegoRitmo;
    }

    private int index = 0;
    private bool escribiendo = false;
    private bool jugadorDentro = false;

    // Player
    private Player player;
    private float velocidadOriginal;
    private bool bloqueoDialogo = false;

    [Header("MiniJuego Ritmo")]
    bool iniciarMiniJuegoDespues = false;
    public MiniJuego_Ritmo miniJuegoRitmo;

    [Header("Dialogos si fallas el ritmo")]
    public Dialogo[] dialogoFalloVoz;
    public Dialogo[] dialogoFalloSinVoz;

    bool minijuegoGanado = false;

    void Start()
    {
        panelDialogo.SetActive(false);

        player = FindObjectOfType<Player>();
        if (player != null)
            velocidadOriginal = player.moveSpeed;

        if (objetoElemental != null)
            objetoElemental.SetActive(false);
    }

    void Update()
    {
        if (jugadorDentro)
        {
            if (!panelDialogo.activeSelf && !activarAutomatico)
            {
                // ELEMENTAL
                if (usarElemental && !elementalYaUsado)
                {
                    if (Input.GetKeyDown(teclaActivacion))
                    {
                        StartCoroutine(IniciarElemental());
                        return;
                    }
                }

                // NORMAL
                if (!usarElemental && Input.GetKeyDown(teclaActivacion))
                {
                    IniciarDialogoSegunVozYEstados();
                }
            }

            // CONTROL DE DIÁLOGO
            if (panelDialogo.activeSelf)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    if (escribiendo)
                    {
                        StopAllCoroutines();
                        textoDialogo.text = dialogosActuales[index].texto;
                        escribiendo = false;
                    }
                    else
                    {
                        SiguienteDialogo();
                    }
                }
            }
        }

        // Forzar Idle si está bloqueado
        if (bloqueoDialogo && player != null)
        {
            player.GetComponent<Animator>().Play("Idle");
        }
    }

    // ==========================================================
    // TRIGGERS
    // ==========================================================
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            jugadorDentro = true;

            if (activarAutomatico)
                IniciarDialogoSegunVozYEstados();
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
            jugadorDentro = false;
    }

    // ==========================================================
    // PROCESO ELEMENTAL
    // ==========================================================
    IEnumerator IniciarElemental()
    {
        elementalYaUsado = true;

        // Bloquear jugador
        if (player != null)
        {
            player.enabled = false;
            player.moveSpeed = 0;
            bloqueoDialogo = true;
        }

        if (objetoElemental != null)
            objetoElemental.SetActive(true);

        yield return new WaitForSeconds(4f);

        if (objetoElemental != null)
            objetoElemental.SetActive(false);

        IniciarDialogoSegunVozYEstados();
    }

    // ==========================================================
    // SELECCIÓN DE DIÁLOGO
    // ==========================================================
    void IniciarDialogoSegunVozYEstados()
    {
        if (player == null) return;

        // --- Si VOZ = true ---
        if (player.Voz)
        {
            // Si ya fue activado antes → usar el otro conjunto
            if (dialogoYaActivado && dialogosVozOn_Repetido.Length > 0)
            {
                dialogosActuales = dialogosVozOn_Repetido;
            }
            else
            {
                dialogosActuales = dialogosVozOn;
            }
        }
        else
        {
            // --- Si VOZ = false ---
            dialogosActuales = dialogosVozOff;
        }

        if (dialogosActuales == null || dialogosActuales.Length == 0)
        {
            Debug.LogWarning("No hay diálogos configurados.");
            return;
        }

        IniciarDialogo();
    }

    // ==========================================================
    // INICIAR DIALOGO REAL
    // ==========================================================
    void IniciarDialogo()
    {
        index = 0;

        panelDialogo.SetActive(true);

        if (player != null)
        {
            player.enabled = false;
            player.moveSpeed = 0;
            bloqueoDialogo = true;
        }

        MostrarDialogoActual();
    }

    // ==========================================================
    // MOSTRAR TEXTO
    // ==========================================================
    void MostrarDialogoActual()
    {
        textoDialogo.text = "";

        if (dialogosActuales[index].usarImagen && dialogosActuales[index].imagen != null)
        {
            imagenDialogo.sprite = dialogosActuales[index].imagen;
            imagenDialogo.gameObject.SetActive(true);
        }
        else
        {
            imagenDialogo.gameObject.SetActive(false);
        }

        // Marcar que el minijuego debe empezar cuando se avance el diálogo
        if (dialogosActuales[index].activarMiniJuegoRitmo)
        {
            iniciarMiniJuegoDespues = true;
        }

        StartCoroutine(Escribir(dialogosActuales[index].texto));
    }

    IEnumerator Escribir(string frase)
    {
        escribiendo = true;
        textoDialogo.text = "";

        foreach (char c in frase)
        {
            textoDialogo.text += c;
            yield return new WaitForSeconds(velocidadEscritura);
        }

        escribiendo = false;
    }

    // ==========================================================
    // SIGUIENTE DIÁLOGO
    // ==========================================================
    void SiguienteDialogo()
    {
        // Si el diálogo anterior activaba minijuego
        if (iniciarMiniJuegoDespues)
        {
            iniciarMiniJuegoDespues = false;

            panelDialogo.SetActive(false);

            if (miniJuegoRitmo != null)
            {
                miniJuegoRitmo.IniciarJuego(this);
            }

            return;
        }

        index++;

        if (index >= dialogosActuales.Length)
        {
            FinalizarDialogo();
            return;
        }

        MostrarDialogoActual();
    }

    // ==========================================================
    // FINALIZAR
    // ==========================================================
    void FinalizarDialogo()
    {
        panelDialogo.SetActive(false);

        // Guardar que ya se activó un diálogo VOZ = TRUE
        if (player.Voz)
            dialogoYaActivado = true;

        // Restaurar control del jugador
        if (player != null)
        {
            player.enabled = true;
            player.moveSpeed = velocidadOriginal;
            bloqueoDialogo = false;
        }

        // Destruir solo si no está marcado "noDestruirAlTerminar"
        // Solo destruir si el minijuego fue ganado
        if (!noDestruirAlTerminar && minijuegoGanado)
        {
            Destroy(gameObject);
        }
    }

    public void ContinuarDespuesMiniJuego()
    {
        panelDialogo.SetActive(true);

        index++;

        if (index >= dialogosActuales.Length)
        {
            FinalizarDialogo();
        }
        else
        {
            MostrarDialogoActual();
        }
    }

    public void ResultadoMiniJuego(bool gano)
    {
        minijuegoGanado = gano;

        if (gano)
        {
            ContinuarDespuesMiniJuego();
            return;
        }

        // Si pierde el minijuego

        if (player != null && player.Voz && dialogoFalloVoz.Length > 0)
        {
            dialogosActuales = dialogoFalloVoz;
        }
        else if (dialogoFalloSinVoz.Length > 0)
        {
            dialogosActuales = dialogoFalloSinVoz;
        }

        index = 0;

        panelDialogo.SetActive(true);

        MostrarDialogoActual();
    }
}
