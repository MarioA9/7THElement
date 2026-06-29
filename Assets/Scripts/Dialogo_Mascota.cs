using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Dialogo_Mascota : MonoBehaviour
{
    // ==========================================================
    // ACTIVACIÓN
    // ==========================================================
    [Header("Activación")]
    public KeyCode teclaActivacion = KeyCode.F;

    private bool jugadorDentro = false;

    // ==========================================================
    // UI
    // ==========================================================
    [Header("UI")]
    public GameObject panelDialogo;
    public TMP_Text textoDialogo;
    public Image imagenDialogo;

    // ==========================================================
    // BOTONES
    // ==========================================================
    [Header("Botones")]
    public GameObject panelBotones;

    public Button btnAlimentar;
    public Button btnBeber;
    public Button btnAcariciar;
    public Button btnSeguir;

    public TMP_Text textoBtnSeguir;

    // ==========================================================
    // AMISTAD
    // ==========================================================
    [Header("Amistad")]
    public Slider barraAmistad;

    public int amistadActual = 0;
    public int amistadMax = 100;

    public int amistadAcariciarMin = 50;
    public int amistadSeguirMin = 100;

    [Header("Incrementos")]
    public int sumarAlimentar = 10;
    public int sumarBeber = 10;
    public int sumarAcariciar = 20;

    // ==========================================================
    // DIÁLOGOS PRINCIPALES
    // ==========================================================
    [Header("Voz ON")]
    public Dialogo[] dialogosVozOn;
    public Dialogo[] dialogosVozOn_Repetido;

    [Header("Voz OFF")]
    public Dialogo[] dialogosVozOff;
    public Dialogo[] dialogosVozOff_Repetido;

    // ==========================================================
    // DIÁLOGOS ESPECIALES
    // ==========================================================
    [Header("Sin comida / agua")]
    public Dialogo[] dialogoSinComida_VozOn;
    public Dialogo[] dialogoSinComida_VozOff;

    [Header("No puede acariciar")]
    public Dialogo[] dialogoNoAcariciar_VozOn;
    public Dialogo[] dialogoNoAcariciar_VozOff;

    [Header("Empezar a seguir")]
    public Dialogo[] dialogoSeguir_VozOn;
    public Dialogo[] dialogoSeguir_VozOff;

    [Header("Dejar de seguir")]
    public Dialogo[] dialogoEsperar_VozOn;
    public Dialogo[] dialogoEsperar_VozOff;

    // ==========================================================
    // CLASE DIÁLOGO
    // ==========================================================
    [System.Serializable]
    public class Dialogo
    {
        [TextArea]
        public string texto;

        public Sprite imagen;
        public bool usarImagen;

        [Header("Eventos")]
        public bool activarBotones;
    }

    private Dialogo[] dialogosActuales;

    // ==========================================================
    // VARIABLES
    // ==========================================================
    private int index = 0;

    private bool escribiendo = false;
    private bool dialogoYaActivado = false;

    private bool botonesActivados = false;
    private bool esperandoRespuesta = false;

    private Player player;

    // ==========================================================
    // SEGUIR
    // ==========================================================
    private bool siguiendo = false;
    private Vector3 escalaOriginal;

    public Transform jugadorTransform;

    public float velocidadSeguimiento = 3f;

    [Header("Distancia Seguimiento")]
    public float distanciaMinima = 1.5f;

    // ==========================================================
    // START
    // ==========================================================
    void Start()
    {
        panelDialogo.SetActive(false);
        panelBotones.SetActive(false);

        player = FindObjectOfType<Player>();

        escalaOriginal = transform.localScale;

        barraAmistad.maxValue = amistadMax;
        barraAmistad.value = amistadActual;

        btnAlimentar.onClick.AddListener(Alimentar);
        btnBeber.onClick.AddListener(Beber);
        btnAcariciar.onClick.AddListener(Acariciar);
        btnSeguir.onClick.AddListener(Seguir);
    }

    // ==========================================================
    // UPDATE
    // ==========================================================
    void Update()
    {
        // ======================================================
        // INTERACTUAR
        // ======================================================
        if (jugadorDentro && Input.GetKeyDown(teclaActivacion))
        {
            if (!panelDialogo.activeSelf)
            {
                IniciarDialogo();
            }
        }

        // ======================================================
        // CONTINUAR DIÁLOGO
        // ======================================================
        if (panelDialogo.activeSelf && Input.GetMouseButtonDown(0))
        {
            if (esperandoRespuesta || dialogosActuales == null) // Seguridad añadida aquí
                return;

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

        // ======================================================
        // SEGUIR JUGADOR
        // ======================================================
        if (siguiendo && jugadorTransform != null)
        {
            float distancia = Vector2.Distance(
                transform.position,
                jugadorTransform.position
            );

            // SOLO seguir si está lejos
            if (distancia > distanciaMinima)
            {
                Vector2 direccion =
                    (jugadorTransform.position - transform.position).normalized;

                Vector2 destino =
                    (Vector2)jugadorTransform.position -
                    (direccion * distanciaMinima);

                transform.position = Vector2.MoveTowards(
                    transform.position,
                    destino,
                    velocidadSeguimiento * Time.deltaTime
                );

                // MIRAR HACIA DONDE CAMINA
                if (direccion.x > 0)
                {
                    transform.localScale = new Vector3(
                        Mathf.Abs(escalaOriginal.x),
                        escalaOriginal.y,
                        escalaOriginal.z
                    );
                }
                else if (direccion.x < 0)
                {
                    transform.localScale = new Vector3(
                        -Mathf.Abs(escalaOriginal.x),
                        escalaOriginal.y,
                        escalaOriginal.z
                    );
                }
            }
        }
    }

    // ==========================================================
    // TRIGGERS
    // ==========================================================
    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            jugadorDentro = true;

            jugadorTransform = col.transform;
        }
    }

    void OnTriggerExit2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            jugadorDentro = false;
        }
    }

    // ==========================================================
    // INICIAR DIÁLOGO
    // ==========================================================
    void IniciarDialogo()
    {
        if (player == null) player = FindObjectOfType<Player>(); // Seguridad por si no se asignó en Start

        botonesActivados = false;

        // ======================================================
        // SELECCIÓN DIÁLOGO
        // ======================================================
        if (player != null && player.Voz)
        {
            if (dialogoYaActivado &&
                dialogosVozOn_Repetido.Length > 0)
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
            if (dialogoYaActivado &&
                dialogosVozOff_Repetido.Length > 0)
            {
                dialogosActuales = dialogosVozOff_Repetido;
            }
            else
            {
                dialogosActuales = dialogosVozOff;
            }
        }

        if (dialogosActuales == null ||
            dialogosActuales.Length == 0)
        {
            return;
        }

        index = 0;

        panelDialogo.SetActive(true);

        MostrarDialogo();
    }

    // ==========================================================
    // MOSTRAR DIÁLOGO
    // ==========================================================
    void MostrarDialogo()
    {
        textoDialogo.text = "";

        // IMAGEN
        if (dialogosActuales[index].usarImagen &&
            dialogosActuales[index].imagen != null)
        {
            imagenDialogo.sprite = dialogosActuales[index].imagen;

            imagenDialogo.gameObject.SetActive(true);
        }
        else
        {
            imagenDialogo.gameObject.SetActive(false);
        }

        // ACTIVAR BOTONES
        if (dialogosActuales[index].activarBotones &&
            !botonesActivados)
        {
            ActivarBotones();

            botonesActivados = true;
        }

        StartCoroutine(
            Escribir(dialogosActuales[index].texto)
        );
    }

    IEnumerator Escribir(string texto)
    {
        escribiendo = true;

        textoDialogo.text = "";

        foreach (char c in texto)
        {
            textoDialogo.text += c;

            yield return new WaitForSeconds(0.03f);
        }

        escribiendo = false;
    }

    // ==========================================================
    // SIGUIENTE
    // ==========================================================
    void SiguienteDialogo()
    {
        // CONTROL ANTIFALLOS: Si por alguna razón la lista es nula, cerramos el diálogo de inmediato
        if (dialogosActuales == null)
        {
            FinalizarDialogo();
            return;
        }

        index++;

        if (index >= dialogosActuales.Length)
        {
            FinalizarDialogo();
            return;
        }

        MostrarDialogo();
    }

    // ==========================================================
    // FINALIZAR
    // ==========================================================
    void FinalizarDialogo()
    {
        panelDialogo.SetActive(false);

        panelBotones.SetActive(false);

        esperandoRespuesta = false;

        botonesActivados = false;

        dialogoYaActivado = true;
    }

    // ==========================================================
    // FORZAR DIÁLOGO
    // ==========================================================
    void ForzarDialogo(Dialogo[] nuevoDialogo)
    {
        if (nuevoDialogo == null ||
            nuevoDialogo.Length == 0)
            return;

        StopAllCoroutines();

        panelBotones.SetActive(false);

        esperandoRespuesta = false;

        dialogosActuales = nuevoDialogo;

        index = 0;

        panelDialogo.SetActive(true);

        MostrarDialogo();
    }

    // ==========================================================
    // ACTIVAR BOTONES
    // ==========================================================
    void ActivarBotones()
    {
        panelBotones.SetActive(true);

        esperandoRespuesta = true;

        btnAcariciar.interactable =
            amistadActual >= amistadAcariciarMin;

        btnSeguir.interactable =
            amistadActual >= amistadSeguirMin;
    }

    // ==========================================================
    // CONTINUAR DESPUÉS DECISIÓN
    // ==========================================================
    void ContinuarDespuesDecision()
    {
        esperandoRespuesta = false;

        panelBotones.SetActive(false);

        botonesActivados = true;

        index++;

        if (dialogosActuales == null || index >= dialogosActuales.Length)
        {
            FinalizarDialogo();
            return;
        }

        MostrarDialogo();
    }

    // ==========================================================
    // ALIMENTAR
    // ==========================================================
    void Alimentar()
    {
        bool alimentoUsado = false;

        if (player.comidaPerro > 0)
        {
            player.comidaPerro--;
            alimentoUsado = true;
        }
        else if (player.pescado > 0)
        {
            player.pescado--;
            alimentoUsado = true;
        }
        else if (player.carne > 0)
        {
            player.carne--;
            alimentoUsado = true;
        }
        else if (player.zanahorias > 0)
        {
            player.zanahorias--;
            alimentoUsado = true;
        }

        if (alimentoUsado)
        {
            AumentarAmistad(sumarAlimentar);

            ContinuarDespuesDecision();
        }
        else
        {
            if (player.Voz)
                ForzarDialogo(dialogoSinComida_VozOn);
            else
                ForzarDialogo(dialogoSinComida_VozOff);
        }
    }

    // ==========================================================
    // BEBER
    // ==========================================================
    void Beber()
    {
        if (player.botellasAgua > 0)
        {
            player.botellasAgua--;

            AumentarAmistad(sumarBeber);

            ContinuarDespuesDecision();
        }
        else
        {
            if (player.Voz)
                ForzarDialogo(dialogoSinComida_VozOn);
            else
                ForzarDialogo(dialogoSinComida_VozOff);
        }
    }

    // ==========================================================
    // ACARICIAR
    // ==========================================================
    void Acariciar()
    {
        if (amistadActual >= amistadAcariciarMin)
        {
            AumentarAmistad(sumarAcariciar);

            ContinuarDespuesDecision();
        }
        else
        {
            if (player.Voz)
                ForzarDialogo(dialogoNoAcariciar_VozOn);
            else
                ForzarDialogo(dialogoNoAcariciar_VozOff);
        }
    }

    // ==========================================================
    // SEGUIR / ESPERAR
    // ==========================================================
    void Seguir()
    {
        if (amistadActual < amistadSeguirMin)
            return;

        siguiendo = !siguiendo;

        // ======================================================
        // EMPEZAR A SEGUIR
        // ======================================================
        if (siguiendo)
        {
            textoBtnSeguir.text = "Espera aquí";

            if (player.Voz)
            {
                ForzarDialogo(dialogoSeguir_VozOn);
            }
            else
            {
                ForzarDialogo(dialogoSeguir_VozOff);
            }
        }
        // ======================================================
        // DEJAR DE SEGUIR
        // ======================================================
        else
        {
            textoBtnSeguir.text = "Sígueme";

            if (player.Voz)
            {
                ForzarDialogo(dialogoEsperar_VozOn);
            }
            else
            {
                ForzarDialogo(dialogoEsperar_VozOff);
            }
        }

        esperandoRespuesta = false;

        panelBotones.SetActive(false);

        botonesActivados = true;
    }

    // ==========================================================
    // AMISTAD
    // ==========================================================
    void AumentarAmistad(int cantidad)
    {
        amistadActual += cantidad;

        amistadActual = Mathf.Clamp(
            amistadActual,
            0,
            amistadMax
        );

        barraAmistad.value = amistadActual;

        btnAcariciar.interactable =
            amistadActual >= amistadAcariciarMin;

        btnSeguir.interactable =
            amistadActual >= amistadSeguirMin;
    }
}