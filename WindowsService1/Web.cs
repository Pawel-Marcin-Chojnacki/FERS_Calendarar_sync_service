using System.Collections.Generic;
using Newtonsoft.Json;

namespace CalendarSyncService
{
    /// <summary>
    /// Klasa do deserializacji JSON'a z sekretem klienta webowego.
    /// </summary>
    public class Web
    {
        /// <summary>
        /// Adres serwera autoryzacji.
        /// </summary>
        [JsonProperty("auth_uri")]
        public string AuthUri { get; set; }

        /// <summary>
        /// Sekret klienta w formie zaszyfrowanego klucza.
        /// </summary>
        [JsonProperty("client_secret")]
        public string ClientSecret { get; set; }

        /// <summary>
        /// Adres serwera generującego tokeny dostępu do API.
        /// </summary>
        [JsonProperty("token_uri")]
        public string TokenUri { get; set; }

        /// <summary>
        /// Email klienta.
        /// Przydzielony dla każdego klienta projektu w postaci:
        /// IdProjektu-WegenerowanyNumerAplikacji@developer.gserviceaccount.com
        /// </summary>
        [JsonProperty("client_email")]
        public string ClientEmail { get; set; }

        /// <summary>
        /// Linki zwrotne tzw. callback.
        /// </summary>
        [JsonProperty("redirect_uris")]
        public List<string> RedirectUris { get; set; }

        /// <summary>
        /// Adres serwera sprawdającego certyfikaty x509.
        /// </summary>
        [JsonProperty("client_x509_cert_url")]
        public string ClientX509CertUrl { get; set; }

        /// <summary>
        /// Numer Id klienta aplikacji w formacie IdProjektu-WegenerowanyNumerAplikacji
        /// </summary>
        [JsonProperty("client_id")]
        public string ClientId { get; set; }

        /// <summary>
        /// Adres serwera wydającego certyfikaty.
        /// </summary>
        [JsonProperty("auth_provider_x509_cert_url")]
        public string AuthProviderX509CertUrl { get; set; }

        /// <summary>
        /// Źródło wywołania Javascript'u.
        /// </summary>
        [JsonProperty("javascript_origins")]
        public List<string> JavascriptOrigins { get; set; }
    }
}
