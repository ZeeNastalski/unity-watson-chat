# Watson Developer Cloud Unity SDK
[![Build Status](https://travis-ci.org/watson-developer-cloud/unity-sdk.svg?branch=develop)](https://travis-ci.org/watson-developer-cloud/unity-sdk)

Use this SDK to build Watson-powered applications in Unity.

## Table of Contents
* [Before you begin](#before-you-begin)
* [Getting the Watson SDK and adding it to Unity](#getting-the-watson-sdk-and-adding-it-to-unity)
  * [Installing the SDK source into your Unity project](#installing-the-sdk-source-into-your-unity-project)
* [Configuring your service credentials](#configuring-your-service-credentials)
* [Authentication](#authentication)
* [Watson Services](#watson-services)
* [Authentication Tokens](#authentication-tokens)
* [Documentation](#documentation)
* [Questions](#questions)
* [Open Source @ IBM](#open-source--ibm)
* [License](#license)
* [Contributing](#contributing)

## Before you begin
Ensure that you have the following prerequisites:

* An IBM Bluemix account. If you don't have one, [sign up][bluemix_registration].
* [Unity][get_unity]. You can use the **free** Personal edition.
* Change the build settings in Unity (**File > Build Settings**) to any platform except for web player/Web GL. The Watson Developer Cloud Unity SDK does not support Unity Web Player.

## Getting the Watson SDK and adding it to Unity
You can get the latest SDK release by clicking [here][latest_release].

### Installing the SDK source into your Unity project
Move the **`unity-sdk`** directory into the **`Assets`** directory of your Unity project. _Optional: rename the SDK directory from `unity-sdk` to `Watson`_.

## Configuring your service credentials
To create instances of Watson services and their credentials, follow the steps below.

**Note:** Service credentials are different from your Bluemix account username and password.

1. Determine which services to configure.
1. If you have configured the services already, complete the following steps. Otherwise, go to step 3.
    1. Log in to Bluemix at https://bluemix.net.
    1. Click the service you would like to use.
    1. Click **Service credentials**.
    1. Click **View credentials** to access your credentials.
1. If you need to configure the services that you want to use, complete the following steps.
    1. Log in to Bluemix at https://bluemix.net.
    1. Click the **Create service** button.
    1. Under **Watson**, select which service you would like to create an instance of and click that service.
    1. Give the service and credential a name. Select a plan and click the **Create** button on the bottom.
    4. Click **Service Credentials**.
    5. Click **View credentials** to access your credentials.
1. Your service credentials can be used to instantiate Watson Services within your application. Most services also support tokens which you can instantiate the service with as well.

The credentials for each service contain either a `username`, `password` and endpoint `url` **or** an `apikey` and endpoint `url`.

**WARNING:** You are responsible for securing your own credentials. Any user with your service credentials can access your service instances!

## Authentication
Before you can use a service, it must be authenticated with the service instance's `username`, `password` and `url`.

```cs
using IBM.Watson.DeveloperCloud.Services.Conversation.v1;
using IBM.Watson.DeveloperCloud.Utilities;

void Start()
{
    Credentials credentials = new Credentials(<username>, <password>, <url>);
    Conversation _conversation = new Conversation(credentials);
}
```

For services that authenticate using an apikey, you can instantiate the service instance using a `Credential` object with an `apikey` and `url`.

```cs
using IBM.Watson.DeveloperCloud.Services.VisualRecognition.v3;
using IBM.Watson.DeveloperCloud.Utilities;

void Start()
{
    Credentials credentials = new Credentials(<apikey>, <url>);
    VisualRecognition _visualRecognition = new VisualRecognition(credentials);
}
```

## Watson Services
To get started with the Watson Services in Unity, click on each service below to read through each of their `README.md`'s and their codes.
* [Alchemy Language](/Scripts/Services/AlchemyAPI/v1)
* [Conversation](/Scripts/Services/Conversation/v1)
* [Discovery](/Scripts/Services/Discovery/v1)
* [Document Conversion](/Scripts/Services/DocumentConversion/v1)
* [Language Translator](/Scripts/Services/LanguageTranslator/v2)
* [Natural Language Classifier](/Scripts/Services/NaturalLanguageClassifier/v2)
* [Natural Language Understanding](/Scripts/Services/NaturalLanguageUnderstanding/v1)
* [Personality Insights](/Scripts/Services/PersonalityInsights/v3)
* [Retrieve and Rank](/Scripts/Services/RetrieveAndRank/v1)
* [Speech to Text](/Scripts/Services/SpeechToText/v1)
* [Text to Speech](/Scripts/Services/TextToSpeech/v1)
* [Tone Analyzer](/Scripts/Services/ToneAnalyzer/v3)
* [Tradeoff Analytics](/Scripts/Services/TradeoffAnalytics/v1)
* [Visual Recognition](/Scripts/Services/VisualRecognition/v3)

## Authentication Tokens
You use tokens to write applications that make authenticated requests to IBM Watson™ services without embedding service credentials in every call.

You can write an authentication proxy in IBM® Bluemix® that obtains and returns a token to your client application, which can then use the token to call the service directly. This proxy eliminates the need to channel all service requests through an intermediate server-side application, which is otherwise necessary to avoid exposing your service credentials from your client application.

```cs
using IBM.Watson.DeveloperCloud.Services.Conversation.v1;
using IBM.Watson.DeveloperCloud.Utilities;

void Start()
{
    Credentials credentials = new Credentials(<service-url>)
    {
        AuthenticationToken = <authentication-token>
    };
    Conversation _conversation = new Conversation(credentials);
}
```

There is a helper class included to obtain tokens from within your Unity application.

```cs
using IBM.Watson.DeveloperCloud.Utilities;

AuthenticationToken _authenticationToken;

void Start()
{
    if (!Utility.GetToken(OnGetToken, <service-url>, <service-username>, <service-password>))
        Log.Debug("ExampleGetToken", "Failed to get token.");
}

private void OnGetToken(AuthenticationToken authenticationToken, string customData)
{
    _authenticationToken = authenticationToken;
    Log.Debug("ExampleGetToken", "created: {0} | time to expiration: {1} minutes | token: {2}", _authenticationToken.Created, _authenticationToken.TimeUntilExpiration, _authenticationToken.Token);
}
```

## Documentation
Documentation can be found [here][documentation]. You can also access the documentation by selecting API Reference the Watson menu (**Watson -> API Reference**).

## Questions

If you are having difficulties using the APIs or have a question about the IBM Watson Services, please ask a question on
[dW Answers](https://developer.ibm.com/answers/questions/ask/?topics=watson)
or [Stack Overflow](http://stackoverflow.com/questions/ask?tags=ibm-watson).

## Open Source @ IBM
Find more open source projects on the [IBM Github Page](http://ibm.github.io/).

## License
This library is licensed under Apache 2.0. Full license text is available in [LICENSE](LICENSE).

## Contributing
See [CONTRIBUTING.md](.github/CONTRIBUTING.md).

[wdc]: http://www.ibm.com/watson/developercloud/
[wdc_unity_sdk]: https://github.com/watson-developer-cloud/unity-sdk
[latest_release]: https://github.com/watson-developer-cloud/unity-sdk/releases/latest
[bluemix_registration]: http://bluemix.net/registration
[get_unity]: https://unity3d.com/get-unity

[speech_to_text]: http://www.ibm.com/watson/developercloud/doc/speech-to-text/
[text_to_speech]: http://www.ibm.com/watson/developercloud/doc/text-to-speech/
[language_translator]: http://www.ibm.com/watson/developercloud/doc/language-translator/index.html
[dialog]: https://console.bluemix.net/docs/services/conversation/index.html#about
[natural_language_classifier]: http://www.ibm.com/watson/developercloud/doc/natural-language-classifier/index.html

[alchemy_language]: http://www.alchemyapi.com/products/alchemylanguage
[alchemyData_news]: http://www.ibm.com/watson/developercloud/alchemy-data-news.html
[sentiment_analysis]: http://www.alchemyapi.com/products/alchemylanguage/sentiment-analysis
[tone_analyzer]: http://www.ibm.com/watson/developercloud/doc/tone-analyzer/
[tradeoff_analytics]: http://www.ibm.com/watson/developercloud/doc/tradeoff-analytics/
[conversation]:https://console.bluemix.net/docs/services/conversation/index.html#about
[visual_recognition]: http://www.ibm.com/watson/developercloud/visual-recognition/api/v3/
[personality_insights]: http://www.ibm.com/watson/developercloud/personality-insights/api/v2/
[conversation_tooling]: https://www.ibmwatsonconversation.com
[retrieve_and_rank]: http://www.ibm.com/watson/developercloud/retrieve-and-rank/api/v1/
[discovery]: http://www.ibm.com/watson/developercloud/discovery/api/v1/
[document_conversion]: http://www.ibm.com/watson/developercloud/document-conversion/api/v1/
[expressive_ssml]: http://www.ibm.com/watson/developercloud/doc/text-to-speech/http.shtml#expressive
[ssml]: http://www.ibm.com/watson/developercloud/doc/text-to-speech/SSML.shtml
[discovery-query]: http://www.ibm.com/watson/developercloud/doc/discovery/using.shtml
[natural_language_understanding]: https://www.ibm.com/watson/developercloud/natural-language-understanding.html
[nlu_models]: https://www.ibm.com/watson/developercloud/doc/natural-language-understanding/customizing.html
[nlu_entities]: https://www.ibm.com/watson/developercloud/natural-language-understanding/api/v1/#entities
[nlu_relations]: https://www.ibm.com/watson/developercloud/natural-language-understanding/api/v1/#relations

[dialog_service]: http://www.ibm.com/watson/developercloud/doc/dialog/
[dialog_migration]: https://console.bluemix.net/docs/services/conversation/index.html#about
[conversation_service]: https://console.bluemix.net/docs/services/conversation/index.html#about
[documentation]: https://watson-developer-cloud.github.io/unity-sdk/
