using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using IBM.Watson.DeveloperCloud.Services.SpeechToText.v1;
using IBM.Watson.DeveloperCloud.Utilities;
using IBM.Watson.DeveloperCloud.DataTypes;
using System;

public delegate void SpeechRecognizedDelegate(string text, double confidence, bool final);
public delegate void SpeechErrorDelegate(string error);
public delegate void StopListeningDelegate();

public class WatsonSpeechToText : MonoBehaviour
{
    public event SpeechRecognizedDelegate SpeechRecognized = delegate { };
    public event SpeechErrorDelegate SpeechError = delegate { };
    public event StopListeningDelegate StoppedListening = delegate { };

    public AudioSource AudioSrc;
    public const int MicDeviceId = 0;
    public bool PlayBackAudio = false;
    public float InactivityTimeoutSec = 6.0f;



    [Header("Credentials")]
    public string Url;
    public string User;
    public string Password;
    
    
    private const int MIC_REC_BUFFER_LEN_SEC = 30;
    private const int MIC_FREQUENCY = 22050;
    private const float PUSH_AUDIO_CHUNK_INTERVAL = 0.5f;
    

    private int _audioChunkStartPosition = -1;
    private AudioClip _rollingAudioClip;
    private Coroutine _pushAudioChunkCroutine;
    private Coroutine _stopListeningTimeoutCoroutine;

    private SpeechToText _speechToText;

    private List<float> _playBackAudioData;


    void Awake()
    {       
        Credentials sttCredentials = new Credentials(User, Password, Url);
        _speechToText = new SpeechToText(sttCredentials);

        _speechToText.DetectSilence = true;
        _speechToText.EnableWordConfidence = false;
        _speechToText.EnableTimestamps = false;
        _speechToText.SilenceThreshold = 0.03f;
        _speechToText.MaxAlternatives = 1;
        _speechToText.EnableContinousRecognition = true;
        _speechToText.EnableInterimResults = true;
        _speechToText.OnError = OnSpeachToTextError;

    }

    private void OnSpeachToTextError(string error)
    {
        Debug.Log(string.Format("Speech to text error! {0}", error));
        SpeechError(error);
    }

    void OnEnable()
    {
        // Starting recording in here. We'll record continuously and loop the audio clip.
        // We'll only send data that's captured between StartTalking and StopTalking calls

        Debug.Log("Starting recording on " + Microphone.devices[MicDeviceId]);

        _rollingAudioClip = Microphone.Start(
            Microphone.devices[MicDeviceId],
            true,
            MIC_REC_BUFFER_LEN_SEC,
            MIC_FREQUENCY);
    }

    void OnDisable()
    {
        Microphone.End(Microphone.devices[MicDeviceId]);
    }



    public bool IsTalking()
    {
		if(_speechToText == null) Debug.Log("STT null");
        return _speechToText.IsListening;
    }
    
    public void StartTalking()
    {
        if (!_speechToText.IsListening)
        {
            Debug.Log("Start talking");
            _playBackAudioData = new List<float>();


            _audioChunkStartPosition = Microphone.GetPosition(Microphone.devices[0]);

            // cancel the timeout if user starts speaking
            if (_stopListeningTimeoutCoroutine != null)
            {
                StopCoroutine(_stopListeningTimeoutCoroutine);
            }
            _speechToText.StartListening(OnSpeechRecognize);
            _pushAudioChunkCroutine = StartCoroutine(PushAudioChunkCroutine());
            _stopListeningTimeoutCoroutine = StartCoroutine(StopTalkingTimeout(InactivityTimeoutSec));
        }
    }

