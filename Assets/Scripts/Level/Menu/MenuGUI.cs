/****************************************************************
                           MenuGUI.cs
    
This script handles the main menu GUI.
As usual, I leave menus for last, meaning this code will likely
be a mess >:V
****************************************************************/

using UnityEngine;
using UnityEngine.UI;

public class MenuGUI : MonoBehaviour
{
    private const float TextScaleTime = 50.0f;
    private const float TextSlightScaleAmount = 0.00015f;
    private const float LogoSpeed = 12.5f;
    private const float LogoTopPos = 216.0f;
    private const float LogoBotPos = 244.0f;
    private const float LogoSeparation = 128.0f;
    private const float FadeoutTime = 3.0f;
    private const float MenuButtonInX = 512.0f;
    private const float MenuButtonOutX = 180.0f;
    private const float MenuButtonSpeed = 30.0f;
    
    // Menu button enums
    private enum MenuButton
    {
        Main,
        Difficulty,
        Game,
    }
    
    // Public values
    public Text m_TopText;
    public Text m_BottomText;
    public Image m_LogoTop;
    public Image m_LogoBot;
    public Image m_Fade;
    public GameObject m_Cursor;
    public GameObject m_MainButtons;
    public GameObject m_DifficultyButtons;
    public GameObject m_ShellCasual;
    public GameObject m_ShellSuited;
    public GameObject m_Skyscraper;
    
    public Texture m_TiredEyes;
    public Texture m_TiredMouth;
    public Texture m_GrinEyes;
    public Texture m_GrinMouth;
    public Texture m_ShockedEyes;
    public Texture m_ShockedMouth;
    
    // Private values
    private MenuButton m_CurrentMenu = MenuButton.Main;
    private bool m_DoAnimation = false;
    private float m_ScaleFactor = 0;
    private int m_CurrSequence = 0;
    private float m_TargetLogoTopX;
    private float m_TargetLogoBotX;
    private Vector3 m_TargetTextScale = Vector2.zero;
    private float m_NextSequenceTime = -1;
    private AudioManager m_audiomngr;
    private SkinnedMeshRenderer m_casualmesh;
    
    
    /*==============================
        Start
        Called when the GUI is initialized
    ==============================*/
    
    void Start()
    {
        this.m_Cursor.SetActive(false);
        this.m_ShellSuited.GetComponent<Animator>().speed = 0.0f;
        this.m_audiomngr = FindObjectOfType<AudioManager>();
        this.m_casualmesh = this.m_ShellCasual.transform.Find("Mesh").GetComponent<SkinnedMeshRenderer>();
        this.m_casualmesh.materials[1] = new Material(this.m_casualmesh.materials[1]);
        this.m_casualmesh.materials[2] = new Material(this.m_casualmesh.materials[2]);
        this.m_casualmesh.materials[1].SetTexture("_MainTex", this.m_TiredEyes);
        this.m_casualmesh.materials[2].SetTexture("_MainTex", this.m_TiredMouth);
        GameObject.Find("SceneController").GetComponent<SceneController>().LoadScene("Level1_1");
    }


    /*==============================
        Update
        Called every frame
    ==============================*/
    
