using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using System.IO;

public class ItemSpawner : Item
{
    private enum State
    {
        Idle,
        NotSpawned,
    }

    private State state;

    private PhotonView PV;

    [SerializeField] private GameObject placesphere;
    [SerializeField] private float respawnDelay = 10f;
    private bool isItemSpawned = false;
    private float respawnTimer = 0f;
    private GameObject spawnedItem;

    private void Awake()
    {
        PV = GetComponent<PhotonView>();
    }

    private void Start()
    {
        placesphere.SetActive(false);
        StartSpawn(State.NotSpawned);
    }

    private void StartSpawn(State state)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PV.RPC(nameof(RPCSpawnItem), RpcTarget.All, state);
        }
    }

    [PunRPC]
    void RPCSpawnItem(State state)
    {
        switch (state)
        {
            case State.NotSpawned:
                spawnedItem = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", itemGameObject.name), 
                    gameObject.transform.position, Quaternion.identity, 0, new object[] { PV.ViewID });
                isItemSpawned = true;
                respawnTimer = 0f;
                break;
            case State.Idle:
                if (spawnedItem != null)
                {
                    if (spawnedItem.GetPhotonView().IsMine)
                    {
                        PhotonNetwork.Destroy(spawnedItem);
                    }
                    else
                    {
                        spawnedItem.gameObject.GetComponent<IDestroyable>().Destroyable();
                    }
                    isItemSpawned = false;
                }
                break;
        }
    }

    private void Update()
    {
        if (!isItemSpawned)
        {
            respawnTimer += Time.deltaTime;
            if (respawnTimer >= respawnDelay)
            {
                StartSpawn(State.NotSpawned);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Collided");
        if (isItemSpawned)
        {
            Debug.Log("Item is Spawned");
            Debug.Log("Called");
            if (itemSystem == 0)
            {
                other.gameObject.GetComponent<PlayerController>()?.AttemptToTakeItem(itemGameObject, itemSystem, 0);
            }
            else
            {
                Debug.Log("Client Touched");
                other.gameObject.GetComponent<PlayerController>()?.AttemptToTakeItem(itemGameObject, itemSystem,
                    ((essentialsInfo)itemInfo).ValueToProvide);
            }
            PV.RPC(nameof(RPCSpawnItem), RpcTarget.All, State.Idle);
        }
        else
        {
            Debug.Log("Is Item Spawned is False:" + isItemSpawned);
        }
    }

    public override void Use()
    {
    }
}
