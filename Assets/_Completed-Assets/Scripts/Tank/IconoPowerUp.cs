using UnityEngine;

namespace Complete
{
    // NUEVO SCRIPT: reemplaza la figura simple (cubo, cilindro o esfera) del power-up
    // por un ícono armado con piezas: una CRUZ para vida, flechas ">>" para velocidad,
    // un ESCUDO para escudo, BALAS para disparo y flechas "^^" para salto. Además agrega un anillo giratorio, una luz y un efecto de latido.
    // Solo hay que agregarlo al prefab del power-up (junto al script PowerUp).
    [RequireComponent (typeof (PowerUp))]
    public class IconoPowerUp : MonoBehaviour
    {
        public bool m_ConAnillo = true;         // Anillo de puntos alrededor.
        public bool m_ConLuz = true;            // Luz de color que ilumina la nieve.
        public float m_Latido = 0.08f;          // Cuánto crece y se encoge (0 = nada).

        private Transform m_Icono;
        private Transform m_Anillo;
        private Vector3 m_EscalaIcono;
        private Material m_Material;
        private Material m_MaterialClaro;


        private void Awake ()
        {
            // Usamos el material que ya tiene el prefab (verde, amarillo o azul) y ocultamos la figura simple.
            MeshRenderer figura = GetComponent<MeshRenderer> ();
            m_Material = figura.sharedMaterial;
            figura.enabled = false;

            // Una versión más clara del mismo color para los detalles.
            m_MaterialClaro = new Material (m_Material);
            m_MaterialClaro.color = Color.Lerp (m_Material.color, Color.white, 0.7f);

            // Contenedor del ícono. Compensa la escala del prefab para que las piezas no se deformen.
            Vector3 escalaPrefab = transform.localScale;
            m_Icono = new GameObject ("Icono").transform;
            m_Icono.SetParent (transform, false);
            m_EscalaIcono = new Vector3 (1f / escalaPrefab.x, 1f / escalaPrefab.y, 1f / escalaPrefab.z);
            m_Icono.localScale = m_EscalaIcono;

            PowerUp powerUp = GetComponent<PowerUp> ();

            if (powerUp.m_Tipo == TipoPowerUp.Vida)
                ConstruirCruz ();
            else if (powerUp.m_Tipo == TipoPowerUp.Velocidad)
                ConstruirFlechas ();
            else if (powerUp.m_Tipo == TipoPowerUp.Escudo)
                ConstruirEscudo ();
            else if (powerUp.m_Tipo == TipoPowerUp.Disparo)
                ConstruirBalas (powerUp.m_ModoDisparo);
            else
                ConstruirSalto ();

            if (m_ConAnillo)
                ConstruirAnillo ();

            if (m_ConLuz)
                ConstruirLuz ();
        }


        // VIDA: una cruz de botiquín.
        private void ConstruirCruz ()
        {
            Pieza (PrimitiveType.Cube, new Vector3 (0f, 0f, 0f), new Vector3 (0.38f, 1.15f, 0.38f), 0f, m_Material);
            Pieza (PrimitiveType.Cube, new Vector3 (0f, 0f, 0f), new Vector3 (1.15f, 0.38f, 0.38f), 0f, m_Material);
            // Centro claro para dar detalle.
            Pieza (PrimitiveType.Cube, new Vector3 (0f, 0f, 0f), new Vector3 (0.4f, 0.4f, 0.42f), 0f, m_MaterialClaro);
        }


        // VELOCIDAD: doble flecha ">>".
        private void ConstruirFlechas ()
        {
            float[] posicionesX = { -0.28f, 0.22f };

            for (int i = 0; i < posicionesX.Length; i++)
            {
                float x = posicionesX[i];
                Pieza (PrimitiveType.Cube, new Vector3 (x, 0.2f, 0f), new Vector3 (0.62f, 0.2f, 0.3f), -45f, i == 0 ? m_Material : m_MaterialClaro);
                Pieza (PrimitiveType.Cube, new Vector3 (x, -0.2f, 0f), new Vector3 (0.62f, 0.2f, 0.3f), 45f, i == 0 ? m_Material : m_MaterialClaro);
            }
        }


        // ESCUDO: un escudo ovalado con un centro claro.
        private void ConstruirEscudo ()
        {
            Pieza (PrimitiveType.Sphere, new Vector3 (0f, 0f, 0f), new Vector3 (1f, 1.25f, 0.35f), 0f, m_Material);
            Pieza (PrimitiveType.Sphere, new Vector3 (0f, 0.05f, 0f), new Vector3 (0.45f, 0.45f, 0.45f), 0f, m_MaterialClaro);
            Pieza (PrimitiveType.Cube, new Vector3 (0f, 0.05f, 0f), new Vector3 (0.12f, 0.95f, 0.38f), 0f, m_MaterialClaro);
        }


