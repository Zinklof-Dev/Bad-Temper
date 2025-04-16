using UnityEngine;
using Unity;
using System;

public class Fireball : MonoBehaviour
{
    [SerializeField] float speed = 4;
    [SerializeField] float radius = 3;
    [SerializeField] ParticleSystem fbparticleSystem;

    Vector3 target;
    float damage;
    
    void OnCollisionEnter(Collision collision)
    {
        Explode();
    }

    public void Explode()
    {
        // do particle system;
        transform.localScale = new Vector3(0.001f, 0.001f, 0.001f);

        Collider[] colliders = Physics.OverlapSphere(transform.position, radius);

        foreach(Collider c in colliders)
        {
            if (c.gameObject.tag == "Player")
                c.gameObject.GetComponent<Stats>().Damage(damage);
            else
                continue;
        }
    }

    public void Initialize(Vector3 target, float damage)
    {
        this.target = target;
        this.damage = damage;
    }

    public void Update()
    {
        transform.LookAt(target);
        transform.position += transform.forward * speed * Time.deltaTime;
    }
}