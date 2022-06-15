using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using UnityEngine.SceneManagement;
public class MultiplayerRoom: MonoBehaviourPunCallbacks{

    
    void Update(){
        if (PhotonNetwork.CurrentRoom.PlayerCount >= 2){
             SceneManager.LoadScene("SampleSceneMultiplayer");
        }
    }
 
   

    public void leaveRoom(){
        PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene("StartMenu");
    }

    
}
