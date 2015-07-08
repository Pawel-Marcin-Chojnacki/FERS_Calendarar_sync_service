using DTO;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Requests;
using Google.Apis.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CalendarSyncService
{
    /// <summary>
    /// Klasa przechowuje informacje o kalendarzu, 
    /// takie jak klucz autoryzacyjny, nawza głównego kalendarza.
    /// </summary>
    class Calendar
    {
        /// <summary>
        /// Konstruktor z nadaniem identyfikatora kalendarzowi.
        /// </summary>
        /// <param name="userEmail">Email użytkownika. Jest to również nazwa głównego kalendarza.</param>
        /// <param name="userId"></param>
        public Calendar(string userEmail, int userId)
        {
            IsCalendarAuthorized = false;
            CalendarId = userEmail;
            Request = new BatchRequest(Service);
            EventsQueue = new Queue<Event>();
            Logs.WriteErrorLog("Konstruktor calendar userid: " + userId.ToString());
            UserIdInDatabase = userId;
        }

        /// <summary>
        /// Numer identyfikacyjny właściciela kalendarza.
        /// </summary>
        private int UserIdInDatabase { get; set; }

        /// <summary>
        /// Kolejka zdarzeń dla niezautoryzowanych użytkowników.
        /// </summary>
        public Queue<Event> EventsQueue;

        /// <summary>
        /// Przechowuje nazwę głównego kalendarza (email użytkownika).
        /// </summary>
        private static string CalendarId { get; set; }

        /// <summary>
        /// Pozwala zarządzać API kalendarza Google.
        /// </summary>
        public CalendarService Service { get; set; }

        /// <summary>
        /// Pole do przechowania kolejki zdarzeń.
        /// </summary>
        private BatchRequest Request;

        /// <summary>
        /// Utrzymuje id klienta oraz jego sekret.
        /// Pole odpowiedzialne za przepływ informacji związanych z autoryzacją konta Google.
        /// </summary>
        private GoogleAuthorizationCodeFlow Flow { get; set; }

        /// <summary>
        /// Przetrzymuje klucz atoryzacyjny użytkownika.
        /// </summary>
        private TokenResponse Token { get; set; }

        /// <summary>
        /// Numer identyfikacyjny klienta (aplikacji) wczytywany z pliku "client_secrets.json".
        /// </summary>
        private string ClientId { get; set; }

        /// <summary>
        /// Sekret klienta (aplikacji) wczytywany z pliku "client_secrets.json".
        /// </summary>
        private string ClientSecret { get; set; }

        /// <summary>
        /// Trzyma wszystkie dane autoryzacyjne użytkownika kalendarza Google.
        /// </summary>
        public UserCredential Credential;

        /// <summary>
        /// Metoda wypełniająca wszystkie niezbędne dane do autoryzacji w usłudze Google Calendar.
        /// </summary>
        private bool SetUserCalendarAuthData()
        {
            Logs.WriteErrorLog("SetUserCalendarAuthData");
            Logs.WriteErrorLog("ClientCredentials.ClientId: " + ClientCredentials.ClientId.ToString() + "ClientCredentials.ClientSecret: " + ClientCredentials.ClientSecret);
            Flow = new GoogleAuthorizationCodeFlow(
                new GoogleAuthorizationCodeFlow.Initializer
                {
                    ClientSecrets = new ClientSecrets()
                    {
                        ClientId = ClientCredentials.ClientId,
                        ClientSecret = ClientCredentials.ClientSecret
                    }
                });
            Logs.WriteErrorLog("UserIdInDatabase: ");
            Logs.WriteErrorLog(UserIdInDatabase.ToString());
            var tokenFromWs = GetTokenFromWebService.GetTokenFromDatabase(UserIdInDatabase).Result;
            if (tokenFromWs == null)
            {
                IsCalendarAuthorized = false;
                return false;
            }
            Token = new TokenResponse
            {
                RefreshToken = tokenFromWs
            };
            IsCalendarAuthorized = true;
            return true;
        }

        /// <summary>
        /// Pole przechowujące stan kalendarza.
        /// Jeśli został zautoryzowany poprawnie przechowuje wartość True.
        /// </summary>
        public bool IsCalendarAuthorized { get; set; }

        /// <summary>
        /// Metoda asynchroniczna, która uzyskuje dostep do kalendarza (użytkownika).
        /// Następnie zachowuje te dane do pliku "user_email"
        /// </summary>
        /// <returns></returns>
        public void AuthorizeTask()
        {
            Logs.WriteErrorLog("Autoryzuje uzytkownika");
            var gToken = GetTokenFromWebService.GetTokenFromDatabase(UserIdInDatabase).Result;
            if (gToken == null)
            {
                Logs.WriteErrorLog("Uzytkownik bez Google Tokenu. Wychodze z autoryzacji i lece dalej");
                return;
            }
            Logs.WriteErrorLog(gToken + " to jest token.");
            //Logs.WriteErrorLog("Token z bazy: " + Token.RefreshToken);
            SetUserCalendarAuthData();
            //Autoryzacja kalendarza przez wywołanie klienta połączenia HTTP. 
            Logs.WriteErrorLog("Po SetUserCalendarAuthData");
            Logs.WriteErrorLog("flow: " + Flow.RevokeTokenUrl.ToString());
            Logs.WriteErrorLog("CalendarId: " + CalendarId);
            //if (Token.AccessToken != null) Logs.WriteErrorLog("Token: " + Token.AccessToken);

            Credential = new UserCredential(Flow, CalendarId, Token);
            Logs.WriteErrorLog("Jestem za przypisaniem Credential");
            
            Service = new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = Credential,
                ApplicationName = "Fast and Easy Reservation System",
            });
            Logs.WriteErrorLog("Stworzylem Service.");
        }

        /// <summary>
        /// Metoda tworząca listę wydarzeń do kalendarza.
        /// </summary>
        /// <param name="booking">Pojedyncza rezerwacja z listą gości.</param>
        public void MakeEventRequest(SimpleBookingUser booking)
        {
            Logs.WriteErrorLog("Robie request eventa (wrzucam zdarzenia na kolejke).");
            Logs.WriteErrorLog(booking.BookingEndTime.ToString("g"));
            Logs.WriteErrorLog(booking.BookingBeginTime.ToString("g"));
            var eventStartDateTime = new EventDateTime { DateTime = booking.BookingBeginTime };
            Logs.WriteErrorLog("Moja data rozp: " + eventStartDateTime.DateTime.ToString());
            var eventEndDateTime = new EventDateTime { DateTime = booking.BookingEndTime };
            Logs.WriteErrorLog("Moja data zakonczenia: " + eventEndDateTime.DateTime.ToString());
            Request = new BatchRequest(Service);
            Logs.WriteErrorLog("tytul: " + booking.BookingTitle);
            Logs.WriteErrorLog("Opis" + booking.BookingDescription);
            Request.Queue<Event>(Service.Events.Insert(
                new Event
                {
                    Summary = booking.BookingTitle,
                    Description = booking.BookingDescription,
                    Start = eventStartDateTime,
                    End = eventEndDateTime
                }, CalendarId),
                (content, error, i, message) =>
                {
                    //Do wypełnienia "Callback"
                });
            Logs.WriteErrorLog("Wrzucilem.");
        }

        /// <summary>
        /// Metoda wrzuca do kolejki żądań zdarzenia z kolejki "bezautoryzacyjnej".
        /// </summary>
        public async Task PushEventsFromQueue()
        {
            foreach (var e in EventsQueue)
            {
                Request.Queue<Event>(Service.Events.Insert(e, CalendarId),
                (content, error, i, message) =>
                {
                    //Do wypełnienia "Callback"
                });
            }
            await SendEventsRequest();

            EventsQueue.Clear();
        }

        /// <summary>
        /// Metoda wysyła wszystkie zdarzenia do kalendarza jako jeden request. 
        /// Wykonywana po zaktualizowaniu wszystkich zdarzeń z bazy danych.
        /// </summary>
        public async Task SendEventsRequest()
        {
            Logs.WriteErrorLog("SendEventsRequest " +Request.Count.ToString());
            
            await Request.ExecuteAsync();
            Request = new BatchRequest(Service);
        }
    }
}