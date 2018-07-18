using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Better.Bookmakers
{
    /// <summary>
    /// Клиент, который хранит сессию между запросами [потокобезопасно]
    /// </summary>
    public class WebClientCookie: WebClient
    {
        public CookieContainer CookieContainer { get; private set; }

        /// <summary>
        /// This will instanciate an internal CookieContainer.
        /// </summary>
        public WebClientCookie()
        {
            this.CookieContainer = new CookieContainer();
        }

        /// <summary>
        /// Use this if you want to control the CookieContainer outside this class.
        /// </summary>
        public WebClientCookie(CookieContainer cookieContainer)
        {
            this.CookieContainer = cookieContainer;
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            var request = base.GetWebRequest(address) as HttpWebRequest;
            if (request == null) return base.GetWebRequest(address);

            // Общий куки контейнер
            request.CookieContainer = CookieContainer;

            // Заголовки по-умолчанию
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/67.0.3396.79 Safari/537.36";
            request.Accept = "*/*";
            request.Headers.Add("Accept-Language", "ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7");

            // Декомпрессия
            request.Headers.Add("Accept-Encoding", "gzip, deflate");
            request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;

            return request;
        }

    }
}
