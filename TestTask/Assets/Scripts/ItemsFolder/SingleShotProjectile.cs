using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;

public class SingleShotProjectile : Gun, IDestroyable
{
    [SerializeField] Camera cam;
    [SerializeField] private Transform pfBullet;
    [SerializeField] private Transform bulletSpawnPoint;

    private PhotonView PV;

    private void Awake()
    {
        PV = GetComponent<PhotonView>();
    }

    public override void Use()
    {
        Shoot();
    }

    void Shoot()
    {
        // Projectile object shoot
        /*Transform bulletTransform =  Instantiate(pfBullet, bulletSpawnPoint.position, Quaternion.identity);
        if (PV.IsMine)
        {
            PhotonNetwork.Instantiate(pfBullet.name, bulletSpawnPoint.position, Quaternion.identity);
        }
        Vector3 shootDir = bulletSpawnPoint.forward;
        bulletTransform.GetComponent<ProjectileBullet>().Setup(shootDir);*/
        PV.RPC(nameof(RPCShoot), RpcTarget.All);
    }

    [PunRPC]
    void RPCShoot() //Transform ProjectileSpawnPoint)
    {
        GameObject projectile = Instantiate(ProjectilePrefab, bulletSpawnPoint.position, Quaternion.identity);
        projectile.GetComponent<ProjectileBullet>().Setup(bulletSpawnPoint.forward);
    }

    public void Destroyable()
    {
        if (PV.IsMine)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }

}
