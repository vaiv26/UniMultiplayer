using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class EssentialPrefabs : Essentials, IDestroyable
{
    private PhotonView PV;

    private void Awake()
    {
        PV = GetComponent<PhotonView>();
    }

    public override void Use()
    {
        
    }
    public void Destroyable()
    {
        if (PV.IsMine)
        {
            Debug.Log("Game Object Destroyed");
            PhotonNetwork.Destroy(gameObject);
        }
    }
}