        // NUEVO - DISPARO: dos balas (doble) o una bala grande.
        private void ConstruirBalas (ModoDisparo modo)
        {
            if (modo == ModoDisparo.Grande)
            {
                Pieza (PrimitiveType.Capsule, new Vector3 (0f, 0.05f, 0f), new Vector3 (0.5f, 0.6f, 0.5f), 0f, m_Material);
                Pieza (PrimitiveType.Cube, new Vector3 (0f, -0.4f, 0f), new Vector3 (0.55f, 0.15f, 0.55f), 0f, m_MaterialClaro);
                return;
            }

            float tamano = (modo == ModoDisparo.Ambos) ? 1.25f : 1f;
            float[] posicionesX = { -0.24f, 0.24f };

            for (int i = 0; i < posicionesX.Length; i++)
            {
                float x = posicionesX[i] * tamano;
                Pieza (PrimitiveType.Capsule, new Vector3 (x, 0.05f, 0f), new Vector3 (0.3f, 0.45f, 0.3f) * tamano, 0f, m_Material);
                Pieza (PrimitiveType.Cube, new Vector3 (x, -0.33f * tamano, 0f), new Vector3 (0.34f, 0.12f, 0.34f) * tamano, 0f, m_MaterialClaro);
            }
        }


        // NUEVO - SALTO: doble flecha hacia arriba "^^".
        private void ConstruirSalto ()
        {
            float[] posicionesY = { -0.25f, 0.2f };

            for (int i = 0; i < posicionesY.Length; i++)
            {
                float y = posicionesY[i];
                Material material = (i == 0) ? m_Material : m_MaterialClaro;
                Pieza (PrimitiveType.Cube, new Vector3 (-0.22f, y, 0f), new Vector3 (0.62f, 0.2f, 0.3f), 45f, material);
                Pieza (PrimitiveType.Cube, new Vector3 (0.22f, y, 0f), new Vector3 (0.62f, 0.2f, 0.3f), -45f, material);
            }
        }


        // Anillo de cubitos en el suelo alrededor del power-up.
        private void ConstruirAnillo ()
        {
            m_Anillo = new GameObject ("Anillo").transform;
            m_Anillo.SetParent (m_Icono, false);
            m_Anillo.localPosition = new Vector3 (0f, -0.85f, 0f);

            int cantidad = 14;
            float radio = 1.05f;

            for (int i = 0; i < cantidad; i++)
            {
                float angulo = i * Mathf.PI * 2f / cantidad;
                Vector3 posicion = new Vector3 (Mathf.Cos (angulo) * radio, 0f, Mathf.Sin (angulo) * radio);
                Transform cubito = Pieza (PrimitiveType.Cube, posicion, new Vector3 (0.13f, 0.13f, 0.13f), 0f, m_MaterialClaro);
                cubito.SetParent (m_Anillo, false);
                cubito.localPosition = posicion;
            }
        }


        private void ConstruirLuz ()
        {
            GameObject objeto = new GameObject ("Luz");
            objeto.transform.SetParent (m_Icono, false);

            Light luz = objeto.AddComponent<Light> ();
            luz.type = LightType.Point;
            luz.color = m_Material.color;
            luz.range = 4f;
            luz.intensity = 2.5f;
        }


        // Crea una pieza (cubo, esfera...) sin collider, dentro del ícono.
        private Transform Pieza (PrimitiveType tipo, Vector3 posicion, Vector3 escala, float rotacionZ, Material material)
        {
            GameObject pieza = GameObject.CreatePrimitive (tipo);
            Destroy (pieza.GetComponent<Collider> ());

            pieza.transform.SetParent (m_Icono, false);
            pieza.transform.localPosition = posicion;
            pieza.transform.localScale = escala;
            pieza.transform.localRotation = Quaternion.Euler (0f, 0f, rotacionZ);
            pieza.GetComponent<Renderer> ().sharedMaterial = material;

            return pieza.transform;
        }


        private void Update ()
        {
            // Latido: crece y se encoge suavemente.
            float factor = 1f + Mathf.Sin (Time.time * 4f) * m_Latido;
            m_Icono.localScale = m_EscalaIcono * factor;

            // El anillo gira al revés que el ícono.
            if (m_Anillo != null)
                m_Anillo.Rotate (0f, -180f * Time.deltaTime, 0f, Space.World);
        }


        private void OnDestroy ()
        {
            // Liberar el material copiado.
            if (m_MaterialClaro != null)
                Destroy (m_MaterialClaro);
        }
    }
}
