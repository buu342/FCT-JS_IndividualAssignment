using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
public class JoinMultiplayer: MonoBehaviourPunCallbacks{
public TMP_InputField createInput;
public TMP_InputField JoinInput;    
public int PlayerCount=0;
 
    public void createRoom(){
        PhotonNetwork.CreateRoom(createInput.text);

    }

    public void JoinRoom(){
        PhotonNetwork.JoinRoom(JoinInput.text);
    }

    
    void Update(){
        if(PlayerCount==1)
        if (PhotonNetwork.CurrentRoom.PlayerCount == 2){
            PhotonNetwork.LoadLevel("SampleScene");
        }
    }
 
    public override void OnJoinedRoom(){
        PlayerCount=1;
        PhotonNetwork.LoadLevel("WaitingRoom");
    }

    public void leaveRoom(){
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.LoadLevel("StartMenu");
    }

    public void LeaveLobby(){
        PhotonNetwork.LeaveLobby();
    }
}
