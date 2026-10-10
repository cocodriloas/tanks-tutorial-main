using UnityEngine;

namespace Complete
{
    public enum ModoDisparo
    {
        Doble,      // Dispara 2 balas a la vez.
        Grande,     // Dispara 1 bala más grande y más fuerte.
        Ambos       // 2 balas grandes.
    }

    // Maneja los efectos temporales de los power-ups sobre un tanque:
    // velocidad, escudo, disparo mejorado (NUEVO) y doble salto (NUEVO).
    // No hace falta ponerlo a mano: el power-up lo agrega solo.
    public class EfectosTanque : MonoBehaviour
    {
        public float m_FuerzaSalto = 8f;                // Qué tan alto salta.

        // Estos valores los lee TankShooting al disparar.
        public bool DisparoDoble { get; private set; }
        public bool BalaGrande { get; private set; }
        public bool PuedeSaltar { get; private set; }

        private TankMovement m_Movimiento;
        private TankHealth m_Salud;
        private Rigidbody m_Cuerpo;

        private float m_VelocidadBase;
        private float m_GiroBase;
        private RigidbodyConstraints m_RestriccionesBase;
        private bool m_GravedadBase;
        private int m_MascaraSuelo;

        private float m_FinVelocidad;
        private float m_FinEscudo;
        private float m_FinDisparo;
        private float m_FinSalto;

        private int m_SaltosRestantes;
        private float m_AlturaSuelo;
        private float m_SiguienteSaltoCPU;
        private float m_SegundoSaltoCPU;

        private Light m_LuzVelocidad;
        private Light m_LuzEscudo;
        private Light m_LuzDisparo;
        private Light m_LuzSalto;


        private void Awake ()
        {
            m_Movimiento = GetComponent<TankMovement> ();
            m_Salud = GetComponent<TankHealth> ();
            m_Cuerpo = GetComponent<Rigidbody> ();

            // Guardar los valores normales para poder volver a ellos.
            m_VelocidadBase = m_Movimiento.m_Speed;
            m_GiroBase = m_Movimiento.m_TurnSpeed;
            m_RestriccionesBase = m_Cuerpo.constraints;
            m_GravedadBase = m_Cuerpo.useGravity;

            // Para detectar el suelo ignoramos a los tanques y a los power-ups.
            int capaTanques = LayerMask.NameToLayer ("Players");
            m_MascaraSuelo = ~(1 << 2);
            if (capaTanques >= 0)
                m_MascaraSuelo &= ~(1 << capaTanques);

            // Luces de colores alrededor del tanque para que se note cada efecto.
            m_LuzVelocidad = CrearLuz ("LuzVelocidad", new Color (1f, 0.85f, 0.1f));
            m_LuzEscudo = CrearLuz ("LuzEscudo", new Color (0.2f, 0.7f, 1f));
            m_LuzDisparo = CrearLuz ("LuzDisparo", new Color (1f, 0.42f, 0f));
            m_LuzSalto = CrearLuz ("LuzSalto", new Color (0.7f, 0.4f, 1f));
        }


        private Light CrearLuz (string nombre, Color color)
        {
            GameObject objeto = new GameObject (nombre);
            objeto.transform.SetParent (transform, false);
            objeto.transform.localPosition = new Vector3 (0f, 2f, 0f);

            Light luz = objeto.AddComponent<Light> ();
            luz.type = LightType.Point;
            luz.color = color;
            luz.range = 6f;
            luz.intensity = 4f;
            luz.enabled = false;

            return luz;
        }


        // ---------- ACTIVAR EFECTOS (los llama el power-up) ----------

        public void ActivarVelocidad (float multiplicador, float duracion)
        {
            m_Movimiento.m_Speed = m_VelocidadBase * multiplicador;
            m_Movimiento.m_TurnSpeed = m_GiroBase * Mathf.Min (multiplicador, 1.3f);
            m_FinVelocidad = Time.time + duracion;
            m_LuzVelocidad.enabled = true;
        }


        public void ActivarEscudo (float duracion)
        {
            m_Salud.m_Escudo = true;
            m_FinEscudo = Time.time + duracion;
            m_LuzEscudo.enabled = true;
        }


        // NUEVO: disparo doble y/o bala grande.
        public void ActivarDisparo (ModoDisparo modo, float duracion)
        {
            DisparoDoble = (modo != ModoDisparo.Grande);
            BalaGrande = (modo != ModoDisparo.Doble);
            m_FinDisparo = Time.time + duracion;
            m_LuzDisparo.enabled = true;
        }


