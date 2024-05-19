using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;
using Photon.Pun.Demo.Asteroids;
using Photon.Realtime;
using UnityEngine.UI;
using Photon.Pun.UtilityScripts;
using System.Linq;

public class ScoreBoard : MonoBehaviourPun
{
    public GameObject ContentPanal;
    public GameObject ScoreBoardPanal;
    public GameObject ScoreboardObject;

    // Start is called before the first frame update
    public void EndingResult()
    { 
        ScoreBoardPanal.SetActive(true);
        // Create a list of players sorted by score in descending order
        List<Player> sortedPlayers = PhotonNetwork.PlayerList.OrderByDescending(player => player.GetScore()).ToList();

        // Iterate through the sorted list to create scoreboard entries
        for (int i = 0; i < sortedPlayers.Count; i++)
        {
            Player p = sortedPlayers[i];
            GameObject entry = Instantiate(ScoreboardObject);
            entry.gameObject.name = p.NickName;

            entry.transform.GetChild(0).gameObject.GetComponent<Text>().text = (i + 1).ToString(); // Rank
            entry.transform.GetChild(1).gameObject.GetComponent<Text>().text = p.NickName; // Player Name
            entry.transform.GetChild(2).gameObject.GetComponent<Text>().text = p.GetScore().ToString(); // Player Score

            entry.transform.SetParent(ContentPanal.transform);
            entry.transform.localScale = Vector3.one;

        }
        return;
    }
    bool alreadyShow = false;
    private void Update()
    {
        if(GameManager.Instance.CanShowResult && !alreadyShow)
        {
            alreadyShow = true;
            ScoreBoardPanal.SetActive(true);
            EndingResult();
            
        }
    }
}
