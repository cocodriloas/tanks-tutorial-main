using UnityEngine;

namespace Complete
{
    // NUEVO SCRIPT: cambia la forma del tanque agregándole piezas: lo convierte en un
    // "TANQUE QUITANIEVES" con pala al frente, faldones de blindaje, tubos de escape,
    // antena, faros que ALUMBRAN, freno de boca en el cañón y un poco de nieve en la torreta.
    // Las piezas se colocan solas midiendo el tamaño del tanque.
    // Se agrega al prefab CompleteTank (juego) y a TanqueMenu1, 2 y 3 (menú).
    public class DecoracionTanque : MonoBehaviour
    {
        [Header ("Piezas (desmarca las que no quieras)")]
        public bool m_Pala = true;
        public bool m_Faldones = true;
        public bool m_TubosEscape = true;
        public bool m_Antena = true;
        public bool m_Faros = true;
        public bool m_FrenoDeBoca = true;
        public bool m_NieveEnTorreta = true;

        [Header ("Luz de los faros")]
        public bool m_FarosAlumbran = true;         // Luz real hacia adelante (desmárcalo en el menú principal).
        public float m_IntensidadFaros = 6f;
        public float m_AlcanceFaros = 14f;
        public float m_AperturaFaros = 70f;         // Qué tan abierto es el haz de luz (grados).

        [Header ("Colores fijos (las demás piezas toman el color del jugador)")]
        public Color m_ColorMetal = new Color (0.23f, 0.23f, 0.25f);
        public Color m_ColorNieve = new Color (0.95f, 0.97f, 1f);
        public Color m_ColorFaros = new Color (1f, 0.95f, 0.6f);

        private Transform m_Piezas;
        private Material m_MaterialBase;
        private System.Collections.Generic.List<Renderer> m_Metal = new System.Collections.Generic.List<Renderer> ();
        private System.Collections.Generic.List<Renderer> m_Nieve = new System.Collections.Generic.List<Renderer> ();
        private System.Collections.Generic.List<Renderer> m_Luces = new System.Collections.Generic.List<Renderer> ();
        private bool m_ColoresAplicados;


        private void Awake ()
        {
            // Material del tanque para las piezas nuevas.
            MeshRenderer primerRenderer = GetComponentInChildren<MeshRenderer> ();
            if (primerRenderer == null)
                return;
            m_MaterialBase = primerRenderer.sharedMaterial;

            // Medir el tanque ANTES de agregar piezas.
            Transform torreta = BuscarHijo (transform, "TankTurret");
            Bounds casco = MedirCasco (torreta);
            Bounds limitesTorreta = torreta != null ? Medir (torreta) : casco;
            Transform canon = BuscarHijo (transform, "FireTransform");

            m_Piezas = new GameObject ("PiezasQuitanieves").transform;
            m_Piezas.SetParent (transform, false);

            if (m_Pala)
                CrearPala (casco);

            if (m_Faldones)
                CrearFaldones (casco);

            if (m_TubosEscape)
                CrearTubos (casco);

            if (m_Faros)
                CrearFaros (casco);

            if (torreta != null && m_Antena)
                CrearAntena (limitesTorreta);

            if (torreta != null && m_NieveEnTorreta)
                CrearNieve (limitesTorreta);

            if (canon != null && m_FrenoDeBoca)
                CrearFrenoDeBoca (canon);
        }


        // ---------- PIEZAS ----------

        // Pala quitanieves al frente, con dos brazos y un filo metálico abajo.
        private void CrearPala (Bounds casco)
        {
            float ancho = casco.size.x * 1.1f;
            float alto = casco.size.y * 0.6f;
            float y = casco.min.y + casco.size.y * 0.35f;
            float z = casco.max.z + 0.3f;

            Pieza (PrimitiveType.Cube, new Vector3 (0f, y, z), new Vector3 (ancho, alto, 0.08f), new Vector3 (15f, 0f, 0f), null);
            Pieza (PrimitiveType.Cube, new Vector3 (0f, y - alto * 0.48f, z + 0.05f), new Vector3 (ancho, 0.08f, 0.14f), Vector3.zero, m_Metal);

            float brazoX = casco.size.x * 0.3f;
            Pieza (PrimitiveType.Cube, new Vector3 (-brazoX, y, casco.max.z + 0.12f), new Vector3 (0.09f, 0.09f, 0.4f), Vector3.zero, m_Metal);
            Pieza (PrimitiveType.Cube, new Vector3 (brazoX, y, casco.max.z + 0.12f), new Vector3 (0.09f, 0.09f, 0.4f), Vector3.zero, m_Metal);
        }


