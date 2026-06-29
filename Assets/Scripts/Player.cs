using UnityEngine;
using System.Collections;
using System.Collections.Generic; // Para manejar la lista dinámica de teclas
using TMPro;

public class Player : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("Attack")]
    public float attackCooldown = 3f;
    public float attackAnimationTime = 0.6f;

    [Header("Elemental / Ultimate Settings")]
    public float elementalDuration = 1.2f;
    public float elementalCooldown = 3f;

    public float ultimateDuration = 2f;
    public float ultimateCooldown = 6f;

    // ==========================================================
    // ESQUIVE
    // ==========================================================
    [Header("Avoid / Esquive")]
    public float avoidDuration = 0.5f;
    public float avoidCooldown = 2f;

    private bool isAvoiding = false;
    private float avoidTimer = 0f;

    private Collider2D playerCollider;

    // ==========================================================
    // PODERES
    // ==========================================================
    [Header("Poderes Activados")]
    public bool Voz = false;
    public bool Tono = false;
    public bool Ritmo = false;
    public bool Fraseo = false;
    public bool Diccion = false;
    public bool Respiracion = false;

    private string triggerActual = "";

    private Animator animator;
    private Rigidbody2D rb;

    private Vector2 movement;
    private float attackTimer = 0f;

    private bool isAttacking = false;
    private bool isElemental = false;
    private bool isUltimate = false;

    private float elementalTimer = 0f;
    private float ultimateTimer = 0f;

    public float maxHealth = 100f;
    public float health = 100f;

    public UnityEngine.UI.Slider healthSlider;
    public GameObject gameOverPanel;
    public string mainMenuScene = "MainMenu";

    [Header("Llave")]
    public bool Llave = false;

    // ==========================================================
    // STUN
    // ==========================================================
    [Header("Stun")]
    public float stunDuration = 3f;
    public float strongEnemyDamage = 20f;

    private bool isStunned = false;

    [Header("Knockback")]
    public float knockbackForce = 6f;
    public float knockbackTime = 0.2f;

    [Header("Critical Knockback")]
    public float criticalKnockbackForce = 1000f;

    [Range(0f, 1f)]
    public float criticalKnockbackChance = 0.02f;

    // ==========================================================
    // INVENTARIO
    // ==========================================================
    [Header("Inventario")]

    public int comidaPerro = 0;
    public int pescado = 0;
    public int carne = 0;
    public int zanahorias = 0;
    public int botellasAgua = 0;

    // NUEVOS OBJETOS DEL INVENTARIO
    public int manzana = 0;
    public int naranja = 0;
    public int pera = 0;

    public bool chamarra = false;
    // Permite saber si la chamarra está puesta o guardada
    [HideInInspector] public bool chamarraEquipada = false;
    [Tooltip("Cantidad de vida que restauran los alimentos")]
    public float curacionPorComida = 20f;

    [Header("UI Inventario")]
    public GameObject inventarioPanel;
    public TextMeshProUGUI inventarioTexto;

    private bool inventarioAbierto = false;
    // Lista para mapear los objetos activos a los números del teclado
    private List<string> mapeoInventario = new List<string>();

    // ==========================================================
    // FRIO Y CALOR
    // ==========================================================
    [Header("Frio")]
    public UnityEngine.UI.Slider frioSlider;
    public float frio = 0f;
    public float maxFrio = 100f;
    public float aumentoFrio = 8f;

    [Header("Shader Graph Integration (Frio)")]
    [Tooltip("Asigna aquí el material que tiene el Shader Graph de Frío")]
    public Material shaderMaterial;
    [Tooltip("Velocidad con la que cambia el AlphaCutoff (valores altos = más rápido)")]
    public float velocidadCambioShader = 1f;
    private float actualAlphaCutoff = 1f;

    [Header("Calor")]
    public UnityEngine.UI.Slider calorSlider;
    public float calor = 0f;
    public float maxCalor = 100f;

    public float aumentoCalorDesierto = 8f;
    public float aumentoCalorLava = 15f;
    // NUEVO: Ritmo de aumento de calor en la zona normal usando la chamarra
    public float aumentoCalorNormal = 4f;
    // NUEVO: Multiplicador de calor en zonas calientes si se lleva puesta la chamarra
    public float multiplicadorCalorChamarra = 2.5f;

    [Header("Shader Graph Integration (Calor)")]
    [Tooltip("Asigna aquí el material que tiene el Shader Graph de Calor")]
    public Material calorMaterial;
    [Tooltip("Velocidad con la que cambia la intensidad del calor")]
    public float velocidadCambioCalorShader = 50f;
    private float actualIntensity = 20f;
    private int intensityPropertyID;

    [Header("Daño Ambiental")]
    public float dañoPorTemperatura = 5f;
    public float tiempoDañoTemperatura = 1f;

    private float timerDañoTemperatura = 0f;

    private bool enZonaNieve = false;
    private bool enZonaDesertica = false;
    private bool enZonaLava = false;
    // NUEVO: Registro de si se encuentra en la zona normal
    private bool enZonaNormal = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();

        // Obtener el ID de la propiedad para optimizar rendimiento en el Update
        intensityPropertyID = Shader.PropertyToID("_Intensity");

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = health;
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        // Inventario inicia cerrado
        if (inventarioPanel != null)
        {
            inventarioPanel.SetActive(false);
        }

        // Configurar sliders
        if (frioSlider != null)
        {
            frioSlider.maxValue = maxFrio;
            frioSlider.value = frio;
            frioSlider.gameObject.SetActive(false);
        }

        if (calorSlider != null)
        {
            calorSlider.maxValue = maxCalor;
            calorSlider.value = calor;
            calorSlider.gameObject.SetActive(false);
        }

        // Inicializar el valor del shader al empezar (en 1)
        if (shaderMaterial != null)
        {
            shaderMaterial.SetFloat("_AlphaCutoff", 1f);
        }

        // Inicializar el valor de intensidad de calor al empezar (en 20)
        if (calorMaterial != null)
        {
            calorMaterial.SetFloat(intensityPropertyID, 20f);
        }
    }

    void Update()
    {
        if (healthSlider != null)
            healthSlider.value = health;

        // ================================
        // TEMPERATURA Y SHADER
        // ================================
        ActualizarTemperatura();
        ActualizarEfectoShader();
        ActualizarShaderCalor(); // NUEVA LLAMADA

        // ================================
        // REDUCIR TIEMPOS
        // ================================
        if (attackTimer > 0) attackTimer -= Time.deltaTime;
        if (elementalTimer > 0) elementalTimer -= Time.deltaTime;
        if (ultimateTimer > 0) ultimateTimer -= Time.deltaTime;
        if (avoidTimer > 0) avoidTimer -= Time.deltaTime;

        // ================================
        // ACTIVAR PODERES (F)
        // ================================
        if (triggerActual != "" && Input.GetKeyDown(KeyCode.F))
        {
            ActivarPoder(triggerActual);
        }

        // ================================
        // INVENTARIO
        // ================================
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleInventario();
        }

        // Detectar el uso de objetos por teclas numéricas cuando el inventario está abierto
        if (inventarioAbierto)
        {
            DetectarUsoObjetos();
        }

        // ================================
        // ESQUIVE
        // ================================
        if (Ritmo && !isAvoiding && avoidTimer <= 0 &&
            Input.GetMouseButtonDown(1) &&
            !isAttacking && !isElemental && !isUltimate)
        {
            StartCoroutine(Avoid());
            return;
        }

        // ================================
        // HABILIDADES ESPECIALES
        // ================================
        int poderesActivos = ContarPoderes();

        if (!isElemental && poderesActivos >= 2 &&
            Input.GetKeyDown(KeyCode.E) &&
            elementalTimer <= 0 &&
            !isUltimate)
        {
            StartCoroutine(Elemental());
        }

        if (!isUltimate && poderesActivos == 6 &&
            Input.GetKeyDown(KeyCode.Q) &&
            ultimateTimer <= 0 &&
            !isElemental)
        {
            StartCoroutine(Ultimate());
        }

        // ================================
        // NO MOVER DURANTE ACCIONES
        // ================================
        if (isAttacking || isElemental || isUltimate || isAvoiding)
        {
            movement = Vector2.zero;
            ResetPhysics();
            return;
        }

        if (isStunned)
        {
            movement = Vector2.zero;
            return;
        }

        // ================================
        // ATAQUE NORMAL
        // ================================
        if (Input.GetMouseButtonDown(0) && attackTimer <= 0)
        {
            StartCoroutine(Attack());
        }

        // ================================
        // MOVIMIENTO (OPCIÓN 2: SIN FLIP POR CÓDIGO)
        // ================================
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        movement = new Vector2(moveX, moveY).normalized;

        // Control de animaciones según la dirección del movimiento
        if (movement.magnitude == 0)
        {
            animator.Play("Idle");
        }
        else
        {
            if (Mathf.Abs(moveY) >= Mathf.Abs(moveX))
            {
                if (moveY > 0) animator.Play("Move_Back");
                else animator.Play("Move_Front");
            }
            else
            {
                if (moveX > 0)
                {
                    animator.Play("Move_Side"); // Hacia la derecha
                }
                else
                {
                    animator.Play("Move_Left"); // Hacia la izquierda
                }
            }
        }
    } // Cierre del método Update()

    void FixedUpdate()
    {
        if (!isAttacking && !isElemental && !isUltimate &&
            !isAvoiding && !isStunned)
        {
            rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
        }
    }

    // ==========================================================
    // TEMPERATURA
    // ==========================================================
    void ActualizarTemperatura()
    {
        // =========================
        // FRIO
        // =========================
        if (enZonaNieve)
        {
            if (frioSlider != null)
                frioSlider.gameObject.SetActive(true);

            // Evalúa si la chamarra está EQUIPADA
            if (chamarra && chamarraEquipada)
            {
                frio = 0;
            }
            else
            {
                frio += aumentoFrio * Time.deltaTime;

                if (frio >= maxFrio)
                {
                    timerDañoTemperatura += Time.deltaTime;

                    if (timerDañoTemperatura >= tiempoDañoTemperatura)
                    {
                        timerDañoTemperatura = 0f;
                        TakeDamage(dañoPorTemperatura);
                    }
                }
            }
        }
        else
        {
            frio -= 20f * Time.deltaTime;

            if (frio <= 0)
            {
                frio = 0;

                if (frioSlider != null)
                    frioSlider.gameObject.SetActive(false);
            }
        }

        // =========================
        // CALOR
        // =========================
        if (enZonaDesertica || enZonaLava || (enZonaNormal && chamarra && chamarraEquipada))
        {
            if (calorSlider != null)
                calorSlider.gameObject.SetActive(true);

            if (enZonaNormal && chamarra && chamarraEquipada)
            {
                calor += aumentoCalorNormal * Time.deltaTime;
            }
            else
            {
                float tasaBaseCalor = enZonaLava ? aumentoCalorLava : aumentoCalorDesierto;

                if (chamarra && chamarraEquipada)
                {
                    tasaBaseCalor *= multiplicadorCalorChamarra;
                }

                calor += tasaBaseCalor * Time.deltaTime;
            }

            if (calor >= maxCalor)
            {
                timerDañoTemperatura += Time.deltaTime;

                if (timerDañoTemperatura >= tiempoDañoTemperatura)
                {
                    timerDañoTemperatura = 0f;
                    TakeDamage(dañoPorTemperatura);
                }
            }
        }
        else
        {
            calor -= 20f * Time.deltaTime;

            if (calor <= 0)
            {
                calor = 0;

                if (calorSlider != null)
                    calorSlider.gameObject.SetActive(false);
            }
        }

        frio = Mathf.Clamp(frio, 0, maxFrio);
        calor = Mathf.Clamp(calor, 0, maxCalor);

        if (frioSlider != null)
            frioSlider.value = frio;

        if (calorSlider != null)
            calorSlider.value = calor;
    }

    // ==========================================================
    // CONTROL DEL SHADER SEGÚN EL FRÍO
    // ==========================================================
    void ActualizarEfectoShader()
    {
        if (shaderMaterial == null) return;

        // Si el frío llega al máximo (barra llena)
        if (frio >= maxFrio)
        {
            // Disminuye lentamente hacia 0
            actualAlphaCutoff = Mathf.MoveTowards(actualAlphaCutoff, 0f, velocidadCambioShader * Time.deltaTime);
        }
        else
        {
            // Si no está al 100%, regresa lentamente hacia 1
            actualAlphaCutoff = Mathf.MoveTowards(actualAlphaCutoff, 1f, velocidadCambioShader * Time.deltaTime);
        }

        // Aplicar el valor al Shader Graph usando el nombre de la variable de referencia
        shaderMaterial.SetFloat("_AlphaCutoff", actualAlphaCutoff);
    }

    // ==========================================================
    // NUEVO: CONTROL DEL SHADER SEGÚN EL CALOR
    // ==========================================================
    void ActualizarShaderCalor()
    {
        if (calorMaterial == null) return;

        // Si la barra de calor está llena (100%)
        if (calor >= maxCalor)
        {
            // Cambia el valor hacia 1.2f
            actualIntensity = Mathf.MoveTowards(actualIntensity, 2.85f, velocidadCambioCalorShader * Time.deltaTime);
        }
        else
        {
            // Si disminuye (no está al 100%), regresa a 20f
            actualIntensity = Mathf.MoveTowards(actualIntensity, 20f, velocidadCambioCalorShader * Time.deltaTime);
        }

        // Aplica el valor al Vector1 "Intensity" en el Shader Graph
        calorMaterial.SetFloat(intensityPropertyID, actualIntensity);
    }

    // ==========================================================
    // ESQUIVE
    // ==========================================================
    IEnumerator Avoid()
    {
        isAvoiding = true;
        avoidTimer = avoidCooldown;

        ResetPhysics();

        animator.Play("Avoid");

        yield return new WaitForSeconds(avoidDuration);

        isAvoiding = false;
    }

    // ==========================================================
    // ATAQUE
    // ==========================================================
    IEnumerator Attack()
    {
        isAttacking = true;
        attackTimer = attackCooldown;

        ResetPhysics();

        animator.Play("Attack");

        yield return new WaitForSeconds(attackAnimationTime);

        ResetPhysics();

        isAttacking = false;
    }

    // ==========================================================
    // ELEMENTAL
    // ==========================================================
    IEnumerator Elemental()
    {
        isElemental = true;
        elementalTimer = elementalCooldown;

        ResetPhysics();

        animator.Play("Elemental");

        yield return new WaitForSeconds(elementalDuration);

        ResetPhysics();

        isElemental = false;
    }

    // ==========================================================
    // ULTIMATE
    // ==========================================================
    IEnumerator Ultimate()
    {
        isUltimate = true;
        ultimateTimer = ultimateCooldown;

        ResetPhysics();

        animator.Play("Ultimate");

        yield return new WaitForSeconds(ultimateDuration);

        ResetPhysics();

        isUltimate = false;
    }

    // ==========================================================
    // STUN + KNOCKBACK
    // ==========================================================
    IEnumerator Stun(Vector2 knockDirection, float force, bool critical)
    {
        isStunned = true;

        rb.linearVelocity = knockDirection * force;

        animator.Play("Stun");

        if (critical)
        {
            yield return new WaitForSeconds(0.2f);

            GameOver();

            yield break;
        }

        yield return new WaitForSeconds(knockbackTime);

        float timer = 0f;

        while (timer < stunDuration)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        ResetPhysics();

        isStunned = false;
    }

    // ==========================================================
    // ACTIVAR PODER
    // ==========================================================
    void ActivarPoder(string tag)
    {
        switch (tag)
        {
            case "Voz": Voz = true; break;
            case "Tono": Tono = true; break;
            case "Ritmo": Ritmo = true; break;
            case "Fraseo": Fraseo = true; break;
            case "Diccion": Diccion = true; break;
            case "Respiracion": Respiracion = true; break;
        }
    }

    int ContarPoderes()
    {
        int count = 0;
        if (Voz) count++;
        if (Tono) count++;
        if (Ritmo) count++;
        if (Fraseo) count++;
        if (Diccion) count++;
        if (Respiracion) count++;
        return count;
    }

    // ==========================================================
    // INVENTARIO 
    // ==========================================================
    void ToggleInventario()
    {
        inventarioAbierto = !inventarioAbierto;

        if (inventarioPanel != null)
        {
            inventarioPanel.SetActive(inventarioAbierto);
        }

        if (inventarioAbierto)
        {
            ActualizarInventarioUI();
        }
    }

    void ActualizarInventarioUI()
    {
        if (inventarioTexto == null)
            return;

        string texto = "";
        mapeoInventario.Clear();

        if (comidaPerro > 0) AddItemToMap("ComidaPerro");
        if (pescado > 0) AddItemToMap("Pescado");
        if (carne > 0) AddItemToMap("Carne");
        if (zanahorias > 0) AddItemToMap("Zanahorias");
        if (botellasAgua > 0) AddItemToMap("Agua");
        if (chamarra) AddItemToMap("Chamarra");

        if (manzana > 0) AddItemToMap("Manzana");
        if (naranja > 0) AddItemToMap("Naranja");
        if (pera > 0) AddItemToMap("Pera");

        for (int i = 0; i < mapeoInventario.Count; i++)
        {
            int numeroTecla = (i == 9) ? 0 : i + 1;
            string itemID = mapeoInventario[i];

            texto += "[" + numeroTecla + "] ";

            switch (itemID)
            {
                case "ComidaPerro": texto += "Comida para perro x" + comidaPerro + "\n"; break;
                case "Pescado": texto += "Pescado x" + pescado + "\n"; break;
                case "Carne": texto += "Carne x" + carne + "\n"; break;
                case "Zanahorias": texto += "Zanahorias x" + zanahorias + "\n"; break;
                case "Agua": texto += "Botellas de agua x" + botellasAgua + "\n"; break;
                case "Chamarra": texto += "Chamarra (" + (chamarraEquipada ? "Equipada" : "Guardada") + ")\n"; break;
                case "Manzana": texto += "Manzana x" + manzana + "\n"; break;
                case "Naranja": texto += "Naranja x" + naranja + "\n"; break;
                case "Pera": texto += "Pera x" + pera + "\n"; break;
            }
        }

        if (mapeoInventario.Count == 0)
        {
            texto = "Inventario vacío";
        }

        inventarioTexto.text = texto;
    }

    void AddItemToMap(string itemName)
    {
        if (mapeoInventario.Count < 10)
        {
            mapeoInventario.Add(itemName);
        }
    }

    void DetectarUsoObjetos()
    {
        int indexPresionado = -1;

        for (int i = 1; i <= 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha0 + i))
            {
                indexPresionado = i - 1;
            }
        }
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            indexPresionado = 9;
        }

        if (indexPresionado >= 0 && indexPresionado < mapeoInventario.Count)
        {
            UsarObjeto(mapeoInventario[indexPresionado]);
        }
    }

    void UsarObjeto(string itemID)
    {
        bool objetoUsado = false;

        switch (itemID)
        {
            case "Zanahorias":
                if (zanahorias > 0)
                {
                    zanahorias--;
                    health = Mathf.Clamp(health + curacionPorComida, 0, maxHealth);
                    objetoUsado = true;
                }
                break;

            case "Carne":
                if (carne > 0)
                {
                    carne--;
                    health = Mathf.Clamp(health + curacionPorComida, 0, maxHealth);
                    objetoUsado = true;
                }
                break;

            case "Pescado":
                if (pescado > 0)
                {
                    pescado--;
                    health = Mathf.Clamp(health + curacionPorComida, 0, maxHealth);
                    objetoUsado = true;
                }
                break;

            case "Agua":
                if (botellasAgua > 0 && calor > 0)
                {
                    botellasAgua--;
                    calor = Mathf.Clamp(calor - 40f, 0, maxCalor);
                    objetoUsado = true;
                }
                break;

            case "Chamarra":
                if (chamarra)
                {
                    chamarraEquipada = !chamarraEquipada;
                    objetoUsado = true;
                }
                break;

            case "ComidaPerro":
                break;

            case "Manzana":
                if (manzana > 0)
                {
                    manzana--;
                    health = Mathf.Clamp(health + curacionPorComida, 0, maxHealth);
                    objetoUsado = true;
                }
                break;

            case "Naranja":
                if (naranja > 0)
                {
                    naranja--;
                    health = Mathf.Clamp(health + curacionPorComida, 0, maxHealth);
                    objetoUsado = true;
                }
                break;

            case "Pera":
                if (pera > 0)
                {
                    pera--;
                    health = Mathf.Clamp(health + curacionPorComida, 0, maxHealth);
                    objetoUsado = true;
                }
                break;
        }

        if (objetoUsado)
        {
            ActualizarInventarioUI();
        }
    }

    // ==========================================================
    // TRIGGERS
    // ==========================================================
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("AttackEnemy"))
        {
            if (!isAvoiding)
            {
                TakeDamage(10f);
            }
        }

        if (other.CompareTag("AttackStrongEnemy"))
        {
            if (!isAvoiding && !isStunned)
            {
                Vector2 knockDir = (transform.position - other.transform.position).normalized;
                float force = knockbackForce;

                if (Random.value < criticalKnockbackChance)
                {
                    force = criticalKnockbackForce;
                    StartCoroutine(Stun(knockDir, force, true));
                }
                else
                {
                    StartCoroutine(Stun(knockDir, force, false));
                }

                TakeDamage(strongEnemyDamage);
            }
        }

        if (other.CompareTag("Zona_Nieve")) enZonaNieve = true;
        if (other.CompareTag("Zona_Desertica")) enZonaDesertica = true;
        if (other.CompareTag("Zona_Lava")) enZonaLava = true;
        if (other.CompareTag("Zona_Normal")) enZonaNormal = true;

        string tag = other.tag;

        if (tag == "Voz" || tag == "Tono" || tag == "Ritmo" ||
            tag == "Fraseo" || tag == "Diccion" || tag == "Respiracion")
        {
            triggerActual = tag;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == triggerActual)
        {
            triggerActual = "";
        }

        if (other.CompareTag("Zona_Nieve")) enZonaNieve = false;
        if (other.CompareTag("Zona_Desertica")) enZonaDesertica = false;
        if (other.CompareTag("Zona_Lava")) enZonaLava = false;
        if (other.CompareTag("Zona_Normal")) enZonaNormal = false;
    }

    // ==========================================================
    // VIDA
    // ==========================================================
    public void TakeDamage(float amount)
    {
        health -= amount;

        if (health < 0)
            health = 0;

        if (healthSlider != null)
            healthSlider.value = health;

        if (health == 0)
            GameOver();
    }

    void GameOver()
    {
        Time.timeScale = 0f;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
    }

    void ResetPhysics()
    {
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
    }
}