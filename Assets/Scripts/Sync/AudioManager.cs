using UnityEngine;
using CSCore;
using CSCore.SoundIn;
using CSCore.CoreAudioAPI;
using System;
using System.Linq;
using System.Collections.Generic;

/// <summary>
/// loopbackCapture: Wrapper for the Windows audio session API (CSCore)
/// devices: array of avaible audio devices
/// AudioEndpoint: the index in the list of currently active audio devices (should be 1)
/// DataFlow
/// deviceState
/// loopbackSamples: the array containing the data fetched by CSCore (implementation of Windows audio session API)
/// LastLoudestSamplesLength: the higher this value(array), the more precised will the environment be(works better with songs that change beat frequently)
/// lastLoudestSamples: last n [CurrentMaxFrequency] => An array containing the last samples in order to get an average/Max
/// LastLoudestSamplesAverage: Average of n [LastSamplesBuffer] of samples
/// LastLoudestSamplesMax: Most hearable sample in [LastSamplesBuffer]
/// LastLoudestSamplesFrameTempo: The "Tempo" of [lastSamples] -> Indicates how many times in average the frequencies are equivalent (more exactly in the same range)
/// LastLoudestSamplesTempoDifferential: What is considered "Equivalent" (ex: 0.33 -> 0.34 isn't a noticable change), the n percent of the initial value (40 - 10 and 40 + 10 = (30 - 50))
/// CurrentLoudestSample: the current loudest sample
/// SessionLoudestSample: loudest sample since the session (game) started
/// CurrentLoudestSampleMultiplier: To Compensate for low volume?
/// NormalizedCurrentLoudestSample_LastLoudestSamplesMax: normalized value [(CurrentMaxFrequency*100)/LastSamplesMaxFrequency] to be used for interactions  (to fix low audio)
/// NormalizedCurrentLoudestSample_SessionLoudestSample
/// MinimumCapturableSample: to handle E notation floats -> 9.040459E-06 returned when no sound is being played
/// </summary>

public class AudioManager : MonoBehaviour
{

    private WasapiLoopbackCapture loopbackCapture;
    [SerializeField]
    private MMDeviceCollection devices;
    public byte AudioEndpoint = 1;
    public DataFlow DataFlow = DataFlow.All;
    [SerializeField]
    private readonly DeviceState deviceState = DeviceState.Active;
    private float[] loopbackSamples;
    [Range(16, 2048)] 
    public short LastLoudestSamplesLength = 128;
    private short lastLoudestSamplesIndex = 0;
    private float[] lastLoudestSamples;
    public float LastLoudestSamplesAverage = 0f;
    public float LastLoudestSamplesMax = 0f;
    public float LastLoudestSamplesMin = 0f;
    public int LastLoudestSamplesFrameTempo;
    [Range(0.1f, 1f)]
    public float NormalizedLastLoudestSamplesTempoDifferential = 0.20f;
    public float CurrentLoudestSample;
    public float SessionLoudestSample = 0f;
    public float CurrentLoudestSampleMultiplier = 1f;
    public float NormalizedCurrentLoudestSample_SessionLoudestSample = 0f;
    public float NormalizedCurrentLoudestSample_LastLoudestSamplesMax = 0f;
    [Range(0.00001f, 0.001f)]
    public float MinimumCapturableSample = 0.001f;
    public event EventHandler OnNormalizedValuesChange;
    public FPSManager FPSManager;
    private readonly ArrayUtils<float> arrayUtils = new ArrayUtils<float>();
    public int CaptureRate = 100;
    //public bool FromSpotify = true;

    /// <summary>
    /// these values need to be set in Awake() to avoid /division by 0 exception
    /// </summary>
    private void Awake()
    {
        loopbackCapture = new WasapiLoopbackCapture(CaptureRate, new WaveFormat(44100, 16, 2));
        this.LastLoudestSamplesFrameTempo = 1;
        this.CurrentLoudestSample = 0f;
    }

    public void Start()
    {
        if (FPSManager == null) FPSManager = GameObject.FindGameObjectWithTag("FPSManager").GetComponent<FPSManager>();
        this.devices = MMDeviceEnumerator.EnumerateDevices(this.DataFlow, this.deviceState);
        this.loopbackCapture.Device = this.devices[this.AudioEndpoint];
        this.lastLoudestSamples = new float[this.LastLoudestSamplesLength];
        this.InitializeLoopbackCapture();
    }

    public void Stop()
    {
        this.loopbackCapture.Stop();
    }

