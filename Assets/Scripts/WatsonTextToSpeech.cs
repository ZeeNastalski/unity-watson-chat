using System.Collections;
using System.Collections.Generic;
using IBM.Watson.DeveloperCloud.Services.TextToSpeech.v1;
using IBM.Watson.DeveloperCloud.Utilities;
using UnityEngine;


public class WatsonTextToSpeech : MonoBehaviour
{
    public AudioSource AudioSrc;
    public VoiceType Voice;

    [Header("Credentials")]
    public string Url;
    public string User;
    public string Password;
    

    private TextToSpeech _textToSpeech;

    // Use this for initialization
    void Awake () {
	    Credentials	ttsCredential = new Credentials(User, Password, Url);
        _textToSpeech = new TextToSpeech(ttsCredential);
        _textToSpeech.Voice = Voice;
    }


    public void Say(string text)
    {
        if (!_textToSpeech.ToSpeech(text, OnSynthetize, true))
        {
            Debug.Log("Error sending text to speech.");
        }                    
    }

    private void OnSynthetize(AudioClip clip, string customdata)
    {
        AudioSrc.clip = clip;
        AudioSrc.Play();
    }
}
