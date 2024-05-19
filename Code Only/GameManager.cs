using System.Collections;

using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using Photon.Pun.UtilityScripts;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine.Rendering.Universal;
using Unity.VisualScripting.Antlr3.Runtime.Tree;


namespace Photon.Pun.Demo.Asteroids
{
    public class GameManager : MonoBehaviourPunCallbacks
    {
        [Header("Game Setting")]
        public static GameManager Instance = null;
        //Player Text
        public GameObject InfoTextGO;
        public Text InfoText;        
        //public GameObject Blindflod;        

        // Dictionary to map Photon Players to their GameObjects        
        private Dictionary<int, PlayerBehavious> playerListObjects = new Dictionary<int, PlayerBehavious>();
        [SerializeField] GameObject ThisPlayerGameObject;
        [SerializeField] int hunterIndex;
        [SerializeField] Text TurnAnnouncement;
        [SerializeField] Light2D GlobalLight;        
        [SerializeField] Font customFont;
        public bool CanShowResult = false;
        [SerializeField] GameObject ScoreBoardPanal;

        [Header("Turn Setting")]
        public int Turn = 1;
        public int Cycle = 1;
        public int MaxCycle = 3;
        public bool IsHidingPhase = true;

        [Header("Connection Setting")]
        public int readyCount = 0;
        [SerializeField] int MaxPlayerNumber;
        private const string TurnNumberKey = "TurnNumber";        
        
        #region UNITY

        public void Awake()
        {
            Instance = this;          
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
            StartCoroutine(PlayerTurnTimer());
            
        }

        [PunRPC]
        private void ResetPlayerSprite()
        {
            foreach (PlayerBehavious playerBehavious in playerListObjects.Values)
            {
                playerBehavious.ResetPlayerSprite();
            }
        }

        [PunRPC]
        private void ResetPlayerOnlyLive()
        {
            foreach (PlayerBehavious playerBehavious in playerListObjects.Values)
            {
                if(playerBehavious.IsDead == false) playerBehavious.ResetPlayerSprite();
            }
        }

        private IEnumerator PlayerTurnTimer()
        {
            IsHidingPhase = true;
            InfoTextGO.transform.localPosition = new Vector3(0, 0, 0);
            TurnAnnouncement.text = string.Format("Round {0} / 3\n\rTurn {1}", Cycle,Turn);

            if(PhotonNetwork.IsMasterClient)
            {
                photonView.RPC("ResetPlayerOnlyLive", RpcTarget.All);


                GameObject JarParent = GameObject.Find("/Jar GameObject");
                foreach (Transform child in JarParent.transform)
                {                    
                    child.GetComponent<Jar>().photonView.RPC("RemoveAllElement", RpcTarget.All);
                }
            }

            yield return new WaitForSeconds(1f);

            if (ThisPlayerGameObject.GetComponent<PhotonView>().ViewID == hunterIndex)
            {

                playerListObjects[hunterIndex].CanWalk = false;
                //Blindflod.SetActive(true);
                GlobalLight.intensity = 0;
                InfoText.text = string.Format("<color=#00FF00><size=30>Hiding Phase</size></color>\nYou are the Hunter who need to Kill another player after <color=#FF0000>20 second will be start Hunting Phase</color>\n");
                yield return new WaitForSeconds(5f);
                float timer = 20f;
                while (timer >= 0.0f)
                {
                    InfoText.text = string.Format("Hunting Phase will Begin in\n <color=#FF0000><size=50> {0} </size></color>", timer.ToString("n2"));

                    yield return new WaitForEndOfFrame();

                    timer -= Time.deltaTime;
                }
                //Blindflod.SetActive(false);            
            }
            else
            {
                GlobalLight.intensity = 1;
                playerListObjects[ThisPlayerGameObject.GetComponent<PhotonView>().ViewID].CanWalk = false;
                InfoText.text = string.Format("<color=#00FF00><size=30>Hiding Phase</size></color>\nYou are the Player who need to hide from hunter after <color=#FF0000>20 second will be start Hunting Phase</color>\n");
                yield return new WaitForSeconds(5f);
                playerListObjects[ThisPlayerGameObject.GetComponent<PhotonView>().ViewID].CanWalk = true;
                InfoTextGO.transform.localPosition = new Vector3(0, -498, 0);
                float timer = 20f;
                while (timer >= 0.0f)
                {
                    InfoText.text = string.Format("Hunting Phase will Begin in\n <color=#FF0000><size=50> {0} </size></color>", timer.ToString("n2"));

                    yield return new WaitForEndOfFrame();

                    timer -= Time.deltaTime;
                }
                InfoTextGO.transform.localPosition = new Vector3(0, 0, 0);
            }


            // After 60 seconds, start the hunting phase           
            SendRPCStartHunt();
        }