        // Placas de blindaje a los costados.
        private void CrearFaldones (Bounds casco)
        {
            float x = casco.max.x + 0.04f;
            float y = casco.min.y + casco.size.y * 0.55f;
            Vector3 escala = new Vector3 (0.06f, casco.size.y * 0.45f, casco.size.z * 0.8f);

            Pieza (PrimitiveType.Cube, new Vector3 (-x, y, casco.center.z), escala, Vector3.zero, null);
            Pieza (PrimitiveType.Cube, new Vector3 (x, y, casco.center.z), escala, Vector3.zero, null);
        }


        // Dos tubos de escape atrás.
        private void CrearTubos (Bounds casco)
        {
            float x = casco.size.x * 0.28f;
            float y = casco.max.y + 0.2f;
            float z = casco.min.z + 0.15f;
            Vector3 escala = new Vector3 (0.12f, 0.2f, 0.12f);

            Pieza (PrimitiveType.Cylinder, new Vector3 (-x, y, z), escala, Vector3.zero, m_Metal);
            Pieza (PrimitiveType.Cylinder, new Vector3 (x, y, z), escala, Vector3.zero, m_Metal);
        }


        // Dos faros al frente.
        private void CrearFaros (Bounds casco)
        {
            float x = casco.size.x * 0.3f;
            float y = casco.max.y - 0.1f;
            float z = casco.max.z + 0.02f;
            Vector3 escala = new Vector3 (0.18f, 0.12f, 0.06f);

            Pieza (PrimitiveType.Cube, new Vector3 (-x, y, z), escala, Vector3.zero, m_Luces);
            Pieza (PrimitiveType.Cube, new Vector3 (x, y, z), escala, Vector3.zero, m_Luces);

            // NUEVO: una luz tipo "linterna" (Spot) que alumbra el suelo delante del tanque.
            if (m_FarosAlumbran)
            {
                GameObject objetoLuz = new GameObject ("LuzFaros");
                objetoLuz.transform.SetParent (m_Piezas, false);
                objetoLuz.transform.localPosition = new Vector3 (0f, y, z + 0.1f);
                objetoLuz.transform.localRotation = Quaternion.Euler (20f, 0f, 0f);   // Inclinada un poco hacia el suelo.

                Light luz = objetoLuz.AddComponent<Light> ();
                luz.type = LightType.Spot;
                luz.color = m_ColorFaros;
                luz.intensity = m_IntensidadFaros;
                luz.range = m_AlcanceFaros;
                luz.spotAngle = m_AperturaFaros;
                luz.shadows = LightShadows.None;
            }
        }


        // Antena con una bolita en la punta.
        private void CrearAntena (Bounds torreta)
        {
            float x = torreta.min.x + torreta.size.x * 0.2f;
            float z = torreta.min.z + torreta.size.z * 0.2f;

            Pieza (PrimitiveType.Cylinder, new Vector3 (x, torreta.max.y + 0.5f, z), new Vector3 (0.035f, 0.5f, 0.035f), Vector3.zero, m_Metal);
            Pieza (PrimitiveType.Sphere, new Vector3 (x, torreta.max.y + 1f, z), new Vector3 (0.15f, 0.15f, 0.15f), Vector3.zero, null);
        }


        // Un montoncito de nieve encima de la torreta.
        private void CrearNieve (Bounds torreta)
        {
            float z = torreta.min.z + torreta.size.x * 0.5f;
            Pieza (PrimitiveType.Sphere, new Vector3 (torreta.center.x, torreta.max.y, z),
                   new Vector3 (torreta.size.x * 0.55f, 0.12f, torreta.size.x * 0.5f), Vector3.zero, m_Nieve);
        }


        // Freno de boca: un bloque en la punta del cañón.
        private void CrearFrenoDeBoca (Transform canon)
        {
            Vector3 posicion = transform.InverseTransformPoint (canon.position);
            Quaternion rotacion = Quaternion.Inverse (transform.rotation) * canon.rotation;
            posicion -= rotacion * Vector3.forward * 0.15f;

            Transform freno = Pieza (PrimitiveType.Cube, posicion, new Vector3 (0.34f, 0.24f, 0.26f), Vector3.zero, m_Metal);
            freno.localRotation = rotacion;
        }


