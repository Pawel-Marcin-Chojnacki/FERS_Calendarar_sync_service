using System;
using System.IO;
using DTO;
namespace CalendarSyncService
{
    /// <summary>
    /// Klasa przechowuje wszystkie informacje niezbędne do komunikacji z WebService'm.
    /// </summary>
    public static class LoginToWebServiceData
    {
        /// <summary>
        /// Login uslugi.
        /// </summary>
        public static string WebServiceLogin { get; set; }

        /// <summary>
        /// Hasło dostępu do WebService'u.
        /// </summary>
        public static string WebServicePassword { get; set; }

        /// <summary>
        /// Id użytkownika (usługi).
        /// </summary>
        public static int WebServiceId { get; set; }

        /// <summary>
        /// Token zapewniający dodatkową warstwę bezpieczeństwa.
        /// Zmienia się co godzinę.
        /// </summary>
        public static string WebServiceToken { get; set; }

        /// <summary>
        /// Medota odpowiedzialna za odświeżenie tokenu co godzinę.
        /// </summary>
        /// <returns>Zwraca nowy token w formie stringa.</returns>
        public static string RefreshToken()
        {
            var updateTokenRequest = new UpdateTokenRequest();
            UpdateTokenResponse updateTokenResponse =
                WebServiceConnection.SendPost<UpdateTokenRequest, UpdateTokenResponse>(updateTokenRequest,
                    "token/update").Result;
            Logs.WriteErrorLog("Nowy token: " + updateTokenResponse.Token);
            WriteLastTokenToFile(updateTokenResponse.Token);
            return updateTokenResponse.Token ?? null;
        }

        /// <summary>
        /// Lokalizacja pliku posiadający ostatni token dostępowy do WebService'u.
        /// </summary>
        public static string LastWebServiceTokenFileLocation { get; set; }

        /// <summary>
        /// Zapisuje poprzedni token do pliku.
        /// Pozwala na dostęp do WebService'u po restarcie usługi.
        /// </summary>
        /// <param name="token">Ostatni token</param>
        public static void WriteLastTokenToFile(string token)
        {
            LastWebServiceTokenFileLocation = AppDomain.CurrentDomain.BaseDirectory + "\\LastWSToken.txt";
            StreamWriter streamWriter = StreamWriter.Null;
            try
            {
                streamWriter = new StreamWriter(LastWebServiceTokenFileLocation,
                    false);
                streamWriter.WriteLine(token);

            }
            catch (Exception exception)
            {
                throw new Exception();
            }
            finally
            {
                if (streamWriter != null)
                {
                    streamWriter.Flush();
                    streamWriter.Close();
                }
            }
        }

        /// <summary>
        /// Odczytuje z pliku ostatni istniejący Token dostępu do WebService'u.
        /// </summary>
        /// <returns></returns>
        public static string ReadLastWebServiceToken()
        {
            using (StreamReader streamReader = new StreamReader(LastWebServiceTokenFileLocation))
            {
                WebServiceToken = streamReader.ReadLine();
            }
            return WebServiceToken;
        }

        /// <summary>
        /// Metoda uzyskuje dostęp do WebService'u dla usługi.
        /// </summary>
        public static void LoginToWebService()
        {
            
        }
    }
}
