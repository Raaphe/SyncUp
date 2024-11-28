using UnityEngine.UI;
using UnityEngine;
using System.Threading.Tasks;
using TMPro;
using System;

public class SpotifyInterfaceBehavior : MonoBehaviour
{

    private Button nextButton;
    private Button previousButton;
    private Button fastForwardButton;
    private Button rewindButton;
    private Button playPauseButton;
    private Button activationButton;
    private TextMeshProUGUI artistTitleText; 
    private SpotifyController spotifyController;
    private int rewindDurationSeconds = 10;
    public bool ControllerEnabled = false;

    private Canvas activationCanvas;
    private Canvas controllerCanvas;

    public Sprite PauseImage;
    public Sprite PlayImage;

    public bool UseKeys = false;
    public KeyCode NextKey;
    public KeyCode PreviousKey;
    public KeyCode TogglePauseKey;

    private void Start()
    {
        spotifyController = SpotifyController.Instance;
        activationCanvas = GameObject.Find("ActivationInterface").GetComponent<Canvas>();
        controllerCanvas = GameObject.Find("SpotifyInterface").GetComponent<Canvas>();
        SetButtons();
        activationButton.onClick.AddListener(async () => await ToggleInterface());
        activationCanvas.gameObject.SetActive(true);
        controllerCanvas.gameObject.SetActive(false);
    }

    private async Task ToggleInterface()
    {
        ControllerEnabled = !ControllerEnabled;
        await HandleControllerInterface();
    }

    private async Task HandleControllerInterface()
    {
        if (ControllerEnabled) 
        {
            controllerCanvas.gameObject.SetActive(true);
            activationCanvas.gameObject.SetActive(false);
            await Init();
        }
        else
        {
            activationCanvas.gameObject.SetActive(true);
            controllerCanvas.gameObject.SetActive(false);
        }
    }

    private async Task Init()
    {
        await spotifyController.Init();
        artistTitleText = GameObject.Find("ArtistTitle").GetComponent<TextMeshProUGUI>();
        spotifyController.OnNext += async (object sender, EventArgs e) =>
        {
            await UpdateArtistTitleText();
        };
        spotifyController.OnPrevious += async (object sender, EventArgs e) =>
        {
            await UpdateArtistTitleText();
        };
        SetButtonsListenner();
        await ChangePauseButtonState();
        await UpdateArtistTitleText();
    }

    private void SetButtons()
    {
        nextButton = GameObject.Find("NextButton").GetComponent<Button>();
        previousButton = GameObject.Find("PreviousButton").GetComponent<Button>();
        fastForwardButton = GameObject.Find("FastForwardButton").GetComponent<Button>();
        rewindButton = GameObject.Find("RewindButton").GetComponent<Button>();
        playPauseButton = GameObject.Find("PlayPauseButton").GetComponent <Button>();
        activationButton = GameObject.Find("ActivationButton").GetComponent<Button>();

    }

    private void SetButtonsListenner()
    {
        nextButton.onClick.AddListener(async () => await spotifyController.Next());
        previousButton.onClick.AddListener(async () => await spotifyController.Previous());
        fastForwardButton.onClick.AddListener(async () => await spotifyController.FastForward(rewindDurationSeconds));
        rewindButton.onClick.AddListener(async () => await spotifyController.Rewind(rewindDurationSeconds));
        playPauseButton.onClick.AddListener(async () => await HandlePlayPause());
    }

    private async Task HandlePlayPause() 
    {
        await spotifyController.TogglePlayPause();
        await ChangePauseButtonState();
    }

    private async Task ChangePauseButtonState()
    {
        if (await spotifyController.GetPlayPauseState()) playPauseButton.image.sprite = PauseImage;
        else playPauseButton.image.sprite = PlayImage;
    }

    private async Task UpdateArtistTitleText()
    {
        var song = await spotifyController.GetCurrentlyPlayingSong();
        if (song != null) artistTitleText.text = song.ArtistName + "\n" + song.Title;
        else artistTitleText.text = "No song currently playing...";
    }

    private async void Update()
    {
        if (UseKeys) 
        {
            if (Input.GetKeyDown(NextKey))
            {
                await spotifyController.Next();
            }
            else if (Input.GetKeyUp(PreviousKey))
            {
                await spotifyController.Previous();
            }
            else if (Input.GetKeyDown(TogglePauseKey))
            {
                await HandlePlayPause();
            }
        }
    }
}