        // ---------- AYUDANTES ----------

        // Crea una pieza sin collider. lista = null → toma el color del jugador.
        private Transform Pieza (PrimitiveType tipo, Vector3 posicion, Vector3 escala, Vector3 rotacion,
                                 System.Collections.Generic.List<Renderer> lista)
        {
            GameObject pieza = GameObject.CreatePrimitive (tipo);
            DestroyImmediate (pieza.GetComponent<Collider> ());   // Sin collider para no afectar la física del tanque.

            pieza.transform.SetParent (m_Piezas, false);
            pieza.transform.localPosition = posicion;
            pieza.transform.localScale = escala;
            pieza.transform.localRotation = Quaternion.Euler (rotacion);

            Renderer render = pieza.GetComponent<Renderer> ();
            render.sharedMaterial = m_MaterialBase;

            if (lista != null)
                lista.Add (render);

            return pieza.transform;
        }


        // Los colores fijos se aplican DESPUÉS de que el juego (o el menú) pinta el tanque con el color del jugador.
        private void LateUpdate ()
        {
            if (m_ColoresAplicados)
                return;

            Pintar (m_Metal, m_ColorMetal);
            Pintar (m_Nieve, m_ColorNieve);
            Pintar (m_Luces, m_ColorFaros);

            // NUEVO: que los faros brillen (emisión de luz propia).
            for (int i = 0; i < m_Luces.Count; i++)
            {
                Material material = m_Luces[i].material;
                material.EnableKeyword ("_EMISSION");
                material.SetColor ("_EmissionColor", m_ColorFaros * 2f);
            }

            m_ColoresAplicados = true;
        }


        private void Pintar (System.Collections.Generic.List<Renderer> lista, Color color)
        {
            for (int i = 0; i < lista.Count; i++)
                lista[i].material.color = color;
        }


        // Mide el casco (chasis y orugas, sin la torreta) en coordenadas del tanque.
        private Bounds MedirCasco (Transform torreta)
        {
            Bounds limites = new Bounds (Vector3.zero, Vector3.zero);
            bool primero = true;
            MeshFilter[] mallas = GetComponentsInChildren<MeshFilter> ();

            for (int i = 0; i < mallas.Length; i++)
            {
                if (torreta != null && mallas[i].transform.IsChildOf (torreta))
                    continue;

                Agregar (mallas[i], ref limites, ref primero);
            }

            // Si no se encontró nada (modelo distinto), medir todo.
            if (primero)
                return Medir (transform);

            return limites;
        }


        private Bounds Medir (Transform objeto)
        {
            Bounds limites = new Bounds (Vector3.zero, Vector3.zero);
            bool primero = true;
            MeshFilter[] mallas = objeto.GetComponentsInChildren<MeshFilter> ();

            for (int i = 0; i < mallas.Length; i++)
                Agregar (mallas[i], ref limites, ref primero);

            return limites;
        }


        // Suma las 8 esquinas de una malla a los límites, en coordenadas del tanque.
        private void Agregar (MeshFilter malla, ref Bounds limites, ref bool primero)
        {
            if (malla.sharedMesh == null)
                return;

            Bounds b = malla.sharedMesh.bounds;

            for (int x = -1; x <= 1; x += 2)
            {
                for (int y = -1; y <= 1; y += 2)
                {
                    for (int z = -1; z <= 1; z += 2)
                    {
                        Vector3 esquina = b.center + new Vector3 (b.extents.x * x, b.extents.y * y, b.extents.z * z);
                        Vector3 local = transform.InverseTransformPoint (malla.transform.TransformPoint (esquina));

                        if (primero)
                        {
                            limites = new Bounds (local, Vector3.zero);
                            primero = false;
                        }
                        else
                        {
                            limites.Encapsulate (local);
                        }
                    }
                }
            }
        }


        private Transform BuscarHijo (Transform padre, string nombre)
        {
            foreach (Transform hijo in padre)
            {
                if (hijo.name == nombre)
                    return hijo;

                Transform encontrado = BuscarHijo (hijo, nombre);
                if (encontrado != null)
                    return encontrado;
            }

            return null;
        }
    }
}
