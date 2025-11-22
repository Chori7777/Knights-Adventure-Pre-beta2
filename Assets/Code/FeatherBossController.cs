using System.Collections;
using UnityEngine;

public class FeatherBossController : MonoBehaviour
{
    [Header(" Configuración de Movimiento")]
    [SerializeField] private Transform posicionIzquierda;
    [SerializeField] private Transform posicionDerecha;
    [SerializeField] private float cooldownMovimiento = 5f;
    [SerializeField] private float duracionTeletransporte = 0.5f;
    [SerializeField] private bool voltearHaciaJugador = true;
    private bool estaEnPosicionIzquierda = true;
    private SpriteRenderer spriteRenderer;

    [Header(" Proyectil Base")]
    [SerializeField] private GameObject prefabPluma;
    [SerializeField] private float velocidadPluma = 8f;

    [Header(" Ataque 1: Plumas Cayendo")]
    [SerializeField] private int cantidadPlumasCayendo = 5;
    [SerializeField] private float intervaloPlumasCayendo = 0.3f;
    [SerializeField] private float velocidadCaida = 5f;
    [SerializeField] private Transform puntoSpawnSuperior;
    [SerializeField] private float rangoSpawnX = 8f;
    [SerializeField] private float alturaSpawn = 6f;
    [SerializeField] private AudioClip sonidoPlumaCayendo;

    [Header(" Ataque 2: Patrón Cruz y X")]
    [SerializeField] private float distanciaCruz = 3f;
    [SerializeField] private float duracionSeguimiento = 2f;
    [SerializeField] private AudioClip sonidoPatronCruz;

    [Header(" Ataque 3: Plumas Teledirigidas Triple")]
    [SerializeField] private int cantidadPlumasTeledirigidas = 3;
    [SerializeField] private float intervaloPlumasTeledirigidas = 0.25f;
    [SerializeField] private float anguloDispersion = 15f;
    [SerializeField] private float velocidadTeledirigida = 6f;
    [SerializeField] private float fuerzaTeledirigida = 2f;
    [SerializeField] private float rangoActivacion = 6f;
    [SerializeField] private AudioClip sonidoPlumaTeledirigida;

    [Header(" Ataque 4: Ola de Plumas (con huecos)")]
    [SerializeField] private int cantidadPlumasOla = 8;
    [SerializeField] private int cantidadHuecos = 2;
    [SerializeField] private float velocidadInicialOla = 3f;
    [SerializeField] private float aceleracionOla = 1.5f;
    [SerializeField] private float espaciadoVertical = 1f;
    [SerializeField] private float offsetInicioY = -3f;
    [SerializeField] private bool usarPosicionJefeParaOla = true;
    [SerializeField] private AudioClip sonidoAtaqueOla;

    [Header(" Ataque 5: Ola Ascendente")]
    [SerializeField] private Transform puntoSpawnInferior;
    [SerializeField] private int cantidadPlumasAscendentes = 6;
    [SerializeField] private float velocidadAscenso = 4f;
    [SerializeField] private float velocidadBusqueda = 2f;
    [SerializeField] private float distanciaBusqueda = 4f;
    [SerializeField] private float rangoSpawnXInferior = 8f;
    [SerializeField] private AudioClip sonidoAtaqueAscendente;

    [Header(" Ataque 6: Explosión de Plumas")]
    [SerializeField] private Transform puntoExplosion;
    [SerializeField] private int puntosExplosion = 4;
    [SerializeField] private float rangoExplosionX = 6f;
    [SerializeField] private float rangoExplosionY = 2f;
    [SerializeField] private int cantidadPlumasPorExplosion = 12;
    [SerializeField] private float velocidadExplosionMin = 0.8f;
    [SerializeField] private float velocidadExplosionMax = 1.2f;
    [SerializeField] private AudioClip sonidoExplosion;

    [Header(" Referencias")]
    [SerializeField] private Transform jugador;
    [SerializeField] private Animator animator;
    [SerializeField] private BossLife vidaJefe;

    [Header(" Audio")]
    [SerializeField] private AudioClip sonidoMovimiento;
    [SerializeField] private AudioClip sonidoTeletransporte;

