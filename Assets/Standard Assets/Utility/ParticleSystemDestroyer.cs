using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

namespace UnityStandardAssets.Utility
{
    public class ParticleSystemDestroyer : MonoBehaviour
    {
        // allows a particle system to exist for a specified duration,
        // then shuts off emission, and waits for all particles to expire
        // before destroying the gameObject

        public float minDuration = 8;
        public float maxDuration = 10;

        private float MaxLifetime;
        private bool StopEarly;


        private IEnumerator Start()
        {
            var systems = GetComponentsInChildren<ParticleSystem>();

            // find out the maximum lifetime of any particles in this effect
            foreach (var system in systems) {
                MaxLifetime = Mathf.Max(system.main.startLifetime.constant, MaxLifetime);
            }

            // wait for random duration

            float stopTime = Time.time + Random.Range(minDuration, maxDuration);

            while (Time.time < stopTime || StopEarly)
            {
                yield return null;
            }
            Debug.Log("stopping " + name);

            // turn off emission
            foreach (var system in systems)
            {
                var emission = system.emission;
                emission.enabled = false;
            }
            BroadcastMessage("Extinguish", SendMessageOptions.DontRequireReceiver);

            // wait for any remaining particles to expire
            yield return new WaitForSeconds(MaxLifetime);

            Destroy(gameObject);
        }


        public void Stop()
        {
            // stops the particle system early
            StopEarly = true;
        }
    }
}
