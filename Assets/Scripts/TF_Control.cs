using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Globalization;
using TMPro;

public class TF_Control : MonoBehaviour
{

    /* General Stuff to make shit work */
    [SerializeField] Camera cam;
    [SerializeField] GameObject player;

    Vector3 cameraRot;
    Vector3 playerRot;
    RaycastHit hit;



    /* Audio Stuff */
    public AudioSource src;
    public AudioClip fire;
    public AudioClip targetShot;



    /* Everything Crosshair-related because I almost killed myself trying to find it all*/ 
    public RawImage crosshair;

    public Button CHBlackButton;
    public Button CHBlueButton;
    public Button CHYellowButton;
    public Button CHGreenButton;
    public Button CHRedButton;
    public Button CHCyanButton;

    public Button CHCrossButton;
    public Button CHDotButton;
    public Button CHEmptyCrossButton;

    public Texture2D Cross;
    public Texture2D Dot;
    public Texture2D EmptyCross;

    


    /* General Menu Stuff */
    public GameObject menuPanel;
    private bool isMenuOpen = false;
    public Button leaveGame;
    public Button loadTileFrenzyMini;



    /* Sensitivity, FOV and Volume related shit */
    [SerializeField] float sensitivity = 2f;
    [SerializeField] float FOV = 90f;
    [SerializeField] float volume = 0.33f;

    public Slider sensitivitySlider;
    public Slider FOVSlider;
    public Slider volumeSlider;

    public TMP_InputField sensitivityInputField;
    public TMP_InputField FOVInputField;
    public TMP_InputField volumeInputField;

    

    /* General Accuracy Calculation */
    public TMP_Text allShotsCounterText;
    public TMP_Text goodShotsCounterText;
    public TMP_Text accuracyText;
    private float allShotCounter;
    private float goodShotCounter;
    private float accuracy;



    /* Challenge Related */
    private bool isChallengeActive = false;
    private float challengeTimeRemaining = 30f;
    public Button challengeButton;
    public TMP_Text challengeTimerText;
    private Color challengeButtonGreen = new Color(0f / 255f, 255f / 255f, 93f / 255f, 180f / 255f); // RGBA(0, 255, 93, 180)
    private Color challengeButtonRed = new Color(255f / 255f, 0f / 255f, 93f / 255f, 180f / 255f);   // RGBA(255, 0, 93, 180)


    /* Scenario Specific/Targets */
    float lowerBound_x = -15f;
    float upperBound_x = 15f;
    float lowerBound_y = -12.5f;
    float upperBound_y = 17.5f;
    float set_z = 17f;

    public GameObject target1;
    public GameObject target2;
    public GameObject target3;

    

    void Start()
    {
        /* Resetting the accuracy stuff */
        accuracy = 0f;
        allShotCounter = 0f;
        goodShotCounter = 0f;



        /* Setting the targets in place */
        SetPos(target1);
        SetPos(target2);
        SetPos(target3);



        /* Disappearing the cursor */
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;



        /* No funny shit in this bitch, we stand on businness */
        if (menuPanel != null)
        {
            menuPanel.SetActive(false);
        }

        if (cam != null)
        {
            cam.fieldOfView = FOV;
        }



        /* Setting the sliders and input fields up, I know it's fucking ugly I ain't changing that */
        if (volumeSlider != null)
        {
            volumeSlider.value = volume;
            src.volume = volume;
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        }
        if (FOVSlider != null)
        {
            cam.fieldOfView = FOV;
            FOVSlider.onValueChanged.AddListener(OnFOVChanged);
        }
        if (sensitivitySlider != null)
        {
            sensitivitySlider.value = sensitivity;
            sensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);
        }

        if (sensitivityInputField != null)
        {
            sensitivityInputField.text = sensitivity.ToString(CultureInfo.InvariantCulture);
            sensitivityInputField.onEndEdit.AddListener(OnSensitivityInputChanged);
        }
        if (FOVInputField != null)
        {
            FOVInputField.text = FOV.ToString(CultureInfo.InvariantCulture);
            FOVInputField.onEndEdit.AddListener(OnFOVInputChanged);
        }
        if (volumeInputField != null)
        {
            volumeInputField.text = volume.ToString(CultureInfo.InvariantCulture);
            volumeInputField.onEndEdit.AddListener(OnVolumeInputChanged);
        }



