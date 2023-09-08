using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using TMPro;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class ScoreboardItem : MonoBehaviourPunCallbacks
{
    public TMP_Text deathsText;
    public TMP_Text killsText;
    public TMP_Text usernameText;
    private Player _player;
    public void Initialize(Player player)
    {
        this._player = player;

        usernameText.text = player.NickName;
        UpdateStats(player);
    }
    
    void UpdateStats(Player _player)
    {
        if(_player.CustomProperties.TryGetValue("kills", out object kills))
        {
            killsText.text = kills.ToString();
            if ((int)kills >= 10)
            {
                PlayerManager.Find(_player).EndGame();
            }
        }
        if(_player.CustomProperties.TryGetValue("deaths", out object deaths))
        {
            deathsText.text = deaths.ToString();
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if(targetPlayer == _player)
        {
            if(changedProps.ContainsKey("kills") || changedProps.ContainsKey("deaths"))
            {
                UpdateStats(targetPlayer);
            }
        }
    }
}
