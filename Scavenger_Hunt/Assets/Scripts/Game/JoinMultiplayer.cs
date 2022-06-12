using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
public class JoinMultiplayer: MonoBehaviourPunCallbacks{
public TMP_InputField createInput;
public TMP_InputField JoinInput;    

    public void createRoom(){
        PhotonNetwork.CreateRoom(createInput.text);

    }

    public void JoinRoom(){
        PhotonNetwork.JoinRoom(JoinInput.text);
    }

    
   
 
    public override void OnJoinedRoom(){
        PhotonNetwork.LoadLevel("WaitingRoom");
    }

   
    public void LeaveLobby(){
        PhotonNetwork.LeaveLobby();
    }
}
