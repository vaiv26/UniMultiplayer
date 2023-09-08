using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Diagnostics;
using Photon.Pun;

public class ProjectileBullet : MonoBehaviour
{
    [SerializeField] private float Speed;
    [SerializeField] private float lifetime;
    [SerializeField] private float blastRadius; 
    [SerializeField]private float damage;
    private Vector3 shootDir;

    private Rigidbody rb;
    
    private PhotonView PV;

    public void Awake()
    {
        rb = rb = GetComponent<Rigidbody>();
        PV = GetComponent<PhotonView>();
    }


    public void Setup(Vector3 shootDir)
    {
        this.shootDir = shootDir;
        Quaternion Desiredirection = Quaternion.LookRotation(this.shootDir);
        transform.rotation = Desiredirection;
        rb.AddForce(this.shootDir * Speed);
        Destroy(gameObject,lifetime);
    }

    private void OnCollisionEnter(Collision other)
    {
        Vector3 location = gameObject.transform.position;
        Collider[] objectsInRange = Physics.OverlapSphere(location, blastRadius);
        for(int i = 0; i < objectsInRange.Length;i++ )
        {
            Debug.Log("objects name:" + objectsInRange[i].name);
            IDamageable enemy = objectsInRange[i].GetComponent<IDamageable>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                Debug.Log("Damage");
            }
        }
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        //PhotonNetwork.Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(this.transform.position, blastRadius);
    }
}