        /* Setting up the Challenge Button */
        challengeButton.onClick.AddListener(() =>
        {
            if (isChallengeActive)
            {
                StopChallenge();
            }
            else
            {
                StartChallenge();
            }
        });



        /* Setting up the Leave button and the Crosshair buttons, as I said, I ain't changing this shit */
        leaveGame.onClick.AddListener(Exit);
        loadTileFrenzyMini.onClick.AddListener(LoadTileFrenzyMini);

        CHBlackButton.onClick.AddListener(CHCHangeColorBlack);
        CHBlueButton.onClick.AddListener(CHCHangeColorBlue);
        CHYellowButton.onClick.AddListener(CHCHangeColorYellow);
        CHGreenButton.onClick.AddListener(CHCHangeColorGreen);
        CHRedButton.onClick.AddListener(CHCHangeColorRed);
        CHCyanButton.onClick.AddListener(CHCHangeColorCyan);

        CHCrossButton.onClick.AddListener(CHChangeTextureCross);
        CHDotButton.onClick.AddListener(CHChangeTextureDot);
        CHEmptyCrossButton.onClick.AddListener(CHChangeTextureEmptyCross);
    }

    
    void Update()
    {
        /* Literally the backbone of it all */
        LookAround();
        cam.fieldOfView = FOV;



        /* Challenge Handler */
        if (isChallengeActive)
        {
            challengeTimeRemaining -= Time.deltaTime;
            challengeTimerText.text = $"Time: {challengeTimeRemaining:F2}s";

            if (challengeTimeRemaining <= 0)
            {
                StopChallenge();
            }
        }



        /* Shooting Mechanic and Shot Counter */
        if (Input.GetKeyDown(KeyCode.Mouse0) && !isMenuOpen)
        {
            allShotCounter += 1;

            Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, 300f);
            src.clip = fire;
            src.Play();
            if (hit.collider != null && hit.collider.CompareTag("Target1"))
            {
                goodShotCounter += 1;
                src.clip = targetShot;
                src.Play();
                SetPos(target1);
            }
            if (hit.collider != null && hit.collider.CompareTag("Target2"))
            {
                goodShotCounter += 1;
                src.clip = targetShot;
                src.Play();
                SetPos(target2);
            }
            if (hit.collider != null && hit.collider.CompareTag("Target3"))
            {
                goodShotCounter += 1;
                src.clip = targetShot;
                src.Play();
                SetPos(target3);
            }

        }



        /* Accuracy Calculator */
        if(allShotCounter != 0)
        {
            accuracy =  Mathf.Floor(goodShotCounter / allShotCounter * 100f);
        } 
        else
        {
            accuracy = 0f;
        }

        accuracyText.text = "Accuracy: " + accuracy.ToString() + "%";
        goodShotsCounterText.text = "Shots landed: " + goodShotCounter.ToString();
        allShotsCounterText.text = "Shots fired: " + allShotCounter.ToString();



        /* Menu Handler */
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            isMenuOpen = !isMenuOpen;
            if(isMenuOpen)
            {
                crosshair.rectTransform.anchoredPosition = new Vector2(800f, -200f);
            } else
            {
                crosshair.rectTransform.anchoredPosition = new Vector2(0f, 0f);
            }
            
            menuPanel.SetActive(isMenuOpen);
            challengeTimerText.gameObject.SetActive(!isMenuOpen);
            Cursor.visible = isMenuOpen;
            Cursor.lockState = isMenuOpen ? CursorLockMode.None : CursorLockMode.Locked;
        }

    }



    /* If it stops working, I'm killing myself */
    void LookAround()
    {
        cameraRot = cam.transform.rotation.eulerAngles;
        cameraRot.x += -Input.GetAxis("Mouse Y") * sensitivity;
        cameraRot.x = Mathf.Clamp((cameraRot.x <= 180) ? cameraRot.x : -(360 - cameraRot.x), -80f, 80f);
        cam.transform.rotation = Quaternion.Euler(cameraRot);
        playerRot.y = Input.GetAxis("Mouse X") * sensitivity;
        player.transform.Rotate(playerRot);
    }



    /* Sets the random position of a target */
    void SetPos(GameObject target)
    {
        float target_x = UnityEngine.Random.Range(lowerBound_x, upperBound_x);
        float target_y = UnityEngine.Random.Range(lowerBound_y, upperBound_y);
        target.transform.position = new Vector3(target_x, target_y, set_z);
    }



    /* Handles Sliders changing and makes them adjust to the Input Fields */
    public void OnSensitivityChanged(float newSensitivity)
    {
        sensitivity = newSensitivity;
        if (sensitivityInputField != null)
        {
            sensitivityInputField.text = sensitivity.ToString(CultureInfo.InvariantCulture);
        }
    }
    public void OnFOVChanged(float newFOV)
    {
        FOV = newFOV;
        if (FOVInputField != null)
        {
            FOVInputField.text = FOV.ToString(CultureInfo.InvariantCulture); 
        }
    }
    public void OnVolumeChanged(float newVolume)
    {
        volume = newVolume;
        src.volume = volume;
        if (volumeInputField != null)
        {
            volumeInputField.text = volume.ToString(CultureInfo.InvariantCulture);
        }
    }



    /* Handles Input Fields changing and makes them adjust to the Sliders */
    private void OnSensitivityInputChanged(string value)
    {
        if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float result))
        {
            sensitivity = result;
            if (sensitivitySlider != null)
                sensitivitySlider.value = result;
        }
        else
        {
            Debug.LogWarning("Invalid input for sensitivity.");
        }
    }
    private void OnFOVInputChanged(string value)
    {
        if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float result))
        {
            FOV = result;
            if (FOVSlider != null)
                FOVSlider.value = result;
        }
        else
        {
            Debug.LogWarning("Invalid input for FOV.");
        }
    }
    private void OnVolumeInputChanged(string value)
    {
        if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float result))
        {
            volume = result;
            if (volumeSlider != null)
                volumeSlider.value = result;
        }
        else
        {
            Debug.LogWarning("Invalid input for volume.");
        }
    }



    /* Crosshair Customization Handlers aka my 13th reason */
    void CHCHangeColorBlack()
    {
        crosshair.color = Color.black;
    }
    void CHCHangeColorBlue()
    {
        crosshair.color = Color.blue;
    }
    void CHCHangeColorYellow()
    {
        crosshair.color = Color.yellow;
    }
    void CHCHangeColorGreen()
    {
        crosshair.color = Color.green;
    }
    void CHCHangeColorRed()
    {
        crosshair.color = Color.red;
    }
    void CHCHangeColorCyan()
    {
        crosshair.color = Color.cyan;
    }
    void CHChangeTextureCross()
    {
        crosshair.texture = Cross;
    }
    void CHChangeTextureDot()
    {
        crosshair.texture = Dot;
    }
    void CHChangeTextureEmptyCross()
    {
        crosshair.texture = EmptyCross;
    }



    /* The Mighty Exiter */
    void Exit()
    {
        Application.Quit();
    }
    void LoadTileFrenzyMini()
    {
        SceneManager.LoadScene("TF_Mini");
    }



    /* Challenge Handlers */
    void StartChallenge()
    {
        allShotCounter = 0;
        goodShotCounter = 0;
        accuracy = 0;

        isChallengeActive = true;
        challengeTimeRemaining = 30f;

        challengeButton.GetComponent<Image>().color = challengeButtonRed;
        challengeButton.GetComponentInChildren<TMP_Text>().text = "Stop Challenge";

        isMenuOpen = false;
        menuPanel.SetActive(isMenuOpen);
        challengeTimerText.gameObject.SetActive(!isMenuOpen);
        crosshair.rectTransform.anchoredPosition = new Vector2(0f, 0f);
        Cursor.visible = isMenuOpen;
        Cursor.lockState = CursorLockMode.Locked;
    }
    void StopChallenge()
    {
        isChallengeActive = false;

        StartCoroutine(DisplayFinalScore());

        challengeButton.GetComponent<Image>().color = challengeButtonGreen;
        challengeButton.GetComponentInChildren<TMP_Text>().text = "Start Challenge";
    }



    IEnumerator DisplayFinalScore()
    {
        challengeTimerText.text = $"Final Accuracy: {accuracy}%";
        yield return new WaitForSeconds(5f);
        challengeTimerText.text = "";
    }

}
