using UnityEngine;

namespace Complete
{
    public enum TipoPowerUp
    {
        Vida,
        Velocidad,
        Escudo,
        Disparo,    // NUEVO: disparo doble o bala grande.
        Salto       // NUEVO: doble salto.
    }

    // NUEVO SCRIPT: objeto que flota y gira en el mapa. Cuando un tanque lo toca, le da un efecto.
    public class PowerUp : MonoBehaviour
    {
        public TipoPowerUp m_Tipo = TipoPowerUp.Vida;
        public float m_CantidadVida = 50f;              // Vida que recupera (el tanque tiene 100 como máximo).
        public float m_MultiplicadorVelocidad = 1.6f;   // 1.6 = 60% más rápido.
        public float m_Duracion = 6f;                   // Segundos que dura el efecto (velocidad, escudo, disparo o salto).
        public ModoDisparo m_ModoDisparo = ModoDisparo.Doble;   // NUEVO: solo para el power-up de Disparo.
        public AudioClip m_Sonido;                      // NUEVO (opcional): tu propio sonido. Si lo dejas vacío, se usa uno generado.
        public float m_Volumen = 0.8f;                  // NUEVO: volumen del sonido al recogerlo.
        public float m_VelocidadGiro = 90f;             // Giro de la caja (grados por segundo).
        public float m_AlturaFlotar = 0.25f;            // Cuánto sube y baja.

        private Vector3 m_PosicionInicial;


        private void Start ()
        {
            m_PosicionInicial = transform.position;

            // Los colliders deben ser "trigger" para que los tanques lo atraviesen y lo recojan.
            Collider[] colliders = GetComponentsInChildren<Collider> ();
            for (int i = 0; i < colliders.Length; i++)
                colliders[i].isTrigger = true;
        }


        private void Update ()
        {
            // Girar y flotar.
            transform.Rotate (0f, m_VelocidadGiro * Time.deltaTime, 0f, Space.World);
            float subeBaja = Mathf.Sin (Time.time * 2f) * m_AlturaFlotar;
            transform.position = m_PosicionInicial + new Vector3 (0f, subeBaja, 0f);
        }


        private void OnTriggerEnter (Collider other)
        {
            // Solo lo recogen los tanques.
            Rigidbody cuerpo = other.attachedRigidbody;
            if (cuerpo == null)
                return;

            TankHealth salud = cuerpo.GetComponent<TankHealth> ();
            if (salud == null)
                return;

            if (m_Tipo == TipoPowerUp.Vida)
            {
                salud.Curar (m_CantidadVida);
            }
            else
            {
                EfectosTanque efectos = cuerpo.GetComponent<EfectosTanque> ();
                if (efectos == null)
                    efectos = cuerpo.gameObject.AddComponent<EfectosTanque> ();

                if (m_Tipo == TipoPowerUp.Velocidad)
                    efectos.ActivarVelocidad (m_MultiplicadorVelocidad, m_Duracion);
                else if (m_Tipo == TipoPowerUp.Escudo)
                    efectos.ActivarEscudo (m_Duracion);
                else if (m_Tipo == TipoPowerUp.Disparo)
                    efectos.ActivarDisparo (m_ModoDisparo, m_Duracion);
                else if (m_Tipo == TipoPowerUp.Salto)
                    efectos.ActivarSalto (m_Duracion);
            }

            // NUEVO: sonido para saber que se recogió.
            SonidosPowerUp.Reproducir (m_Tipo, m_Sonido, m_Volumen);

            Destroy (gameObject);
        }
    }
}
