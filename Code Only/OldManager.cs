/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OldManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
*/
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AsteroidsGameManager.cs" company="Exit Games GmbH">
//   Part of: Asteroid demo
// </copyright>
// <summary>
//  Game Manager for the Asteroid Demo
// </summary>
// <author>developer@exitgames.com</author>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections;

using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using Photon.Pun.UtilityScripts;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using System.Collections.Generic;
using Photon.Pun.Demo.PunBasics;
using System.Linq;
/*using System.Xml;
using System.Data;
using System.Reflection;
using UnityEngine.SocialPlatforms.Impl;
using ExitGames.Client.Photon.StructWrapping;
using static UnityEngine.GraphicsBuffer;*/

namespace Photon.Pun.Demo.Asteroids
{
    public class OldManager : MonoBehaviourPunCallbacks
    {
        public static GameManager Instance = null;
        //Player Text
        public GameObject InfoTextGO;
        public Text InfoText;
        public GameObject Blindflod;
        // Dictionary to map Photon Players to their GameObjects
        private Dictionary<int, GameObject> playerObjects = new();

        private int localViewId;


        #region UNITY

        public void Awake()
        {
            //Instance = this;
        }

        public override void OnEnable()
        {
            base.OnEnable();

            CountdownTimer.OnCountdownTimerHasExpired += OnCountdownTimerIsExpired;
        }

        public void Start()
        {
            Hashtable props = new Hashtable
            {
                {PlayerData.PLAYER_LOADED_LEVEL, true}
            };
            PhotonNetwork.LocalPlayer.SetCustomProperties(props);
        }

        public override void OnDisable()
        {
            base.OnDisable();

            CountdownTimer.OnCountdownTimerHasExpired -= OnCountdownTimerIsExpired;
        }

        #endregion

        #region COROUTINES  

      
        private IEnumerator EndOfGame(string winner)
        {
            InfoTextGO.transform.localPosition = new Vector3(0, 0, 0);
            float timer = 5.0f;

            while (timer > 0.0f)
            {
                InfoText.text = string.Format("<color=#FF0000><size=30> {0} </size></color>.\n\n\nReturning to login screen in {1} seconds.", winner, timer.ToString("n2"));

                yield return new WaitForEndOfFrame();

                timer -= Time.deltaTime;
            }

            PhotonNetwork.LeaveRoom();
        }

        #endregion

        [PunRPC]
        private void SendRPCStartPlayerHidState()
        {
            if (!PhotonNetwork.IsMasterClient)
                return;

            photonView.RPC("RPCStartPlayerHideState", RpcTarget.All);
        }

        [PunRPC]
        private void RPCStartPlayerHideState()
        {
            StartCoroutine(UpdateTimerPlayerHideState());
        }

        private IEnumerator UpdateTimerPlayerHideState()
        {
            foreach (int playerID in playerObjects.Keys)
            {
                Debug.Log("PLayerList: " + playerID);
                GameObject playerGO = playerObjects[playerID];

                if (playerGO != null)
                {
                    // Get the PlayerBehavious component of the player GameObject
                    PlayerBehavious playerBehavious = playerGO.GetComponent<PlayerBehavious>();

                    if (playerBehavious != null)
                    {

                        if (playerBehavious.PlayerType.Equals(PlayerData.PlayerType.Player))
                        {
                            //Active ui
                            InfoTextGO.transform.localPosition = new Vector3(0, -498, 0);
                            InfoText.text = string.Format("<color=#00FF00><size=30>Hiding Phase</size></color>\nYou are the Player who need to hide from hunter after <color=#FF0000>60 second will be start Hunting Phase</color>\n");
                            yield return new WaitForSeconds(5f);

                            float waitTime = 10f;
                            while (waitTime > 0f)
                            {
                                //Update ui
                                InfoText.text = string.Format("Hunting Phase will Begin in\n <color=#FF0000><size=50> {0} </size></color>", waitTime.ToString("n2"));
                                yield return null;
                                waitTime -= Time.deltaTime;

                            }
                            InfoTextGO.transform.localPosition = new Vector3(0, 0, 0);

                            StopPlayerHideState();
                        }
                        if (playerBehavious.PlayerType.Equals(PlayerData.PlayerType.Hunter))
                        {
                            //Active ui
                            playerGO.GetComponent<PlayerController>().CanWalk = false;
                            Blindflod.SetActive(true);
                            InfoText.text = string.Format("<color=#00FF00><size=30>Hiding Phase</size></color>\nYou are the Hunter who need to Kill another player after <color=#FF0000>60 second will be start Hunting Phase</color>\n");
                            yield return new WaitForSeconds(5f);


                            float waitTime = 10f;
                            while (waitTime > 0f)
                            {
                                //Update ui
                                InfoText.text = string.Format("Hunting Phase will Begin in\n <color=#FF0000><size=50> {0} </size></color>", waitTime.ToString("n2"));
                                yield return null;
                                waitTime -= Time.deltaTime;

                            }
                            Blindflod.SetActive(false);
                            StopPlayerHideState();
                        }
                    }
                }
            }
        }


