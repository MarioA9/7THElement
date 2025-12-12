using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class EnemyManager : MonoBehaviour
{
    public enum TipoEnemigo { Normal, Fuerte, Elite }
    [Header("Tipo")]
    public TipoEnemigo tipo = TipoEnemigo.Normal;

    [Header("Movimiento")]
    public float velocidad = 2f;
    public float rangoDeteccion = 5f;
    public float rangoAtaque = 1.2f;

    [Header("Patrulla (zona)")]
    public Transform zonaPatrulla;           // objeto que contiene BoxCollider2D (isTrigger)
    private Collider2D colPatrulla;

    [Header("Patrulla - tiempos")]
    public float patrolIdleMin = 1.2f;       // tiempo quieto antes de moverse
    public float patrolIdleMax = 3f;
    public float puntoLlegadaThreshold = 0.25f;

    private Vector2 puntoPatrullaActual;
    private float patrolIdleTimer = 0f;

    [Header("Vida / UI")]
    public float vidaMax = 100f;
    [HideInInspector] public float vidaActual;
    public Slider barraVida;                 // slider ubicado sobre el enemigo (world canvas)

    [Header("Ataque")]
    public float cooldownAtaque = 1.5f;      // tiempo entre ataques
    private bool puedeAtacar = true;

    [Header("Ajustes extra")]
    public float tiempoMaxMoverAHastaLlegar = 10f; // por seguridad si algo falla

    // componentes
    private Animator anim;
    private Rigidbody2D rb;
    private Transform player;

    // estado simple
    private enum Estado { PatrolIdle, PatrolMove, Pursue, Attacking, Dead }
    private Estado estado = Estado.PatrolIdle;

    // escala original para no romper tama�o al voltear
    private Vector3 escalaOriginal;

    // control interno
    private float tiempoMoverTimeout = 0f;

    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        vidaActual = vidaMax;

        if (zonaPatrulla != null)
            colPatrulla = zonaPatrulla.GetComponent<Collider2D>();

        var pObj = GameObject.FindGameObjectWithTag("Player");
        if (pObj != null) player = pObj.transform;

        escalaOriginal = transform.localScale;

        // iniciar patrulla: generar tiempo idle aleatorio
        SetNuevoPatrolIdle();
        SeleccionarNuevoPuntoPatrulla();

        if (barraVida != null)
        {
            barraVida.maxValue = vidaMax;
            barraVida.value = vidaActual;
        }
    }

    void Update()
    {
        if (estado == Estado.Dead) return;

        // actualizar barra vida
        if (barraVida != null) barraVida.value = vidaActual;

        // comprobar si player est� dentro de la zona de patrulla (si hay zona y player)
        bool playerEnZona = false;
        if (colPatrulla != null && player != null)
            playerEnZona = colPatrulla.OverlapPoint(player.position);

        float distanciaPlayer = player != null ? Vector2.Distance(transform.position, player.position) : Mathf.Infinity;

        // Transici�n a Pursue si el jugador est� en zona y dentro de rangoDeteccion
        if (playerEnZona && distanciaPlayer <= rangoDeteccion && estado != Estado.Attacking)
        {
            // pasar a perseguir
            estado = Estado.Pursue;
            // reset timeout seguridad
            tiempoMoverTimeout = 0f;
            // animar� Walk al moverse (ver en la secci�n de movimiento)
        }
        else
        {
            // si estaba persiguiendo pero perdi� al player, volver a patrulla
            if (estado == Estado.Pursue && (!playerEnZona || distanciaPlayer > rangoDeteccion))
            {
                // volver a patrulla en modo idle
                estado = Estado.PatrolIdle;
                SetNuevoPatrolIdle();
                SeleccionarNuevoPuntoPatrulla();
            }
        }

        // comportamiento seg�n estado
        switch (estado)
        {
            case Estado.PatrolIdle:
                rb.linearVelocity = Vector2.zero;
                CambiarAnimSiNecesario("Idle");
                patrolIdleTimer -= Time.deltaTime;
                if (patrolIdleTimer <= 0f)
                {
                    SeleccionarNuevoPuntoPatrulla();
                    estado = Estado.PatrolMove;
                    tiempoMoverTimeout = tiempoMaxMoverAHastaLlegar;
                    CambiarAnimSiNecesario("Walk");
                }
                break;

            case Estado.PatrolMove:
                MoverHacia(puntoPatrullaActual);
                tiempoMoverTimeout -= Time.deltaTime;
                float dist = Vector2.Distance(transform.position, puntoPatrullaActual);
                if (dist <= puntoLlegadaThreshold || tiempoMoverTimeout <= 0f)
                {
                    rb.linearVelocity = Vector2.zero;
                    estado = Estado.PatrolIdle;
                    SetNuevoPatrolIdle();
                    CambiarAnimSiNecesario("Idle");
                }
                break;

            case Estado.Pursue:
                // si cerca -> atacar; sino -> moverse al jugador
                if (player == null) break;
                float d = Vector2.Distance(transform.position, player.position);
                if (d <= rangoAtaque)
                {
                    rb.linearVelocity = Vector2.zero;
                    CambiarAnimSiNecesario("Idle");
                    if (puedeAtacar)
                        StartCoroutine(RealizarAtaqueSegunTipo());
                }
                else
                {
                    MoverHacia(player.position);
                }
                break;

            case Estado.Attacking:
                // durante ataque el coroutine maneja la animaci�n; dejamos velocity en cero
                rb.linearVelocity = Vector2.zero;
                break;
        }

        // flip respecto a la direcci�n actual (si se est� moviendo o persiguiendo)
        // Volteo dentro de MoverHacia al pasar el dir
    }

    // mueve usando rb.MovePosition para movimiento limpio y evita "jitter"
    void MoverHacia(Vector2 destino)
    {
        Vector2 dir = (destino - (Vector2)transform.position);
        float mag = dir.magnitude;
        if (mag <= 0.001f)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }
        Vector2 dirN = dir.normalized;
        rb.MovePosition((Vector2)transform.position + dirN * velocidad * Time.deltaTime);

        // ajustar flip seg�n dir.x
        if (dirN.x > 0.01f)
            transform.localScale = new Vector3(Mathf.Abs(escalaOriginal.x), escalaOriginal.y, escalaOriginal.z);
        else if (dirN.x < -0.01f)
            transform.localScale = new Vector3(-Mathf.Abs(escalaOriginal.x), escalaOriginal.y, escalaOriginal.z);

        // animar Walk si no en ataque
        if (estado != Estado.Attacking)
            CambiarAnimSiNecesario("Walk");
    }

    // selecciona un punto aleatorio dentro del collider de la zona de patrulla
    void SeleccionarNuevoPuntoPatrulla()
    {
        if (colPatrulla == null) return;

        Bounds b = colPatrulla.bounds;
        float x = Random.Range(b.min.x, b.max.x);
        float y = Random.Range(b.min.y, b.max.y);
        puntoPatrullaActual = new Vector2(x, y);
    }

    void SetNuevoPatrolIdle()
    {
        patrolIdleTimer = Random.Range(patrolIdleMin, patrolIdleMax);
    }

    // maneja la animaci�n solo si cambia (evita llamar Play cada frame)
    private string animActual = "";
    void CambiarAnimSiNecesario(string nombre)
    {
        if (anim == null) return;
        if (animActual == nombre) return;
        anim.Play(nombre);
        animActual = nombre;
    }

    // ATAQUE seg�n tipo
    IEnumerator RealizarAtaqueSegunTipo()
    {
        puedeAtacar = false;
        estado = Estado.Attacking;

        // decidir animaci�n seg�n tipo
        if (tipo == TipoEnemigo.Normal)
        {
            CambiarAnimSiNecesario("Attack");
        }
        else if (tipo == TipoEnemigo.Fuerte)
        {
            int prob = Random.Range(0, 100);
            if (prob < 20) CambiarAnimSiNecesario("AttackStrong");
            else CambiarAnimSiNecesario("Attack");
        }
        else // Elite
        {
            CambiarAnimSiNecesario("AttackStrong");
        }

        // esperar cooldown (duraci�n del ataque)
        yield return new WaitForSeconds(cooldownAtaque);

        // volver a perseguir (si a�n hay player en rangoDeteccion), si no, volver a patrulla
        puedeAtacar = true;
        estado = Estado.Pursue;
    }

    // recibir da�o por triggers
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (estado == Estado.Dead) return;

        if (collision.CompareTag("Attack"))
            RecibirDano(10f);

        else if (collision.CompareTag("Elemental"))
            RecibirDano(25f);

        else if (collision.CompareTag("Ultimate"))
            RecibirDano(50f);
    }

    void RecibirDano(float cantidad)
    {
        vidaActual -= cantidad;
        if (barraVida != null) barraVida.value = vidaActual;

        if (vidaActual <= 0f)
            Morir();
    }

    void Morir()
    {
        // puedes poner animaci�n de muerte aqu� antes de destruir
        estado = Estado.Dead;
        rb.linearVelocity = Vector2.zero;
        CambiarAnimSiNecesario("Idle"); // o "Die" si existe
        Destroy(gameObject, 0.1f);
    }

    // Gizmos para depuraci�n (opcional)
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, rangoDeteccion);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, rangoAtaque);

        if (zonaPatrulla != null && zonaPatrulla.GetComponent<Collider2D>() != null)
        {
            Gizmos.color = new Color(0f, 1f, 1f, 0.2f);
            Gizmos.DrawCube(zonaPatrulla.GetComponent<Collider2D>().bounds.center,
                            zonaPatrulla.GetComponent<Collider2D>().bounds.size);
        }
    }
}


