using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace CalendarSyncService
{
    /// <summary>
    /// Klasa obsługująca połączenia z WebService'm.
    /// </summary>
    public static class WebServiceConnection
    {
        /// <summary>
        /// Adres serwera na którym znajduje się WebService
        /// </summary>
        private static readonly Uri Address = new Uri("http://192.168.132.28:9999/");
        //private static readonly Uri Address = new Uri("http://localhost:9999/");

        /// <summary>
        /// Metoda pozwalajaca na wyslanie zadania GET do webservicu
        /// </summary>
        /// <typeparam name="T">Typ obiektu odbieranego</typeparam>
        /// <param name="route">Sciezka kontrolera i metody</param>
        /// <returns>Odpowiedz od serwera typu T</returns>
        public static async Task<T> SendGet<T>(string route)
        {
            var client = new HttpClient()
            {
                BaseAddress = Address
            };
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpResponseMessage response = null;
            try
            {
                response = await client.GetAsync(route);
            }
            catch (Exception)
            {
                throw;
            }

            if (response.IsSuccessStatusCode)
            {
                T data = await response.Content.ReadAsAsync<T>();
                client.Dispose();
                return data;
            }
            client.Dispose();
            return default(T);
        }

        /// <summary>
        /// Metoda pozwalajaca na wyslanie zadania POST do webservicu
        /// </summary>
        /// <typeparam name="T">Typ obiektu wysylanego</typeparam>
        /// <typeparam name="U">Typ obiektu odbieranego</typeparam>
        /// <param name="request">Obiekt przesylany do webservicu</param>
        /// <param name="route">Sciezka kontrolera i metody</param>
        /// <returns>Odpowiedz od serwera typu U</returns>
        public static async Task<U> SendPost<T, U>(T request, string route)
        {
            var client = new HttpClient()
            {
                BaseAddress = Address
            };
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpResponseMessage responseMessage = null;
            try
            {
                responseMessage = await client.PostAsJsonAsync(route, request);
            }
            catch (Exception)
            {
                throw;
            }
            if (responseMessage.IsSuccessStatusCode)
            {
                U responseObject = await responseMessage.Content.ReadAsAsync<U>();
                client.Dispose();
                return responseObject;
            }
            client.Dispose();
            return default(U);
        }
    }
}
