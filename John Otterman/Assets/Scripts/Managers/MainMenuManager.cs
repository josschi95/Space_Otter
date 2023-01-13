using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private Button playButton, settingsButton, quitButton;
    [SerializeField] private Image pressAnyKeyImage;
    private Color imageColor;

    [SerializeField] private Image backGroundImage;
    [SerializeField] private Sprite[] introComic;
    private int comicIndex = 0;
    private bool saveFileExists = false;
    private bool saveLoaded = false;

    private void Start()
    {
        imageColor = pressAnyKeyImage.color;

        playButton.onClick.AddListener(OnPlay);
        settingsButton.onClick.AddListener(OnSettings);
        quitButton.onClick.AddListener(OnQuit);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        saveFileExists = GameManager.instance.saveSystem.ExistingSavesExist();
    }

    private void Update()
    {
        if (Input.anyKeyDown) OnPlay();
        FlashImage();

        if ( saveFileExists && !saveLoaded)
        {
            GameManager.instance.saveSystem.LoadData();
            saveLoaded = true;
        }
    }

    private void FlashImage()
    {
        imageColor.a = Mathf.PingPong(Time.time, 1);
        pressAnyKeyImage.color = imageColor;
    }

    private void OnPlay()
    {       
        pressAnyKeyImage.gameObject.SetActive(false);

        if (saveFileExists) SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        else PlayComic();
    }

    private void PlayComic()
    {
        if (comicIndex >= introComic.Length - 1)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
        backGroundImage.sprite = introComic[comicIndex];
        comicIndex++;
    }

    private void OnSettings()
    {
        Debug.Log("Not yet implemented");
    }

    private void OnQuit()
    {
        Application.Quit();
    }

}
