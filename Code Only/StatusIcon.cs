using Photon.Pun;
using Photon.Pun.Demo.Asteroids;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatusIcon : MonoBehaviourPun
{
    public GameObject ContentPanal;
    public GameObject PlayerIcon;

    private Dictionary<string, GameObject> playerListEntries;
    public PlayerBehavious[] PlayerValue; 
    // Start is called before the first frame update

    public void Awake()
    {
        playerListEntries = new Dictionary<string, GameObject>();

        foreach (Player p in PhotonNetwork.PlayerList)
        {
            GameObject entry = Instantiate(PlayerIcon);
            entry.gameObject.name = p.NickName;
            entry.transform.GetChild(1).gameObject.GetComponent<Text>().text = p.NickName;            
            entry.transform.SetParent(ContentPanal.transform);
            entry.transform.localScale = Vector3.one;           
            playerListEntries.Add(p.NickName, entry);    
        }
    }

    private void Update()
    {

        foreach (Player p in PhotonNetwork.PlayerList)
        {          
            GameObject PlayerGO = GameObject.Find(p.NickName);
            if(PlayerGO.GetComponent<PlayerBehavious>() != null)
            {
                if (PlayerGO.GetComponent<PlayerBehavious>().IsDead == true) playerListEntries[p.NickName].transform.GetChild(0).GetComponent<Image>().color = Color.black;
                if (PlayerGO.GetComponent<PlayerBehavious>().IsDead == false)
                {
                    if(PlayerGO.GetComponent<PlayerBehavious>().PlayerType == PlayerData.PlayerType.Player) playerListEntries[p.NickName].transform.GetChild(0).GetComponent<Image>().color = Color.white;
                    if(PlayerGO.GetComponent<PlayerBehavious>().PlayerType == PlayerData.PlayerType.Hunter) playerListEntries[p.NickName].transform.GetChild(0).GetComponent<Image>().color = Color.red;
                }
            }

        }
    }



}
