using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class MiniJuego_Ritmo : MonoBehaviour
{
    [Header("Carriles")]
    public Transform carrilLeft;
    public Transform carrilDown;
    public Transform carrilUp;
    public Transform carrilRight;

    [Header("Zona de golpe")]
    public Transform hitLine;

    [Header("Prefabs Flechas")]
    public GameObject flechaLeft;
    public GameObject flechaDown;
    public GameObject flechaUp;
    public GameObject flechaRight;

    [Header("Configuración")]
    public float velocidad = 300f;
    public float tiempoEntreNotas = 0.7f;
    public int totalNotas = 20;

    [Header("Aumento de dificultad")]
    public float aumentoVelocidad = 15f;
    public float velocidadMaxima = 600f;
    float velocidadInicial;

    [Header("UI")]
    public GameObject panelRitmo;
    public TMP_Text resultadoTexto;

    private int aciertos = 0;
    private int notas = 0;
    private int intentos = 0;

    private bool jugando = false;

    private Player player;

    private List<FlechaRitmo> flechas = new List<FlechaRitmo>();

    Dialogo_Ritmo dialogo;

    bool gano = false;

    void Start()
    {
        player = FindObjectOfType<Player>();

        velocidadInicial = velocidad; // guardamos la velocidad original

        panelRitmo.SetActive(false);
    }

    public void IniciarJuego(Dialogo_Ritmo d)
    {
        dialogo = d;

        panelRitmo.SetActive(true);

        aciertos = 0;
        notas = 0;
        intentos = 0;

        velocidad = velocidadInicial; // reinicia velocidad

        resultadoTexto.text = ""; // limpia texto

        jugando = true;

        StartCoroutine(GenerarNotas());
    }

    IEnumerator GenerarNotas()
    {
        while (notas < totalNotas)
        {
            GenerarNota();
            notas++;

            yield return new WaitForSeconds(tiempoEntreNotas);
        }
    }

    void GenerarNota()
    {
        // aumentar velocidad gradualmente
        velocidad += aumentoVelocidad;

        if (velocidad > velocidadMaxima)
            velocidad = velocidadMaxima;

        int carril = Random.Range(0, 4);

        Transform spawn = carrilLeft;
        GameObject prefab = flechaLeft;

        if (carril == 0)
        {
            spawn = carrilLeft;
            prefab = flechaLeft;
        }

        if (carril == 1)
        {
            spawn = carrilDown;
            prefab = flechaDown;
        }

        if (carril == 2)
        {
            spawn = carrilUp;
            prefab = flechaUp;
        }

        if (carril == 3)
        {
            spawn = carrilRight;
            prefab = flechaRight;
        }

        GameObject obj = Instantiate(prefab, spawn.position, Quaternion.identity, panelRitmo.transform);

        FlechaRitmo f = obj.GetComponent<FlechaRitmo>();

        f.direccion = carril;
        f.hitLine = hitLine;
        f.juego = this;

        flechas.Add(f);
    }

    void Update()
    {
        if (!jugando) return;

        if (Input.GetKeyDown(KeyCode.LeftArrow)) Evaluar(0);
        if (Input.GetKeyDown(KeyCode.DownArrow)) Evaluar(1);
        if (Input.GetKeyDown(KeyCode.UpArrow)) Evaluar(2);
        if (Input.GetKeyDown(KeyCode.RightArrow)) Evaluar(3);
    }

    void Evaluar(int dir)
    {
        intentos++;

        foreach (FlechaRitmo f in flechas)
        {
            if (f == null) continue;

            if (f.direccion == dir)
            {
                float distancia = Mathf.Abs(f.transform.position.y - hitLine.position.y);

                if (distancia < 50f)
                {
                    aciertos++;
                    Destroy(f.gameObject);
                    break;
                }
            }
        }

        if (intentos >= totalNotas)
        {
            Finalizar();
        }
    }

    void Finalizar()
    {
        jugando = false;

        float porcentaje = (float)aciertos / totalNotas * 100f;

        resultadoTexto.text = "Precisión: " + porcentaje.ToString("F0") + "%";

        if (porcentaje >= 80f)
        {
            gano = true;

            player.Ritmo = true;

            resultadoTexto.text += "\nRITMO DESBLOQUEADO";
        }
        else
        {
            gano = false;

            resultadoTexto.text += "\nFallaste";
        }

        StartCoroutine(CerrarMiniJuego());
    }

    IEnumerator CerrarMiniJuego()
    {
        yield return new WaitForSeconds(2.5f);

        panelRitmo.SetActive(false);

        velocidad = velocidadInicial;
        resultadoTexto.text = "";

        if (dialogo != null)
        {
            dialogo.ResultadoMiniJuego(gano);
        }
    }
}
