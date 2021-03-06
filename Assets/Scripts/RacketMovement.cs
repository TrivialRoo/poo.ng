﻿using UnityEngine;
using System.Collections;

public class RacketMovement : Photon.PunBehaviour, IPunObservable
{
    //Offline playing
    public string axis = "VerticalLeft"; // can only be VerticalLeft or VerticalRight

    //Online playing (lag compensation)
    public Vector2 realPosition = Vector2.zero;
    public Vector2 positionAtLastPacket = Vector2.zero;
    public float currentTime = 0f;
    public float currentPacketTime = 0f;
    public float lastPacketTime = 0f;
    public float timeToReachGoal = 0f;

    public GameController gc;

    //General
    public float speed = 40f;

    void Start()
    {
        positionAtLastPacket = transform.position;
    }

    [PunRPC]
    public void ResetPos()
    {
        transform.position = new Vector2(transform.position.x, 0);
    }

    void FixedUpdate()
    {
        if (PhotonNetwork.offlineMode)
        {
            float v = Input.GetAxisRaw(axis);
            GetComponent<Rigidbody2D>().velocity = new Vector2(0, v) * speed;
        }

        else
        {
            if (photonView.isMine)
            {
                float v = Input.GetAxisRaw("Vertical");
                //GetComponent<Rigidbody2D>().velocity = new Vector2(0, v) * speed;
                if ((transform.position + new Vector3(0, v * speed * Time.deltaTime, 0)).y < 26 && (transform.position + new Vector3(0, v * speed * Time.deltaTime, 0)).y > -26)
                {
                    transform.Translate(new Vector3(0, v, 0) * speed * Time.deltaTime);
                }                
            }

            else if (!photonView.isMine)
            {
                timeToReachGoal = currentPacketTime - lastPacketTime;
                currentTime += Time.deltaTime;
                //transform.position = Vector2.Lerp(positionAtLastPacket, realPosition, currentTime / timeToReachGoal);
            }
        }
    }

    public override void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        /*if (PhotonNetwork.isMasterClient)
        {
            this.name = "LeftRacket";
        }
        else
        {
            this.name = "RightRacket";
        }*/
    }

    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            stream.SendNext((Vector2)transform.position);
        }
        else
        {
            currentTime = 0f;
            positionAtLastPacket = transform.position;
            realPosition = (Vector2)stream.ReceiveNext();
            lastPacketTime = currentPacketTime;
            currentPacketTime = (float)info.timestamp;
        }
    }
}