        private void StopPlayerHideState()
        {
            StopCoroutine(PlayerTurnTimer());

            photonView.RPC("SendRPCStartHunt", RpcTarget.All);
        }

        [PunRPC]
        private void SendRPCStartHunt()
        {
            if (!PhotonNetwork.IsMasterClient)
                return;

            photonView.RPC(nameof(RPCStartHunt), RpcTarget.All);
        }

        [PunRPC]
        private void RPCStartHunt()
        {
            //Start hunt.
            StartCoroutine(HuntingPhaseTimer());
        }
        [PunRPC]
        void ResetJar()
        {
            GameObject JarParent = GameObject.Find("/Jar GameObject");
            foreach (Transform child in JarParent.transform)
            {
                child.GetComponent<Jar>().photonView.RPC("ResetJar", RpcTarget.All);
            }
        }
        private IEnumerator HuntingPhaseTimer()
        {
            //Blindflod.SetActive(false);
            
            yield return new WaitForSeconds(2f);
            IsHidingPhase = false;

            if(PhotonNetwork.IsMasterClient)
            {
                //Check Player Who are not hiding yet after hiding phase
                foreach (PlayerBehavious playerBehavious in playerListObjects.Values)
                {
                    if (playerBehavious.PlayerType == PlayerData.PlayerType.Player)
                    {
                        if (playerBehavious.IsHide == false)
                        {
                            playerBehavious.photonView.RPC("ChangeSpriteRPC", RpcTarget.All);
                            playerBehavious.photonView.RPC("SetThisPlayerDead", RpcTarget.All);
                        }
                    }
                }
            }

         
            
            if (ThisPlayerGameObject.GetComponent<PhotonView>().ViewID == hunterIndex)
            {
                GlobalLight.intensity = 1;
                playerListObjects[hunterIndex].CanWalk = false;
                InfoText.text = string.Format("<color=#FF0000><size=30>Hunting Phase</size></color>\nYou are the Hunter who need to Kill another player in <color=#FF0000>15 second you will be win</color>\n");
                yield return new WaitForSeconds(5f);
                float timer = 5f;
                while (timer >= 0.0f)
                {
                    InfoText.text = string.Format("Hunting Phase will Begin in\n <color=#FF0000><size=50> {0} </size></color>", timer.ToString("n2"));

                    yield return new WaitForEndOfFrame();

                    timer -= Time.deltaTime;
                }

                playerListObjects[hunterIndex].CanWalk = true;
            }
            else
            {
                GlobalLight.intensity = 1;
                playerListObjects[ThisPlayerGameObject.GetComponent<PhotonView>().ViewID].CanWalk = false;
                InfoText.text = string.Format("<color=#FF0000><size=30>Hunting Phase</size></color>\nYou are the Player who need to hide from hunter after <color=#00FF00>15 second you will win</color>\n");
                yield return new WaitForSeconds(5f);
                float timer = 5f;
                while (timer >= 0.0f)
                {
                    InfoText.text = string.Format("Hunting Phase will Begin in\n <color=#FF0000><size=50> {0} </size></color>", timer.ToString("n2"));

                    yield return new WaitForEndOfFrame();

                    timer -= Time.deltaTime;
                }
            }                   

            InfoText.text = "";
            yield return new WaitForSeconds(1f);
            playerListObjects[hunterIndex].EnableHit = true;
            InfoTextGO.transform.localPosition = new Vector3(0, -498, 0);

            float timer2 = 15f;
            while (timer2 >= 0.0f)
            {
                InfoText.text = string.Format("Hunting Phase will be End in\n <color=#FF0000><size=50> {0} </size></color>", timer2.ToString("n2"));

                yield return new WaitForEndOfFrame();

                timer2 -= Time.deltaTime;
            }

           

            StopPlayerHuntState();
        }

        [PunRPC]
        public void SetCycle(int HostCycle)
        {                 
            this.Cycle = HostCycle; //using cycle in host as cycle in all client           
            
        }

        [PunRPC]
        public void SetTurn(int HostTurn)
        {           
            this.Turn = HostTurn;                      
        }

