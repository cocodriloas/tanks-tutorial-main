using System.Collections.Generic;
using UnityEngine;

namespace Complete
{
    public class ShellExplosion : MonoBehaviour
    {
        public LayerMask m_TankMask;                        // Used to filter what the explosion affects, this should be set to "Players".
        public ParticleSystem m_ExplosionParticles;         // Reference to the particles that will play on explosion.
        public AudioSource m_ExplosionAudio;                // Reference to the audio that will play on explosion.
        public float m_MaxDamage = 100f;                    // NUEVO USO: 100 = daño normal. La bala grande lo sube a 130 (30% más).
        public float m_ExplosionForce = 1000f;              // The amount of force added to a tank at the centre of the explosion.
        public float m_MaxLifeTime = 2f;                    // The time in seconds before the shell is removed.
        public float m_ExplosionRadius = 5f;                // The maximum distance away from the explosion tanks can be and are still affected.

        // NUEVO: sistema de "impactos". Un tanque se destruye con 3 balas.
        public float m_ImpactosParaDestruir = 3f;           // Cuántas balas normales se necesitan para destruir un tanque.
        public float m_ZonaDeImpacto = 0.6f;                // Parte del radio que cuenta como impacto (0.6 = 60%). Más lejos solo empuja, no daña.


        private void Start ()
        {
            // If it isn't destroyed by then, destroy the shell after it's lifetime.
            Destroy (gameObject, m_MaxLifeTime);
        }


        private void OnTriggerEnter (Collider other)
        {
            // Collect all the colliders in a sphere from the shell's current position to a radius of the explosion radius.
            Collider[] colliders = Physics.OverlapSphere (transform.position, m_ExplosionRadius, m_TankMask);

            // NUEVO: para no dañar dos veces al mismo tanque si tiene varios colliders.
            HashSet<Rigidbody> yaAfectados = new HashSet<Rigidbody> ();

            // Go through all the colliders...
            for (int i = 0; i < colliders.Length; i++)
            {
                // ... and find their rigidbody.
                Rigidbody targetRigidbody = colliders[i].GetComponent<Rigidbody> ();

                // If they don't have a rigidbody, or it was already hit by this explosion, go on to the next collider.
                if (!targetRigidbody || yaAfectados.Contains (targetRigidbody))
                    continue;

                yaAfectados.Add (targetRigidbody);

                // Add an explosion force.
                targetRigidbody.AddExplosionForce (m_ExplosionForce, transform.position, m_ExplosionRadius);

                // Find the TankHealth script associated with the rigidbody.
                TankHealth targetHealth = targetRigidbody.GetComponent<TankHealth> ();

                // If there is no TankHealth script attached to the gameobject, go on to the next collider.
                if (!targetHealth)
                    continue;

                // NUEVO: solo cuenta como impacto si la explosión fue cerca del tanque.
                float distancia = (targetRigidbody.position - transform.position).magnitude;
                if (distancia > m_ExplosionRadius * m_ZonaDeImpacto)
                    continue;

                // NUEVO: cada impacto quita 1/3 de la vida total (con bala normal).
                float damage = CalcularDanoPorImpacto (targetHealth);

                // Deal this damage to the tank.
                targetHealth.TakeDamage (damage);
            }

            // Unparent the particles from the shell.
            m_ExplosionParticles.transform.parent = null;

            // Play the particle system.
            m_ExplosionParticles.Play();

            // Play the explosion sound effect.
            m_ExplosionAudio.Play();

            // Once the particles have finished, destroy the gameobject they are on.
            ParticleSystem.MainModule mainModule = m_ExplosionParticles.main;
            Destroy (m_ExplosionParticles.gameObject, mainModule.duration);

            // Destroy the shell.
            Destroy (gameObject);
        }


        // NUEVO: daño fijo por impacto, sin importar la distancia.
        // Con vida 100 y 3 impactos: cada bala quita 33.8 → 3 balas = 101.5 (destruido).
        private float CalcularDanoPorImpacto (TankHealth objetivo)
        {
            float danoBase = objetivo.m_StartingHealth / m_ImpactosParaDestruir + 0.5f;

            // Si la bala es grande (m_MaxDamage mayor a 100), hace más daño.
            return danoBase * (m_MaxDamage / 100f);
        }
    }
}
