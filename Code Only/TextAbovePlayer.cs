using Photon.Pun;
using Photon.Pun.Demo.Asteroids;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class TextAbovePlayer : MonoBehaviourPun
{
    [SerializeField] Transform playerTransform;
    [SerializeField] Text uiText;
    [SerializeField] Vector3 offset;
    [SerializeField] Font customFont;

    // Start is called before the first frame update
    void Start()
    {
        if (uiText == null)
        {
            string Name = this.GetComponent<PhotonView>().Owner.NickName;
            // Create a new GameObject for the Text
            GameObject textObject = new GameObject(Name + " Text");

            // Set the parent to the Canvas (assuming Canvas exists in the scene)
            textObject.transform.SetParent(GameObject.Find("Canvas").transform, false);

            // Add Text component to the GameObject
            uiText = textObject.AddComponent<Text>();

            uiText.text = Name;

            // Set the font size
            uiText.fontSize = 24;

            uiText.font = customFont;

            // Set the font color
            uiText.color = PlayerData.GetColorByViewID(this.GetComponent<PhotonView>().ViewID);

            // Set the alignment
            uiText.alignment = TextAnchor.MiddleCenter;

            uiText.transform.SetSiblingIndex(0);
        }

        if (playerTransform == null) playerTransform = this.transform;              
    }

    // Update is called once per frame
    void Update()
    {
        if(photonView.IsMine)
        {
            bool ishide = this.GetComponent<PlayerBehavious>().IsHide;
            if (ishide) uiText.text = "";
            else uiText.text = this.GetComponent<PhotonView>().Owner.NickName;
            // Check if playerTransform is assigned
            if (playerTransform != null && uiText != null)
            {
                // Convert player's position from world space to screen space
                Vector3 screenPos = Camera.main.WorldToScreenPoint(playerTransform.position + offset);

                // Set the position of the UI text
                uiText.transform.position = screenPos;
            }
        }
        else uiText.text = "";
    }
}