        private void StopPlayerHuntState()
        {
            StopCoroutine(HuntingPhaseTimer());
            
            //using for create another round
            if (PhotonNetwork.IsMasterClient)
            {
                Cycle += 1; //Set cycle of Host 
                photonView.RPC("SetCycle", RpcTarget.Others, Cycle); //Set cycle of Client
                Debug.ClearDeveloperConsole();

                int SurvivalCount = playerListObjects.Count - 1; // - 1 Hunter

                foreach (PlayerBehavious playerBehavious in playerListObjects.Values)
                {
                    Debug.LogWarning("This player" + playerBehavious.photonView.Owner.NickName +" type in host " + playerBehavious.PlayerType);
                        
                    if (playerBehavious.PlayerType == PlayerData.PlayerType.Player)
                    {
                        if (playerBehavious.IsDead == false)
                        {
                            Debug.LogWarning("This player " + photonView.Owner.NickName + "Pass Adding Score to " + playerBehavious.photonView.Owner.NickName);
                            playerBehavious.Score += 1;
                            /* Debug.Log(playerBehavious.Score);
                            playerBehavious.AddThisPlayerScore(playerBehavious.Score);*/
                            playerBehavious.photonView.RPC("AddThisPlayerScore", RpcTarget.All, playerBehavious.Score);
                        }
                        else SurvivalCount--;
                    }
                }

                if (Cycle > MaxCycle || SurvivalCount <= 0)
                {
                    if (Turn == 5 && Cycle == 4)
                    {
                        StartCoroutine(EndOfGame());
                        //StopAllCoroutines();
                    }
                    else
                    {
                        //Set Cycle and Turn of Host
                        Cycle = 1;
                        Turn++;

                        //Set Cycle and Turn of Client
                        photonView.RPC("SetCycle", RpcTarget.Others, Cycle);
                        photonView.RPC("SetTurn", RpcTarget.Others, Turn);

                        //Reset All Player
                        photonView.RPC("ResetPlayerSprite", RpcTarget.All);
                        ResetJar();
                    }
                }

                photonView.RPC("SendRPCStartPlayerHidState", RpcTarget.All);
               // }
            }
        }
       
        private IEnumerator EndOfGame()
        {           
            CanShowResult = true;
            ScoreBoardPanal.SetActive(true);
            yield return new WaitForSeconds(5f);

            PhotonNetwork.LeaveRoom();
        }

