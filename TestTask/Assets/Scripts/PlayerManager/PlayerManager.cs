using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;
using Photon.Realtime;
using System.Linq;
using ExitGames.Client.Photon;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PlayerManager : MonoBehaviour
{
    Hashtable hash = new Hashtable();
    PhotonView PV;

    GameObject controller;

    int kills;
    int deaths;

    void Awake()
    {
        PV = GetComponent<PhotonView>();
    }

    void Start()
    {
        if(PV.IsMine)
        {
            CreateController();
        }
    }

    void CreateController()
    {
        Transform spawnpoint = SpawnManager.Instance.GetSpawnpoint();
        controller = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PlayerController"), 
            spawnpoint.position, spawnpoint.rotation, 0, new object[] { PV.ViewID });
    }
    
    public void Die()
    {
        PhotonNetwork.Destroy(controller);
        CreateController();

        deaths++;
        if (hash.ContainsKey("deaths"))
        {
            hash["deaths"] = deaths;
        }
        else
        {
            hash.Add("deaths",deaths);
        }
        PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GetKill()
    {
        PV.RPC(nameof(RPC_GetKill),PV.Owner);
    }

    [PunRPC]
    void RPC_GetKill()
    {
        kills++;
        if (hash.ContainsKey("kills"))
        {
            hash["kills"] = kills;
        }
        else
        {
            hash.Add("kills",kills);
        }
        PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
    }

    public static PlayerManager Find(Player player)
    {
        return FindObjectsOfType<PlayerManager>().SingleOrDefault(x => x.PV.Owner == player);
    }

    public void EndGame()
    {
        Debug.Log("End");
        RoomManager.Instance.EndGame();
    }
}
