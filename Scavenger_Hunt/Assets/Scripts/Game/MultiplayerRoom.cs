using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
public class MultiplayerRoom: MonoBehaviourPunCallbacks{

    
    void Update(){
        if (PhotonNetwork.CurrentRoom.PlayerCount == 2){
            PhotonNetwork.LoadLevel("SampleScene");
        }
    }
 
   

    public void leaveRoom(){
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.LoadLevel("StartMenu");
    }

    
}
