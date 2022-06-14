using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
public class MultiplayerRoom: MonoBehaviourPunCallbacks{

    
    void Update(){
        if (PhotonNetwork.CurrentRoom.PlayerCount >= 1){
            PhotonNetwork.LoadLevel("SampleSceneMultiplayer");
        }
    }
 
   

    public void leaveRoom(){
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.LoadLevel("StartMenu");
    }

    
}
