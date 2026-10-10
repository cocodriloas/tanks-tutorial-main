using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// NUEVO SCRIPT: menú principal.
// Permite elegir para cada jugador: HUMANO, CPU o APAGADO, y luego iniciar la partida.
public class MenuPrincipal : MonoBehaviour
{
    public Dropdown[] m_SelectoresJugador;       // Los 3 menús desplegables (Jugador 1, 2 y 3), en orden.
    public Text m_TextoAviso;                    // Texto para avisar errores (por ejemplo, si hay menos de 2 tanques).
    public string m_EscenaJuego = "MapaNieve";   // Nombre exacto de la escena del juego.


    private void Start ()
    {
        // Las opciones deben estar en el mismo orden que TipoJugador (Humano = 0, CPU = 1, Apagado = 2).
        List<string> opciones = new List<string> { "HUMANO", "CPU", "APAGADO" };

        for (int i = 0; i < m_SelectoresJugador.Length; i++)
        {
            m_SelectoresJugador[i].ClearOptions ();
            m_SelectoresJugador[i].AddOptions (opciones);

            // Mostrar lo que se eligió la última vez (o los valores por defecto).
            m_SelectoresJugador[i].value = (int)ConfiguracionPartida.ObtenerTipo (i);
            m_SelectoresJugador[i].RefreshShownValue ();
        }

        if (m_TextoAviso != null)
            m_TextoAviso.text = "";
    }


    public void Jugar ()
    {
        TipoJugador[] elegidos = new TipoJugador[m_SelectoresJugador.Length];
        int tanquesActivos = 0;

        for (int i = 0; i < m_SelectoresJugador.Length; i++)
        {
            elegidos[i] = (TipoJugador)m_SelectoresJugador[i].value;

            if (elegidos[i] != TipoJugador.Apagado)
                tanquesActivos++;
        }

        // Se necesitan al menos 2 tanques para pelear.
        if (tanquesActivos < 2)
        {
            if (m_TextoAviso != null)
                m_TextoAviso.text = "Se necesitan al menos 2 tanques";

            return;
        }

        // Guardar la elección y cargar el mapa.
        ConfiguracionPartida.Jugadores = elegidos;
        SceneManager.LoadScene (m_EscenaJuego);
    }


    public void Salir ()
    {
        Debug.Log ("Saliendo del juego...");
        Application.Quit ();
    }
}
