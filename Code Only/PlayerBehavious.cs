using Photon.Pun.UtilityScripts;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using static Photon.Pun.Demo.Asteroids.PlayerData;

namespace Photon.Pun.Demo.Asteroids
{
    public class PlayerBehavious : MonoBehaviourPun
    {
        [Header("Player Setting")]
        public PlayerType PlayerType;
        public bool IsHide = false; // using for check if player are hide during hiding phase or not
        public bool IsDead = false; // using for check if player are dead at the end of cycle or not       

        [SerializeField] GameObject Target;
        public bool CanWalk = true;
        [SerializeField] int HitableCount = 1;
        public bool EnableHit = false;

        SpriteRenderer sprite;
        Animator animator;        
        BoxCollider2D collider;        
        PlayerController controller;
        GameObject Interaction;
        Image IconPlayer;
        [SerializeField] Light2D PersonalLight;
        [SerializeField] Text ThisPlayerText;
        public int Score = 0;
        void Start()
        {
            sprite = this.GetComponent<SpriteRenderer>();
            animator = this.GetComponent<Animator>();
            collider = this.GetComponent<BoxCollider2D>();
            controller = this.GetComponent<PlayerController>(); 
            sprite.color = PlayerData.GetColorByViewID(photonView.ViewID);                       

            this.gameObject.name = photonView.Owner.NickName;

            GameManager.Instance.OnPlayerObjCreated(this);

            if(photonView.IsMine)
            {
                Interaction = GameObject.Find("/Canvas/Interaction Button");

                Interaction.GetComponent<Button>().onClick.RemoveAllListeners();
                Interaction.GetComponent<Button>().onClick.AddListener(InteractWithObject);
             
                if (PlayerType == PlayerData.PlayerType.Hunter) Interaction.transform.GetChild(0).GetComponent<Text>().text = "Break Jar";
                else Interaction.transform.GetChild(0).GetComponent<Text>().text = "Hide";
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!photonView.IsMine) return; // Ensure only the local player processes this

            if (PlayerType == PlayerType.Player)
            {
                if (other.CompareTag("HiddenObject"))
                {
                    other.GetComponent<SpriteRenderer>().color = Color.yellow;
                    Debug.Log("Found");
                    Target = other.gameObject;
                }
            }

            if (PlayerType == PlayerType.Hunter)
            {
                if (other.CompareTag("HiddenObject"))
                {
                    other.GetComponent<SpriteRenderer>().color = Color.yellow;
                    //other.GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.LocalPlayer);
                    Debug.Log("Found Player");
                    Target = other.gameObject;
                }
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!photonView.IsMine) return; // Ensure only the local player processes this            

            if (PlayerType == PlayerType.Player)
            {
                if (other.CompareTag("HiddenObject"))
                {
                    other.GetComponent<SpriteRenderer>().color = Color.white;
                    
                }
            }

            if (PlayerType == PlayerType.Hunter)
            {
                if (other.CompareTag("HiddenObject"))
                {
                    other.GetComponent<SpriteRenderer>().color = Color.white;
                   
                }
            }
        }

        public void InteractWithObject()
        {    
            if (PlayerType == PlayerType.Player)
            {
                if (Target)
                {
                    photonView.RPC("ChangeSpriteRPC", RpcTarget.All);
                    Target.GetComponent<Jar>().photonView.RPC("AddElement", RpcTarget.All, photonView.Owner.NickName);

                    transform.position = Target.transform.position;

                    this.GetComponent<PlayerController>().CanWalk = false;
                    return;
                }
            }

            if (PlayerType == PlayerType.Hunter)
            {
                if (Target && EnableHit)
                {
                    int PlayerCount = Target.GetComponent<Jar>().PlayerIDList.Count;
                    photonView.RPC("KillPlayer", RpcTarget.All, PlayerCount);
                    //Target.GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.LocalPlayer);
                    Target.GetComponent<Jar>().photonView.RPC("DestroySelf", RpcTarget.All);
                    return;
                }
            }

        }

        // Start is called before the first frame update


        // Update is called once per frame
        void Update()
        {
            if (photonView.IsMine)
            {                                         
                
                Interaction = GameObject.Find("/Canvas/Interaction Button");
                //Interaction.GetComponent<Button>().onClick.AddListener(InteractWithObject);
                if (ThisPlayerText == null) ThisPlayerText = GameObject.Find("/Canvas/Player Panal/Text").GetComponent<Text>();
                IconPlayer = GameObject.Find("/Canvas/Player Panal/Icon").GetComponent<Image>();
                if (PlayerType == PlayerData.PlayerType.Hunter)
                {
                    PersonalLight.intensity = 1;
                    Interaction.transform.GetChild(0).GetComponent<Text>().text = "Break Jar";                    
                    ThisPlayerText.text = string.Format("{0}\nScore: {1}", photonView.Owner.NickName, PhotonNetwork.LocalPlayer.GetScore());
                    ThisPlayerText.color = PlayerData.GetColor(PhotonNetwork.LocalPlayer.GetPlayerNumber());

                    IconPlayer.color = Color.red;
                }
                if (PlayerType == PlayerData.PlayerType.Player)
                {
                    PersonalLight.intensity = 0;
                    Interaction.transform.GetChild(0).GetComponent<Text>().text = "Hide";
                    ThisPlayerText.text = string.Format("{0}\nScore: {1}\n Is Hiding: {2}", photonView.Owner.NickName, PhotonNetwork.LocalPlayer.GetScore(), IsHide);                    
                    ThisPlayerText.color = PlayerData.GetColor(PhotonNetwork.LocalPlayer.GetPlayerNumber());

                    if (IsDead) IconPlayer.color = Color.black;
                    if (!IsDead) IconPlayer.color = Color.white;
                }                                             
        
                if (Input.GetKeyDown(KeyCode.E))
                {
                    Debug.Log("Press E");
                 
                    InteractWithObject();
                }
            }

            if(CanWalk) { controller.CanWalk = true; }
            if(!CanWalk) { controller.CanWalk = false; }


        }

