
using System;
using UnityEngine;
using UnityEngine.UI;


public class WatsonDemo : MonoBehaviour
{
    public bool ShowIntentConfidence = false;
    public InputField TextInputField;
    public Text ConversationText;
    public Button TalkButton;

    public WatsonSpeechToText SpeechToText;
    public WatsonTextToSpeech TextToSpeech;
    public WatsonConversation Conversation;



    void OnEnable ()
    {
        SpeechToText.SpeechRecognized += SpeechToTextSpeechRecognized;
        SpeechToText.SpeechError += SpeechToTextError;
        SpeechToText.StoppedListening += OnSpeechToTextStop;

        Conversation.ConversationResponse += OnConversationResponse;
    }

    private void OnConversationResponse(string text, string intent, float confidence)
    {
        if(text ==null)
        {
            text = "";
        }

        if (ShowIntentConfidence)
        {
            ConversationText.text += String.Format("Watson: {0} (#{1} {2:0.000})", text, intent, confidence);
        }
        else
        {
            ConversationText.text += String.Format("Watson: {0}", text);
        }

        //Send the conversation response to speech synthesis service
        TextToSpeech.Say(text);
    }

    void OnDisable()
    {
        SpeechToText.SpeechRecognized -= SpeechToTextSpeechRecognized;
        SpeechToText.SpeechError -= SpeechToTextError;
        SpeechToText.StoppedListening -= OnSpeechToTextStop;
        Conversation.ConversationResponse -= OnConversationResponse;
    }



    private void OnSpeechToTextStop()
    {
       StopTalking();
    }

    private void SpeechToTextError(string error)
    {
        Debug.LogError("Speech to text error: " + error);
        StopTalking();
    }


    public void StartTalking()
    {
        if (!SpeechToText.IsTalking())
        {
            TextInputField.text = "";
            SpeechToText.StartTalking();
            TalkButton.interactable = false;
        }
    }

    public void StopTalking()
    {
        TalkButton.interactable = true;

        if (SpeechToText.IsTalking())
        {
            SpeechToText.StopTalking();
        }
    }

    private void SpeechToTextSpeechRecognized(string text, double confidence, bool final)
    {
        TextInputField.text = String.Format(" {0} ({1})\n\n", text, confidence);
        

        if (final)
        {
            // put final recognition results on the screen and send them to Conversation service.
            string finalText = text;
            finalText = finalText.Replace("%HESITATION", "");

            ConversationText.text = String.Format("You: {0}\n\n", finalText, confidence);

            
            Conversation.SendConversationMessage(text);
            
        }
    }

    public void EnterText()
    {
        Conversation.SendConversationMessage(TextInputField.text);
        ConversationText.text = String.Format("You: {0}\n\n", TextInputField.text);
        TextInputField.text = "";        
    }

    



}
