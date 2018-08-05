using System;
using System.Collections;
using System.Collections.Generic;
using IBM.Watson.DeveloperCloud.Connection;
using IBM.Watson.DeveloperCloud.Services.TextToSpeech.v1;
using IBM.Watson.DeveloperCloud.Utilities;
using UnityEngine;


public class WatsonTextToSpeech : MonoBehaviour
{
    public AudioSource AudioSrc;
    public VoiceType Voice;

    [Header("Credentials")]
    public string ApiKey;
    public string Url;


    [HideInInspector]
    public bool IsReady = false;

    private TextToSpeech _textToSpeech;
    private Credentials _credentials;

    // Use this for initialization
    void Start () {
        StartCoroutine(AuthenticateAndConfigure());
    }

    private IEnumerator AuthenticateAndConfigure()
    {
        TokenOptions iamTokenOptions = new TokenOptions()
        {
            IamApiKey = ApiKey,
            IamUrl = "https://iam.bluemix.net/identity/token"
        };

        // Create credentials using the IAM token options
        _credentials = new Credentials(iamTokenOptions, Url);

        while (!_credentials.HasIamTokenData())
            yield return null;

        _textToSpeech = new TextToSpeech(_credentials);
        _textToSpeech.Voice = Voice;

        IsReady = true;    
    }

    public void Say(string text)
    {
        if (!_textToSpeech.ToSpeech(OnSynthetize, OnFail, text, true))
        {
            Debug.Log("Error sending text to speech.");
        }                    
    }

   

    private void OnFail(RESTConnector.Error error, Dictionary<string, object> customData)
    {
        string errorMessage = string.Format("ExampleTextToSpeech.OnFail()", "Error received: {0}", error.ToString());
        Debug.LogError(errorMessage);
    }

    private void OnSynthetize(AudioClip clip, Dictionary<string, object> customData)
    {
        AudioSrc.clip = clip;
        AudioSrc.Play();
    }    
}