        #endregion

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
                    Debug.Log("setting text waiting for players! ",this.InfoText);
                    InfoText.text = "Waiting for other players...";
                }
            }
        
        }

        #endregion
        
        // called by OnCountdownTimerIsExpired() when the timer ended
        void StartGame()
        {
            Debug.Log("StartGame!");
            MaxPlayerNumber = PhotonNetwork.CurrentRoom.PlayerCount;

            // on rejoin, we have to figure out if the spaceship exists or not
            // if this is a rejoin (the ship is already network instantiated and will be setup via event) we don't need to call PN.Instantiate

            //float angularStart = (360.0f / PhotonNetwork.CurrentRoom.PlayerCount) * PhotonNetwork.LocalPlayer.GetPlayerNumber();
            float angularStart = PhotonNetwork.CurrentRoom.PlayerCount * PhotonNetwork.LocalPlayer.GetPlayerNumber();
            Quaternion rotation = Quaternion.Euler(0.0f, angularStart, 0.0f);
            Vector3 position = new Vector3(0,0,0);

            GameObject PlayerGO = PhotonNetwork.Instantiate("Player", position, rotation, 0);           
            ThisPlayerGameObject = PlayerGO;          

            switch (PlayerGO.GetComponent<PhotonView>().ViewID)
            {
                case 1001: PlayerGO.transform.position = new Vector3(-1, 3); break;
                case 2001: PlayerGO.transform.position = new Vector3(-1, 0.08f); break;
                case 3001: PlayerGO.transform.position = new Vector3(-1, -2); break;
                case 4001: PlayerGO.transform.position = new Vector3(0.48f, -3.15f); break;
                case 5001: PlayerGO.transform.position = new Vector3(2.85f, -3.15f); break;
                case 6001: PlayerGO.transform.position = new Vector3(5.19f, -3.15f); break;
                case 7001: PlayerGO.transform.position = new Vector3(6.99f, -1.92f); break;
                case 8001: PlayerGO.transform.position = new Vector3(6.99f, 0.25f); break;
                case 9001: PlayerGO.transform.position = new Vector3(6.99f, 3.02f); break;
                case 10001: PlayerGO.transform.position = new Vector3(4.06f, 4.22f); break;

                case 20001: PlayerGO.transform.position = new Vector3(-1/2, 3/2); break;
                case 30001: PlayerGO.transform.position = new Vector3(-1/2, 0.08f); break;
                case 40001: PlayerGO.transform.position = new Vector3(-1/2, -2/2); break;
                case 50001: PlayerGO.transform.position = new Vector3(0.48f /2, -3.15f / 2); break;
                case 60001: PlayerGO.transform.position = new Vector3(2.85f / 2, -3.15f); break;
                case 70001: PlayerGO.transform.position = new Vector3(5.19f / 2, -3.15f / 2); break;
                case 80001: PlayerGO.transform.position = new Vector3(6.99f / 2, -1.92f / 2); break;
                case 90001: PlayerGO.transform.position = new Vector3(6.99f / 2, 0.25f / 2); break;
                case 100001: PlayerGO.transform.position = new Vector3(6.99f / 2, 3.02f / 2); break;
            }            
        }
        
        //void start after All player are ready
        void OnCountdownTimerIsExpired()
        {
            StartGame();
            //Start Turn here
            //await Task.Delay(100);
            //StartCoroutine(PlayerTurnTimer());
        }

        public void OnPlayerObjCreated(PlayerBehavious player)
        {
            playerListObjects.Add(player.photonView.ViewID, player);

            if (playerListObjects.Count < MaxPlayerNumber)
                return;

            photonView.RPC(nameof(SendThisClientIsReady), RpcTarget.All);
            Debug.Log("Ready");

            
        }

        [PunRPC]
        private void SendThisClientIsReady()
        {
            if (!PhotonNetwork.IsMasterClient)
                return;

            readyCount++;

            if (readyCount < MaxPlayerNumber)
                return;

            Debug.Log("Next state");

            photonView.RPC("SendRPCStartPlayerHidState", RpcTarget.All);

            hunterIndex = Random.Range(0, PhotonNetwork.PlayerList.Length);
            Debug.Log("Player who gone na be hunter is: " + hunterIndex);
            Debug.Log("PhotonNetwork.PlayerList.length: " + PhotonNetwork.PlayerList);
            

            switch (hunterIndex)
            {
                case 0: hunterIndex = 1001; break;
                case 1: hunterIndex = 2001; break;
                case 2: hunterIndex = 3001; break;
                case 3: hunterIndex = 4001; break;
                case 4: hunterIndex = 5001; break;
                case 5: hunterIndex = 6001; break;
                case 6: hunterIndex = 7001; break;
                case 7: hunterIndex = 8001; break;
                case 8: hunterIndex = 9001; break;
                case 9: hunterIndex = 1011; break;
            }
            photonView.RPC("SetHunterRPC", RpcTarget.All, hunterIndex);
            photonView.RPC("SetRole", RpcTarget.All, hunterIndex);
        }
        [PunRPC]
        void SetRole(int hunterIndex)
        {
            playerListObjects[hunterIndex].PlayerType = PlayerData.PlayerType.Hunter;
            //playerListObjects[hunterIndex].IsDead = true;
        }

        private bool CheckAllPlayerLoadedLevel()
        {
            foreach (Player p in PhotonNetwork.PlayerList)
            {
                object playerLoadedLevel;

                if (p.CustomProperties.TryGetValue(PlayerData.PLAYER_LOADED_LEVEL, out playerLoadedLevel))
                {
                    if ((bool) playerLoadedLevel)
                    {
                        continue;
                    }
                }

                return false;
            }

            return true;
        }               

        private async void CheckEndOfGame()
        {         
    /*        await Task.Delay(1000);

            int HighScore = 0;
            string HighScorePlayerName = "";
            string winner = "";

            foreach (Player p in PhotonNetwork.PlayerList)
            {
                if (p.GetScore() > HighScore)
                {
                    HighScore = p.GetScore();
                    HighScorePlayerName = p.NickName;
                    winner = "Winner is: " + HighScorePlayerName;                    
                }
               else if (p.GetScore() == HighScore)
                {
                    
                    winner += "Draw";
                }
            }*/
            
            StartCoroutine(EndOfGame());          
        }       

        [PunRPC]
        void AddPlayer(int PlayerID, string PlayerGOname )
        {
            Debug.Log("===== testing add to playerListObjects =====");
            Debug.Log("Player Name: " + PlayerGOname);
            Debug.Log("Player ID: " + PlayerID);
            readyCount++;
        }

        [PunRPC]
        void SetHunterRPC(int ID)
        {
            hunterIndex = ID;
        }            
    }
       
    
}