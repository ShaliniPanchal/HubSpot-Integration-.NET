using Newtonsoft.Json.Linq;
using FormUrlEncodedContent = System.Net.Http.FormUrlEncodedContent;

public class HubSpotAuth
{
    private readonly string _clientId;
    private readonly string _clientSecret;
    private readonly string _redirectUri;

    public HubSpotAuth(string clientId, string clientSecret, string redirectUri)
    {
        _clientId = clientId;
        _clientSecret = clientSecret;
        _redirectUri = redirectUri;
    }

    public string GetAuthorizationUrl()
    {
        return $"https://app.hubspot.com/oauth/authorize?client_id={_clientId}&redirect_uri={_redirectUri}&scope=contacts";
    }

    public async Task<string> GetAccessTokenAsync(string authorizationCode)
    {
        using (var httpClient = new HttpClient())
        {
            var tokenEndpoint = "https://api.hubapi.com/oauth/v1/token";

            var requestBody = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("client_id", _clientId),
                new KeyValuePair<string, string>("client_secret", _clientSecret),
                new KeyValuePair<string, string>("redirect_uri", _redirectUri),
                new KeyValuePair<string, string>("code", authorizationCode)
            });

            var response = await httpClient.PostAsync(tokenEndpoint, requestBody);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var tokenResponse = JObject.Parse(responseContent);

            return tokenResponse["access_token"].ToString();
        }
    }
}

public class HubSpotClient
{
    private readonly string _accessToken;

    public HubSpotClient(string accessToken)
    {
        _accessToken = accessToken;
    }

    public async Task GetContactsAsync()
    {
        using (var httpClient = new HttpClient())
        {
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _accessToken);

            var endpoint = "https://api.hubapi.com/crm/v3/objects/contacts";

            var response = await httpClient.GetAsync(endpoint);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var json = JObject.Parse(content);

            Console.WriteLine(json.ToString());
        }
    }
}

public class Program
{
    public static async Task Main(string[] args)
    {
        // Replace with your HubSpot app credentials and redirect URI
        var clientId = "your_client_id";
        var clientSecret = "your_client_secret";
        var redirectUri = "https://your-redirect-uri.com";

        var auth = new HubSpotAuth(clientId, clientSecret, redirectUri);

        // Step 1: Output the authorization URL
        var authorizationUrl = auth.GetAuthorizationUrl();
        Console.WriteLine($"Please visit this URL to authorize: {authorizationUrl}");

        // Step 2: After the user authorizes, HubSpot will redirect with a code. 
        // In a web application, you would handle this redirect and extract the code.
        // For this example, you'll need to manually input the authorization code.
        Console.WriteLine("Enter the authorization code:");
        var authorizationCode = Console.ReadLine();

        // Step 3: Exchange the authorization code for an access token
        var accessToken = await auth.GetAccessTokenAsync(authorizationCode);

        Console.WriteLine($"Access Token: {accessToken}");

        // Step 4: Use the access token to make API requests
        var hubSpotClient = new HubSpotClient(accessToken);
        await hubSpotClient.GetContactsAsync();
    }
}
