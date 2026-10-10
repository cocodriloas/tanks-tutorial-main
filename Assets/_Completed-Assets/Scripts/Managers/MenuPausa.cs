using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Complete
{
    // NUEVO SCRIPT: menú de pausa. Se abre y se cierra con la tecla ESC o P.o P.
    // IMPORTANTE: ponlo en un objeto que siempre esté activo (no en el panel que se oculta).
    public class MenuPausa : MonoBehaviour
    {
        public GameObject m_Panel;                      // El panel oscuro con los botones.
        public int m_EscenaMenu = 0;                    // Número de la escena del menú principal (Build Settings).

        public static bool EstaPausado { get; private set; }

        // Controles de los tanques que apagamos al pausar, para volver a encenderlos al continuar.
        private List<Behaviour> m_Desactivados = new List<Behaviour> ();


        private void Start ()
        {
            m_Panel.SetActive (false);
            EstaPausado = false;
            Time.timeScale = 1f;
        }


        private void Update ()
        {
            // ESC o P para pausar (en el navegador, ESC a veces lo usa el propio navegador).
            if (Input.GetKeyDown (KeyCode.Escape) || Input.GetKeyDown (KeyCode.P))
            {
                if (EstaPausado)
                    Continuar ();
                else
                    Pausar ();
            }
        }


        public void Pausar ()
        {
            EstaPausado = true;
            Time.timeScale = 0f;            // Congela el tiempo del juego (física, balas, rondas, power-ups).
            AudioListener.pause = true;     // Pausa los sonidos.
            m_Panel.SetActive (true);

            // Apagar los controles de los tanques para que no disparen ni se muevan con el teclado en pausa.
            m_Desactivados.Clear ();

            TankMovement[] movimientos = FindObjectsOfType<TankMovement> ();
            for (int i = 0; i < movimientos.Length; i++)
            {
                if (movimientos[i].enabled)
                {
                    movimientos[i].enabled = false;
                    m_Desactivados.Add (movimientos[i]);
                }
            }

            TankShooting[] disparos = FindObjectsOfType<TankShooting> ();
            for (int i = 0; i < disparos.Length; i++)
            {
                if (disparos[i].enabled)
                {
                    disparos[i].enabled = false;
                    m_Desactivados.Add (disparos[i]);
                }
            }
        }


        public void Continuar ()
        {
            // Volver a encender solo lo que estaba encendido antes de pausar.
            for (int i = 0; i < m_Desactivados.Count; i++)
            {
                if (m_Desactivados[i] != null)
                    m_Desactivados[i].enabled = true;
            }
            m_Desactivados.Clear ();

            Time.timeScale = 1f;
            AudioListener.pause = false;
            m_Panel.SetActive (false);
            EstaPausado = false;
        }


        public void IrAlMenu ()
        {
            RestaurarTiempo ();
            SceneManager.LoadScene (m_EscenaMenu);
        }


        public void Salir ()
        {
            Debug.Log ("Saliendo del juego...");
            Application.Quit ();
        }


        private void RestaurarTiempo ()
        {
            Time.timeScale = 1f;
            AudioListener.pause = false;
            EstaPausado = false;
        }


        // Seguridad: si se cambia de escena estando en pausa, el tiempo vuelve a la normalidad.
        private void OnDestroy ()
        {
            RestaurarTiempo ();
        }
    }
}
