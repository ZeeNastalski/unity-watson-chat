# Personality Insights

The IBM Watson™ [Personality Insights][personality-insights] service enables applications to derive insights from social media, enterprise data, or other digital communications. The service uses linguistic analytics to infer individuals' intrinsic personality characteristics, including Big Five, Needs, and Values, from digital communications such as email, text messages, tweets, and forum posts.

The service can automatically infer, from potentially noisy social media, portraits of individuals that reflect their personality characteristics. The service can infer consumption preferences based on the results of its analysis and, for JSON content that is timestamped, can report temporal behavior.

For information about the meaning of the models that the service uses to describe personality characteristics, see [Personality models][personality-models]. For information about the meaning of the consumption preferences, see [Consumption preferences][consumption-preferences].

## Usage
The service offers a single `profile` method that accepts up to 20 MB of input data and produces results in JSON or CSV format. The service accepts input in Arabic, English, Japanese, or Spanish and can produce output in a variety of languages.

### Instantiating and authenticating the service
Before you can send requests to the service it must be instantiated and credentials must be set.
```cs
using IBM.Watson.DeveloperCloud.Services.PersonalityInsights.v3;
using IBM.Watson.DeveloperCloud.Utilities;

void Start()
{
    Credentials credentials = new Credentials(<username>, <password>, <url>);
    PersonalityInsights _personalityInsights = new PersonalityInsights(credentials);
}
```

### Profile
Extract personality characteristics based on how a person writes.
```cs
private void GetProfile()
{
  if(!m_personalityInsights.GetProfile(OnGetProfileJson, dataPath, ContentType.TEXT_HTML, ContentLanguage.ENGLISH, ContentType.APPLICATION_JSON, AcceptLanguage.ENGLISH, true, true, true))
    Log.Debug("ExamplePersonalityInsights", "Failed to get profile!");
}

private void OnGetProfile(Profile profile, string data)
{
  Log.Debug("ExamplePersonalityInsights", "Profile result: {0}", data);
}
```

[personality-insights]: https://www.ibm.com/watson/developercloud/personality-insights.html
[personality-models]: https://www.ibm.com/watson/developercloud/doc/personality-insights/models.html
[consumption-preferences]:https://www.ibm.com/watson/developercloud/doc/personality-insights/preferences.html
