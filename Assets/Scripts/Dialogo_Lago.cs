using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Dialogo_Lago : MonoBehaviour
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

    [Header("Botones de Decisión (Lago)")]
    public Button botonExplorar;
    public Button botonNoExplorar;

    [Header("Configuración de escritura")]
    public float velocidadEscritura = 0.03f;

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

    [Header("NUEVOS: Diálogos Lago del Mapa")]
    public Dialogo[] dialogosLagoVozOn;
    public Dialogo[] dialogosLagoVozOff;

    private Dialogo[] dialogosActuales;

    [System.Serializable]
    public class Dialogo
    {
        [TextArea] public string texto;
        public Sprite imagen;
        public bool usarImagen;
        [Tooltip("Si está marcado, al mostrar ESTE diálogo específico se pausará y activará los botones de Explorar/No Explorar en el Lago")]
        public bool activarBotonesLago;
    }

    private int index = 0;
    private bool escribiendo = false;
    private bool jugadorDentro = false;
    private bool esperandoDecision = false;

    // Player
    private Player player;
    private float velocidadOriginal;
    private bool bloqueoDialogo = false;

    // ==========================================================
    // UNITY
    // ==========================================================
    void Start()
    {
        panelDialogo.SetActive(false);

        // Asegurar que los botones arranquen apagados
        if (botonExplorar != null) botonExplorar.gameObject.SetActive(false);
        if (botonNoExplorar != null) botonNoExplorar.gameObject.SetActive(false);

        // Asignar funciones a los botones por código
        if (botonExplorar != null) botonExplorar.onClick.AddListener(OpcionExplorar);
        if (botonNoExplorar != null) botonNoExplorar.onClick.AddListener(OpcionNoExplorar);

        player = FindObjectOfType<Player>();
        if (player != null)
            velocidadOriginal = player.moveSpeed;

        if (objetoElemental != null)
            objetoElemental.SetActive(false);
    }

    void Update()
    {
        if (jugadorDentro && !esperandoDecision)
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

                        // Si el texto se saltó y este diálogo requería botones, los activamos de inmediato
                        if (dialogosActuales[index].activarBotonesLago)
                        {
                            ActivarBotonesDecision(true);
                        }
                    }
                    else
                    {
                        // Si el diálogo actual tiene el bool activado, no deja avanzar con click hasta que decida
                        if (!dialogosActuales[index].activarBotonesLago)
                        {
                            SiguienteDialogo();
                        }
                    }
                }
            }
        }

        // Forzar Idle si está bloqueado
        if (bloqueoDialogo && player != null)
        {
            Animator anim = player.GetComponent<Animator>();
            if (anim != null) anim.Play("Idle");
        }
    }

    // ==========================================================
    // DETECCIÓN POR COLISIÓN
    // ==========================================================
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            jugadorDentro = true;

            if (activarAutomatico)
                IniciarDialogoSegunVozYEstados();
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
            jugadorDentro = false;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            jugadorDentro = true;
            if (activarAutomatico) IniciarDialogoSegunVozYEstados();
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) jugadorDentro = false;
    }

    // ==========================================================
    // PROCESO ELEMENTAL
    // ==========================================================
    IEnumerator IniciarElemental()
    {
        elementalYaUsado = true;

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

        bool esLagoVozOn = (dialogosLagoVozOn != null && dialogosLagoVozOn.Length > 0);
        bool esLagoVozOff = (dialogosLagoVozOff != null && dialogosLagoVozOff.Length > 0);

        if (player.Voz)
        {
            if (esLagoVozOn)
            {
                dialogosActuales = dialogosLagoVozOn;
            }
            else if (dialogoYaActivado && dialogosVozOn_Repetido.Length > 0)
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
            if (esLagoVozOff)
            {
                dialogosActuales = dialogosLagoVozOff;
            }
            else
            {
                dialogosActuales = dialogosVozOff;
            }
        }

        if (dialogosActuales == null || dialogosActuales.Length == 0)
        {
            Debug.LogWarning("No hay diálogos configurados en este objeto.");
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
        esperandoDecision = false;
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

        if (dialogosActuales[index].activarBotonesLago)
        {
            ActivarBotonesDecision(true);
        }
    }

    // ==========================================================
    // SIGUIENTE DIÁLOGO
    // ==========================================================
    void SiguienteDialogo()
    {
        if (dialogosActuales == null || dialogosActuales.Length == 0)
        {
            Debug.LogWarning("dialogosActuales era nulo o vacío al intentar avanzar. Forzando cierre seguro.");
            FinalizarDialogo();
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
    // MANEJO DE BOTONES Y DECISIONES
    // ==========================================================
    void ActivarBotonesDecision(bool activar)
    {
        esperandoDecision = activar;
        if (botonExplorar != null) botonExplorar.gameObject.SetActive(activar);
        if (botonNoExplorar != null) botonNoExplorar.gameObject.SetActive(activar);
    }

    public void OpcionExplorar()
    {
        if (!esperandoDecision) return;

        ActivarBotonesDecision(false);

        // Al explorar el lago, la consecuencia directa y 100% segura es GAME OVER.
        EjecutarGameOver();
    }

    public void OpcionNoExplorar()
    {
        if (!esperandoDecision) return;

        ActivarBotonesDecision(false);

        if (player != null)
        {
            player.enabled = true;
            player.moveSpeed = velocidadOriginal;
        }
        bloqueoDialogo = false;

        FinalizarDialogo();
    }

    private void EjecutarGameOver()
    {
        Debug.Log("¡GAME OVER! Decidiste explorar las peligrosas aguas del lago.");

        // Aquí puedes descomentar e integrar tu lógica de muerte/reinicio:
        // UnityEngine.SceneManagement.SceneManager.LoadScene("MenuPrincipal");

        FinalizarDialogo();
    }

    // ==========================================================
    // FINALIZAR
    // ==========================================================
    void FinalizarDialogo()
    {
        panelDialogo.SetActive(false);
        ActivarBotonesDecision(false);
        esperandoDecision = false;

        if (player != null)
        {
            if (player.Voz)
                dialogoYaActivado = true;

            player.enabled = true;
            player.moveSpeed = velocidadOriginal;
        }

        bloqueoDialogo = false;

        StartCoroutine(ResetearJugadorDentro());

        if (!noDestruirAlTerminar)
        {
            Destroy(gameObject);
        }
    }

    IEnumerator ResetearJugadorDentro()
    {
        yield return new WaitForEndOfFrame();
        escribiendo = false;
    }
}