    /// <summary>
    /// BlockCopy takes blocks of bytes (4 bytes for floats) in the byte data array
    /// returned by [_event.Data], assembles them in binary and converts the results into floats
    /// by diving it by Int32.Max() -> 2 147 483 647
    /// </summary>
    public void InitializeLoopbackCapture()
    {
        try
        {
            Debug.Log("AUDIO MANAGER INIT");
            loopbackCapture.Initialize();
            loopbackCapture.DataAvailable += (sender, _event) =>
            {
                //Debug.Log("Capture : " + _event.Data.Length/2);
                ProcessDefaultAudioStream(_event.Data);
                this.UpdateProperties();
            };
            StartCapture();
            loopbackCapture.Stopped += LoopbackCapture_Stopped;
        } catch (Exception ex)
        {
            Debug.Log(ex);
        }
    }

    public void StartCapture()
    {
        loopbackCapture.Start();
    }

    public void StopCapture()
    {
        loopbackCapture.Stop();
    }

    private void LoopbackCapture_Stopped(object sender, RecordingStoppedEventArgs e)
    {
        Debug.Log("CSCore Capture Stopped");
    }

    private void ProcessDefaultAudioStream(byte[] data)
    {
        this.loopbackSamples = new float[data.Length / 4];
        Buffer.BlockCopy(data, 0, this.loopbackSamples, 0, data.Length);
    }

    private void ProcessSpotifyAudioStream(byte[] data) 
    {
        throw new NotImplementedException();
    }

    private void UpdateProperties()
    {
        this.UpdateCurrentLoudestSample();
        this.AddLoudestSample();
        this.LastLoudestSamplesMax = this.lastLoudestSamples.Max();
        this.LastLoudestSamplesMin = this.lastLoudestSamples.Min();
        this.LastLoudestSamplesAverage = this.lastLoudestSamples.Average();
        this.UpdateSessionLoudestSample();
        this.UpdateNormalizedValues();
        this.UpdateLastLoudestSamplesFrameTempo();
    }

    private void UpdateCurrentLoudestSample()
    {
        float loudestSampleBuffer;
        loudestSampleBuffer = this.loopbackSamples.Max();
        if (loudestSampleBuffer > this.MinimumCapturableSample)
            this.CurrentLoudestSample = this.loopbackSamples.Max() * this.CurrentLoudestSampleMultiplier;
        else this.CurrentLoudestSample = 0f;
    }

    private void AddLoudestSample()
    {
        if (this.lastLoudestSamplesIndex < this.lastLoudestSamples.Length) this.lastLoudestSamples[this.lastLoudestSamplesIndex++] = this.CurrentLoudestSample;
        else this.lastLoudestSamples = arrayUtils.AddLast(this.lastLoudestSamples, this.CurrentLoudestSample);
    }

    private void UpdateNormalizedValues()
    {
        if (this.LastLoudestSamplesMax > 0f) 
            this.NormalizedCurrentLoudestSample_LastLoudestSamplesMax = this.CurrentLoudestSample/this.LastLoudestSamplesMax;
        else
            this.NormalizedCurrentLoudestSample_LastLoudestSamplesMax = 0f;

        if (this.SessionLoudestSample > 0f)
            this.NormalizedCurrentLoudestSample_SessionLoudestSample = this.CurrentLoudestSample/this.SessionLoudestSample;
        else
            this.NormalizedCurrentLoudestSample_SessionLoudestSample = 0f;

        if (this.OnNormalizedValuesChange != null) this.OnNormalizedValuesChange(this, EventArgs.Empty);
    }

    private void UpdateSessionLoudestSample()
    {
        if (this.CurrentLoudestSample > this.SessionLoudestSample) this.SessionLoudestSample = this.CurrentLoudestSample;
    }

    private void UpdateLastLoudestSamplesFrameTempo()
    {
        List<int> tempoValues = new List<int>();
        float percentagePart;
        int tempoValue = 0;
        for (int elem = 0; elem < this.lastLoudestSamples.Length - 1; elem++)
        {
            percentagePart = this.lastLoudestSamples[elem] * this.NormalizedLastLoudestSamplesTempoDifferential;
            if (this.lastLoudestSamples[elem + 1] >= this.lastLoudestSamples[elem] - percentagePart && this.lastLoudestSamples[elem + 1] <= this.lastLoudestSamples[elem] + percentagePart) 
                tempoValue++;
            else if (tempoValue > 0)
            {
                tempoValues.Add(tempoValue);
                tempoValue = 0;
            }
        }
        this.LastLoudestSamplesFrameTempo = tempoValues.Count > 0 ? (int)tempoValues.Average() : 1;
    }

    private void OnDestroy()
    {
        loopbackCapture.Dispose();
    }
}
