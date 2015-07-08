using System;
using System.IO;
using Newtonsoft.Json;

namespace CalendarSyncService
{
    /// <summary>
    /// Klasa statyczna, przechowuje informacje o numerze klienta webowego i jego sekret.
    /// Odczytuje informacje o kliencie z pliku.
    /// </summary>
    public static class ClientCredentials
    {
        /// <summary>
        /// Pole na numer identyfikujący klienta webowego.
        /// </summary>
        public static string ClientId { get; set; }

        /// <summary>
        /// Pole na zaszyfrowany sekret klienta webowego.
        /// </summary>
        public static string ClientSecret { get; set; }

        /// <summary>
        /// Lokalizacja pliku z sekretem klienta webowego.
        /// </summary>
        public static string ClientSecretsWebClientFileLocation { get; set; }

        /// <summary>
        /// Lokalizacja pliku z sekretem klienta natywnego.
        /// </summary>
        public static string ClientSecretsNativeClientFileLocation { get; set; }

        /// <summary>
        /// Metoda statyczna ładująca niezbędne dane klienta webowego z pliku typu JSON do usługi.
        /// </summary>
        /// <returns>True, jeśli sekret załadowano pomyślnie.
        ///         False, jeśli sekretu nie udało się załadować.</returns>
        public static bool LoadClietSecretsFromFile()
        {
            ClientSecretsWebClientFileLocation = AppDomain.CurrentDomain.BaseDirectory + "client_secret.json";
            string secrets = null;
            Logs.WriteErrorLog("Laduje sekret klienta z pliku " + ClientSecretsWebClientFileLocation + ".");
            try
            {
                using (StreamReader sr = new StreamReader(ClientSecretsWebClientFileLocation))
                {
                    secrets = sr.ReadToEnd();
                }
            }
            catch (Exception e)
            {
                Logs.WriteErrorLog("Wystapil blad podczas czytania pliku \"" + ClientSecretsWebClientFileLocation + "\".");
                Logs.WriteErrorLog(e.Message);
            }

            ClientSecretFile cs = null;
            if (secrets != null)
            {
                cs = JsonConvert.DeserializeObject<ClientSecretFile>(secrets);
            }
            if (cs == null)
            {
                Logs.WriteErrorLog("Wystapil blad podczas serializacji danych pliku " + ClientSecretsWebClientFileLocation);
                return false;
            }
            ClientId = cs.Web.ClientId;
            ClientSecret = cs.Web.ClientSecret;
            return true;
        }

        /// <summary>
        /// Metoda sprawdza czy zapisane obecnie dane klienta zgadzają się z danymi w pliku.
        /// </summary>
        /// <returns>True, jeśli wystąpiła zgodność.
        ///          False, jeśli dane były niezgodne i należało je poprawić.</returns>
        public static bool ValidateClientCredentials()
        {
            string secrets = null;
            try
            {
                using (StreamReader sr = new StreamReader(ClientSecretsWebClientFileLocation))
                {
                    secrets = sr.ReadToEnd();
                }
            }
            catch (Exception e)
            {
                Logs.WriteErrorLog("Wystapil blad podczas czytania pliku \"" + ClientSecretsWebClientFileLocation + "\". (walidacja nie powiodła się).");
                Logs.WriteErrorLog(e.Message);
            }
            ClientSecretFile cs = null;
            if (secrets != null)
            {
                cs = JsonConvert.DeserializeObject<ClientSecretFile>(secrets);
            }
            if (cs == null)
            {
                Logs.WriteErrorLog("Wystapil blad podczas serializacji danych pliku " + ClientSecretsWebClientFileLocation + " (w trakcie walidacji danych).");
                return false;
            }
            if (ClientId == cs.Web.ClientId && ClientSecret == cs.Web.ClientSecret) 
                return true;

            ClientId = cs.Web.ClientId;
            ClientSecret = cs.Web.ClientSecret;
            return false;
        }
    }
}