    void Update()
    {
        if (!this.m_DoAnimation)
            return;
        
        // Skip the intro on mouse click
        if (Input.GetButton("Fire") && this.m_CurrSequence < 8)
            SkipCreditsAnimation();
        
        // Handle the title screen animation
        if (this.m_CurrSequence < 8)
            HandleCreditsText();
        else
            HandleLogo();
        
        // Scale the text to the target size
        this.m_TopText.rectTransform.localScale = Vector3.Lerp(this.m_TopText.rectTransform.localScale, this.m_TargetTextScale, MenuGUI.TextScaleTime*Time.deltaTime);
        if (this.m_TargetTextScale != Vector3.zero)
            this.m_TargetTextScale = new Vector3(this.m_TargetTextScale.x+MenuGUI.TextSlightScaleAmount*this.m_ScaleFactor, this.m_TargetTextScale.y+MenuGUI.TextSlightScaleAmount*this.m_ScaleFactor, this.m_TargetTextScale.z);
        
        // Move the game logo
        if (this.m_CurrSequence >= 8)
        {
            this.m_LogoTop.rectTransform.position = Vector2.Lerp(this.m_LogoTop.rectTransform.position, new Vector2(this.m_TargetLogoTopX, this.m_LogoTop.rectTransform.position.y), MenuGUI.LogoSpeed*Time.deltaTime);
            this.m_LogoBot.rectTransform.position = Vector2.Lerp(this.m_LogoBot.rectTransform.position, new Vector2(this.m_TargetLogoBotX, this.m_LogoBot.rectTransform.position.y), MenuGUI.LogoSpeed*Time.deltaTime);
        }
        
        // Menu button positions
        Vector2 mainpos = this.m_MainButtons.GetComponent<RectTransform>().anchoredPosition;
        Vector2 diffpos = this.m_DifficultyButtons.GetComponent<RectTransform>().anchoredPosition;
        switch (this.m_CurrentMenu)
        {
            case MenuButton.Main:
                if (diffpos.x < (MenuGUI.MenuButtonOutX+2.0f))
                    this.m_MainButtons.GetComponent<RectTransform>().anchoredPosition = new Vector2(Mathf.Lerp(mainpos.x, MenuGUI.MenuButtonInX, MenuButtonSpeed*Time.deltaTime), mainpos.y);
                this.m_DifficultyButtons.GetComponent<RectTransform>().anchoredPosition = new Vector2(Mathf.Lerp(diffpos.x, MenuGUI.MenuButtonOutX, MenuButtonSpeed*Time.deltaTime), diffpos.y);
                break;
            case MenuButton.Difficulty:
                if (mainpos.x < (MenuGUI.MenuButtonOutX+2.0f)   )
                    this.m_DifficultyButtons.GetComponent<RectTransform>().anchoredPosition = new Vector2(Mathf.Lerp(diffpos.x, MenuGUI.MenuButtonInX, MenuButtonSpeed*Time.deltaTime), diffpos.y);
                this.m_MainButtons.GetComponent<RectTransform>().anchoredPosition = new Vector2(Mathf.Lerp(mainpos.x, MenuGUI.MenuButtonOutX, MenuButtonSpeed*Time.deltaTime), mainpos.y);
                break;
            case MenuButton.Game:
                this.m_DifficultyButtons.GetComponent<RectTransform>().anchoredPosition = new Vector2(Mathf.Lerp(diffpos.x, MenuGUI.MenuButtonOutX, MenuButtonSpeed*Time.deltaTime), diffpos.y);
                break;
        }
        
        // Fade out the screen
        if (this.m_CurrSequence == 10)
            this.m_Fade.color = Color.Lerp(this.m_Fade.color, Color.clear, MenuGUI.FadeoutTime*Time.deltaTime);
        
        // Intro scene
        if (this.m_CurrentMenu == MenuButton.Game)
            HandleIntroScene();
    }


    /*==============================
        HandleCreditsText
        Handles the credits text
    ==============================*/
    
    private void HandleCreditsText()
    {
        if (this.m_NextSequenceTime < Time.unscaledTime)
        {
            switch (this.m_CurrSequence)
            {
                case 0:
                    this.m_TargetTextScale = Vector3.one;
                    this.m_TopText.text = "A GAME BY";
                    this.m_BottomText.text = "Lourenço Soares";
                    this.m_NextSequenceTime = Time.unscaledTime + 2.479f;
                    break;
                case 1:
                    this.m_TargetTextScale = Vector3.zero;
                    this.m_NextSequenceTime = Time.unscaledTime + 0.163f;
                    break;
                case 2:
                    this.m_TargetTextScale = Vector3.one;
                    this.m_TopText.text = "ASSETS BY";
                    this.m_BottomText.text = "Lourenço Soares";
                    this.m_NextSequenceTime = Time.unscaledTime + 2.579f;
                    break;
                case 3:
                    this.m_TargetTextScale = Vector3.zero;
                    this.m_NextSequenceTime = Time.unscaledTime + 0.163f;
                    break;
                case 4:
                    this.m_TargetTextScale = Vector3.one;
                    this.m_TopText.text = "FEATURING THE VOICE OF";
                    this.m_BottomText.text = "Maria Brigida";
                    this.m_NextSequenceTime = Time.unscaledTime + 2.513f;
                    break;
                case 5:
                    this.m_TargetTextScale = Vector3.zero;
                    this.m_NextSequenceTime = Time.unscaledTime + 0.189f;
                    break;
                case 6:
                    this.m_TargetTextScale = Vector3.one;
                    this.m_TopText.text = "MUSIC BY";
                    this.m_BottomText.text = "Hidenori Shoji";
                    this.m_NextSequenceTime = Time.unscaledTime + 2.136f;
                    break;
                case 7:
                    this.m_TargetTextScale = Vector3.zero;
                    this.m_NextSequenceTime = Time.unscaledTime + 0.089f;
                    this.m_TargetLogoTopX = MenuGUI.LogoTopPos*this.m_ScaleFactor + MenuGUI.LogoSeparation*this.m_ScaleFactor;
                    this.m_TargetLogoBotX = MenuGUI.LogoBotPos*this.m_ScaleFactor - MenuGUI.LogoSeparation*this.m_ScaleFactor;
                    break;
                    
            }
            this.m_CurrSequence++;
        }
    }


