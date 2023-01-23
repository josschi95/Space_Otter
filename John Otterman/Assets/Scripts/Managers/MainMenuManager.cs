using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private Image pressAnyKeyImage;
    private Color imageColor;

    [SerializeField] private Image backGroundImage;
    [SerializeField] private Sprite[] introComic;
    private int comicIndex = 0;
    //private bool saveFileExists = false;
    //private bool saveLoaded = false;

    private void Start()
    {
        imageColor = pressAnyKeyImage.color;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        //saveFileExists = GameManager.instance.saveSystem.ExistingSavesExist();
    }

    private void Update()
    {
        if (Input.anyKeyDown) OnPlay();
        FlashImage();

        /*if ( saveFileExists && !saveLoaded)
        {
            GameManager.instance.saveSystem.LoadData();
            saveLoaded = true;
        }*/
    }

    private void FlashImage()
    {
        imageColor.a = Mathf.PingPong(Time.time, 1);
        pressAnyKeyImage.color = imageColor;
    }

    private void OnPlay()
    {       
        pressAnyKeyImage.gameObject.SetActive(false);

        //if (saveFileExists) SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 2);
        PlayComic();
    }

    private void PlayComic()
    {
        if (comicIndex >= introComic.Length - 1)
        {
            GameManager.LoadScene(GameManager.instance.GetSceneIndex() + 1);
        }
        backGroundImage.sprite = introComic[comicIndex];
        comicIndex++;
    }
}
