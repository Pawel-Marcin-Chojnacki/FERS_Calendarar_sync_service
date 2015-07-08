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
        /// Ścieżka do obecnego folderu z logami usługi.
        /// </summary>
        private static string CurrentLogDirectoryPath { get; set; }

        private static string GetCurrentLogDirectoryPath()
        {
            var logFolder = DateTime.Now.Year + "\\" + DateTime.Now.Month;
            var directoryPath = Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "\\" + logFolder);
            return directoryPath.FullName;
        }

        /// <summary>
        /// Zapisuje informacje o wyjątku do pliku w formacie "Rok\Miesiąc\Log yy-MM-dd.txt"
        /// </summary>
        /// <param name="ex">Przechowuje szczegóły wyjątku, który wystąpił w trakcie działania usługi.
        /// Informacje z tego parametru zapisane są do dziennika zdarzeń usługi.</param>
        public static void WriteErrorLog(Exception ex)
        {
            StreamWriter streamWriter = StreamWriter.Null;
            CurrentLogDirectoryPath = GetCurrentLogDirectoryPath();
            try
            {
                streamWriter = new StreamWriter(CurrentLogDirectoryPath + "\\Log" + DateTime.Now.ToString("yy-MM-dd") + ".txt",
                    true);
                streamWriter.WriteLine(DateTime.Now + ": " + ex.Source.Trim() + "; " + ex.Message.Trim());

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
        /// Zapisuje wiadomość do pliku dziennika zdarzeń usługi w formacie "Rok\Miesiąc\Log yy-MM-dd.txt". 
        /// </summary>
        /// <param name="message">Wiadomość do zapisania w dzienniku zdarzeń.</param>
        public static void WriteErrorLog(string message)
        {
            StreamWriter streamWriter = StreamWriter.Null;
            CurrentLogDirectoryPath = GetCurrentLogDirectoryPath();
            try
            {
                streamWriter = new StreamWriter(CurrentLogDirectoryPath + "\\Log" + DateTime.Now.ToString("yy-MM-dd") + ".txt",
                    true);
                streamWriter.WriteLine(DateTime.Now + ": " + message);

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
    }
}
