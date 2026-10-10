using UnityEngine;

namespace Complete
{
    // NUEVO SCRIPT: inteligencia artificial básica para un tanque manejado por la computadora.
    // Busca al enemigo más cercano, gira hacia él, mantiene una distancia de combate,
    // calcula la fuerza del disparo según la distancia y se "desatasca" si choca con algo.
    // No hace falta agregarlo a mano: el TankManager lo agrega solo a los tanques CPU.
    [RequireComponent (typeof (TankMovement))]
    [RequireComponent (typeof (TankShooting))]
    public class TankAI : MonoBehaviour
    {
        public float m_DistanciaIdeal = 15f;        // Distancia a la que le gusta pelear.
        public float m_TiempoEntreDisparos = 2f;    // Segundos (promedio) entre disparos.
        public float m_ErrorPunteria = 0.1f;        // 0 = puntería perfecta, 0.1 = hasta 10% de error.
        public float m_AnguloParaDisparar = 8f;     // Qué tan alineado debe estar con el enemigo para disparar.

        private TankMovement m_Movimiento;
        private TankShooting m_Disparo;
        private Transform m_Objetivo;
        private float m_SiguienteBusqueda;
        private float m_SiguienteDisparo;
        private Vector3 m_UltimaPosicion;
        private float m_TemporizadorAtasco;
        private float m_FinManiobra;
        private float m_GiroManiobra;


        private void Awake ()
        {
            m_Movimiento = GetComponent<TankMovement> ();
            m_Disparo = GetComponent<TankShooting> ();
        }


        private void OnEnable ()
        {
            // Se llama al inicio de cada ronda: reiniciamos todo.
            m_Objetivo = null;
            m_SiguienteBusqueda = 0f;
            m_SiguienteDisparo = Time.time + Random.Range (1.5f, 3f);
            m_UltimaPosicion = transform.position;
            m_TemporizadorAtasco = 0f;
            m_FinManiobra = 0f;
        }


        private void Update ()
        {
            // Si el GameManager desactivó el control (inicio o fin de ronda), no hacemos nada.
            if (!m_Movimiento.enabled)
            {
                Mover (0f, 0f);
                return;
            }

            // Buscar al enemigo más cercano cada medio segundo.
            if (Time.time >= m_SiguienteBusqueda || m_Objetivo == null || !m_Objetivo.gameObject.activeInHierarchy)
            {
                BuscarObjetivo ();
                m_SiguienteBusqueda = Time.time + 0.5f;
            }

            if (m_Objetivo == null)
            {
                Mover (0f, 0f);
                return;
            }

            // Si está haciendo la maniobra para desatascarse: retroceder girando.
            if (Time.time < m_FinManiobra)
            {
                Mover (-1f, m_GiroManiobra);
                return;
            }

            // Dirección y distancia hacia el enemigo (solo en el plano, sin altura).
            Vector3 direccion = m_Objetivo.position - transform.position;
            direccion.y = 0f;
            float distancia = direccion.magnitude;
            float angulo = Vector3.SignedAngle (transform.forward, direccion, Vector3.up);

            // Girar hacia el enemigo (más fuerte mientras más desalineado esté).
            float girar = Mathf.Clamp (angulo / 30f, -1f, 1f);

            // Acercarse si está lejos, alejarse si está demasiado cerca.
            float mover = 0f;
            if (distancia > m_DistanciaIdeal + 3f)
                mover = Mathf.Abs (angulo) < 60f ? 1f : 0.3f;
            else if (distancia < m_DistanciaIdeal - 5f)
                mover = -0.6f;

            Mover (mover, girar);
            RevisarSiEstaAtascado (mover);

            // Disparar cuando esté apuntando al enemigo y haya pasado el tiempo de espera.
            if (Time.time >= m_SiguienteDisparo && Mathf.Abs (angulo) < m_AnguloParaDisparar)
            {
                if (IntentarDisparar ())
                    m_SiguienteDisparo = Time.time + m_TiempoEntreDisparos * Random.Range (0.7f, 1.3f);
            }
        }


        private void Mover (float mover, float girar)
        {
            m_Movimiento.m_EntradaCPUMover = mover;
            m_Movimiento.m_EntradaCPUGirar = girar;
        }


        private void BuscarObjetivo ()
        {
            m_Objetivo = null;
            float mejorDistancia = Mathf.Infinity;

            // Todos los tanques activos de la escena (humanos y CPU).
            TankMovement[] tanques = FindObjectsOfType<TankMovement> ();

            for (int i = 0; i < tanques.Length; i++)
            {
                if (tanques[i] == m_Movimiento || !tanques[i].gameObject.activeInHierarchy)
                    continue;

                float d = (tanques[i].transform.position - transform.position).sqrMagnitude;
                if (d < mejorDistancia)
                {
                    mejorDistancia = d;
                    m_Objetivo = tanques[i].transform;
                }
            }
        }


        private void RevisarSiEstaAtascado (float mover)
        {
            m_TemporizadorAtasco += Time.deltaTime;

            // Cada 0.75 segundos revisamos cuánto avanzó.
            if (m_TemporizadorAtasco < 0.75f)
                return;

            float avance = (transform.position - m_UltimaPosicion).magnitude;

            // Si quería avanzar pero casi no se movió, está chocando con algo.
            if (Mathf.Abs (mover) > 0.5f && avance < 1f)
            {
                m_FinManiobra = Time.time + 1f;
                m_GiroManiobra = Random.value < 0.5f ? -1f : 1f;
            }

            m_UltimaPosicion = transform.position;
            m_TemporizadorAtasco = 0f;
        }


        private bool IntentarDisparar ()
        {
            Transform canon = m_Disparo.m_FireTransform;

            // Distancia desde la punta del cañón hasta el enemigo.
            Vector3 haciaObjetivo = m_Objetivo.position - canon.position;
            haciaObjetivo.y = 0f;
            float distancia = haciaObjetivo.magnitude;

            float fuerza = CalcularFuerza (distancia);

            // Si el enemigo está fuera de alcance, no dispara (se seguirá acercando).
            if (fuerza < 0f)
                return false;

            // Un poco de error para que no sea imposible de vencer.
            fuerza *= 1f + Random.Range (-m_ErrorPunteria, m_ErrorPunteria);

            m_Disparo.DispararCPU (fuerza);
            return true;
        }


        // Física de tiro parabólico: alcance = v² · sen(2θ) / g  →  v = √(alcance · g / sen(2θ))
        private float CalcularFuerza (float distancia)
        {
            Vector3 frente = m_Disparo.m_FireTransform.forward;
            float anguloDisparo = Mathf.Asin (Mathf.Clamp (frente.y, -1f, 1f));   // ángulo del cañón hacia arriba (radianes)
            float sen2 = Mathf.Sin (2f * anguloDisparo);
            float gravedad = -Physics.gravity.y;

            if (sen2 < 0.05f)
                return m_Disparo.m_MaxLaunchForce;

            float velocidad = Mathf.Sqrt (distancia * gravedad / sen2);

            if (velocidad > m_Disparo.m_MaxLaunchForce * 1.05f)
                return -1f;

            return Mathf.Clamp (velocidad, m_Disparo.m_MinLaunchForce, m_Disparo.m_MaxLaunchForce);
        }
    }
}
