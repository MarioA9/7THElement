using UnityEngine;
using System.Collections;

public class NPC_StateMachine : MonoBehaviour
{
    private enum EstadoNPC
    {
        Trabajo,
        Movimiento,
        Descanso,
        Interactuar
    }

    private EstadoNPC estadoActual;

    private Animator animator;
    private Rigidbody2D rb;

    public float moveSpeed = 2f;

    [Header("Tiempos")]
    public float tiempoTrabajo = 3f;
    public float tiempoDescanso = 3f;

    [Header("Puntos de Ruta")]
    public Transform[] puntos; // NPC se mueve entre ellos
    private int indexActual = 0;

    private Transform player;
    private bool playerDentro = false;
    private bool interactuando = false;

    private Vector2 objetivoActual;

    // ---- NUEVO: almacenar la escala original para no romper tamaños ----
    private Vector3 escalaOriginal;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        var pObj = GameObject.FindGameObjectWithTag("Player");
        if (pObj != null) player = pObj.transform;

        // Guardar la escala original EXACTA del transform para usarla cuando hagamos flip.
        escalaOriginal = transform.localScale;

        estadoActual = EstadoNPC.Trabajo;
        StartCoroutine(EstadoTrabajo());
    }

    void Update()
    {
        if (estadoActual == EstadoNPC.Interactuar)
            return;

        ActualizarAnimaciones();
    }

    IEnumerator EstadoTrabajo()
    {
        estadoActual = EstadoNPC.Trabajo;

        float t = tiempoTrabajo;
        while (t > 0)
        {
            if (interactuando) yield break;
            rb.linearVelocity = Vector2.zero;
            t -= Time.deltaTime;
            yield return null;
        }

        PasarAMovimiento();
    }

    void PasarAMovimiento()
    {
        estadoActual = EstadoNPC.Movimiento;

        indexActual++;
        if (indexActual >= puntos.Length) indexActual = 0;

        objetivoActual = puntos[indexActual].position;
        StartCoroutine(EstadoMovimiento());
    }

    IEnumerator EstadoMovimiento()
    {
        while (estadoActual == EstadoNPC.Movimiento)
        {
            if (interactuando)
            {
                rb.linearVelocity = Vector2.zero;
                yield break;
            }

            Vector2 dir = (objetivoActual - (Vector2)transform.position).normalized;
            rb.linearVelocity = dir * moveSpeed;

            GirarSprite(dir);

            float dist = Vector2.Distance(transform.position, objetivoActual);

            if (dist < 0.1f)
            {
                rb.linearVelocity = Vector2.zero;
                StartCoroutine(EstadoDescanso());
                yield break;
            }

            yield return null;
        }
    }

    IEnumerator EstadoDescanso()
    {
        estadoActual = EstadoNPC.Descanso;

        float t = tiempoDescanso;

        while (t > 0)
        {
            if (interactuando) yield break;
            rb.linearVelocity = Vector2.zero;
            t -= Time.deltaTime;
            yield return null;
        }

        StartCoroutine(EstadoTrabajo());
    }

    void UpdateInteraccion()
    {
        if (playerDentro && Input.GetKeyDown(KeyCode.F) && !interactuando)
        {
            interactuando = true;
            estadoActual = EstadoNPC.Interactuar;

            rb.linearVelocity = Vector2.zero;
            GirarNPCalPlayer();
        }

        if (interactuando)
        {
            GirarNPCalPlayer();
        }

        if (!playerDentro && interactuando)
        {
            interactuando = false;
            StartCoroutine(EstadoTrabajo());
        }
    }

    void LateUpdate()
    {
        UpdateInteraccion();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerDentro = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerDentro = false;
    }

    void ActualizarAnimaciones()
    {
        if (estadoActual == EstadoNPC.Trabajo || estadoActual == EstadoNPC.Descanso)
        {
            animator.Play("Idle");
            return;
        }

        if (rb.linearVelocity.magnitude > 0.1f)
        {
            animator.Play("Walk");
        }
        else
        {
            animator.Play("Idle");
        }
    }

    // ---- USAR escalaOriginal para hacer flip sin romper tamaño ----
    void GirarSprite(Vector2 dir)
    {
        if (dir.x > 0)
            transform.localScale = new Vector3(Mathf.Abs(escalaOriginal.x), escalaOriginal.y, escalaOriginal.z);
        else if (dir.x < 0)
            transform.localScale = new Vector3(-Mathf.Abs(escalaOriginal.x), escalaOriginal.y, escalaOriginal.z);
    }

    void GirarNPCalPlayer()
    {
        if (player == null) return;

        if (player.position.x > transform.position.x)
            transform.localScale = new Vector3(Mathf.Abs(escalaOriginal.x), escalaOriginal.y, escalaOriginal.z);
        else
            transform.localScale = new Vector3(-Mathf.Abs(escalaOriginal.x), escalaOriginal.y, escalaOriginal.z);

        animator.Play("Idle");
    }
}