    [Header(" Timing de Ataques")]
    [SerializeField] private float tiempoEntreAtaques = 2.5f;

    [Header(" Introducción del Jefe")]
    [SerializeField] private bool mostrarIntroduccion = true;

    private Coroutine coroutinaMovimiento;
    private bool jefaIniciado = false;
    private bool jefe = false;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (vidaJefe == null)
        {
            vidaJefe = GetComponent<BossLife>();
        }

        transform.position = posicionIzquierda.position;

        if (mostrarIntroduccion)
        {
            IniciarCombate();
        }
    }

    private void IniciarCombate()
    {
        jefaIniciado = true;
        StartCoroutine(CicloAtaques());
        StartCoroutine(CicloMovimiento());
    }

    private void Update()
    {
        if (!jefaIniciado || jefe) return;

        if (voltearHaciaJugador && jugador != null)
        {
            if (jugador.position.x < transform.position.x)
                spriteRenderer.flipX = false;
            else
                spriteRenderer.flipX = true;
        }

        if (vidaJefe != null && vidaJefe.health <= 0 && !jefe)
        {
            OnJefaMuerto();
        }
    }

    private void OnJefaMuerto()
    {
        jefe = true;
        StopAllCoroutines();
        this.enabled = false;
    }

    private IEnumerator CicloMovimiento()
    {
        while (!jefe)
        {
            yield return new WaitForSeconds(cooldownMovimiento);
            if (!jefe)
                MoverAPosicionOpuesta();
        }
    }

    public void OnBossHit()
    {
        if (jefe) return;
        MoverAPosicionOpuesta();
    }

    private void MoverAPosicionOpuesta()
    {
        if (jefe) return;

        if (coroutinaMovimiento != null)
            StopCoroutine(coroutinaMovimiento);

        coroutinaMovimiento = StartCoroutine(CorutinaTeletransporte());
    }

    private IEnumerator CorutinaTeletransporte()
    {
        Vector3 posicionDestino = estaEnPosicionIzquierda ? posicionDerecha.position : posicionIzquierda.position;

        if (animator != null)
            animator.SetTrigger("Disappear");

        if (sonidoMovimiento != null)
            AudioManager.Instance.PlaySFX(sonidoMovimiento);

        float tiempoTranscurrido = 0f;
        Color colorOriginal = spriteRenderer.color;

        while (tiempoTranscurrido < duracionTeletransporte / 2)
        {
            tiempoTranscurrido += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, tiempoTranscurrido / (duracionTeletransporte / 2));
            spriteRenderer.color = new Color(colorOriginal.r, colorOriginal.g, colorOriginal.b, alpha);
            yield return null;
        }

        transform.position = posicionDestino;
        estaEnPosicionIzquierda = !estaEnPosicionIzquierda;

        if (sonidoTeletransporte != null)
            AudioManager.Instance.PlaySFX(sonidoTeletransporte);

        tiempoTranscurrido = 0f;
        while (tiempoTranscurrido < duracionTeletransporte / 2)
        {
            tiempoTranscurrido += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, tiempoTranscurrido / (duracionTeletransporte / 2));
            spriteRenderer.color = new Color(colorOriginal.r, colorOriginal.g, colorOriginal.b, alpha);
            yield return null;
        }

        spriteRenderer.color = colorOriginal;
    }

    private IEnumerator CicloAtaques()
    {
        yield return new WaitForSeconds(1f);

        while (!jefe)
        {
            yield return new WaitForSeconds(tiempoEntreAtaques);

            if (jefe) break;

            if (vidaJefe != null && vidaJefe.health > vidaJefe.maxHealth / 2)
            {
                int numeroAtaque = Random.Range(1, 7);
                StartCoroutine(EjecutarAtaque(numeroAtaque));
            }
            else if (vidaJefe != null && vidaJefe.health > 0)
            {
                int primerAtaque = Random.Range(1, 7);
                int segundoAtaque = Random.Range(1, 7);
                while (segundoAtaque == primerAtaque)
                    segundoAtaque = Random.Range(1, 7);

                StartCoroutine(EjecutarAtaque(primerAtaque));
                StartCoroutine(EjecutarAtaque(segundoAtaque));
            }
        }
    }

    private IEnumerator EjecutarAtaque(int numeroAtaque)
    {
        if (jefe) yield break;

        if (animator != null)
            animator.SetBool("Attack", true);

        switch (numeroAtaque)
        {
            case 1: yield return StartCoroutine(Ataque1_PlumasCayendo()); break;
            case 3: yield return StartCoroutine(Ataque3_PlumasTeledirigidas()); break;
            case 4: yield return StartCoroutine(Ataque4_OlaPlumas()); break;
            case 5: yield return StartCoroutine(Ataque5_OlaAscendente()); break;
            case 6: yield return StartCoroutine(Ataque6_ExplosionPlumas()); break;
        }
    }

    private IEnumerator Ataque1_PlumasCayendo()
    {
        Vector3 centroSpawn = puntoSpawnSuperior != null ? puntoSpawnSuperior.position : Vector3.zero;

        for (int i = 0; i < cantidadPlumasCayendo; i++)
        {
            if (jefe) yield break;

            float randomX = centroSpawn.x + Random.Range(-rangoSpawnX, rangoSpawnX);
            Vector3 posSpawn = new Vector3(randomX, centroSpawn.y + alturaSpawn, 0);

            GameObject pluma = Instantiate(prefabPluma, posSpawn, Quaternion.Euler(0, 0, 90));

            PlumaCayendo cayendo = pluma.AddComponent<PlumaCayendo>();
            cayendo.velocidadCaida = velocidadCaida;
            cayendo.prefabPluma = prefabPluma;
            cayendo.velocidadPluma = velocidadPluma;

            if (sonidoPlumaCayendo != null)
                AudioManager.Instance.PlaySFX(sonidoPlumaCayendo);

            yield return new WaitForSeconds(intervaloPlumasCayendo);
        }

        yield return new WaitForSeconds(0.5f);
    }

    
    private IEnumerator Ataque3_PlumasTeledirigidas()
    {
        if (jugador == null || jefe) yield break;

        Vector2 direccionJugador = (jugador.position - transform.position).normalized;

        if (animator != null)
            animator.SetTrigger("AttackMid");

        for (int i = 0; i < cantidadPlumasTeledirigidas; i++)
        {
            if (jefe) yield break;

            float anguloDispersionActual = (i - (cantidadPlumasTeledirigidas - 1) / 2f) * this.anguloDispersion;
            Vector2 direccion = Quaternion.Euler(0, 0, anguloDispersionActual) * direccionJugador;

            GameObject pluma = Instantiate(prefabPluma, transform.position + (Vector3)direccion * 0.5f, Quaternion.identity);

            PlumaTeledirigida teledirigida = pluma.AddComponent<PlumaTeledirigida>();
            teledirigida.jugador = jugador;
            teledirigida.velocidad = velocidadTeledirigida;
            teledirigida.fuerzaBusqueda = fuerzaTeledirigida;
            teledirigida.rangoActivacion = rangoActivacion;

            RotarPlumaHaciaDireccion(pluma, direccion);

            if (sonidoPlumaTeledirigida != null)
                AudioManager.Instance.PlaySFX(sonidoPlumaTeledirigida);

            yield return new WaitForSeconds(intervaloPlumasTeledirigidas);
        }

        yield return new WaitForSeconds(0.5f);
    }

    private IEnumerator Ataque4_OlaPlumas()
    {
        if (jefe) yield break;

        Vector2 direccionOla = estaEnPosicionIzquierda ? Vector2.right : Vector2.left;
        float inicioX = estaEnPosicionIzquierda ? posicionIzquierda.position.x : posicionDerecha.position.x;

        float inicioYCalculado = usarPosicionJefeParaOla ?
            transform.position.y + offsetInicioY :
            offsetInicioY;

        if (animator != null)
            animator.SetTrigger("AttackMid");

        int[] indicesHuecos = new int[cantidadHuecos];
        for (int i = 0; i < cantidadHuecos; i++)
        {
            int indiceHueco;
            do
            {
                indiceHueco = Random.Range(0, cantidadPlumasOla);
            } while (System.Array.IndexOf(indicesHuecos, indiceHueco) != -1);

            indicesHuecos[i] = indiceHueco;
        }

        for (int i = 0; i < cantidadPlumasOla; i++)
        {
            if (jefe) yield break;

            if (System.Array.IndexOf(indicesHuecos, i) != -1)
                continue;

            float posY = inicioYCalculado + (i * espaciadoVertical);
            Vector3 posSpawn = new Vector3(inicioX, posY, 0);

            GameObject pluma = Instantiate(prefabPluma, posSpawn, Quaternion.identity);

            PlumaAcelerada acelerada = pluma.AddComponent<PlumaAcelerada>();
            acelerada.direccion = direccionOla;
            acelerada.velocidadInicial = velocidadInicialOla;
            acelerada.aceleracion = aceleracionOla;

            RotarPlumaHaciaDireccion(pluma, direccionOla);
        }

        if (sonidoAtaqueOla != null)
            AudioManager.Instance.PlaySFX(sonidoAtaqueOla);

        yield return new WaitForSeconds(1f);
    }

    private IEnumerator Ataque5_OlaAscendente()
    {
        if (jefe) yield break;

        Vector3 centroSpawn = puntoSpawnInferior != null ? puntoSpawnInferior.position : new Vector3(0, -6, 0);

        for (int i = 0; i < cantidadPlumasAscendentes; i++)
        {
            if (jefe) yield break;

            float randomX = centroSpawn.x + Random.Range(-rangoSpawnXInferior, rangoSpawnXInferior);
            Vector3 posSpawn = new Vector3(randomX, centroSpawn.y, 0);

            GameObject pluma = Instantiate(prefabPluma, posSpawn, Quaternion.Euler(0, 0, -90));

            PlumaAscendente ascendente = pluma.AddComponent<PlumaAscendente>();
            ascendente.velocidadAscenso = velocidadAscenso;
            ascendente.velocidadBusqueda = velocidadBusqueda;
            ascendente.distanciaBusqueda = distanciaBusqueda;
            ascendente.jugador = jugador;

            if (sonidoAtaqueAscendente != null)
                AudioManager.Instance.PlaySFX(sonidoAtaqueAscendente);

            yield return new WaitForSeconds(0.3f);
        }

        yield return new WaitForSeconds(1.5f);
    }

    private IEnumerator Ataque6_ExplosionPlumas()
    {
        if (jefe) yield break;

        if (animator != null)
            animator.SetTrigger("AttackUp");

        Vector3[] puntos = new Vector3[puntosExplosion];
        Vector3 centroExplosion = puntoExplosion != null ? puntoExplosion.position : Vector3.zero;

        for (int i = 0; i < puntosExplosion; i++)
        {
            float randomX = centroExplosion.x + Random.Range(-rangoExplosionX, rangoExplosionX);
            float randomY = centroExplosion.y + Random.Range(-rangoExplosionY, rangoExplosionY);
            puntos[i] = new Vector3(randomX, randomY, 0);
        }

        foreach (Vector3 punto in puntos)
        {
            if (jefe) yield break;

            float anguloPaso = 360f / cantidadPlumasPorExplosion;

            for (int i = 0; i < cantidadPlumasPorExplosion; i++)
            {
                float angulo = i * anguloPaso + Random.Range(-10f, 10f);
                Vector2 direccion = new Vector2(
                    Mathf.Cos(angulo * Mathf.Deg2Rad),
                    Mathf.Sin(angulo * Mathf.Deg2Rad)
                );

                GameObject pluma = Instantiate(prefabPluma, punto, Quaternion.identity);

                Rigidbody2D rb = pluma.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    float multiplicadorVelocidad = Random.Range(velocidadExplosionMin, velocidadExplosionMax);
                    rb.linearVelocity = direccion * velocidadPluma * multiplicadorVelocidad;
                }

                RotarPlumaHaciaDireccion(pluma, direccion);
            }

            if (sonidoExplosion != null)
                AudioManager.Instance.PlaySFX(sonidoExplosion);

            yield return new WaitForSeconds(0.3f);
        }

        yield return new WaitForSeconds(0.5f);
    }

    private void RotarPlumaHaciaDireccion(GameObject pluma, Vector2 direccion)
    {
        float angulo = Mathf.Atan2(direccion.y, direccion.x) * Mathf.Rad2Deg;
        pluma.transform.rotation = Quaternion.Euler(0, 0, angulo - 90);
    }
}

