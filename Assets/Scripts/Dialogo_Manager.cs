using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Dialogo_Manager : MonoBehaviour
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

    [Header("UI de decisiones")]
    public GameObject panelDecision;
    public Button botonContinuar;
    public Button botonGameOver;

    [Header("Panel de Game Over")]
    public GameObject panelGameOver;

    [System.Serializable]
    public class Dialogo
    {
        [TextArea] public string texto;
        public Sprite imagen;
        public bool usarImagen;

        [Header("Decisión al final de este diálogo")]
        public bool usarDecision;
    }

    private int index = 0;
    private bool escribiendo = false;
    private bool jugadorDentro = false;

    // Player
    private Player player;
    private float velocidadOriginal;
    private bool bloqueoDialogo = false;

    [Header("Sistema de Llave / Puerta")]
    public bool usarSistemaLlave = false;

    [Header("Diálogo si tiene llave")]
    public Dialogo[] dialogoConLlave;

    [Header("Diálogo si NO tiene llave")]
    public Dialogo[] dialogoSinLlave;

    [Header("Objeto que se activará al abrir")]
    public GameObject objetoActivar;

    [Header("Probabilidad de abrir sin llave (0-1)")]
    [Range(0f, 1f)]
    public float probabilidadAbrir = 0.3f;

    [Header("Daño por intentar sin llave")]
    public float danoIntento = 10f;

    [Header("Botones sistema llave")]
    public Button botonIntentar;
    public Button botonRendirse;

    private bool usandoSistemaLlave = false;

    void Start()
    {
        panelDialogo.SetActive(false);

        player = FindObjectOfType<Player>();
        if (player != null)
            velocidadOriginal = player.moveSpeed;

        if (objetoElemental != null)
            objetoElemental.SetActive(false);

        if (panelDecision != null)
            panelDecision.SetActive(false);

        if (panelGameOver != null)
            panelGameOver.SetActive(false);

        botonContinuar.onClick.AddListener(OpcionContinuar);
        botonGameOver.onClick.AddListener(OpcionGameOver);

        botonIntentar.onClick.AddListener(IntentarAbrir);
        botonRendirse.onClick.AddListener(Rendirse);

        StartCoroutine(CheckPlayerInside());
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

        usandoSistemaLlave = usarSistemaLlave;

        // ================================
        // SISTEMA DE LLAVE
        // ================================
        if (usandoSistemaLlave)
        {
            if (player.Llave)
            {
                dialogosActuales = dialogoConLlave;
            }
            else
            {
                dialogosActuales = dialogoSinLlave;
            }
        }
        else
        {
            // ================================
            // SISTEMA NORMAL
            // ================================
            if (player.Voz)
            {
                if (dialogoYaActivado && dialogosVozOn_Repetido.Length > 0)
                    dialogosActuales = dialogosVozOn_Repetido;
                else
                    dialogosActuales = dialogosVozOn;
            }
            else
            {
                dialogosActuales = dialogosVozOff;
            }
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
        // Si el diálogo actual tiene decisión
        if (dialogosActuales[index].usarDecision)
        {
            MostrarDecision();
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

    void MostrarDecision()
    {
        if (panelDecision != null)
            panelDecision.SetActive(true);

        // Si usa sistema llave y NO tiene llave
        if (usandoSistemaLlave && !player.Llave)
        {
            botonContinuar.gameObject.SetActive(false);
            botonGameOver.gameObject.SetActive(false);

            botonIntentar.gameObject.SetActive(true);
            botonRendirse.gameObject.SetActive(true);
        }
        else
        {
            botonContinuar.gameObject.SetActive(true);
            botonGameOver.gameObject.SetActive(true);

            botonIntentar.gameObject.SetActive(false);
            botonRendirse.gameObject.SetActive(false);
        }
    }

    void OpcionContinuar()
    {
        panelDecision.SetActive(false);

        index++;

        if (index >= dialogosActuales.Length)
        {
            FinalizarDialogo();
            return;
        }

        MostrarDialogoActual();
    }

    void OpcionGameOver()
    {
        panelDecision.SetActive(false);

        if (panelGameOver != null)
            panelGameOver.SetActive(true);

        FinalizarDialogo();
    }

    void IntentarAbrir()
    {
        panelDecision.SetActive(false);

        // Quitar vida
        if (player != null)
            player.TakeDamage(danoIntento);

        float rand = Random.value;

        if (rand <= probabilidadAbrir)
        {
            Debug.Log("PUERTA ABIERTA");

            player.Llave = true; // Simula que logró abrir

            index++;

            if (index >= dialogosActuales.Length)
            {
                FinalizarDialogo();
                return;
            }

            MostrarDialogoActual();
        }
        else
        {
            Debug.Log("FALLÓ");

            // Vuelve a mostrar decisión
            MostrarDecision();
        }
    }

    void Rendirse()
    {
        panelDecision.SetActive(false);

        panelDialogo.SetActive(false);

        if (player != null)
        {
            player.enabled = true;
            player.moveSpeed = velocidadOriginal;
            bloqueoDialogo = false;
        }

        // NO destruir → puede volver a interactuar
    }

    // ==========================================================
    // FINALIZAR
    // ==========================================================
    void FinalizarDialogo()
    {
        panelDialogo.SetActive(false);

        if (player.Voz)
            dialogoYaActivado = true;

        if (player != null)
        {
            player.enabled = true;
            player.moveSpeed = velocidadOriginal;
            bloqueoDialogo = false;
        }

        // ================================
        // SI ES SISTEMA LLAVE Y SE ABRIÓ
        // ================================
        if (usandoSistemaLlave && player.Llave)
        {
            // Activar objeto de la escena
            if (objetoActivar != null)
            {
                objetoActivar.SetActive(true);
            }

            // destruir la puerta / trigger
            Destroy(gameObject);
            return;
        }

        // NORMAL
        if (!noDestruirAlTerminar)
        {
            Destroy(gameObject);
        }
    }

    IEnumerator CheckPlayerInside()
    {
        yield return null;

        Collider2D trigger = GetComponent<Collider2D>();
        Collider2D playerCol = FindObjectOfType<Player>().GetComponent<Collider2D>();

        if (trigger.IsTouching(playerCol))
        {
            jugadorDentro = true;

            if (activarAutomatico)
                IniciarDialogoSegunVozYEstados();
        }
    }
}
