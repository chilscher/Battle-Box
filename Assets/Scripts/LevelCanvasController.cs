﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelCanvasController : MonoBehaviour{

    [Header("Heart Icon")]
    public GameObject heartPrefab;
    public Vector2 heartPos;
    public float gapBetweenHearts = 60f;

    [Header("Canvases")]
    public GameObject ingameCanvas;
    public GameObject pausedCanvas;
    public GameObject winCanvas;
    public GameObject loseCanvas;

    
    private UnityEngine.UI.Text ingameLevelNumberTextBox;
    private UnityEngine.UI.Text pausedeLevelNumberTextBox;
    private UnityEngine.UI.Text winLevelNumberTextBox;
    private UnityEngine.UI.Text loseLevelNumberTextBox;
    

    [Header("Pause/Resume Key")]
    public KeyCode pauseKey = KeyCode.Escape;

    [Header("Tutorials and Menus")]
    public bool showHealth = true;

    private Player player;
    private List<GameObject> hearts;
    private string nextLevelName;
    private bool isPaused = false;
    private bool isPlaying = true;
    private bool doesNextLevelExist;
    private bool onWinScreenYet = false;
    private bool onLoseScreenYet = false;
    private AudioManager audioManager;


    private GameObject nextLevelBtn;

    void Start() {
        player = GameObject.FindWithTag("Player").GetComponent<Player>();
        nextLevelBtn = winCanvas.transform.Find("Next Level Button").gameObject;
        audioManager = FindObjectOfType<AudioManager>();

        setAllLevelNumbers();

        ingameCanvas.SetActive(true);
        pausedCanvas.SetActive(false);
        winCanvas.SetActive(false);
        loseCanvas.SetActive(false);

        if (showHealth) { drawAllHearts(); }
        

        getNextLevelName();
        doesNextLevelExist = Application.CanStreamedLevelBeLoaded(nextLevelName);
        hideNextLevelButton();
        
        if (!audioManager.isPlaying("Level Theme")) {
            audioManager.fadeIn("Level Theme");
        }
        

    }

    
    void Update(){
        if (showHealth && hearts.Count != player.getHP()) { redrawHearts(); }

        hitPauseKey();
        win();
        lose();
    }

    private void drawHearts(GameObject canvas) {
        int heartAmount = player.getHP();
        hearts = new List<GameObject>();
        float firstX = heartPos.x;
        float firstY = heartPos.y;
        float firstZ = 0f;

        for (int i = 0; i<heartAmount; i++) {
            float x = firstX + (gapBetweenHearts * i);
            float y = firstY;
            float z = firstZ;

            Vector3 pos = new Vector3(x,y,z);
            GameObject h = Instantiate(heartPrefab);
            h.transform.SetParent(canvas.transform);
            RectTransform rt = h.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;
            hearts.Add(h);
        }
    }

    private void redrawHearts() {
        foreach (GameObject heart in hearts) { Destroy(heart); }
        hearts = new List<GameObject>();
        drawAllHearts();
    }

    private void drawAllHearts() {
        drawHearts(ingameCanvas);
        //drawHearts(pausedCanvas);
    }
    


    private void pause() {
        isPaused = true;
        isPlaying = false;
        Time.timeScale = 0f;
        pausedCanvas.SetActive(true);
        ingameCanvas.SetActive(false);
        winCanvas.SetActive(false);
        loseCanvas.SetActive(false);
        audioManager.pause();
    }

    private void hitPauseKey() {
        if (Input.GetKeyDown(pauseKey)) {
            if (isPlaying) { pause(); }
            else if (isPaused) { resume(); }
        }
    }

    private void win() {
        if (player.hasWonYet() && !onWinScreenYet) {
            onWinScreenYet = true;
            isPaused = false;
            isPlaying = false;
            winCanvas.SetActive(true);
            ingameCanvas.SetActive(false);
            pausedCanvas.SetActive(false);
            loseCanvas.SetActive(false);
            audioManager.stopAll();
            audioManager.play("Victory Jingle");
        }
    }

    private void lose() {
        if (player.hasLostYet() && !onLoseScreenYet) {
            onLoseScreenYet = true;
            isPaused = false;
            isPlaying = false;
            loseCanvas.SetActive(true);
            ingameCanvas.SetActive(false);
            pausedCanvas.SetActive(false);
            winCanvas.SetActive(false);
            audioManager.fadeIn("Defeat Jingle");
        }
    }

    private void resume() {
        isPaused = false;
        isPlaying = true;
        Time.timeScale = 1f;
        ingameCanvas.SetActive(true);
        pausedCanvas.SetActive(false);
        winCanvas.SetActive(false);
        loseCanvas.SetActive(false);
        audioManager.resume();
    }

    private void quit() {
        if (Time.timeScale == 0) { audioManager.resumeWithMusicFadeout(); } //if the game was paused, then fade out the music from its already quieter level
        else { audioManager.fadeOutAll(); } //if the game was not paused, fade out the music like normal
        Time.timeScale = 1f;
        SceneManager.LoadScene("Main Menu");
    }

    private void restart() {
        audioManager.stopPausableSounds();
        Time.timeScale = 1f;
        audioManager.resume();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void nextLevel() {
        if (doesNextLevelExist) {
            audioManager.fadeOutAll();
            audioManager.resume();
            SceneManager.LoadScene(nextLevelName);
        }
    }

    private void getNextLevelName() {
        string name = SceneManager.GetActiveScene().name;
        string[] words = name.Split(' ');
        string num = words[words.Length - 1];
        int n = int.Parse(num) + 1;
        words[words.Length - 1] = n.ToString();
        string newName = string.Join(" ", words);
        nextLevelName = newName;
    }

    private void hideNextLevelButton() {
        if (!doesNextLevelExist) { nextLevelBtn.SetActive(false); }
    }

    private void setAllLevelNumbers() {
        setLevelNumber(ingameCanvas);
        setLevelNumber(pausedCanvas);
        setLevelNumber(winCanvas);
        setLevelNumber(loseCanvas);
    }

    private void setLevelNumber(GameObject canvas) {
        UnityEngine.UI.Text tb = canvas.transform.Find("Level number").gameObject.GetComponent<UnityEngine.UI.Text>();
        string name = SceneManager.GetActiveScene().name;
        tb.text = name.ToUpper();
    }

    public void _btnPause() { pause(); }
    public void _btnResume() { resume(); }
    public void _btnQuit() { quit(); }
    public void _btnRestart() { restart(); }
    public void _btnNextLevel() { nextLevel(); }
    
    

}