public class PlumaCayendo : MonoBehaviour
{
    public float velocidadCaida = 5f;
    public GameObject prefabPluma;
    public float velocidadPluma = 8f;

    private Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.linearVelocity = Vector2.down * velocidadCaida;
    }

    private void Update()
    {
        float angulo = Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angulo - 90);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Suelo") || collision.CompareTag("Player"))
        {
            DispararPlumasLaterales();
            Destroy(gameObject);
        }
    }

    private void DispararPlumasLaterales()
    {
        GameObject plumaIzquierda = Instantiate(prefabPluma, transform.position, Quaternion.identity);
        Rigidbody2D rbIzq = plumaIzquierda.GetComponent<Rigidbody2D>();
        if (rbIzq != null)
            rbIzq.linearVelocity = Vector2.left * velocidadPluma;
        plumaIzquierda.transform.rotation = Quaternion.Euler(0, 0, 180 - 90);

        GameObject plumaDerecha = Instantiate(prefabPluma, transform.position, Quaternion.identity);
        Rigidbody2D rbDer = plumaDerecha.GetComponent<Rigidbody2D>();
        if (rbDer != null)
            rbDer.linearVelocity = Vector2.right * velocidadPluma;
        plumaDerecha.transform.rotation = Quaternion.Euler(0, 0, -90);
    }
}

