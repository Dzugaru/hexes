using UnityEngine;
using System.Collections;

public class CustomParticlesMotion : MonoBehaviour
{
    ParticleSystem ps;
    ParticleSystem.Particle[] particles;    

    void Start()
    {
        ps = GetComponent<ParticleSystem>();

        
        particles = new ParticleSystem.Particle[4096];
    }

    void LateUpdate()
    {
        int numParticlesAlive = ps.GetParticles(particles);
        Quaternion rot = Quaternion.AngleAxis(360 * Time.deltaTime, new Vector3(0,0,1));
        
        for (int i = 0; i < numParticlesAlive; i++)
        {
            var p = particles[i];
            Vector3 r = p.position - ps.transform.position;
            particles[i].position = rot * r + ps.transform.position;
        }

        // Apply the particle changes to the particle system
        ps.SetParticles(particles, numParticlesAlive);
    }    
}