    public void StopTalking()
    {
        if (_speechToText.IsListening)
        {
            Debug.Log("Stop listening");

            StopCoroutine(_pushAudioChunkCroutine);
            _pushAudioChunkCroutine = null;

            StopCoroutine(_stopListeningTimeoutCoroutine);
            _stopListeningTimeoutCoroutine = null;

            _speechToText.StopListening();

            if (PlayBackAudio)
            {
                AudioClip clip = AudioClip.Create("testClip", _playBackAudioData.Count, _rollingAudioClip.channels,
                    MIC_FREQUENCY, false);
                clip.SetData(_playBackAudioData.ToArray(), 0);

                AudioSrc.clip = clip;
                AudioSrc.Play();
            }

            if (StoppedListening != null)
                StoppedListening();
        }
    }


    private IEnumerator PushAudioChunkCroutine()
    {

        while (true)
        {
            yield return new WaitForSeconds(PUSH_AUDIO_CHUNK_INTERVAL);
            PushAudioChunk();
        }
    }


    private void PushAudioChunk()
    {
        int endPosition = Microphone.GetPosition(Microphone.devices[0]);

        if (endPosition == _audioChunkStartPosition)
        {
            //no data to send
            return;
        }

        AudioData recording = new AudioData();
        float[] speechAudioData;
        int newClipLength;

        if (endPosition > _audioChunkStartPosition)
        {
            newClipLength = endPosition - _audioChunkStartPosition + 1;
            speechAudioData = new float[newClipLength * _rollingAudioClip.channels];
            _rollingAudioClip.GetData(speechAudioData, _audioChunkStartPosition);

        }
        else
        {   
            // We've wrapped around the rolling audio clip. We have to take the audio from start position till the end of the rolling clip. Then, add clip from 0 to endPosition;
            int newClipLengthLeft = _rollingAudioClip.samples - _audioChunkStartPosition + 1;
            int newClipLengthRight = endPosition + 1;

            float[] speechAudioDataLeft = new float[newClipLengthLeft * _rollingAudioClip.channels];
            float[] speechAudioDataRight = new float[newClipLengthRight * _rollingAudioClip.channels];

            _rollingAudioClip.GetData(speechAudioDataLeft, _audioChunkStartPosition);
            _rollingAudioClip.GetData(speechAudioDataRight, 0);

            newClipLength = speechAudioDataLeft.Length + speechAudioDataRight.Length;
            speechAudioData = new float[newClipLength];

            Array.Copy(speechAudioDataLeft, speechAudioData, newClipLengthLeft);
            Array.Copy(speechAudioDataRight, 0, speechAudioData, newClipLengthLeft, newClipLengthRight);
        }

        if (PlayBackAudio)
        {
            _playBackAudioData.AddRange(speechAudioData);
        }

        recording.Clip = AudioClip.Create("clip", newClipLength, _rollingAudioClip.channels, MIC_FREQUENCY, false);
        recording.Clip.SetData(speechAudioData, 0);

        _audioChunkStartPosition = endPosition;

        recording.MaxLevel = Mathf.Max(speechAudioData);
        //Debug.Log("Pushing chunk length " + recording.Clip.length);

        _speechToText.OnListen(recording);


    }


    private void OnSpeechRecognize(SpeechRecognitionEvent result)
    {
        
        if (result != null && result.results.Length > 0)
        {
            var res = result.results[0];
            var alt = res.alternatives[0];
              
                    // We've got some results, reset inactivity timeout
                    StopCoroutine(_stopListeningTimeoutCoroutine);
                    _stopListeningTimeoutCoroutine = StartCoroutine(StopTalkingTimeout(InactivityTimeoutSec));

                    string text = alt.transcript;
                    string resp = string.Format("{0} ({1}, {2:0.00})\n", text, res.final ? "Final" : "Interim",
                        alt.confidence);

                    if (res.final)
                    {
                        StopTalking();
                    }

                    SpeechRecognized(text, alt.confidence, res.final);
                    
                    Debug.Log(resp);
                }            
        
    }

    IEnumerator StopTalkingTimeout(float delay)
    {
        yield return new WaitForSeconds(delay);
        StopTalking();
    }


}

  