        [PunRPC]
        public void KillPlayer(int PlayerCount)
        {
            if (photonView.IsMine)
            {
                //PhotonNetwork.LocalPlayer.AddScore(PLayerNumber);
                if(PlayerCount > 0) PhotonNetwork.LocalPlayer.AddScore(1); //Change to 1 due game design

                if(HitableCount > 0) HitableCount--;
                if(HitableCount <= 0) EnableHit = false;

            }      
        }

        [PunRPC]
        public void AddThisPlayerScore(int HostScore,PhotonMessageInfo info) //using for player to adding score
        {            
            if (photonView.IsMine)
            {
                if (PlayerType == PlayerData.PlayerType.Player)
                {
                    if (!IsDead)
                    {
                        PhotonNetwork.LocalPlayer.SetScore(HostScore);
                        Debug.LogWarning(photonView.Owner.NickName + " has been called AddThisPlayerScore()");
                    }
                }
            }
        }     

        [PunRPC]
        public void SetThisPlayerDead() //using for player to adding score
        {
           IsDead = true;
           Debug.LogWarning("Set Death");          
        }    

        [PunRPC]
        void ChangeSpriteRPC()
        {
            Debug.Log("ChangeSpriteRPC");           
            IsHide = true;
            CanWalk = false;
            PersonalLight.intensity = 0;
            this.GetComponent<SpriteRenderer>().sprite = null;
            animator.enabled = false;
            collider.enabled = false;
            this.transform.GetChild(0).gameObject.SetActive(false);
            this.GetComponent<PlayerController>().CanWalk = false;
            sprite.color = Color.white;

            //PhotonView photonView = PhotonView.Get(this);
        }

        [PunRPC]
        public void ResetPlayerSprite()
        {
           // Debug.Log("ChangeSpriteRPC");
            EnableHit = false;
            CanWalk = true;
            IsDead = false; //Prevent Bug to adding score to player
            if (this.PlayerType == PlayerData.PlayerType.Player)
            {                             
                PersonalLight.intensity = 0;               
            }         

            if(this.PlayerType == PlayerData.PlayerType.Hunter)
            {               
                if (GameManager.Instance.Turn > 3) { HitableCount = 2; }
                else HitableCount = 1;
                PersonalLight.intensity = 1;
            }

            IsHide = false;
            switch (photonView.ViewID)
            {
                case 1001: transform.position = new Vector3(-1, 3); break;
                case 2001: transform.position = new Vector3(-1, 0.08f); break;
                case 3001: transform.position = new Vector3(-1, -2); break;
                case 4001: transform.position = new Vector3(0.48f, -3.15f); break;
                case 5001: transform.position = new Vector3(2.85f, -3.15f); break;
                case 6001: transform.position = new Vector3(5.19f, -3.15f); break;
                case 7001: transform.position = new Vector3(6.99f, -1.92f); break;
                case 8001: transform.position = new Vector3(6.99f, 0.25f); break;
                case 9001: transform.position = new Vector3(6.99f, 3.02f); break;
                case 10001: transform.position = new Vector3(4.06f, 4.22f); break;

                case 20001: transform.position = new Vector3(-1 / 2, 3 / 2); break;
                case 30001: transform.position = new Vector3(-1 / 2, 0.08f); break;
                case 40001: transform.position = new Vector3(-1 / 2, -2 / 2); break;
                case 50001: transform.position = new Vector3(0.48f / 2, -3.15f / 2); break;
                case 60001: transform.position = new Vector3(2.85f / 2, -3.15f); break;
                case 70001: transform.position = new Vector3(5.19f / 2, -3.15f / 2); break;
                case 80001: transform.position = new Vector3(6.99f / 2, -1.92f / 2); break;
                case 90001: transform.position = new Vector3(6.99f / 2, 0.25f / 2); break;
                case 100001: transform.position = new Vector3(6.99f / 2, 3.02f / 2); break;
            }

            
            this.GetComponent<SpriteRenderer>().sprite = null;
            animator.enabled = true;
            collider.enabled = true;
            this.transform.GetChild(0).gameObject.SetActive(true);
            this.GetComponent<PlayerController>().CanWalk = true;
            sprite.color = PlayerData.GetColorByViewID(photonView.ViewID);


        }
    }

}