        private void StopPlayerHideState()
        {
            StopCoroutine(UpdateTimerPlayerHideState());

            photonView.RPC("SendRPCStartHunt", RpcTarget.All);
        }

        [PunRPC]
        private void SendRPCStartHunt()
        {
            if (!PhotonNetwork.IsMasterClient)
                return;

            photonView.RPC("RPCStartHunt", RpcTarget.All);
        }

        [PunRPC]
        private void RPCStartHunt()
        {
            //Start hunt.
            StartCoroutine(UpdateTimerPlayerHuntState());
        }

        private IEnumerator UpdateTimerPlayerHuntState()
        {
            Blindflod.SetActive(false);
            yield return new WaitForSeconds(2f);

            foreach (int playerID in playerObjects.Keys)
            {
                Debug.Log("PLayerList: " + playerID);
                GameObject playerGO = playerObjects[playerID];


                if (playerGO != null)
                {
                    // Get the PlayerBehavious component of the player GameObject
                    PlayerBehavious playerBehavious = playerGO.GetComponent<PlayerBehavious>();

                    if (playerBehavious != null)
                    {

                        if (playerBehavious.PlayerType.Equals(PlayerData.PlayerType.Player))
                        {
                            playerGO.GetComponent<PlayerController>().CanWalk = false;
                            InfoText.text = string.Format("<color=#FF0000><size=30>Hunting Phase</size></color>\nYou are the Player who need to hide from hunter after <color=#00FF00>60 second you will win</color>\n");
                            yield return new WaitForSeconds(5f);
                            float timer = 10f;
                            while (timer > 0.0f)
                            {
                                InfoText.text = string.Format("Hunting Phase will Begin in\n <color=#FF0000><size=50> {0} </size></color>", timer.ToString("n2"));

                                yield return new WaitForEndOfFrame();

                                timer -= Time.deltaTime;
                            }

                            StopPlayerHuntState();
                        }
                        if (playerBehavious.PlayerType.Equals(PlayerData.PlayerType.Hunter))
                        {
                            InfoText.text = string.Format("<color=#FF0000><size=30>Hunting Phase</size></color>\nYou are the Hunter who need to Kill another player in <color=#FF0000>60 second you will be win</color>\n");
                            yield return new WaitForSeconds(5f);
                            float timer = 10f;
                            while (timer > 0.0f)
                            {
                                InfoText.text = string.Format("Hunting Phase will Begin in\n <color=#FF0000><size=50> {0} </size></color>", timer.ToString("n2"));

                                yield return new WaitForEndOfFrame();

                                timer -= Time.deltaTime;
                            }

                            playerGO.GetComponent<PlayerController>().CanWalk = true;
                            StopPlayerHuntState();
                        }
                    }
                }
            }

            InfoText.text = "";
            yield return new WaitForSeconds(1f);
            InfoTextGO.transform.localPosition = new Vector3(0, -498, 0);

            float timer2 = 60f;
            while (timer2 > 0.0f)
            {
                InfoText.text = string.Format("Hunting Phase will be End in\n <color=#FF0000><size=50> {0} </size></color>", timer2.ToString("n2"));

                yield return new WaitForEndOfFrame();

                timer2 -= Time.deltaTime;
            }
            CheckEndOfGame();
        }

        private void StopPlayerHuntState()
        {
            StopCoroutine(UpdateTimerPlayerHuntState());

            //using for create another round
        }
        #region PUN CALLBACKS

        public override void OnDisconnected(DisconnectCause cause)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Lobby");
        }

        public override void OnLeftRoom()
        {
            PhotonNetwork.Disconnect();
        }

