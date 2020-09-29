
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace RedisClient
{
    public delegate void MessageEventHandler(GlobalEventArgs arg);

    public static class RedisDataAccess
    {
        public static event MessageEventHandler CallBackEvent;

        private static ConnectionMultiplexer _redis;
        public static IDatabase RedisDb { get; private set; }
        public static string URL { get; set; }
        public static int Port { get; set; }
        private static IServer _redisServer;


        public static async Task<bool> isRedisConnected()
        {
            
            return await Task.Run(() => {
                if(!CheckUrl(URL))
                {
                    MessageBox.Show($"Your URL \n\"{URL}\"\n is NOT valid");
                    
                    return false;
                }

                bool isConnected = false;
                try
                {
                    _redis = ConnectionMultiplexer.Connect(URL);
                    if (_redis != null)
                        isConnected = true;
                }
                catch(RedisConnectionException)
                {
                    isConnected = false;
                }
                return isConnected;
            });
        }

        public async static Task<bool> TryToConnect(string pathToRedisServer)
        {
            bool isConnected = false;
            isConnected = await Task.Run(() => {
                if (!CheckUrl(URL))
                {
                    MessageBox.Show($"Your URL \n\"{URL}\"\n is NOT valid");

                    return false;
                }



                _redis = GetConnectionMultiplexer(pathToRedisServer, URL);
                if (_redis != null)
                {
                    RedisDb = _redis.GetDatabase();
                    _redisServer = _redis.GetServer(URL, Port);
                    if (_redis != null && RedisDb != null && _redisServer != null)
                        isConnected = true;
                }
                return isConnected;
            });
            return isConnected;
        }


        private static ConnectionMultiplexer GetConnectionMultiplexer(string pathToRedisServer, string connectionURL)
        {
            ConnectionMultiplexer multiplexer = null;
            var options = ConfigurationOptions.Parse(connectionURL);
            options.ConnectRetry = 5;
            options.AllowAdmin = true;
            try
            {
                multiplexer = ConnectionMultiplexer.Connect(options);
            }
            catch(RedisConnectionException)
            {
                StartProcess(pathToRedisServer, "");
                multiplexer = ConnectionMultiplexer.Connect(options);

            }
            return multiplexer;
        }

        public async static IAsyncEnumerable<RedisKey> GetKeys(string pathToRedisServer)
        {
            if (_redisServer == null)
                await TryToConnect(pathToRedisServer);

            await foreach (RedisKey s in _redisServer.KeysAsync())
            {
                yield return s;
            }
                
        }

        /// <summary>
        /// If you want to start process without any parameters, simply
        /// pass an empty string as "processParameters"
        /// </summary>
        /// <param name="pathToRedisServer"></param>
        /// <param name="processParameters"></param>
        private static void StartProcess(string pathToRedisServer, string processParameters)
        {
            if(processParameters != string.Empty)
                processParameters = " " + processParameters;
            ProcessStartInfo psi = new ProcessStartInfo(pathToRedisServer + processParameters);
            psi.UseShellExecute = false;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            psi.CreateNoWindow = true;
            Process process = Process.Start(psi);
           

            Thread.Sleep(100);

        }

        private async static Task CheckRedis(string message)
        {
            Task<bool> isRedisConnectedTask = isRedisConnected();
            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
            timer.Interval = 100;
            timer.Tick += (object sender, EventArgs e) =>
            {
                CallBackEvent?.Invoke(new GlobalEventArgs { Message = ".", clearDisplay = false });
            };
            timer.Start();
            bool is_RedisConnected = await isRedisConnectedTask;
            timer.Stop();
            if (!is_RedisConnected)
                CallBackEvent?.Invoke(new GlobalEventArgs { Message = message, clearDisplay = true });
        }

        public static async void KillRedis(string pathToRedisServer)
        {
            Process prs = Process.GetProcesses().Where(_ => _.ProcessName.Equals("redis-server")).FirstOrDefault();
            if (prs != null)
            {
                prs.Kill();
                await CheckRedis("Redis was killed");
            }
            else
            {
                CallBackEvent?.Invoke(new GlobalEventArgs { Message = "The process \"redis-server\" was null.\n", clearDisplay = true }); ;
                await CheckRedis("Redis wasn't running in the first place");
            }
        }








        /// <summary>
        /// Currentrly I have no idea how to check remote redis server URL validity
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private static bool CheckUrl(string url)
        {
            //Currentrly I have no idea how to check remote redis server URL validity
            return true;
        }


    }
}
