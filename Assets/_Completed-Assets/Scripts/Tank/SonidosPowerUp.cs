using System.Collections.Generic;
using UnityEngine;

namespace Complete
{
    // NUEVO SCRIPT: crea por código un sonido distinto para cada power-up
    // (no hace falta descargar archivos de audio) y lo reproduce al recogerlo.
    // No se pone en ningún objeto: lo usa el script PowerUp automáticamente.
    public static class SonidosPowerUp
    {
        private const int FRECUENCIA = 44100;       // Muestras por segundo (calidad de CD).
        private const int SENO = 0;
        private const int CUADRADA = 1;
        private const int TRIANGULAR = 2;

        // Los sonidos se crean una sola vez y se guardan aquí.
        private static Dictionary<TipoPowerUp, AudioClip> s_Guardados = new Dictionary<TipoPowerUp, AudioClip> ();


        public static void Reproducir (TipoPowerUp tipo, AudioClip sonidoPropio, float volumen)
        {
            // Si pusiste un sonido propio en el prefab, se usa ese. Si no, se usa el generado.
            AudioClip clip = sonidoPropio != null ? sonidoPropio : Obtener (tipo);

            // Fuente de sonido temporal en 2D (se escucha igual en toda la pantalla).
            GameObject objeto = new GameObject ("SonidoPowerUp");
            AudioSource fuente = objeto.AddComponent<AudioSource> ();
            fuente.spatialBlend = 0f;
            fuente.volume = volumen;
            fuente.clip = clip;
            fuente.Play ();

            Object.Destroy (objeto, clip.length + 0.1f);
        }


        private static AudioClip Obtener (TipoPowerUp tipo)
        {
            AudioClip clip;
            if (s_Guardados.TryGetValue (tipo, out clip) && clip != null)
                return clip;

            switch (tipo)
            {
                case TipoPowerUp.Vida:
                    // Arpegio alegre hacia arriba: Do - Mi - Sol - Do.
                    clip = Crear ("SonidoVida",
                                  new float[] { 523f, 659f, 784f, 1047f },
                                  new float[] { 523f, 659f, 784f, 1047f },
                                  new float[] { 0.07f, 0.07f, 0.07f, 0.18f }, SENO, 0f);
                    break;

                case TipoPowerUp.Velocidad:
                    // Barrido rápido de grave a agudo: "¡zuuum!".
                    clip = Crear ("SonidoVelocidad",
                                  new float[] { 400f }, new float[] { 1600f },
                                  new float[] { 0.28f }, TRIANGULAR, 0f);
                    break;

                case TipoPowerUp.Escudo:
                    // Sonido brillante con vibración: "campo de energía".
                    clip = Crear ("SonidoEscudo",
                                  new float[] { 330f, 440f }, new float[] { 440f, 660f },
                                  new float[] { 0.12f, 0.3f }, SENO, 18f);
                    break;

                case TipoPowerUp.Disparo:
                    // Sonido grave y fuerte hacia abajo: "carga de arma".
                    clip = Crear ("SonidoDisparo",
                                  new float[] { 110f, 220f }, new float[] { 220f, 90f },
                                  new float[] { 0.1f, 0.25f }, CUADRADA, 0f);
                    break;

                default:
                    // Salto: "¡boing!" hacia arriba.
                    clip = Crear ("SonidoSalto",
                                  new float[] { 250f }, new float[] { 900f },
                                  new float[] { 0.22f }, TRIANGULAR, 0f);
                    break;
            }

            s_Guardados[tipo] = clip;
            return clip;
        }


        // Crea un sonido uniendo "tramos". Cada tramo va de una frecuencia inicial a una final en cierto tiempo.
        private static AudioClip Crear (string nombre, float[] desde, float[] hasta, float[] duraciones, int onda, float vibracion)
        {
            float duracionTotal = 0f;
            for (int i = 0; i < duraciones.Length; i++)
                duracionTotal += duraciones[i];

            int totalMuestras = Mathf.CeilToInt (duracionTotal * FRECUENCIA);
            float[] datos = new float[totalMuestras];

            double fase = 0.0;
            int indice = 0;

            for (int tramo = 0; tramo < duraciones.Length; tramo++)
            {
                int muestrasTramo = Mathf.CeilToInt (duraciones[tramo] * FRECUENCIA);
                bool esUltimo = (tramo == duraciones.Length - 1);

                for (int i = 0; i < muestrasTramo && indice < totalMuestras; i++)
                {
                    float t = (float)i / muestrasTramo;                         // Avance dentro del tramo (0 a 1).
                    float frecuencia = Mathf.Lerp (desde[tramo], hasta[tramo], t);

                    fase += 2.0 * System.Math.PI * frecuencia / FRECUENCIA;
                    if (fase > 2.0 * System.Math.PI)
                        fase -= 2.0 * System.Math.PI;

                    float valor = Onda ((float)fase, onda);

                    // Volumen: entrada rápida y luego baja poco a poco (evita "clics").
                    float envolvente = Mathf.Min (1f, i / (0.005f * FRECUENCIA)) * Mathf.Lerp (1f, 0.35f, t);
                    if (esUltimo)
                        envolvente *= (1f - t);

                    // Vibración (sube y baja el volumen muy rápido).
                    if (vibracion > 0f)
                        valor *= 0.7f + 0.3f * Mathf.Sin (2f * Mathf.PI * vibracion * indice / FRECUENCIA);

                    datos[indice] = valor * envolvente * 0.5f;
                    indice++;
                }
            }

            AudioClip clip = AudioClip.Create (nombre, totalMuestras, 1, FRECUENCIA, false);
            clip.SetData (datos, 0);
            return clip;
        }


        private static float Onda (float fase, int tipo)
        {
            float seno = Mathf.Sin (fase);

            if (tipo == CUADRADA)
                return (seno >= 0f ? 1f : -1f) * 0.6f;      // Más áspera, se baja un poco el volumen.

            if (tipo == TRIANGULAR)
                return Mathf.Asin (seno) * 2f / Mathf.PI;

            return seno;
        }
    }
}
