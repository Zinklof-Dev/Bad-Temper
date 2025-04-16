using Unity;
using UnityEngine;
using System;

public class campfire : MonoBehaviour
{
    [SerializeField] float healTimer = 4;
    [SerializeField] float healAmount = 20;
    [SerializeField] float radius = 3;
    [SerializeField] bool debug;

    float healCooldown;

    void OnDrawGizmos()
    {
        if (!debug)
        {
            return;
        }
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, radius);
    }

    public void Update()
    {
        healCooldown += Time.deltaTime;

        healCooldown = 0;

        if (healCooldown >= healTimer)
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, radius);

            foreach(Collider c in colliders)
            {
                if (GetComponent<Collider>().gameObject.tag == "Player")
                    GetComponent<Collider>().gameObject.GetComponent<Stats>().Damage(-healAmount);
                else
                    continue;
            }
        }
    }
}