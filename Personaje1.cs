using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Personaje1 : MonoBehaviour
{
    public GameObject golpeEfecto, bomba, bala, explosion;
    public Image barra;
    Rigidbody2D rb;
    Animator anim;
    SpriteRenderer sr;
    public float vel = 5, z = 0, carga = 0, cargaMax = 2, vida = 5, timerParpadeo = 0, duracion = 1, timerGople = 0;
    public int dir = 0, apuntar = 0, puntos = 0;
    public bool vulnerable = true, golpe = false;
    public Text textVida, textPuntos;
    short habilidad = 0;
    Vector3 destino;

    public static Personaje1 instance; // static hace que todas las copias tengan ese valor

    // Start is called before the first frame update
    void Start()
    {
        //SINGLETON
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }

        destino = transform.position;
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();

        DontDestroyOnLoad(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.timeScale == 1)
        {
            //Movimiento();
            MovimientoAxis();
            //MovimientoRaton();
            //MovimientoRaton2();
            CalcularDir();
            ComunicarDir();
            CambiarHabilidad();

            if (Input.GetKeyDown(KeyCode.D)) //Ataque habilidad
            {
                switch (habilidad)
                {
                    case 0:
                        if (carga >= 0.5f)
                        {
                            CrearBala();
                            carga -= 0.5f;
                        }
                        break;
                    case 1:
                        if (carga >= 2)
                        {
                            CrearBomba();
                            carga -= 2;
                        }
                        break;
                }
            }

            if (Input.GetKeyDown(KeyCode.F)) // Ataque basico
            {
                if (timerGople <= 0)
                {
                    Golpe();
                    timerGople = 0.2f;
                } 
            }
            timerGople -= Time.deltaTime;
        }
        ActualizarHUD();
        
        if (Input.GetKeyDown(KeyCode.F2))
        {
            GuardarPartida();
        }

        if (Input.GetKeyDown(KeyCode.F3))
        {
            CargarPartida();
        }

        

        if (carga < cargaMax)  //   nº que tienes / nº maximo
        {
            carga += Time.deltaTime;
        }
        else
        {
            carga = cargaMax;
        }
        //print(carga);

        if (duracion <= 0.5f)
        {
            Parpadeo();
            duracion += Time.deltaTime;
        }
        else
        {
            sr.enabled = true;
            vulnerable = true;
        }

    }
    // FUNCIONES
    // Movimineto y dirección
    void Movimiento()
    {
        float x = 0, y = 0;
        if (Input.GetKey(KeyCode.RightArrow))
        {
            x = vel;
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            x = -vel;
        }
        if (Input.GetKey(KeyCode.UpArrow))
        {
            y = vel;
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            y = -vel;
        }
        rb.velocity = new Vector2(x, y);
    }

    void MovimientoAxis()
    {
        Vector2 vectorMovimineto = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        if (vectorMovimineto.magnitude > 1) // Para que el vector del movimineto diagonal no sea mayor.
        {
            vectorMovimineto.Normalize();
        }

        rb.velocity = vel * vectorMovimineto;

        /*float x, y;
        x = Input.GetAxis("Horizontal");
        y = Input.GetAxis("Vertical");
        print("Valor X/Y" + x + "/" + y);*/

    }

    void MovimientoRaton()
    {
        if (Input.GetMouseButton(1))
        {
            Vector3 posRaton = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 direccion = posRaton - transform.position;

            rb.velocity = direccion.normalized * vel;
        }
        else
        {
            rb.velocity = Vector2.zero;
        }
    }

    void MovimientoRaton2()
    {
        if (Input.GetMouseButton(1))
        {
            Vector3 posRaton = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            destino = new Vector3(posRaton.x, posRaton.y, 0);

        }
        Vector2 direccion = destino - transform.position;
        rb.velocity = direccion.normalized * vel;
    }

    void CalcularDir()
    {
        if (rb.velocity.x > 0)
        {
            if (rb.velocity.y > 0)
            {
                dir = 1;
                apuntar = 1;
            }
            else if (rb.velocity.y < 0)
            {
                dir = 3;
                apuntar = 3;
            }
            else
            {
                dir = 2;
                apuntar = 2;
            }
        }
        else if (rb.velocity.x < 0)
        {
            if (rb.velocity.y > 0)
            {
                dir = 7;
                apuntar = 7;
            }
            else if (rb.velocity.y < 0)
            {
                dir = 5;
                apuntar = 5;
            }
            else
            {
                dir = 6;
                apuntar = 6;
            }
        }
        else
        {
            if (rb.velocity.y > 0)
            {
                dir = 0;
                apuntar = 0;
            }
            else if (rb.velocity.y < 0)
            {
                dir = 4;
                apuntar = 4;
            }
            else
            {
                dir = 8;
            }
        }
    }

    void ComunicarDir()
    {
        anim.SetInteger("Direccion", dir);
    }

    void ActualizarHUD() //lo que tienes / maximo      (float)vidaMax
    {
        textVida.text = "Vida: " + vida;
        textPuntos.text = "Puntos: " + puntos;
        barra.fillAmount = carga / (float)cargaMax;
    }

    void CambiarHabilidad()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            habilidad = 0;
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            habilidad = 1;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        switch (collision.gameObject.tag)
        {
            case "Bala":
                if (vulnerable)
                {
                    vida--;
                    duracion = 0;
                    vulnerable = false;
                    Destroy(collision.gameObject);
                }
                break;
            case "Moneda":
                puntos++;
                Destroy(collision.gameObject);
                break;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        switch (collision.gameObject.tag)
        {
            case "Enemigo":
                GameController.instance.CrearRoca();
                break;
            case "GolpeE":
                if (vulnerable)
                {
                    vida--;
                    duracion = 0;
                    vulnerable = false;
                }
                break;
            
        }
    }

    public void GuardarPartida()
    {
        Scene escena = SceneManager.GetActiveScene();
        PlayerPrefs.SetString("Escena", escena.name);
        PlayerPrefs.SetFloat("VidaJugador", vida);
        PlayerPrefs.SetInt("PuntosJugador", puntos);
        PlayerPrefs.SetFloat("PosicionJugadorX", transform.position.x);
        PlayerPrefs.SetFloat("PosicionJugadorY", transform.position.y);
        PlayerPrefs.Save();
    }

    public void CargarPartida()
    {
        vida = PlayerPrefs.GetFloat("VidaJugador", vida);
        puntos = PlayerPrefs.GetInt("PuntosJugador", puntos);
        transform.position = new Vector3(PlayerPrefs.GetFloat("PosicionJugadorX", transform.position.x), PlayerPrefs.GetFloat("PosicionJugadorY", transform.position.y), transform.position.z);
        SceneManager.LoadScene(PlayerPrefs.GetString("Escena"));
    }

    void Parpadeo()
    {
        timerParpadeo -= Time.deltaTime;
        if (timerParpadeo <= 0)
        {
            timerParpadeo = 0.1f;
            sr.enabled = !sr.enabled;
        }
    }

    // HABILIDADES
    // Golpe
    void Golpe()
    {
        GameObject clon;
        clon = Instantiate(golpeEfecto);
        clon.transform.position = transform.position;
        golpe = true;
        switch (apuntar)
        {
            case 0:
                z = 90;
                break;
            case 1:
                z = 45;
                break;
            case 2:
                z = 0;
                break;
            case 3:
                z = -45;
                break;
            case 4:
                z = -90;
                break;
            case 5:
                z = -135;
                break;
            case 6:
                z = 180;
                break;
            case 7:
                z = 135;
                break;

        }
        clon.transform.Rotate(0, 0, z);
    }

    // Explosion
    void CrearBomba()
    {
        GameObject clon;
        clon = Instantiate(bomba);
        clon.transform.position = new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z);
        switch (apuntar)
        {
            case 0:
                z = 90;
                break;
            case 1:
                z = 45;
                break;
            case 2:
                z = 0;
                break;
            case 3:
                z = -45;
                break;
            case 4:
                z = -90;
                break;
            case 5:
                z = -135;
                break;
            case 6:
                z = 180;
                break;
            case 7:
                z = 135;
                break;

        }
        clon.transform.Rotate(0, 0, z);
        Bomba script = clon.GetComponent<Bomba>();
        if (dir != 8)
        {
            script.SetVel(10);
        }
        else
        {
            script.SetVel(6);
        }

    }

    // Disparo
    void CrearBala()
    {
        GameObject clon;
        clon = Instantiate(bala);
        clon.transform.position = transform.position;
        switch (apuntar)
        {
            case 0:
                z = 90;
                break;
            case 1:
                z = 45;
                break;
            case 2:
                z = 0;
                break;
            case 3:
                z = -45;
                break;
            case 4:
                z = -90;
                break;
            case 5:
                z = -135;
                break;
            case 6:
                z = 180;
                break;
            case 7:
                z = 135;
                break;

        }
        clon.transform.Rotate(0, 0, z);

    }

}
