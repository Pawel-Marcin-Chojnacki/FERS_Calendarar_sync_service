
using System.Collections.Generic;

namespace CalendarSyncService
{
    public class ClientSecret
    {
        public string auth_uri { get; set; }
        public string client_secret { get; set; }
        public string token_uri { get; set; }
        public string client_email { get; set; }
        public string redirect_uris { get; set; }
        public string client_x509_cert_url { get; set; }
        public string client_id { get; set; }
        public string auth_provider_x509_cert_url { get; set; }
        public IList<string> javascript_origins { get; set; }
    }
}
