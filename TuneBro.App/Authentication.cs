using SpotifyAPI.Web;

namespace TuneBro.App
{
  public class Authentication : IAuthentication
  {
    public void SpotifyLogin()
    {
      var loginRequest = new LoginRequest(
        new Uri("http://localhost:5543"),
        "ClientId",
        LoginRequest.ResponseType.Code
      )
      {
        Scope = new[] { Scopes.Streaming }
      };

      var uri = loginRequest.ToUri();
    }
  }
}
