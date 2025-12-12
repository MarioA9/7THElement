using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("Attack")]
    public float attackCooldown = 3f;
    public float attackAnimationTime = 0.6f;

    [Header("Elemental / Ultimate Settings")]
    public float elementalDuration = 1.2f;   // Duración de animación Elemental
    public float elementalCooldown = 3f;

    public float ultimateDuration = 2f;      // Duración Ultimate
    public float ultimateCooldown = 6f;

    // ==========================================================
    // 6 BOOLEANS DE PODER
    // ==========================================================
    [Header("Poderes Activados")]
    public bool Voz = false;
    public bool Tono = false;
    public bool Ritmo = false;
    public bool Fraseo = false;
    public bool Diccion = false;
    public bool Respiracion = false;

    // Para saber dentro de qué trigger está parado
    private string triggerActual = "";

    // ==========================================================
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

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = health;
        }

        gameOverPanel.SetActive(false);
    }

    void Update()
    {
        if (healthSlider != null)
            healthSlider.value = health;

        // ================================
        // REDUCIR TIEMPOS
        // ================================
        if (attackTimer > 0) attackTimer -= Time.deltaTime;
        if (elementalTimer > 0) elementalTimer -= Time.deltaTime;
        if (ultimateTimer > 0) ultimateTimer -= Time.deltaTime;

        // ================================
        // ACTIVAR PODERES (F)
        // ================================
        if (triggerActual != "" && Input.GetKeyDown(KeyCode.F))
        {
            ActivarPoder(triggerActual);
        }

        // ================================
        // HABILIDADES ESPECIALES
        // ================================

        int poderesActivos = ContarPoderes();

        // ELEMENTAL → mínimo 2 poderes activados
        if (!isElemental && poderesActivos >= 2 && Input.GetKeyDown(KeyCode.E) && elementalTimer <= 0 && !isUltimate)
        {
            StartCoroutine(Elemental());
        }

        // ULTIMATE → los 6 poderes activados
        if (!isUltimate && poderesActivos == 6 && Input.GetKeyDown(KeyCode.Q) && ultimateTimer <= 0 && !isElemental)
        {
            StartCoroutine(Ultimate());
        }

        // NO PERMITIR MOVIMIENTO DURANTE ATAQUE/ELEMENTAL/ULTIMATE
        if (isAttacking || isElemental || isUltimate)
        {
            movement = Vector2.zero;
            return;
        }

        // ================================
        // ATAQUE NORMAL (CLICK IZQUIERDO)
        // ================================
        if (Input.GetMouseButtonDown(0) && attackTimer <= 0)
        {
            StartCoroutine(Attack());
        }

        // ================================
        // MOVIMIENTO
        // ================================
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        movement = new Vector2(moveX, moveY).normalized;

        // Girar sprite
        if (moveX > 0)
            transform.localScale = new Vector3(1.808842f, 1.488365f, 1);
        else if (moveX < 0)
            transform.localScale = new Vector3(-1.808842f, 1.488365f, 1);

        // Animaciones
        if (movement.magnitude == 0)
            animator.Play("Idle");
        else
            animator.Play("MoveRight");
    }

    void FixedUpdate()
    {
        if (!isAttacking && !isElemental && !isUltimate)
        {
            rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
        }
    }

    // ==========================================================
    // ATAQUE NORMAL
    // ==========================================================
    IEnumerator Attack()
    {
        isAttacking = true;
        attackTimer = attackCooldown;

        animator.Play("Attack");

        yield return new WaitForSeconds(attackAnimationTime);

        isAttacking = false;
    }

    // ==========================================================
    // ELEMENTAL
    // ==========================================================
    IEnumerator Elemental()
    {
        isElemental = true;
        elementalTimer = elementalCooldown;

        animator.Play("Elemental");

        yield return new WaitForSeconds(elementalDuration);

        isElemental = false;
    }

    // ==========================================================
    // ULTIMATE
    // ==========================================================
    IEnumerator Ultimate()
    {
        isUltimate = true;
        ultimateTimer = ultimateCooldown;

        animator.Play("Ultimate");

        yield return new WaitForSeconds(ultimateDuration);

        isUltimate = false;
    }

    // ==========================================================
    // ACTIVAR PODER SEGÚN TRIGGER
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

    // Contar cuántos poderes están activados
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
    // DETECCIÓN DE TRIGGERS
    // ==========================================================
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("AttackEnemy"))
        {
            TakeDamage(10f);
        }

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
    }

    public void TakeDamage(float amount)
    {
        health -= amount;
        if (health < 0) health = 0;

        if (health == 0)
            GameOver();
    }

    void GameOver()
    {
        Time.timeScale = 0f;  // Congelar juego
        gameOverPanel.SetActive(true);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(mainMenuScene);
    }

    public void LoadLastSave()
    {
        Time.timeScale = 1f;
        SaveManager.instance.RequestLoadAfterScene();
        UnityEngine.SceneManagement.SceneManager.LoadScene("Game");
    }
}