    /*==============================
        HandleLogo
        Handles the logo positioning
    ==============================*/
    
    private void HandleLogo()
    {
        if (this.m_NextSequenceTime < Time.unscaledTime && this.m_CurrSequence == 8)
        {
            if (this.m_LogoTop.rectTransform.position.x > MenuGUI.LogoTopPos*this.m_ScaleFactor + (MenuGUI.LogoSeparation-2.0f)*this.m_ScaleFactor)
            {
                this.m_TargetLogoTopX = MenuGUI.LogoTopPos*this.m_ScaleFactor;
                this.m_TargetLogoBotX = MenuGUI.LogoBotPos*this.m_ScaleFactor;
                this.m_CurrSequence++;
            }
        }
        else if (this.m_CurrSequence == 9)
        {
            if (this.m_LogoTop.rectTransform.position.x < (MenuGUI.LogoTopPos+2.0f)*this.m_ScaleFactor)
            {
                this.m_Fade.color = Color.white;
                this.m_CurrSequence++;
                this.m_Cursor.SetActive(true);
            }
        }
    }


    /*==============================
        StartCreditsAnimation
        Allows the credits sequence to play
    ==============================*/
    
    public void StartCreditsAnimation()
    {
        this.m_DoAnimation = true;
        this.m_ScaleFactor = this.GetComponent<Canvas>().scaleFactor;
        this.m_NextSequenceTime = Time.unscaledTime + 1.33f + 0.5f;
    }


    /*==============================
        SkipCreditsAnimation
        Skips the credits sequence of the intro
    ==============================*/
    
    public void SkipCreditsAnimation()
    {
        this.m_CurrSequence = 8;
        this.m_TargetTextScale = Vector3.zero;
        this.m_NextSequenceTime = Time.unscaledTime + 0.089f;
        this.m_TargetLogoTopX = MenuGUI.LogoTopPos*this.m_ScaleFactor + MenuGUI.LogoSeparation*this.m_ScaleFactor;
        this.m_TargetLogoBotX = MenuGUI.LogoBotPos*this.m_ScaleFactor - MenuGUI.LogoSeparation*this.m_ScaleFactor;
        this.m_TopText.rectTransform.localScale = Vector3.zero;
        FindObjectOfType<MusicManager>().StopMusic();
        FindObjectOfType<MusicManager>().PlaySong("Music/Menu", true, false);
    }


    /*==============================
        PressPlay
        Shows the difficulty buttons
    ==============================*/
    
    public void PressPlay()
    {
        if (this.m_CurrentMenu != MenuButton.Main)
            return;
        this.m_CurrentMenu = MenuButton.Difficulty;
    }


    /*==============================
        PressQuit
        Closes the game when quit is pressed
    ==============================*/
    
    public void PressQuit()
    {
        if (this.m_CurrentMenu != MenuButton.Main)
            return;
        Application.Quit();
    }


    /*==============================
        PressEasy
        Sets the game difficulty to easy
    ==============================*/
    
    public void PressEasy()
    {
        if (this.m_CurrentMenu != MenuButton.Difficulty)
            return;
        GameObject.Find("SceneController").GetComponent<SceneController>().SetDifficulty(SceneController.Difficulty.Easy);
        this.m_CurrentMenu = MenuButton.Game;
        StartIntroScene();
    }


