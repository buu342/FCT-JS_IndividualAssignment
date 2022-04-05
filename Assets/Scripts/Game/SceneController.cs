/****************************************************************
                       SceneController.cs
    
This script handles the loading of scenes
****************************************************************/

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    private bool m_GoToNextScene = false;
    
    
    /*==============================
        LoadAsyncScene
        An asynchronous coroutine that loads
        a given scene
        @param The scene name to load
    ==============================*/
    
    IEnumerator LoadAsyncScene(string scenename)
    {
        AsyncOperation nextscene = SceneManager.LoadSceneAsync(scenename);
        nextscene.allowSceneActivation = false;

        // Wait until the asynchronous scene fully loads
        while (!nextscene.isDone)
        {
            if (this.m_GoToNextScene)
                nextscene.allowSceneActivation = true;
            yield return null;
        }
    }
    
    
    /*==============================
        RestartCurrentScene
        Restarts the currently active scene
    ==============================*/
    
    public void RestartCurrentScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    
    
    /*==============================
        LoadScene
        Loads a scene with a given name
        @param The scene name to load
    ==============================*/
    
    public void LoadScene(string scenename)
    {
        StartCoroutine(LoadAsyncScene(scenename));
    }
    
    
    /*==============================
        StartNextScene
        Allows the scene switch to occur
        (If the loading is finished)
    ==============================*/
    
    public void StartNextScene()
    {
        this.m_GoToNextScene = true;
    }
}



    
