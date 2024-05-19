using Photon.Pun;
using Photon.Pun.Demo.Asteroids;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class Jar : MonoBehaviourPun
{
    private List<string> previousListState;
    public List<string> PlayerIDList; 

    void Start()
    {
        PlayerIDList = new List<string>() {};       
        previousListState = new List<string>(PlayerIDList);
        
    }

    void Update()
    {       
        CheckListChanges();
               
    }

    void CheckListChanges()
    {
    
        if (PlayerIDList.Count != previousListState.Count)
        {
            Debug.Log("List count has changed.");
           
            previousListState = new List<string>(PlayerIDList);
            return;
        }

      
        for (int i = 0; i < PlayerIDList.Count; i++)
        {
            if (PlayerIDList[i] != previousListState[i])
            {
                Debug.Log("Element at index " + i + " has changed from " + previousListState[i] + " to " + PlayerIDList[i]);
                // Update the previous state to reflect the current state
                previousListState = new List<string>(PlayerIDList);

                return;
            }
        }  
       
    }

    [PunRPC]
    public void AddElement(string element)
    {
        PlayerIDList.Add(element);
        return;        
    }
    [PunRPC]
    public void RemoveAllElement()
    {
        PlayerIDList.Clear();
        previousListState = new List<string>(PlayerIDList);
    }

    [PunRPC]
    public async void DestroySelf()
    {       
        foreach (string player in PlayerIDList)
        {
            Debug.Log("Player who are dead is: " + player);
            GameObject PlayerGO = GameObject.Find($"/{player}");
            PlayerGO.GetComponent<PlayerBehavious>().photonView.RPC("SetThisPlayerDead", RpcTarget.All);
        }

        await Task.Delay(100);
        
        PlayerIDList.Clear();
        previousListState = new List<string>(PlayerIDList);
        
        this.gameObject.SetActive(false);
    }

    [PunRPC]
    public void ResetJar()
    {              
        PlayerIDList.Clear();
        previousListState = new List<string>(PlayerIDList);
        this.gameObject.SetActive(true);
    }

}