public class PlumaAcelerada : MonoBehaviour
{
    public Vector2 direccion;
    public float velocidadInicial;
    public float aceleracion;

    private Rigidbody2D rb;
    private float velocidadActual;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();

        rb.gravityScale = 0;
        velocidadActual = velocidadInicial;
        rb.linearVelocity = direccion * velocidadActual;
    }

    private void Update()
    {
        velocidadActual += aceleracion * Time.deltaTime;
        rb.linearVelocity = direccion * velocidadActual;

        float angulo = Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angulo - 90);
    }
}

public class PlumaAscendente : MonoBehaviour
{
    public float velocidadAscenso = 4f;
    public float velocidadBusqueda = 2f;
    public float distanciaBusqueda = 4f;
    public Transform jugador;

    private Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();

        rb.gravityScale = 0;
        rb.linearVelocity = Vector2.up * velocidadAscenso;
    }

    private void Update()
    {
        if (jugador != null)
        {
            float distancia = Vector3.Distance(transform.position, jugador.position);
            if (distancia < distanciaBusqueda)
            {
                Vector2 direccionJugador = (jugador.position - transform.position).normalized;
                Vector2 velocidadActual = rb.linearVelocity;
                rb.linearVelocity = Vector2.Lerp(velocidadActual, direccionJugador * velocidadAscenso, velocidadBusqueda * Time.deltaTime);
            }
        }

        float angulo = Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angulo - 90);
    }
}

public class PlumaTeledirigida : MonoBehaviour
{
    public Transform jugador;
    public float velocidad = 8f;
    public float fuerzaBusqueda = 3f;
    public float rangoActivacion = 5f;

    private Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();

        rb.gravityScale = 0;

        if (jugador != null)
        {
            Vector2 direccion = (jugador.position - transform.position).normalized;
            rb.linearVelocity = direccion * velocidad;
        }
    }

    private void Update()
    {
        if (jugador != null)
        {
            float distancia = Vector3.Distance(transform.position, jugador.position);
            if (distancia < rangoActivacion)
            {
                Vector2 direccionJugador = (jugador.position - transform.position).normalized;
                Vector2 velocidadActual = rb.linearVelocity.normalized * velocidad;
                rb.linearVelocity = Vector2.Lerp(velocidadActual, direccionJugador * velocidad, fuerzaBusqueda * Time.deltaTime);
            }
        }

        float angulo = Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angulo - 90);
    }
}