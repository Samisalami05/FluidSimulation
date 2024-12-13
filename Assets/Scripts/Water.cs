using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class Water : MonoBehaviour
{
    public float gravity;
    public float particleSize;
    public float collisionDamping;
    public float particleSpacing;
    public float collisionRadius;
    public float Strength;
    public float colorthing = 5;
    public float halflife = 1;

    private Vector2[] positions;
    private Vector2[] velocitys;
    public Vector2 boundsSize;

    public bool showSpawnBoundsGizmos;

    public int numParticles;

    public GameObject spawnPrefab;
    public GameObject[] particles;

    public Gradient gradient;

    void Awake()
    {
        positions = new Vector2[numParticles];
        velocitys = new Vector2[numParticles];
        particles = new GameObject[numParticles];

        int particlesPerRow = (int)Mathf.Sqrt(numParticles);
        int particlesPerCol = (int)Mathf.Sqrt(numParticles);
        float spacing = particleSize * 2 + particleSpacing;

        for (int i = 0; i < numParticles; i++)
        {
            float x = (i % particlesPerRow - particlesPerRow / 2f + 0.5f) * spacing;
            float y = (i / particlesPerRow - particlesPerCol / 2f + 0.5f) * spacing;
            positions[i] = new Vector2(x, y);
        }
        for (int i = 0; i < numParticles; i++)
        {
            GameObject spawnedObj = Instantiate(spawnPrefab, positions[i], Quaternion.identity);
            
            
            particles[i] = spawnedObj;
            
        }
    }

    void Update()
    {
        for (int i = 0; i < numParticles; i++)
        {
            velocitys[i] *= Mathf.Exp(Mathf.Log(0.5f) * Time.deltaTime / halflife);
            Vector2 lastpos = particles[i].transform.position;

            velocitys[i] += Vector2.down * gravity * Time.deltaTime;
            positions[i] += velocitys[i] * Time.deltaTime;

            Vector2[] vector2s = ResolveCollisions(positions[i], velocitys[i]);
            positions[i] = vector2s[0];
            velocitys[i] = vector2s[1];

            

            for (int j = 0; j < numParticles; j++)
            {
                if (j != i)
                {
                    velocitys[i] += calculateForce(particles[i].transform.position, particles[j].transform.position);
                }
            }

            particles[i].transform.localScale = new Vector2(particleSize * 2, particleSize * 2);
            particles[i].transform.position = positions[i];

            
            float speed = Vector2.Distance(particles[i].transform.position, lastpos);

            SpriteRenderer spriteRenderer = particles[i].GetComponent<SpriteRenderer>();
            spriteRenderer.color = gradient.Evaluate(Mathf.Clamp(speed * colorthing, 0, 1));
        }
    }
    Vector2[] ResolveCollisions(Vector2 position, Vector2 velocity)
    {
        Vector2 halfBoundsSize = boundsSize / 2 - Vector2.one * particleSize;

        if (Mathf.Abs(position.x) > halfBoundsSize.x)
        {
            position.x = halfBoundsSize.x * Mathf.Sign(position.x);
            velocity.x *= -1 * collisionDamping;
        }
        if (Mathf.Abs(position.y) > halfBoundsSize.y)
        {
            position.y = halfBoundsSize.y * Mathf.Sign(position.y);
            velocity.y *= -1 * collisionDamping;
        }
        Vector2[] vector2s = { position, velocity };
        return vector2s;
    }
    void OnDrawGizmos()
    {
        if (showSpawnBoundsGizmos)
        {
            Gizmos.color = new Color(0, 1, 0, 0.4f);
            Gizmos.DrawWireCube(Vector2.zero, boundsSize);
        }
    }

    Vector2 calculateForce(Vector2 pos1, Vector2 pos2)
    {
        
        float distance = Vector2.Distance(pos1, pos2);

        Vector2 force = pos1 - pos2;

        float multiplier = Mathf.Max(0, collisionRadius * collisionRadius - distance * distance);

        if (distance < collisionRadius)
        {
            return force / (distance + 0.0000001f) * (multiplier * multiplier * multiplier) / Strength * Time.deltaTime;
        }
        return Vector2.zero;
    }
}
