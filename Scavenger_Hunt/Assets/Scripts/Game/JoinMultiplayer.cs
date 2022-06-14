using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
public class JoinMultiplayer: MonoBehaviourPunCallbacks{
public TMP_InputField createInput;
public TMP_InputField JoinInput;    
public static bool RoomCreator=false;
public static bool Multiplayer=false;
    public void createRoom(){
        PhotonNetwork.CreateRoom(createInput.text);
        RoomCreator=true;
        Multiplayer=true;
    }

    public void JoinRoom(){
        PhotonNetwork.JoinRoom(JoinInput.text);
        Multiplayer=true;
    }

    
   
 
    public override void OnJoinedRoom(){
        Application.LoadLevel("WaitingRoom");
    }

   
    public void LeaveLobby(){
        PhotonNetwork.LeaveLobby();
    Multiplayer=false;
    }

    public bool RoomOwner(){
        return RoomCreator;
    }
}
