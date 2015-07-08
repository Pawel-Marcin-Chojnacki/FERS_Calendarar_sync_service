namespace CalendarSyncService
{
    /// <summary>
    /// Klasa odpowiedzialna za przechowanie danych o użytkowniku.
    /// </summary>
    class User
    {
        /// <summary>
        /// Email użytkownika. Służy jako jego identyfikator oraz nazwa jego kalendarza.
        /// </summary>
        public string UserEmail { get; set; }

        /// <summary>
        /// Id użytkownika z bazy.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Kalendarz użytkownika.
        /// </summary>
        public Calendar Calendar { get; set; }

        /// <summary>
        /// Pole przechowujące token użytkownika zautoryzowanego w kaneldarzu Google.
        /// </summary>
        public string GoogleCalendarToken { get; set; }

        /// <summary>
        /// Pozwala szybko sprawdzić czy użytkownik zezwolił na dostęp do kalendarza.
        /// </summary>
        public bool IsAuthorized { get; set; }

        /// <summary>
        /// Konkstruktor. Tworzy kalendarz użytkownika, pozwalając na jego autoryzację.
        /// </summary>
        /// <param name="userName">Nazwa użytkownika.</param>
        /// <param name="uId">Id użytkownika.</param>
        public User(string userName, int uId)
        {
            UserEmail = userName;
            Logs.WriteErrorLog("Konstruktor USER, pole UserId: " + UserId.ToString());
            UserId = uId;
            Calendar = new Calendar(UserEmail, uId);
        }


    }
}
