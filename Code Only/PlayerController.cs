using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Pun.Demo.Asteroids;

public class PlayerController : MonoBehaviourPun, IPunObservable
{
    public float speed;
    private Animator animator;
    private Rigidbody2D rb;

    private Vector2 networkPosition;
    private Quaternion networkRotation;
    [SerializeField] FixedJoystick joystick;
    PlayerBehavious playerBehavious;

    public bool CanWalk = true;

    private void Start()
    {
        // Cache the Animator and Rigidbody2D components for efficiency
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        GameObject JoyGO = GameObject.Find("/Canvas/Fixed Joystick");
        joystick = JoyGO.GetComponent<FixedJoystick>();
        playerBehavious = gameObject.GetComponent<PlayerBehavious>();   
        // Disable movement for remote players (controlled by other clients)
        if (!photonView.IsMine)
        {
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }
    }

    private void FixedUpdate()
    {
        // Only the local player should control movement
        if (photonView.IsMine)
        {
            //MovePlayer();
            MoveByJoystick();
            SendNetworkData();
        }
        else
        {
            // Update position and rotation based on network data
            transform.position = Vector2.MoveTowards(transform.position, networkPosition, speed * Time.deltaTime);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, networkRotation, speed * Time.deltaTime * 100f);
        }
    }   

    void MoveByJoystick()
    {
        if (CanWalk) rb.velocity = new Vector2(joystick.Horizontal * speed, joystick.Vertical * speed);
        if (!CanWalk && playerBehavious.PlayerType == PlayerData.PlayerType.Hunter)
        {
            Vector2 Slow = new Vector2(joystick.Horizontal * speed, joystick.Vertical * speed);
            rb.velocity = Slow / 3;
        }
            
    }

    private void SendNetworkData()
    {
        // Send local player's position and rotation to other players
        photonView.RPC("SendMovementData", RpcTarget.Others, transform.position, transform.rotation);
    }

    [PunRPC]
    private void SendMovementData(Vector3 position, Quaternion rotation)
    {
        // Receive position and rotation data from the network
        networkPosition = position;
        networkRotation = rotation;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // Send local player's position and rotation data
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
        }
        else
        {
            // Receive position and rotation data for remote players
            networkPosition = (Vector3)stream.ReceiveNext();
            networkRotation = (Quaternion)stream.ReceiveNext();
        }
    }
}

