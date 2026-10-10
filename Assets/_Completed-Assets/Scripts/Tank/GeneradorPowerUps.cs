using System.Collections.Generic;
using UnityEngine;

namespace Complete
{
    // NUEVO SCRIPT: hace aparecer power-ups al azar en los puntos que tú elijas del mapa.
    public class GeneradorPowerUps : MonoBehaviour
    {
        public GameObject[] m_Prefabs;          // Los power-ups que pueden aparecer (vida, velocidad, escudo).
        public Transform[] m_Puntos;            // Lugares del mapa donde pueden aparecer.
        public float m_TiempoEntre = 8f;        // Segundos entre cada aparición.
        public int m_MaximoEnMapa = 3;          // Máximo de power-ups al mismo tiempo.
        public float m_Altura = 1f;             // Altura sobre el suelo.

        private GameObject[] m_Ocupados;        // Qué power-up hay en cada punto (o vacío).
        private float m_SiguienteAparicion;
        private const int CAPA_POWERUP = 2;     // Capa "Ignore Raycast" de Unity.


        private void Start ()
        {
            m_Ocupados = new GameObject[m_Puntos.Length];
            m_SiguienteAparicion = Time.time + m_TiempoEntre;

            // Que las balas y el escenario atraviesen los power-ups: solo los tanques (capa "Players") los tocan.
            int capaTanques = LayerMask.NameToLayer ("Players");
            for (int capa = 0; capa < 32; capa++)
            {
                if (capa != capaTanques)
                    Physics.IgnoreLayerCollision (CAPA_POWERUP, capa, true);
            }
        }


        private void Update ()
        {
            if (Time.time < m_SiguienteAparicion)
                return;

            m_SiguienteAparicion = Time.time + m_TiempoEntre;
            Generar ();
        }


        private void Generar ()
        {
            if (m_Prefabs.Length == 0 || m_Puntos.Length == 0)
                return;

            // Contar cuántos hay y qué puntos están libres.
            // (Un power-up recogido se destruye, y Unity lo considera "null").
            List<int> libres = new List<int> ();
            int enMapa = 0;

            for (int i = 0; i < m_Puntos.Length; i++)
            {
                if (m_Ocupados[i] == null)
                    libres.Add (i);
                else
                    enMapa++;
            }

            if (enMapa >= m_MaximoEnMapa || libres.Count == 0)
                return;

            // Elegir un punto libre y un tipo de power-up al azar.
            int punto = libres[Random.Range (0, libres.Count)];
            GameObject prefab = m_Prefabs[Random.Range (0, m_Prefabs.Length)];

            Vector3 posicion = m_Puntos[punto].position + Vector3.up * m_Altura;
            GameObject nuevo = Instantiate (prefab, posicion, Quaternion.identity);

            PonerCapa (nuevo.transform, CAPA_POWERUP);
            m_Ocupados[punto] = nuevo;
        }


        private void PonerCapa (Transform objeto, int capa)
        {
            objeto.gameObject.layer = capa;

            foreach (Transform hijo in objeto)
                PonerCapa (hijo, capa);
        }
    }
}