        // NUEVO: permite saltar 2 veces (doble salto).
        public void ActivarSalto (float duracion)
        {
            if (!PuedeSaltar)
                m_AlturaSuelo = transform.position.y;   // Altura normal del suelo.

            PuedeSaltar = true;
            m_SaltosRestantes = 2;
            m_FinSalto = Time.time + duracion;
            m_LuzSalto.enabled = true;

            // Liberar el movimiento vertical (el tanque normalmente lo tiene bloqueado)
            // y asegurar que no se voltee.
            m_Cuerpo.constraints = (m_RestriccionesBase & ~RigidbodyConstraints.FreezePositionY)
                                   | RigidbodyConstraints.FreezeRotationX
                                   | RigidbodyConstraints.FreezeRotationZ;
            m_Cuerpo.useGravity = true;

            m_SiguienteSaltoCPU = Time.time + Random.Range (1f, 2f);
            m_SegundoSaltoCPU = 0f;
        }


        // ---------- CADA FRAME ----------

        private void Update ()
        {
            // Cuando se acaba el tiempo, quitar el efecto.
            if (m_LuzVelocidad.enabled && Time.time >= m_FinVelocidad)
                QuitarVelocidad ();

            if (m_LuzEscudo.enabled && Time.time >= m_FinEscudo)
                QuitarEscudo ();

            if (m_LuzDisparo.enabled && Time.time >= m_FinDisparo)
                QuitarDisparo ();

            if (PuedeSaltar)
                ActualizarSalto ();
        }


        private void ActualizarSalto ()
        {
            bool enSuelo = EstaEnSuelo ();

            // Al tocar el suelo se recuperan los 2 saltos.
            if (enSuelo)
                m_SaltosRestantes = 2;

            // Si ya se acabó el tiempo: no más saltos, y cuando vuelva al suelo normal se bloquea la altura otra vez.
            if (Time.time >= m_FinSalto)
            {
                m_LuzSalto.enabled = false;

                if (enSuelo && Mathf.Abs (transform.position.y - m_AlturaSuelo) < 0.3f)
                    QuitarSalto ();

                return;
            }

            // Sin control (pausa, inicio o fin de ronda) no se salta.
            if (!m_Movimiento.enabled)
                return;

            if (m_Movimiento.m_EsCPU)
                SaltoCPU ();
            else if (Input.GetKeyDown (TeclaSalto ()))
                Saltar ();
        }


        private void Saltar ()
        {
            if (m_SaltosRestantes <= 0)
                return;

            Vector3 velocidad = m_Cuerpo.velocity;
            velocidad.y = m_FuerzaSalto;
            m_Cuerpo.velocity = velocidad;

            m_SaltosRestantes--;
        }


        // La CPU salta cada cierto tiempo, haciendo el doble salto.
        private void SaltoCPU ()
        {
            if (m_SegundoSaltoCPU > 0f && Time.time >= m_SegundoSaltoCPU)
            {
                Saltar ();
                m_SegundoSaltoCPU = 0f;
            }

            if (Time.time >= m_SiguienteSaltoCPU)
            {
                Saltar ();
                m_SegundoSaltoCPU = Time.time + 0.4f;
                m_SiguienteSaltoCPU = Time.time + Random.Range (2.5f, 4.5f);
            }
        }


        // Tecla de salto de cada jugador.
        // Jugador 2 usa Shift derecho (no Ctrl): en el navegador, Ctrl + W cerraría la pestaña del juego
        // si el Jugador 1 está presionando W al mismo tiempo.
        private KeyCode TeclaSalto ()
        {
            switch (m_Movimiento.m_PlayerNumber)
            {
                case 2:
                    return KeyCode.RightShift;
                case 3:
                    return KeyCode.O;
                default:
                    return KeyCode.Q;
            }
        }


        // Lanza un rayo hacia abajo para saber si el tanque está apoyado en algo.
        private bool EstaEnSuelo ()
        {
            if (m_Cuerpo.velocity.y > 0.5f)
                return false;

            Vector3 origen = transform.position + Vector3.up * 0.5f;
            return Physics.Raycast (origen, Vector3.down, 0.8f, m_MascaraSuelo, QueryTriggerInteraction.Ignore);
        }


        // ---------- QUITAR EFECTOS ----------

        private void QuitarVelocidad ()
        {
            m_Movimiento.m_Speed = m_VelocidadBase;
            m_Movimiento.m_TurnSpeed = m_GiroBase;
            m_LuzVelocidad.enabled = false;
        }


        private void QuitarEscudo ()
        {
            m_Salud.m_Escudo = false;
            m_LuzEscudo.enabled = false;
        }


        private void QuitarDisparo ()
        {
            DisparoDoble = false;
            BalaGrande = false;
            m_LuzDisparo.enabled = false;
        }


        private void QuitarSalto ()
        {
            PuedeSaltar = false;
            m_LuzSalto.enabled = false;
            m_Cuerpo.constraints = m_RestriccionesBase;
            m_Cuerpo.useGravity = m_GravedadBase;
        }


        // Cuando el tanque muere o empieza una nueva ronda, se quitan todos los efectos.
        private void OnDisable ()
        {
            QuitarVelocidad ();
            QuitarEscudo ();
            QuitarDisparo ();
            QuitarSalto ();
        }
    }
}
