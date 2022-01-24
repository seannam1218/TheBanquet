using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;


public class PlayerItem : MonoBehaviour
{
    public Text playerName;
    private Image image;
    public Color highlightColor;

    void Awake()
    {
        image = GetComponent<Image>();
    }

    public void SetPlayerInfo(Player _player)
    {
        playerName.text = _player.NickName;
    }

    public void ApplyLocalChanges()
    {
        image.color = highlightColor;
    }
}
