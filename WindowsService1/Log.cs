using System;
using System.IO;

namespace CalendarSyncService
{
    /// <summary>
    /// Klasa odpowiedzialna za zapisywanie wszelkich wydarzeń związanych z działaniem usługi.
    /// </summary>
    public static class Logs
    {
        /// <summary>
        /// Zapisuje informacje o wyjątku do pliku w formacie "Rok\Miesiąc\Log yy-MM-dd.txt"
        /// </summary>
        /// <param name="ex">Przechowuje szczegóły wyjątku, który wystąpił w trakcie działania usługi.
        /// Informacje z tego parametru zapisane są do dziennika zdarzeń usługi.</param>
        public static void WriteErrorLog(Exception ex)
        {
            try
            {
                //string loggingFolder = DateTime.Now.Year.ToString() + "\\" + DateTime.Now.Month.ToString();
                var sw = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\Log" + DateTime.Now.ToString("yy-MM-dd") + ".txt", true);
                sw.WriteLine(DateTime.Now + ": " + ex.Source.Trim() + "; " + ex.Message.Trim());
                sw.Flush();
                sw.Close();
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Zapisuje wiadomość do pliku dziennika zdarzeń usługi w formacie "Rok\Miesiąc\Log yy-MM-dd.txt". 
        /// </summary>
        /// <param name="message">Wiadomość do zapisania w dzienniku zdarzeń.</param>
        public static void WriteErrorLog(string message)
        {
            try
            {
                var sw = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\Log" + DateTime.Now.ToString("yy-MM-dd") + ".txt", true);
                sw.WriteLine(DateTime.Now + ": " + message);
                sw.Flush();
                sw.Close();
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
