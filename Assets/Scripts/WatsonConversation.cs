using System.Collections.Generic;
using UnityEngine;
using IBM.Watson.DeveloperCloud.Services.Conversation.v1;
using IBM.Watson.DeveloperCloud.Utilities;
using FullSerializer;
using IBM.Watson.DeveloperCloud.Connection;
using System;

public delegate void ConversationResponseDelegate(string text, string intent, float confidence);

public class WatsonConversation : MonoBehaviour
{
    public ConversationResponseDelegate ConversationResponse = delegate { };

    [Header("Credentials")]
    public string Url;
    public string User;
    public string Password;    
    public string WorkspaceId;

    private Conversation _conversationApi;

    private Dictionary<string, object> _context; // context to persist
    private fsSerializer _serializer = new fsSerializer();

    // Use this for initialization
    void Start()
    {
        Credentials conversationCred = new Credentials(User, Password, Url);
        _conversationApi = new Conversation(conversationCred);
        _conversationApi.VersionDate = "2017-07-26";
    }


    public void SendConversationMessage(string text)
    {
        MessageRequest messageRequest = new MessageRequest()
        {
            input = new Dictionary<string, object>()
            {
                { "text", text }
            },
            context = _context
        };

        if (!_conversationApi.Message(OnConversationResponse, OnConversationError, WorkspaceId, messageRequest))
        {
            Debug.LogError("Failed to send the message to Conversation service!");
        }

    }

    private void OnConversationResponse(object resp, Dictionary<string, object> customData)
    {
        if (resp != null)
        {
            //  Convert resp to fsdata
            fsData fsdata = null;
            fsResult r = _serializer.TrySerialize(resp.GetType(), resp, out fsdata);
            if (!r.Succeeded)
                throw new WatsonException(r.FormattedMessages);

            //  Convert fsdata to MessageResponse
            MessageResponse messageResponse = new MessageResponse();
            object obj = messageResponse;
            r = _serializer.TryDeserialize(fsdata, obj.GetType(), ref obj);
            if (!r.Succeeded)
                throw new WatsonException(r.FormattedMessages);


            // remember the context for the next message
            object tempContext = null;

            Dictionary<string, object> respAsDict = resp as Dictionary<string, object>;
            if (respAsDict != null)
            {
                respAsDict.TryGetValue("context", out tempContext);
            }

            if (tempContext != null)
                _context = tempContext as Dictionary<string, object>;
            else
                Debug.LogError("Failed to get context");


            if (ConversationResponse != null)
            {
                string respText = "";
                string intent = "";
                float confidence = 0f;

                if (messageResponse.output.text.Length > 0)
                {
                    respText = messageResponse.output.text[0];
                }

                if (messageResponse.intents.Length > 0)
                {
                    intent = messageResponse.intents[0].intent;
                    confidence = messageResponse.intents[0].confidence;
                }

                ConversationResponse(respText, intent, confidence);
            }
        }
    }

    private void OnConversationError(RESTConnector.Error error, Dictionary<string, object> customData)
    {
        Debug.LogError("Conversation Error: " + error.ErrorMessage);
    }

 


}
