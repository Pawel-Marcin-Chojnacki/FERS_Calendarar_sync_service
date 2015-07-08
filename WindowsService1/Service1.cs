using DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace CalendarSyncService
{
    /// <summary>
    /// Klasa zapewniająca działanie programu w formie usługi systemu Windows.
    /// </summary>
    public partial class CalendarSyncService : ServiceBase
    {
        /// <summary>
        /// Pole trzymające aktualny czas.
        /// </summary>
        private DateTime SyncCurrentTime { get; set; }

        /// <summary>
        /// Pole przechowujące id oraz token dla autoryzacji w WebServisie.
        /// </summary>
        private LoginResponse LoginResponse { get; set; }

        /// <summary>
        /// Przechowuje powiązania między id użytkownika oraz adresem email (nazwą kalendarza).
        /// </summary>
        private List<User> _usersList = new List<User>();

        /// <summary>
        /// 
        /// </summary>
        private GetBookingsCreatedBetweenDatesRequest _bookingsRequest;

        /// <summary>
        /// Pole informujące czy użytkownik został zalogowany w serwisie webowym
        /// </summary>
        private bool LoggedInWebService { get; set; }

        /// <summary>
        /// Inicjalizacja komponentu usługi oraz nastawienie aktualnego czasu.
        /// </summary>
        public CalendarSyncService()
        {
            InitializeComponent();
            SyncCurrentTime = new DateTime();
            SyncCurrentTime = DateTime.Now;
            LoggedInWebService = false;
        }

        /// <summary>
        /// Ustawienie czasomierza na 10 minut (cyklicznie).
        /// Dodaje zdarzenie OnTimer wywołane co interwał.
        /// </summary>
        /// <param name="args"></param>
        protected override async void OnStart(string[] args)
        {
    //        System.Diagnostics.Debugger.Launch();

            ClientCredentials.LoadClietSecretsFromFile();
            ClientCredentials.ClientId = "845629958395-ucf3ultlgni49ahidshsovaj8io69enu.apps.googleusercontent.com";
            ClientCredentials.ClientSecret = "liuG9cQM_01RNoLRABk2nXHC";
            Logs.WriteErrorLog("Usluga uruchamia sie.");
            await LoginToWebService();
            Logs.WriteErrorLog("Czekam na zalogowanie do serwisu.");

            SetTimers();
            Logs.WriteErrorLog("Ustawilem timery.");

            //pawel.Calendar.AuthorizeTask();G
            //Logs.WriteErrorLog("jestem sobue usser trolololo");
            //var t = pawel.Calendar.LoadClietSecretsFromFile();
        }

        /// <summary>
        /// Metoda logowania do WebServisu wywoływana na starcie 
        /// </summary>
        private async Task LoginToWebService()
        {
            var time = DateTime.Today.ToShortDateString();
            var tokenRequest = new UpdateTokenRequest()
            {
                SenderId = LoginToWebServiceData.WebServiceId,
               // Token = time
                Token = "6z6d3YD4UT7faV3nZk6kp4hOG1KDsaNLTDyfRK3NZim2JR39caRiwGDbsrGcn9MTgwvHxYFprsugRpeZ7KBrtEon9wWzd0z0GCWn"
               // Token = LoginToWebServiceData.ReadLastWebServiceToken()
            };
            var tokenResponse = await WebServiceConnection.SendPost<UpdateTokenRequest, UpdateTokenResponse>(tokenRequest, "token/update");
            LoginToWebServiceData.WebServiceToken = tokenResponse.Token;
            if (tokenResponse != null)
            {
                if (tokenResponse.Status == 0)
                { 
                    Logs.WriteErrorLog("Zalogowano do WS z " + tokenResponse.Token);
                    LoginToWebServiceData.WriteLastTokenToFile(tokenResponse.Token);
                }
                Logs.WriteErrorLog(tokenResponse.Status.ToString());
            }
        }


        /// <summary>
        /// Ustawia zegary, dodaje wyzwalacze interwałowe.
        /// </summary>
        private void SetTimers()
        {
            // Zegar odpowiedzialny za synchronizację nowych rezerwacji w bazie danych.
            var timerSyncCalendars = new Timer { Interval =  10 * 1000 };
            timerSyncCalendars.Elapsed += OnTimerSync_Elapsed;
            timerSyncCalendars.Start();

            // Zegar odpowiedzialny za aktualizację wydarzeń dla użytkowników, 
            // którzy zautoryzowali swoje konto Google posiadając wcześniej wydarzenia w bazie.
            var timerUserMaintenance = new Timer { Interval = 30 * 60 * 1000 };
            timerUserMaintenance.Elapsed += OnTimerUserMaintenance_Elapsed;
            timerUserMaintenance.Start();

            // Zegar odpowiedzialny za zadania wykonywane raz dziennie:
            // Archiwizacja starych wpisów z bazy danych.
            // Sprawdzanie sekretu klienta (czy plik się nie zmienił).
            var timerArchiveEvents = new Timer { Interval = 24 * 60 * 60 * 1000 };
            timerArchiveEvents.Elapsed +=timerArchiveEvents_Elapsed;
            timerArchiveEvents.Start();

            // Zegar odpowiedzialny za wygenerowanie nowego tokenu z WebService'u.
            // Wymienia co godzinę token na nowy aby zapewnić wyższy poziom bezpieczeństwa.
            var timerGenerateNewTokenForWebService = new Timer { Interval = 60*60*1000 };
            timerGenerateNewTokenForWebService.Elapsed += timerGenerateNewTokenForWebService_Elapsed;
            timerGenerateNewTokenForWebService.Start();
        }

        /// <summary>
        /// Metoda generująca nowy token z WebService'u.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timerGenerateNewTokenForWebService_Elapsed(object sender, ElapsedEventArgs e)
        {
            LoginToWebServiceData.WebServiceToken = LoginToWebServiceData.RefreshToken();
            LoginToWebServiceData.WriteLastTokenToFile(LoginToWebServiceData.WebServiceToken);
            Logs.WriteErrorLog("Nowy token do web serwisu: " + LoginToWebServiceData.WebServiceToken);
        }

        /// <summary>
        /// Zadania wykonywane raz dziennie.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timerArchiveEvents_Elapsed(object sender, ElapsedEventArgs e)
        {
            // Wyślij odbyte spotkania do archiwum.
            SendToArchive();
            // Sprawdź czy sekret klienta się zmienił.
            ClientCredentials.ValidateClientCredentials();
        }

        /// <summary>
        /// Metoda sprawdza czy przechowywany w klasie ClientCredentials sekret klienta jest wciąż aktualny.
        /// </summary>
        private void ValidateClientSecretFile()
        {

            throw new NotImplementedException();
        }

        /// <summary>
        /// Metoda wywołująca żądanie przeniesienia odbytych spotkań do archiwum.
        /// Wydarzenia przenoszone są do archiwum po 3 miesiącach od daty zakończenia.
        /// </summary>
        private async void SendToArchive()
        {
            var sendToArchiveRequest = new SendToArchiveRequest()
            {
                Months = 6,
                SenderId = LoginToWebServiceData.WebServiceId,
                Token = LoginToWebServiceData.WebServiceToken
            };

            var sendToArchiveResponse = await WebServiceConnection.
                SendPost<SendToArchiveRequest, SendToArchiveResponse>(sendToArchiveRequest, "send/to/archive");
            if (sendToArchiveResponse == null) return;
            if (sendToArchiveResponse.Status == 0)
            {
                Logs.WriteErrorLog("Wydarzenia z przed " + sendToArchiveRequest.Months + " miesiecy, zostały zarchiwizowane");
                return;
            }
            Logs.WriteErrorLog("Archiwizacja nie powiodla sie.");
        }
         
        /// <summary>
        /// Metoda synchronizuje kalendarze użytkowników, mających wydarzenia w kolejce.
        /// Zautoryzowanych od czasu otrzymania pierwszego wydarzenia im przypisanego.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnTimerUserMaintenance_Elapsed(object sender, ElapsedEventArgs e)
        {
            foreach (var user in _usersList.Where(user => user.Calendar.IsCalendarAuthorized))
                await user.Calendar.PushEventsFromQueue();
        }

        /// <summary>
        /// Metoda wywoływana jako wyzwalacz zdarzenia cyklicznego.
        /// Obecnie do wywołania synchronizacji kalendarzy użytkowników z bazą danych.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnTimerSync_Elapsed(object sender, ElapsedEventArgs e)
        {

            //var pawel = new User("pawel.marcin.chojnacki@gmail.com");
            //pawel.Calendar.AuthorizeTask();

            //Logs.WriteErrorLog(pawel.GoogleCalendarToken.ToString());

            //pawel.Calendar.Service.Events.QuickAdd("primary",
                //"Wydarzenie dodano po autoryzacji w bazie danych przez mvc");
            SyncCurrentTime = DateTime.Now;
            //DEBUG
//            var lastTime = new DateTime(2015, 2, 2);
//            var nowTime = new DateTime(2015, 3, 22);
//#pragma warning disable 4014
//            AskAboutNewEventsTask(lastTime, nowTime);
//#pragma warning restore 4014
            //RELEASE
            AskAboutNewEventsTask(SyncCurrentTime.AddHours(-4), SyncCurrentTime);

        }


        /// <summary>
        /// Metoda odpytująca WebService o nowe zdarzenia w przedziale czasowym określonym w jako argumenty metody.
        /// </summary>
        /// <param name="timeBefore">Początek przedziału czasowego.</param>
        /// <param name="timeNow">Koniec przedziału czasowego.</param>
        /// <returns></returns>
        private async void AskAboutNewEventsTask(DateTime timeBefore, DateTime timeNow)
        {
            // Obiekt metody WebServisu pozwalająca zainicjalizować żądanie pobrania rezerwacji WYKONANYMI między określonym czasem.
            _bookingsRequest = new GetBookingsCreatedBetweenDatesRequest()
            {
                BeginTime = timeBefore,
                EndTime = timeNow,
                SenderId = LoginToWebServiceData.WebServiceId,
                Token = LoginToWebServiceData.WebServiceToken
            };

            // Obiekt metody WebServisu przyjmujący odpowiedź na zapytanie o rezerwację sal, 
            // wykonane w podanym przedziale czasowym.
            var bookingsResponse = await WebServiceConnection.
                SendPost<GetBookingsCreatedBetweenDatesRequest, GetBookingsCreatedBetweenDatesResponse>(_bookingsRequest, "booking/dates");
            if (bookingsResponse != null)
            {
                if (bookingsResponse.Status == 0)
                {
                    foreach (var booking in bookingsResponse.BookingsList)
                    {
                        Logs.WriteErrorLog("Ilosc rezerwacji w booking: " + booking.GuestList.Count.ToString());
                        foreach (var guest in booking.GuestList)
                        {
                            var userInfo =
                            await WebServiceConnection.SendGet<GetUserInfoResponse>("user/" + guest.UserId + "/info");
                            if (userInfo == null) Logs.WriteErrorLog("Nie odzyskalem odpowiedzi od WS");
                            if (userInfo.Email.EndsWith("@gmail.com") == false) continue;
                            Logs.WriteErrorLog("guest.BookingTitle: " + guest.BookingTitle);
                            Logs.WriteErrorLog("guest.userId: " + guest.UserId.ToString());
                            
                            var userItem = _usersList.FirstOrDefault(u => u.UserId == guest.UserId);

                            if (userItem == null)
                            {
                                Logs.WriteErrorLog("Uzytkownika nie ma w kolejce, ani na liscie. Dodaje go...");
                                await AddNewUser(guest);
                            }
                            Logs.WriteErrorLog("Dodalem uzytkownika, tutaj sie zacinam.");
                            userItem = _usersList.FirstOrDefault(u => u.UserId == guest.UserId);
                            AddNewEvent(guest, userItem);
                            await UpdateAllCalendars();
                        }
                        //Dodaj Kalendarz i wywołaj metodę booking dla kalendarza użytkownika.
                        //booking.GuestList[0].Confirmation
                        //Logs.WriteErrorLog(booking.Title + ": " + booking.Description);
                    }
                    await UpdateAllCalendars();
                }
                else
                {
                    Logs.WriteErrorLog(bookingsResponse.Status + ": " + bookingsResponse.Message);
                }
            }
            else
            {
                Logs.WriteErrorLog("Nie udalo sie otrzymac odpowiedzi od WebService'u");
            }
        }

        /// <summary>
        /// Metoda aktualizuje kalendarze wszystkich użytkowników z serwisami zewnętrznymi (Google Calendar).
        /// </summary>
        private async Task UpdateAllCalendars()
        {
            Logs.WriteErrorLog("Aktualizuje kalendarze uzytkownikow.");
            foreach (var user in _usersList)
            {
                Logs.WriteErrorLog("Jestem w " + user.UserEmail + " o id: " + user.UserId + "z Gtokenem: " + user.Calendar.Credential.Token.RefreshToken);
                await user.Calendar.SendEventsRequest();
                Logs.WriteErrorLog("Wyslalem jego request.");
            }
        }

        /// <summary>
        /// Metoda pobiera z WebServisu informacje o użytkowniku, którego nie ma w Usłudze.
        /// </summary>
        /// <param name="guest">Parametr z którego pobierane są informacje, którego użytkownika dodać.</param>
        private async Task AddNewUser(SimpleBookingUser guest)
        {
            Logs.WriteErrorLog("AddNewUser");
            Logs.WriteErrorLog("guest.UserId: " + guest.UserId.ToString() + ", Booking title: " + guest.BookingTitle);

            GetUserInfoResponse userInfo = null;
            userInfo = await WebServiceConnection.SendGet<GetUserInfoResponse>("user/" + guest.UserId + "/info");
            if(userInfo == null) Logs.WriteErrorLog("UserInfo zwrocilo null z WebSerwisu");
            if(userInfo.Email == null) Logs.WriteErrorLog("UserEmil zwrocil null");
            var userNew = new User(userInfo.Email, guest.UserId);
            //userNew.UserId = guest.UserId;
            Logs.WriteErrorLog("UserNew.UserEmail: " + userNew.UserEmail + ", UserNew.UserId" + userNew.UserId);
            //Autoryzuj użytkownika, zakładając że jego token istnieje.
            userNew.Calendar.AuthorizeTask();
            if (userNew.Calendar.IsCalendarAuthorized == false)
            {
                return;
            }
            Logs.WriteErrorLog("uzytkownik " + userNew.UserEmail + " zostal zautoryzowany.");
            userNew.Calendar.Service.Events.QuickAdd("primary", "Testujemy poprawnosc autoryzacji").ExecuteAsync();
            
            _usersList.Add(userNew);
            Logs.WriteErrorLog("Pierwszy uzytkownik w kolejce to: " + _usersList.First().UserEmail);
        }

        /// <summary>
        /// Metoda dodaje nowe zdarzenie do kalendarza użytkownika.
        /// Wydarzenie zostaje wrzucone na stos zdarzeń do aktualizacji.
        /// </summary>
        /// <param name="guest">Zaproszony gość z w bazie danych.</param>
        /// <param name="user">Użytkownik, któremu przypisuje się wydarzenie do kalendarza.</param>
        private static void AddNewEvent(SimpleBookingUser guest, User user)
        {
            Logs.WriteErrorLog("Dodaje nowe wydazenie uzytkownikowi " + user.UserEmail);
            if (user.Calendar.IsCalendarAuthorized)
            {
                Logs.WriteErrorLog("Zautoryzowany!");
                user.Calendar.MakeEventRequest(guest);
                Logs.WriteErrorLog("wykonalem MakeEventRequest.");
            }
            else
            {
                Logs.WriteErrorLog("Dodaje zdarzenie do kolejki, bo uzytkownik nie jest zautoryzowany.");
                AddEventToQueue(guest, user);
                Logs.WriteErrorLog("Dodalem zdarzenie do kolejki.");
            }
        }

        /// <summary> 
        /// Metoda dodaje wydarzenie do kolejki zdarzeń.
        /// </summary>
        /// <param name="guest">Rezerwacja, która zostanie dodana do kolejki.</param>
        /// <param name="user">Użytkownik, któremu zostanie zakolejkowane wydarzenie.</param>
        private static void AddEventToQueue(SimpleBookingUser guest, User user)
        {
            Logs.WriteErrorLog("AddEventToQueue");
            var eventStartDateTime = new Google.Apis.Calendar.v3.Data.EventDateTime { DateTime = guest.BookingBeginTime };
            var eventEndDateTime = new Google.Apis.Calendar.v3.Data.EventDateTime { DateTime = guest.BookingEndTime };

            var e = new Google.Apis.Calendar.v3.Data.Event()
            {
                Start = eventStartDateTime,
                End = eventEndDateTime,
                Description = guest.BookingDescription,
                Summary = guest.BookingTitle,
                Location = guest.BookingRoomName
            };
            user.Calendar.EventsQueue.Enqueue(e);
            Logs.WriteErrorLog("Koniec AddEventToQueue");
        }

        /// <summary> 
        /// Zatrzymuje usługę.
        /// </summary>
        protected override void OnStop()
        {
            Logs.WriteErrorLog("Usluga zostaje zatrzymana...");
        }
    }
}
