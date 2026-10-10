using UnityEngine;
using UnityEngine.UI;

// NUEVO SCRIPT: muestra un tanque 3D girando en el menú principal.
// Se pone en un objeto vacío que tenga como hijo el modelo del tanque.
public class TanqueMenu : MonoBehaviour
{
    public Color m_Color = Color.red;          // Color del tanque (el mismo del jugador en el juego).
    public Dropdown m_Selector;                // Selector de ese jugador: si eligen APAGADO, el tanque se oculta.
    public float m_VelocidadGiro = 45f;        // Grados por segundo.
    public float m_Inclinacion = -20f;         // Inclinación hacia la cámara para ver el tanque desde arriba.

    private float m_Angulo;
    private Renderer[] m_Renderers;


    private void Start ()
    {
        m_Renderers = GetComponentsInChildren<Renderer> ();

        // Pintar el tanque igual que en el juego.
        for (int i = 0; i < m_Renderers.Length; i++)
            m_Renderers[i].material.color = m_Color;

        // Cada tanque empieza en un ángulo distinto para que no giren iguales.
        m_Angulo = Random.Range (0f, 360f);
    }


    private void Update ()
    {
        // Girar sobre su propio eje, con una pequeña inclinación hacia la cámara.
        m_Angulo += m_VelocidadGiro * Time.deltaTime;
        transform.rotation = Quaternion.Euler (m_Inclinacion, 0f, 0f) * Quaternion.Euler (0f, m_Angulo, 0f);

        // Ocultar el tanque si ese jugador está APAGADO (opción 2 del selector).
        bool visible = m_Selector == null || m_Selector.value != (int)TipoJugador.Apagado;

        for (int i = 0; i < m_Renderers.Length; i++)
            m_Renderers[i].enabled = visible;
    }
}
