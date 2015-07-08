using System.Net.Http;
using DTO;
using System.Threading.Tasks;

namespace CalendarSyncService
{
    /// <summary>
    /// Klasa statyczna odpowiadająca za pobieranie danych o tokenie z bazy danych.
    /// </summary>
    public static class GetTokenFromWebService
    {
        /// <summary>
        /// Metoda wysyła zapytanie do Webservice'u o token wybranego użytkownika.
        /// </summary>
        /// <param name="userId">Numer id użytkownika (w bazie danych), którego token ma być zwrócony.</param>
        /// <returns>Zwraca string jako wynik zadania. Jest to wartość tokenu do odświeżenia autoryzacji użytkownika lub w przypadku niepowodzenia, wartość null .</returns>
        public static async Task<string> GetTokenFromDatabase(int userId)
        {
            Logs.WriteErrorLog("GetTokenFromDatabase.");
            var tokenRequest = new GetGoogleTokenRequest()
            {
                Id = userId,
                SenderId = LoginToWebServiceData.WebServiceId,
                Token = LoginToWebServiceData.WebServiceToken
            };
            Logs.WriteErrorLog("User id: " + tokenRequest.Id.ToString() + ", ServiceID" + tokenRequest.SenderId.ToString() + ", Token: " + tokenRequest.Token.ToString());
            var tokenResponse = await WebServiceConnection.
                SendPost<GetGoogleTokenRequest, GetGoogleTokenResponse>(tokenRequest, "google/token/show");
            if (tokenResponse == null)
            {
                Logs.WriteErrorLog("Brak odpowiedzi o tokenie od WebService'u.");
                return null;
            }
            Logs.WriteErrorLog("Status: " + tokenResponse.Status.ToString()+ ", Message: " + tokenResponse.Message + ", GoogleToken: " + tokenResponse.GoogleToken);
            if (tokenResponse.Status == 2) return null;
            Logs.WriteErrorLog("GetTokenFromWebService: " + tokenResponse.GoogleToken.ToString());
            return tokenResponse.Status == 0 ? tokenResponse.GoogleToken : null;
        }
    }
}
