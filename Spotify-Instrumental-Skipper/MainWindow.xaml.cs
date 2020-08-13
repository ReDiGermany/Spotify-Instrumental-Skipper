using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Threading;
/**
 * This tool was developed by ReDiGermany.
 * I (ReDi) was annoyed by rap music artists uploading bunch of instrumentals and live versions of songs.
 * So i developped this. Not the best, not the most fancy stuff. But it does what i need.
 * Use it, extend it, do whatever u want to with it.
 * 
 * Using following online ressources:
 * https://stackoverflow.com/questions/17887211/c-sharp-get-process-window-titles
 * https://stackoverflow.com/questions/1192335/automatic-vertical-scroll-bar-in-wpf-textblock
 * https://stackoverflow.com/questions/15013582/send-multimedia-commands
 * https://www.wpf-tutorial.com/misc/dispatchertimer/
 * https://stackoverflow.com/questions/17054824/correct-way-to-set-scrollviewer-for-vertical-scrolling-on-a-wpf-frame
 * 
 * */
namespace Spotify_Instrumental_Skipper
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer_Tick;
            timer.Start();

        }
        /**
         * Default timer Tick function redirecting scan function
         * */
        void timer_Tick(object sender, EventArgs e)
        {
            scan();
        }

        /* loading some hotkey stuff to skip */
        [DllImport("user32.dll", SetLastError = true)]
        public static extern void keybd_event(byte virtualKey, byte scanCode, uint flags, IntPtr extraInfo);
        public const int VK_MEDIA_NEXT_TRACK = 0xB0;
        public const int KEYEVENTF_EXTENDEDKEY = 0x0001; //Key down flag
        public const int KEYEVENTF_KEYUP = 0x0002; //Key up flag

        /**
         * Sends "NEXT TRACK" key, adds the current track to log and scrolls to bottom of the log
         */
        private void skip(string reason,string title)
        {
            _log.Text += "[" + reason.ToUpper() + "] " + title + "\n";
            _scroll.ScrollToBottom();
            keybd_event(VK_MEDIA_NEXT_TRACK, 0, KEYEVENTF_EXTENDEDKEY, IntPtr.Zero);
            keybd_event(VK_MEDIA_NEXT_TRACK, 0, KEYEVENTF_KEYUP, IntPtr.Zero);
        }
        private string lastTitle = "";
        /**
         * Basic scan function. Searches all tasks for Spotify,
         *  if running: checks if the title is different
         *      if so: checks if "instrumental" or "live" is in title
         *          if so: send skip()
         *  else updates the label
         */
        public void scan()
        {
            Process[] processes = Process.GetProcesses();
            var foundSpotify = false;
            var spotifyPlaying = false;
            foreach (var process in processes)
            {
                //Console.WriteLine(process.ProcessName);
                if(process.ProcessName == "Spotify")
                {
                    foundSpotify = true;
                    if(process.MainWindowTitle != "")
                    {
                        spotifyPlaying = true;
                        if (process.MainWindowTitle!=lastTitle)
                        {
                            if (process.MainWindowTitle.Equals("Spotify Premium"))
                            {
                                spotifyPlaying = false;
                            }
                            else if(process.MainWindowTitle.Equals("Spotify"))
                            {
                                spotifyPlaying = false;
                            }else if (process.MainWindowTitle.Contains("Instrumental"))
                            {
                                skip("instrumental", process.MainWindowTitle);
                            }
                            else if (process.MainWindowTitle.Contains("Live"))
                            {
                                skip("live", process.MainWindowTitle);
                            }
                            else
                            {
                                lastTitle = process.MainWindowTitle;
                                _status.Content = lastTitle;
                            }
                        }
                    }
                }
            }
            if (!foundSpotify) _status.Content = "Spotify not found.";
            if (!spotifyPlaying) _status.Content = "Spotify not playing";
        }
    }
}
