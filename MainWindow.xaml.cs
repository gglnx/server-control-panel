using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ServerControlPanel
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Status of nginx
        public bool nginx = false;

        // Status of PHP
        public bool php = false;

        // Status of MongoDB
        public bool mongodb = false;

        // Status of Memcache
        public bool memcache = false;

        // Our background worker
        private BackgroundWorker worker;

        public MainWindow()
        {
            // Start application
            InitializeComponent();

            // Start background worker for server status
            worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = true;
            worker.DoWork += new DoWorkEventHandler(checkStatus);
            worker.RunWorkerAsync();
        }

        protected void checkStatus(object sender, DoWorkEventArgs e)
        {
            // Generate list of our processes
            List<string> approvedProcesses = new List<string>();
            approvedProcesses.Add("nginx");
            approvedProcesses.Add("php-cgi");
            approvedProcesses.Add("mongod");
            approvedProcesses.Add("memcached");

            // Run worker
            while (!worker.CancellationPending) {
                // Get running processes
                var processes = from p in System.Diagnostics.Process.GetProcesses()
                                where approvedProcesses.Contains(p.ProcessName)
                                select p.ProcessName;

                // Set nginx status
                if (nginx != processes.Contains("nginx"))
                {
                    nginx = processes.Contains("nginx");
                    StartNginx.Dispatcher.Invoke(setStatusNginx);
                }

                // Set PHP status
                if (php != processes.Contains("php-cgi"))
                {
                    php = processes.Contains("php-cgi");
                    StartPHP.Dispatcher.Invoke(setStatusPHP);
                }
                
                // Set MongoDB status
                if (mongodb != processes.Contains("mongod"))
                {
                    mongodb = processes.Contains("mongod");
                    StartMongoDB.Dispatcher.Invoke(setStatusMongoDB);
                }

                // Set Memcache status
                if (memcache != processes.Contains("memcached"))
                {
                    memcache = processes.Contains("memcached");
                    StartMemcache.Dispatcher.Invoke(setStatusMemcache);
                }

                // Enable stop all button if one of the processes is running
                if (nginx || php || mongodb || memcache)
                    StopAllButton.Dispatcher.Invoke(enableStopAllButton);

                // Disable stop all button if notthing is running
                if (!nginx && !php && !mongodb && !memcache)
                    StopAllButton.Dispatcher.Invoke(disableStopAllButton);

                // Disable start all button if anything is running
                if ( nginx && php && mongodb && memcache )
                    StartAllButton.Dispatcher.Invoke(disableStartAllButton);
            }
        }

        private void StopWorker(object sender, CancelEventArgs e)
        {
            // Stop worker on closing
            worker.CancelAsync();
        }

        protected void setStatusNginx() {
            // Enable/disable buttons
            StartNginx.IsEnabled = !nginx;
            StopNginx.IsEnabled = nginx;
            RestartNginx.IsEnabled = nginx;

            // Enable start all button
            StartAllButton.IsEnabled = true;

            // Set status image
            StatusNginx.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("/Server Control Panel;component/" + (nginx ? "running" : "stopped") + ".png", System.UriKind.Relative));
        }

        protected void setStatusPHP()
        {
            // Enable/disable buttons
            StartPHP.IsEnabled = !php;
            StopPHP.IsEnabled = php;
            RestartPHP.IsEnabled = php;

            // Enable start all button
            StartAllButton.IsEnabled = true;

            // Set status image
            StatusPHP.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("/Server Control Panel;component/" + (php ? "running" : "stopped") + ".png", System.UriKind.Relative));
        }

        protected void setStatusMongoDB()
        {
            // Enable/disable buttons
            StartMongoDB.IsEnabled = !mongodb;
            StopMongoDB.IsEnabled = mongodb;
            RestartMongoDB.IsEnabled = mongodb;

            // Enable start all button
            StartAllButton.IsEnabled = true;

            // Set status image
            StatusMongoDB.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("/Server Control Panel;component/" + (mongodb ? "running" : "stopped") + ".png", System.UriKind.Relative));
        }

        protected void setStatusMemcache()
        {
            // Enable/disable buttons
            StartMemcache.IsEnabled = !memcache;
            StopMemcache.IsEnabled = memcache;
            RestartMemcache.IsEnabled = memcache;

            // Enable start all button
            StartAllButton.IsEnabled = true;

            // Set status image
            StatusMemcache.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("/Server Control Panel;component/" + (memcache ? "running" : "stopped") + ".png", System.UriKind.Relative));
        }

        protected void enableStopAllButton() {
            // Enable stop all button
            StopAllButton.IsEnabled = true;
        }

        protected void disableStopAllButton()
        {
            // Disable stop all button
            StopAllButton.IsEnabled = false;
        }

        protected void disableStartAllButton()
        {
            // Disable start all button
            StartAllButton.IsEnabled = false;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            // Enable glass effect
            base.OnSourceInitialized(e);
            GlassHelper.ExtendGlassFrame(this, new Thickness(-1));
        }

        private void StartNginxAction(object sender, RoutedEventArgs e)
        {
            // Init a process
            Process CMD = new Process();
            
            // Set settings
            CMD.StartInfo.FileName = "C:\\server\\bin\\nginx\\nginx.exe";
            CMD.StartInfo.Arguments = "-p C:\\server\\ -c C:\\server\\bin\\nginx\\conf\\nginx.conf";
            CMD.StartInfo.UseShellExecute = false;
            CMD.StartInfo.CreateNoWindow = true;

            // Start process
            CMD.Start();
        }

        private void StartPHPAction(object sender, RoutedEventArgs e)
        {
            // Init a process
            Process CMD = new Process();

            // Set settings
            CMD.StartInfo.FileName = "C:\\server\\bin\\php\\php-cgi.exe";
            CMD.StartInfo.Arguments = "-b 127.0.0.1:9100 -c C:\\server\\bin\\php\\php.ini";
            CMD.StartInfo.UseShellExecute = false;
            CMD.StartInfo.CreateNoWindow = true;

            // Start process
            CMD.Start();
        }

        private void StartMongoDBAction(object sender, RoutedEventArgs e)
        {
            // Init a process
            Process CMD = new Process();

            // Set settings
            CMD.StartInfo.FileName = "C:\\server\\bin\\mongodb\\mongod.exe";
            CMD.StartInfo.Arguments = "--dbpath=C:\\server\\database";
            CMD.StartInfo.UseShellExecute = false;
            CMD.StartInfo.CreateNoWindow = true;

            // Start process
            CMD.Start();
        }

        private void StartMemcacheAction(object sender, RoutedEventArgs e)
        {
            // Init a process
            Process CMD = new Process();

            // Set settings
            CMD.StartInfo.FileName = "C:\\server\\bin\\memcached\\memcached.exe";
            CMD.StartInfo.Arguments = "-p 11211 -U 0 -t 2 -c 2048 -m 2048";
            CMD.StartInfo.UseShellExecute = false;
            CMD.StartInfo.CreateNoWindow = true;

            // Start process
            CMD.Start();
        }

        private void StopNginxAction(object sender, RoutedEventArgs e)
        {
            // Get nginx processes
            Process[] aProc = Process.GetProcessesByName("nginx");

            // Kill processes
            for (int i = 0; i < aProc.Length; i++)
            {
                aProc[i].Kill();
            }
        }

        private void StopPHPAction(object sender, RoutedEventArgs e)
        {
            // Get PHP processes
            Process[] aProc = Process.GetProcessesByName("php-cgi");

            // Kill processes
            for (int i = 0; i < aProc.Length; i++)
            {
                aProc[i].Kill();
            }
        }

        private void StopMongoDBAction(object sender, RoutedEventArgs e)
        {
            // Get MongoDB processes
            Process[] aProc = Process.GetProcessesByName("mongod");

            // Kill processes
            for (int i = 0; i < aProc.Length; i++)
            {
                aProc[i].Kill();
            }
        }

        private void StopMemcacheAction(object sender, RoutedEventArgs e)
        {
            // Get Memcache processes
            Process[] aProc = Process.GetProcessesByName("memcached");

            // Kill processes
            for (int i = 0; i < aProc.Length; i++)
            {
                aProc[i].Kill();
            }
        }

        private void RestartNginxAction(object sender, RoutedEventArgs e)
        {
            // Disable button
            RestartNginx.IsEnabled = false;
            
            // Stop nginx
            StopNginx.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

            // Start nginx
            StartNginx.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

            // Enable button
            RestartNginx.IsEnabled = true;
        }

        private void RestartPHPAction(object sender, RoutedEventArgs e)
        {
            // Disable button
            RestartPHP.IsEnabled = false;
            
            // Stop PHP
            StopPHP.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

            // Start PHP
            StartPHP.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

            // Enable button
            RestartPHP.IsEnabled = true;
        }

        private void RestartMongoDBAction(object sender, RoutedEventArgs e)
        {
            // Disable button
            RestartMongoDB.IsEnabled = false;
            
            // Stop MongoDB
            StopMongoDB.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

            // Start MongoDB
            StartMongoDB.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

            // Enable button
            RestartMongoDB.IsEnabled = true;
        }

        private void RestartMemcacheAction(object sender, RoutedEventArgs e)
        {
            // Disable button
            RestartMemcache.IsEnabled = false;

            // Stop Memcache
            StopMemcache.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

            // Start Memcache
            StartMemcache.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

            // Enable button
            RestartMemcache.IsEnabled = true;
        }

        private void StartAll(object sender, RoutedEventArgs e)
        {
            // Start nginx
            if (!nginx)
                StartNginx.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

            // Start PHP
            if (!php)
                StartPHP.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

            // Start MongoDB
            if (!mongodb)
                StartMongoDB.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

            // Start memcache
            if (!memcache)
                StartMemcache.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

            // Disable start all button, enable stop all button
            StartAllButton.IsEnabled = false;
            StopAllButton.IsEnabled = true;
        }

        private void StopAll(object sender, RoutedEventArgs e)
        {
            // Stop nginx
            if (nginx)
                StopNginx.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

            // Stop PHP
            if (php)
                StopPHP.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

            // Stop MongoDB
            if (mongodb)
                StopMongoDB.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

            // Stop memcache
            if (memcache)
                StopMemcache.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

            // Disable start all button, enable stop all button
            StartAllButton.IsEnabled = true;
            StopAllButton.IsEnabled = false;
        }
    }
}