    /*==============================
        PressHard
        Sets the game difficulty to hard
    ==============================*/
    
    public void PressHard()
    {
        if (this.m_CurrentMenu != MenuButton.Difficulty)
            return;
        GameObject.Find("SceneController").GetComponent<SceneController>().SetDifficulty(SceneController.Difficulty.Hard);
        this.m_CurrentMenu = MenuButton.Game;
        StartIntroScene();
    }


    /*==============================
        PressBack
        Shows the main buttons again
    ==============================*/
    
    public void PressBack()
    {
        if (this.m_CurrentMenu != MenuButton.Difficulty)
            return;
        this.m_CurrentMenu = MenuButton.Main;
    }


    /*==============================
        StartIntroScene
        Starts the introductory scene
    ==============================*/
    
    private void StartIntroScene()
    {
        FindObjectOfType<MusicManager>().StopMusic();
        Animator anim1 = this.m_ShellCasual.GetComponent<Animator>();
        Animator anim2 = this.m_ShellSuited.GetComponent<Animator>();
        anim1.SetBool("StartScene", true);
        this.m_TargetLogoTopX = -187*this.m_ScaleFactor;
        this.m_TargetLogoBotX = -187*this.m_ScaleFactor;
        GameObject.Find("SceneController").GetComponent<SceneController>().StartingNewLevel();
        Camera.main.GetComponent<CameraLogic>().AddTrauma(0.5f);
        this.m_NextSequenceTime = Time.unscaledTime + 1.0f;
        FindObjectOfType<MusicManager>().PlaySong("Music/Level1", true, true);
        this.m_casualmesh.materials[1].SetTexture("_MainTex", this.m_ShockedEyes);
        this.m_casualmesh.materials[2].SetTexture("_MainTex", this.m_ShockedMouth);
    }
    

    /*==============================
        HandleIntroScene
        Handles the introductory scene
    ==============================*/
    
    private void HandleIntroScene()
    {
        // Make the skyscraper collapse
        this.m_Skyscraper.transform.position += new Vector3(0.0f, -1.0f*Time.deltaTime, 0.0f);
        this.m_Skyscraper.transform.rotation *= Quaternion.Euler(0.0f, 0.0f, -3.0f*Time.deltaTime);
        this.m_Skyscraper.transform.Find("Mesh").localPosition = new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f), 0.0f);
        
        // Do the other animations
        if (this.m_NextSequenceTime < Time.unscaledTime)
        {
            int currseq = this.m_CurrSequence-10;
            switch(currseq)
            {
                case 0:
                    this.m_NextSequenceTime = Time.unscaledTime + 1.0f;
                    this.m_casualmesh.materials[1].SetTexture("_MainTex", this.m_TiredEyes);
                    this.m_casualmesh.materials[2].SetTexture("_MainTex", this.m_TiredMouth);
                    break;
                case 1:
                    this.m_audiomngr.Play("Voice/Shell/Intro1");
                    this.m_casualmesh.materials[1].SetTexture("_MainTex", this.m_GrinEyes);
                    this.m_casualmesh.materials[2].SetTexture("_MainTex", this.m_GrinMouth);
                    this.m_NextSequenceTime = Time.unscaledTime + 4.5f;
                    break;
                case 2:
                    this.m_ShellSuited.GetComponent<Animator>().speed = 1.0f;
                    this.m_NextSequenceTime = Time.unscaledTime + 2.87f;
                    break;
                case 3:
                    this.m_ShellSuited.transform.Find("Sword").GetComponent<SkinnedMeshRenderer>().enabled = false;
                    this.m_NextSequenceTime = Time.unscaledTime + 0.43f;
                    break;
                case 4:
                    this.m_audiomngr.Play("Voice/Shell/Intro2");
                    this.m_ShellSuited.transform.Find("Gun").GetComponent<SkinnedMeshRenderer>().enabled = true;
                    this.m_NextSequenceTime = Time.unscaledTime + 2.441f;
                    break;
                case 6:
                    GameObject.Find("SceneController").GetComponent<SceneController>().StartNextScene();
                    break;
            }
            this.m_CurrSequence++;
        }
    }
}