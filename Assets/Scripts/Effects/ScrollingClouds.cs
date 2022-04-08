/****************************************************************
                       ScrollingClouds.cs
    
This script handles the cloud scrolling in Level1_2.
****************************************************************/

using UnityEngine;
using UnityEngine.UI;

public class ScrollingClouds : MonoBehaviour
{
    public Vector2 m_ScrollSpeed;
    private Vector2 m_Position;
    private Canvas m_canvas;
    
    
    /*==============================
        Start
        Called when the canvas is initialized
    ==============================*/
    
    void Start()
    {
        this.m_canvas = this.transform.parent.GetComponent<Canvas>();
    }
    

    /*==============================
        Update
        Called every frame
    ==============================*/
    
    void Update()
    {
        this.GetComponent<RawImage>().uvRect = new Rect(Time.time/m_ScrollSpeed.x, Time.time/m_ScrollSpeed.y, 2, 2);
    }
}
