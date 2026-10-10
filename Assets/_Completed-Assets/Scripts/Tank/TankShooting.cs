using UnityEngine;
using UnityEngine.UI;

namespace Complete
{
    public class TankShooting : MonoBehaviour
    {
        public int m_PlayerNumber = 1;              // Used to identify the different players.
        public Rigidbody m_Shell;                   // Prefab of the shell.
        public Transform m_FireTransform;           // A child of the tank where the shells are spawned.
        public Slider m_AimSlider;                  // A child of the tank that displays the current launch force.
        public AudioSource m_ShootingAudio;         // Reference to the audio source used to play the shooting audio. NB: different to the movement audio source.
        public AudioClip m_ChargingClip;            // Audio that plays when each shot is charging up.
        public AudioClip m_FireClip;                // Audio that plays when each shot is fired.
        public float m_MinLaunchForce = 15f;        // The force given to the shell if the fire button is not held.
        public float m_MaxLaunchForce = 30f;        // The force given to the shell if the fire button is held for the max charge time.
        public float m_MaxChargeTime = 0.75f;       // How long the shell can charge for before it is fired at max force.

        // NUEVO: si es true, este tanque lo controla la computadora (dispara desde TankAI).
        [HideInInspector] public bool m_EsCPU = false;

        private string m_FireButton;                // The input axis that is used for launching shells.
        private float m_CurrentLaunchForce;         // The force that will be given to the shell when the fire button is released.
        private float m_ChargeSpeed;                // How fast the launch force increases, based on the max charge time.
        private bool m_Fired;                       // Whether or not the shell has been launched with this button press.


        private void OnEnable()
        {
            // When the tank is turned on, reset the launch force and the UI
            m_CurrentLaunchForce = m_MinLaunchForce;
            m_AimSlider.value = m_MinLaunchForce;
        }


        private void Start ()
        {
            // The fire axis is based on the player number.
            m_FireButton = "Fire" + m_PlayerNumber;

            // The rate that the launch force charges up is the range of possible forces by the max charge time.
            m_ChargeSpeed = (m_MaxLaunchForce - m_MinLaunchForce) / m_MaxChargeTime;
        }


        private void Update ()
        {
            // The slider should have a default value of the minimum launch force.
            m_AimSlider.value = m_MinLaunchForce;

            // NUEVO: la computadora no usa el teclado.
            if (m_EsCPU)
                return;

            // If the max force has been exceeded and the shell hasn't yet been launched...
            if (m_CurrentLaunchForce >= m_MaxLaunchForce && !m_Fired)
            {
                // ... use the max force and launch the shell.
                m_CurrentLaunchForce = m_MaxLaunchForce;
                Fire ();
            }
            // Otherwise, if the fire button has just started being pressed...
            else if (BotonPresionado ())
            {
                // ... reset the fired flag and reset the launch force.
                m_Fired = false;
                m_CurrentLaunchForce = m_MinLaunchForce;

                // Change the clip to the charging clip and start it playing.
                m_ShootingAudio.clip = m_ChargingClip;
                m_ShootingAudio.Play ();
            }
            // Otherwise, if the fire button is being held and the shell hasn't been launched yet...
            else if (BotonMantenido () && !m_Fired)
            {
                // Increment the launch force and update the slider.
                m_CurrentLaunchForce += m_ChargeSpeed * Time.deltaTime;

                m_AimSlider.value = m_CurrentLaunchForce;
            }
            // Otherwise, if the fire button is released and the shell hasn't been launched yet...
            else if (BotonSoltado () && !m_Fired)
            {
                // ... launch the shell.
                Fire ();
            }
        }


        // NUEVO: el Jugador 3 dispara con la tecla U; los jugadores 1 y 2 como en el original.
        private bool BotonPresionado ()
        {
            if (m_PlayerNumber == 3)
                return Input.GetKeyDown (KeyCode.U);

            return Input.GetButtonDown (m_FireButton);
        }


        private bool BotonMantenido ()
        {
            if (m_PlayerNumber == 3)
                return Input.GetKey (KeyCode.U);

            return Input.GetButton (m_FireButton);
        }


        private bool BotonSoltado ()
        {
            if (m_PlayerNumber == 3)
                return Input.GetKeyUp (KeyCode.U);

            return Input.GetButtonUp (m_FireButton);
        }


        // NUEVO: la computadora llama a este método para disparar con una fuerza exacta.
        public void DispararCPU (float fuerza)
        {
            if (!enabled)
                return;

            m_CurrentLaunchForce = Mathf.Clamp (fuerza, m_MinLaunchForce, m_MaxLaunchForce);
            Fire ();
        }


        private void Fire ()
        {
            // Set the fired flag so only Fire is only called once.
            m_Fired = true;

            // NUEVO: revisar si el tanque tiene activo el power-up de disparo.
            EfectosTanque efectos = GetComponent<EfectosTanque> ();
            bool doble = efectos != null && efectos.DisparoDoble;
            bool grande = efectos != null && efectos.BalaGrande;

            if (doble)
            {
                // Dos balas, una a cada lado, un poco abiertas.
                Rigidbody balaIzquierda = CrearBala (-1, grande);
                Rigidbody balaDerecha = CrearBala (1, grande);

                // Que las dos balas no choquen entre ellas al salir.
                IgnorarEntreSi (balaIzquierda, balaDerecha);
            }
            else
            {
                CrearBala (0, grande);
            }

            // Change the clip to the firing clip and play it.
            m_ShootingAudio.clip = m_FireClip;
            m_ShootingAudio.Play ();

            // Reset the launch force.  This is a precaution in case of missing button events.
            m_CurrentLaunchForce = m_MinLaunchForce;
        }


        // NUEVO: crea una bala. lado = -1 (izquierda), 0 (centro) o 1 (derecha).
        private Rigidbody CrearBala (int lado, bool grande)
        {
            Vector3 posicion = m_FireTransform.position + m_FireTransform.right * (0.6f * lado);
            Quaternion rotacion = Quaternion.AngleAxis (5f * lado, Vector3.up) * m_FireTransform.rotation;

            Rigidbody bala = Instantiate (m_Shell, posicion, rotacion) as Rigidbody;

            if (grande)
            {
                // Bala más grande, con explosión más amplia y más fuerte.
                bala.transform.localScale *= 1.8f;

                ShellExplosion explosion = bala.GetComponent<ShellExplosion> ();
                if (explosion != null)
                {
                    explosion.m_ExplosionRadius *= 1.5f;
                    explosion.m_MaxDamage *= 1.3f;
                    explosion.m_ExplosionForce *= 1.3f;
                }
            }

            bala.velocity = m_CurrentLaunchForce * (rotacion * Vector3.forward);
            return bala;
        }


        // NUEVO: evita que dos balas se detecten entre sí.
        private void IgnorarEntreSi (Rigidbody a, Rigidbody b)
        {
            Collider[] collidersA = a.GetComponentsInChildren<Collider> ();
            Collider[] collidersB = b.GetComponentsInChildren<Collider> ();

            for (int i = 0; i < collidersA.Length; i++)
            {
                for (int j = 0; j < collidersB.Length; j++)
                    Physics.IgnoreCollision (collidersA[i], collidersB[j]);
            }
        }
    }
}