        public override void OnMasterClientSwitched(Player newMasterClient)
        {
            if (PhotonNetwork.LocalPlayer.ActorNumber == newMasterClient.ActorNumber)
            {
                //StartCoroutine(SpawnAsteroid());
            }
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            CheckEndOfGame();
        }

        public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
            if (changedProps.ContainsKey(PlayerData.PLAYER_LIVES))
            {
                CheckEndOfGame();
                return;
            }

            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }


            // if there was no countdown yet, the master client (this one) waits until everyone loaded the level and sets a timer start
            int startTimestamp;
            bool startTimeIsSet = CountdownTimer.TryGetStartTime(out startTimestamp);

            if (changedProps.ContainsKey(PlayerData.PLAYER_LOADED_LEVEL))
            {
                if (CheckAllPlayerLoadedLevel())
                {
                    if (!startTimeIsSet)
                    {
                        CountdownTimer.SetStartTime();
                    }
                }
                else
                {
                    // not all players loaded yet. wait:
                    Debug.Log("setting text waiting for players! ", this.InfoText);
                    InfoText.text = "Waiting for other players...";
                }
            }

        }

        #endregion


        // called by OnCountdownTimerIsExpired() when the timer ended
        private void StartGame()
        {
            Debug.Log("StartGame!");

            // on rejoin, we have to figure out if the spaceship exists or not
            // if this is a rejoin (the ship is already network instantiated and will be setup via event) we don't need to call PN.Instantiate

            //float angularStart = (360.0f / PhotonNetwork.CurrentRoom.PlayerCount) * PhotonNetwork.LocalPlayer.GetPlayerNumber();
            float angularStart = PhotonNetwork.CurrentRoom.PlayerCount * PhotonNetwork.LocalPlayer.GetPlayerNumber();
            float x = 2.0f * Mathf.Sin(angularStart * Mathf.Deg2Rad);
            float y = 2.0f * Mathf.Cos(angularStart * Mathf.Deg2Rad);
            Vector3 position = new Vector3(x, y, 0.0f);
            Quaternion rotation = Quaternion.Euler(0.0f, angularStart, 0.0f);

            GameObject Player = PhotonNetwork.Instantiate("Player", position, rotation, 0);

            var playerView = Player.GetComponent<PhotonView>();

            localViewId = playerView.ViewID;

            playerView.RPC("AddPlayerRPC", RpcTarget.All, playerView.ViewID);

            /*if (PhotonNetwork.IsMasterClient) //this is host or not
            {
                // Randomly select one player index to be the hunter
                int hunterIndex = Random.Range(0, PhotonNetwork.PlayerList.Length);
                Debug.Log("Player who gone na be hunter is: " + hunterIndex);
                Debug.Log("PhotonNetwork.PlayerList.length: " + PhotonNetwork.PlayerList);

                // Loop through players and assign roles

                var hunterPlayer = PhotonNetwork.PlayerList[hunterIndex];
                PlayerBehavious playerBehavious = playerGO.GetComponent<PlayerBehavious>();

                int i = 0;
                foreach (Player player in PhotonNetwork.PlayerList)
                {
                    Debug.Log("PLayerList: " + player);
                    PhotonNetwork.LocalPlayer.CustomProperties["PlayerName"] = player;
                    // Find the GameObject associated with the Photon Player
                    GameObject playerGO = playerObjects[player];

                    if (playerGO != null)
                    {
                        // Get the PlayerBehavious component of the player GameObject
                        PlayerBehavious playerBehavious = playerGO.GetComponent<PlayerBehavious>();
                        int pId = player.GetPlayerNumber();
                        if (playerBehavious != null)
                        {
                            // Set the player as a hunter if the index matches
                            if (i == hunterIndex)
                            {
                                //playerGO.GetComponent<SpriteRenderer>().color = PlayerData.GetColor(player.GetPlayerNumber());
                                playerBehavious.GetComponent<PhotonView>().RPC("SetPlayerType", RpcTarget.All, "Hunter", pId );                                
                            }
                            else
                            {
                                //playerGO.GetComponent<SpriteRenderer>().color = PlayerData.GetColor(player.GetPlayerNumber());
                                playerBehavious.GetComponent<PhotonView>().RPC("SetPlayerType", RpcTarget.All, "Player", pId );
                            }
                        }
                    }
                    i++;
                }
            }
*/
        }

        [PunRPC]
        private void AddPlayerRPC(int viewID)
        {
            //bring PlayerID & GameObject into Dictionary
            var playerPhotonView = PhotonNetwork.GetPhotonView(viewID);
            playerObjects.Add(viewID, playerPhotonView.gameObject);

            if (playerObjects.Count < PhotonNetwork.CurrentRoom.PlayerCount) //Start Adding Role when all players are ready
            {
                return;
            }

            if (!PhotonNetwork.IsMasterClient) //this is host or not
            {
                return;
            }

            //Only host will do this 


            var keyViewIdArray = playerObjects.Keys.ToArray();

            // Randomly select one player index to be the hunter
            int hunterIndex = Random.Range(0, keyViewIdArray.Length);
            int playerPhotonViewId = keyViewIdArray[hunterIndex];

            Debug.Log("Player who gone na be hunter is: " + hunterIndex);

            // Loop through players with RPC and assign roles to apply hunter Role
            photonView.RPC("RPCApplyHuterRole", RpcTarget.All, playerPhotonViewId);

            photonView.RPC("SendRPCStartPlayerHidState", RpcTarget.All);
        }

        [PunRPC]
        private void RPCApplyHuterRole(int viewID)
        {
            var hunterPlayerObject = playerObjects[viewID]; //get specific plyaer gameobject

            if (localViewId == viewID)
            {
                //Hunter player
                hunterPlayerObject.GetComponent<PlayerBehavious>().PlayerType = PlayerData.PlayerType.Hunter;
            }
            else
            {
                //Normal player
                hunterPlayerObject.GetComponent<PlayerBehavious>().PlayerType = PlayerData.PlayerType.Player;
            }
        }

        private bool CheckAllPlayerLoadedLevel()
        {
            foreach (Player p in PhotonNetwork.PlayerList)
            {
                object playerLoadedLevel;

                if (p.CustomProperties.TryGetValue(PlayerData.PLAYER_LOADED_LEVEL, out playerLoadedLevel))
                {
                    if ((bool)playerLoadedLevel)
                    {
                        continue;
                    }
                }

                return false;
            }

            return true;
        }

        private void CheckEndOfGame()
        {
            bool allDestroyed = true;

            foreach (Player p in PhotonNetwork.PlayerList)
            {
                object lives;
                if (p.CustomProperties.TryGetValue(PlayerData.PLAYER_LIVES, out lives))
                {
                    if ((int)lives <= 0)
                    {
                        allDestroyed = false;
                        break;
                    }
                }
            }

            if (allDestroyed)
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    StopAllCoroutines();
                }

                string winner = "Player Win";
                int survivalCount = 0;

                foreach (GameObject playerGO in playerObjects.Values)
                {
                    PlayerBehavious playerBehavious = playerGO.GetComponent<PlayerBehavious>();
                    if (playerBehavious.IsDead == false && playerBehavious.PlayerType.Equals(PlayerData.PlayerType.Player))
                    {
                        survivalCount += 1;
                    }
                }

                if (survivalCount > 0)
                {
                    winner = "Player Win";
                    StartCoroutine(EndOfGame(winner));
                }
                else
                {
                    winner = "Hunter Win";
                    StartCoroutine(EndOfGame(winner));
                }

            }
        }

        private void OnCountdownTimerIsExpired()
        {
            StartGame();
            //Start Turn here
            //StartCoroutine(PlayerTurnTimer());            
        }

        /* [PunRPC]
         private void SetRole(GameObject playerGO, PlayerData.PlayerType Role)
         {            
             PlayerBehavious playerBehavious = playerGO.GetComponent<PlayerBehavious>();
             playerBehavious.SetPlayerType(Role);                       
         }*/
        [PunRPC]
        void StartTurnSequence()
        {
            Debug.LogWarning(PhotonNetwork.LocalPlayer.CustomProperties["PlayerName"] + " : " + PhotonNetwork.LocalPlayer.CustomProperties["PlayerType"]);
        }

        private void Update()
        {
            /*  foreach (KeyValuePair<Player, GameObject> kvp in playerObjects)
              {
                  if (kvp.Key.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
                  {
                      if(kvp.Value.GetComponent<PlayerBehavious>().PlayerType == PlayerData.PlayerType.Hunter)
                      {
                          kvp.Value.GetComponent<SpriteRenderer>().color = Color.red;
                      }
                      else kvp.Value.GetComponent<SpriteRenderer>().color = Color.green;             
                  }
              }*/

        }
    }
}