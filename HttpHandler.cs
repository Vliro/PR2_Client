using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using Newtonsoft.Json;

namespace PR2_Client
{
    class HttpHandler
    {

        private static HttpClient httpClient;
        private static Rijndael128Encryptor encryptor;
        private static string encryptionIV = "JmM5KnkqNXA9MVVOeC9Ucg==";
        private static String encryptionKEY = "VUovam5GKndSMHFSSy9kSA==";
        private static String loginCode = "eisjI1dHWG4vVTAtNjB0Xw";
        public static string Send(string user, string pass, int login_id, int server_id)
        {
            if (httpClient == null)
                InitHttpClient();

            var m_Data = new Dictionary<string, string>();

            string encrypted = encryptor.AES_encrypt(genLoginJson(user, pass, login_id, server_id), encryptionKEY, encryptionIV);
            pass = "";
            m_Data["i"] = encrypted;
            var rand = new Random();
            int randNum = (int)(rand.NextDouble() * 10000000);
            m_Data["rand"] = randNum.ToString();
            m_Data["version"] = "24-dec-2013-v1";
            m_Data["token"] = "";
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            var content = new FormUrlEncodedContent(m_Data);

            var resp = httpClient.PostAsync("login.php", content);
            string s = resp.Result.Content.ReadAsStringAsync().Result;
            return s;
        }

        private static int getRand()
        {
            var rand = new Random();
            int randNum = (int)(rand.NextDouble() * 10000000);
            return randNum;
        }

        public static Level[] getCampaign(string m_Token)
        {
            if(httpClient == null)
                InitHttpClient();
            
            var resp = httpClient.GetAsync("https://pr2hub.com/files/lists/campaign/2?rand=" + getRand() + "&token=" + m_Token);
            string s = resp.Result.Content.ReadAsStringAsync().Result;
                var a = s.Split("levelID");
        var m_Result = new ArrayList();
        for(int i = 1; i < a.Length; i++)
        {
            var m_Levels = a[i].Split("&");
            Level m_Level = new Level();
            for(int j = 0; j < m_Levels.Length-1; j++)
            {
                string[] d = m_Levels[j].Split("=");
                if(j == 0)
                {
                    m_Level.level_id = Int32.Parse(d[1]);
                } else if(d[0].Contains("title")) {
                    m_Level.title = HttpUtility.UrlDecode(d[1]);
                } else if(d[0].Contains("version")) {
                    m_Level.version = Int32.Parse(d[1]);
                }
                if(j == m_Levels.Length-2)
                    m_Result.Add(m_Level);
            }
        }
        object[] arr = m_Result.ToArray();
            return Array.ConvertAll<object, Level>(arr, d => (Level)d);
        }

        public static dynamic getServers()
        {
            if(httpClient == null)
                InitHttpClient();
            var resp = httpClient.GetAsync("http://pr2hub.com/files/server_status_2.txt");
            string s = resp.Result.Content.ReadAsStringAsync().Result;
            dynamic o = JsonConvert.DeserializeObject(s);
            return o;
        }
        private static void InitHttpClient()
        {
            httpClient = new HttpClient();
            encryptor = new Rijndael128Encryptor();
            httpClient.BaseAddress = new Uri("https://pr2hub.com");

            httpClient.DefaultRequestHeaders.Add("Host", "pr2hub.com");
            httpClient.DefaultRequestHeaders.Add("Referer", "https://pr2hub.com");
            httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (X11; Ubuntu; Linux x86_64; rv:61.0) Gecko/20100101 Firefox/61.0");
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
        }
        private static String genLoginJson(String user, String password, int login_id, int server_id)
        {
            dynamic expJson = new ExpandoObject();
            expJson.domain = "cdn.jiggmin.com";
            expJson.remember = false;
            expJson.user_name = user;
            expJson.login_id = login_id;
            expJson.version = "24-dec-2013-v1";
            expJson.login_code = loginCode;
            expJson.user_pass = password;

            dynamic expServer = new ExpandoObject();
            Server m_Server = Program.handler.servers[server_id-1];
            expServer.address = m_Server.address;
            expServer.population = m_Server.pop;
            expServer.guild_id = 0;
            expServer.server_id = server_id;
            expServer.port = m_Server.port;
            expServer.server_name = m_Server.port;
            expServer.happy_hour = 0;
            expServer.status = "open";
            expServer.tournament = 0;
            expJson.server = expServer;

            return JsonConvert.SerializeObject(expJson);
        }
        public static byte[] EncryptStringToBytes(string plainText)
        {
            // Check arguments.
            byte[] Key = Encoding.UTF8.GetBytes(encryptionKEY);
            byte[] IV = Encoding.UTF8.GetBytes(encryptionIV);
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");
            byte[] encrypted;
            // Create an Rijndael object
            // with the specified key and IV.
            using (Rijndael rijAlg = Rijndael.Create())
            {
                // rijAlg.KeySize = 128;
                rijAlg.Key = Key;
                rijAlg.IV = IV;

                // Create an encryptor to perform the stream transform.
                ICryptoTransform encryptor = rijAlg.CreateEncryptor(rijAlg.Key, rijAlg.IV);

                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {

                            //Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }


            // Return the encrypted bytes from the memory stream.
            return encrypted;

        }
    }
}
