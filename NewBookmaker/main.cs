using BetterTypes;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Http;
using System.Net;

/// <summary>
/// * Букмекер [ИМЯ]
/// </summary>
namespace Better.Bookmakers
{

    /// <summary>
    /// * Парсер букмекера [ИМЯ]
    /// </summary>
    public class NewBookmaker : IBookmakerParser
    {

        /// <summary>
        /// * ID букмекера
        /// </summary>
        public uint BookmakerID { get; set; } = 1;

        /// <summary>
        /// * Кодировка страницы матчей
        /// 65001 = utf8; 1251 = win-1251; 20866 = KOI8-R;
        /// </summary>
        public int Encoding_Matches { get; set; } = 65001;

        /// <summary>
        /// * Кодировка страницы одного матча
        /// </summary>
        public int Encoding_OneMatch { get; set; } = 65001;

        /// <summary>
        /// * Тип сайта: многостраничник (страница матчей и отдельно страницы кефов) или одностраничник (достаточно загрузки главной страницы)
        /// </summary>
        public ParserPageModes pageMode { get; set; } = ParserPageModes.MultiPage;

        /// <summary>
        /// * Ссылка на главную страницу с матчами
        /// </summary>
        public string MainPageURL { get; set; } = "";

        /// <summary>
        /// * Список разрешённых URL, которые должны приходить на роутер
        /// Для понимания: фильтроваться записи будут через request_url.Contains( approvedUrls[i] ), так что можно вводить только части адреса
        /// [Нужно для быстродействия, чтобы браузер не отгружал ненужные URL]
        /// </summary>
        public List<string> approvedUrls { get; set; } = new List<string>();

        /// <summary>
        /// Инициализация при первичной подгрузке парсера
        /// </summary>
        /// <returns>Возвращает результат инициализации, т.е. готовность парсера к работе</returns>
        public bool init()
        {

            // * Заполняем фильтр по URL [для запросов из браузера]
            // Все approvedUrls в боевом режиме будут разрешаться через URL.Contains(approvedUrls[i])
            // approvedUrls.Add("olimp.kz/index.php?page=line");
            // approvedUrls.Add("ajax_index.php");

            // Предзагрузка любых одноразовых данных, создание своих объектов и любая другая необходимая разовая работа

            return true;
        }

        #region "START"

        /// <summary>
        /// Запустить парсер нужной страницы, отправляя запросы вручную
        /// Предпочтение отдавать асинхронным запросам и использованию WebClientCookie
        /// </summary>
        /// <param name="page">Какую страницу парсить: список матчей или одного матча</param>
        /// <param name="urlMatches">Ссылка на страницу списка матчей, обязательно указывать даже если парсится страница матча</param>
        /// <param name="urlOneMatch">Ссылка на страницу конкретного матча, не нужно указывать, если парсится список матчей</param>
        /// <param name="speed_kef">Коэффициент ускорения отправки запросов</param>
        /// <param name="callback">Коллбек, который надо вызвать со структурой Parsed, которую вернёт роутер, если нужно</param>
        public async Task Start(pageTypes page, string urlMatches, string urlOneMatch, double speed_kef = 1, Action<Parsed> callback = null)
        {

            // Пример того как этой функции желательно выглядеть, можете даже просто подставить свои Fetch-запросы

            // WebClientCookie - расширенная версия вебклиента, которая сама хранит сессию, подставляет User-agent
            // и другие стандартные заголовки
            // использовать глобальный куки контейнер GlobalCookies для каждого нового экземпляра WebClientCookie
            WebClientCookie WC = new WebClientCookie(GlobalCookies);



            // Разогреть страницу и получить куки
            if (!await preloadPage(page, urlMatches, urlOneMatch, WC)) { sendException("Не удалось разогреть страницу"); return; }


            // ID матча из URL, если нужно
            string matchID = "";
            // if(page== pageTypes.OneMatch)
            //    matchID = urlOneMatch.Split(new string[] { "hl=" }, StringSplitOptions.None)[1];


            // запустить цикл обновлений
            NormalRequest request = new NormalRequest(); WebSocketRequest noRequest = null;
            Parsed data = null; string response;
            while (true)
            {

                // Запускаем парсер страницы матчей
                if (page == pageTypes.MatchesList)
                {

                    try
                    {
                        response = await WC.DownloadStringTaskAsync("https://www.parimatch.com/live_as.html?curs=0&curName=$&shed=0");
                    }
                    catch { continue; }

                    // Экземпляр запроса
                    request = new NormalRequest
                    {
                        request_url = "https://www.parimatch.com/live_as.html?curs=0&curName=$&shed=0",
                        response_data = response,
                        //response_headers = WC.ResponseHeaders // только если нужно для парсинга
                    };

                    // отправить NormalRequest в роутер
                    data = Router(pageTypes.MatchesList, requestTypes.Normal, ref request, ref noRequest);

                    // Вызывать коллбек с данными, если он есть [асинхронен в вызывающем коде]
                    if (callback != null)
                        callback.Invoke(data);

                    Debugger.Break();

                    // Подождать стандартное для букмекера время обновления списка матчей
                    await Task.Delay(TimeSpan.FromMilliseconds(speed_kef * 7500));

                }

                // Страница одного матча [если для этого букмекера это применимо]
                else if (page == pageTypes.OneMatch)
                {

                    // * Запросим данные
                    try
                    {
                        response = await WC.DownloadStringTaskAsync("https://www.parimatch.com/live_ar.html?hl=" + matchID + "&hl=" + matchID + ",&curs=0&curName=$");
                    }
                    catch { continue; }

                    // Экземпляр запроса
                    request = new NormalRequest
                    {
                        request_url = "https://www.parimatch.com/live_ar.html?hl=" + matchID + "&hl=" + matchID + ",&curs=0&curName=$",
                        response_data = response,
                        //response_headers = wc.response_headers // только если нужно для парсинга
                    };

                    // отправить NormalRequest в роутер
                    data = Router(pageTypes.OneMatch, requestTypes.Normal, ref request, ref noRequest);

                    // Вызывать коллбек с данными, если он есть [асинхронен в вызывающем коде]
                    if (callback != null)
                        callback.Invoke(data);

                    // Подождать стандартное для букмекера время обновления 1 страницы матча
                    await Task.Delay(TimeSpan.FromMilliseconds(speed_kef * 7500));

                }

                // Останавливаем поток, если нужно
                if (needStop) return;

            }


        }


        /// <summary>
        /// Глобальный куки контейнер [единый для всех страниц]
        /// </summary>
        public System.Net.CookieContainer GlobalCookies { get; set; } = new System.Net.CookieContainer();

        /// <summary>
        /// Семафор, что парсеру следует остановить работу
        /// </summary>
        public bool needStop { get; set; } = false;

        /// <summary>
        /// Разогреть страницу: получить базовые cookies, и просто её получить с нужным реферером, чтобы быть меньше похожими на бота
        /// </summary>
        /// <param name="page"></param>
        /// <param name="urlMatches"></param>
        /// <param name="urlOneMatch"></param>
        /// <param name="WC"></param>
        /// <returns></returns>
        async Task<bool> preloadPage(pageTypes page, string urlMatches, string urlOneMatch, WebClient WC)
        {

            // "Разогреть" страницу и получить основные куки букмекера
            if (page == pageTypes.MatchesList)
            {
                // Для страницы матчей реферер - гугл
                WC.Headers.Add("Referer", "https://www.google.ru/");
                WC.Encoding = System.Text.Encoding.GetEncoding(Encoding_Matches);
                try { await WC.DownloadStringTaskAsync(urlMatches); }
                catch (WebException ex) { return false; }

            }
            else if (page == pageTypes.OneMatch)
            {
                // Для страницы кефов реферер - страница матчей
                WC.Headers.Add("Referer", urlMatches);
                WC.Encoding = System.Text.Encoding.GetEncoding(Encoding_OneMatch);
                try { await WC.DownloadStringTaskAsync(urlOneMatch); }
                catch { return false; }
                
            }

            // Разогрев завершён
            return true;

        }

        #endregion

        #region "Роутер"

        /// <summary>
        /// Отделяет полезные запросы от бесполезных и направляет каждый на свои парсеры
        /// </summary>
        /// <param name="page_type">Тип страницы: список матчей или страница кефа. Если страницы кефов нет - по умолчанию страница матчей</param>
        /// <param name="request_type">Тип запроса: обычный или фрейм WebSocket`а</param>
        /// <param name="request">Инфо с обычного запроса, либо NULL</param>
        /// <param name="wsFrame">Инфо о фрейме WebSocket`а, либо NULL</param>
        /// <returns>Вернуть распаршенную структуру Parsed или NULL, если запросу парсинг не требуется</returns>
        public Parsed Router(pageTypes page_type, requestTypes request_type, ref NormalRequest request, ref WebSocketRequest wsFrame)
        {

            // Выходная структура
            Parsed parsed = new Parsed { BookmakerID = BookmakerID };

            // По request.request_url (если надо request_headers) направить на нужный парсер

            // Пример: получено xhr-обновление матчей
            if (page_type == pageTypes.MatchesList && request.request_url.Contains("/live_as.html?curs=0&curName=$"))
            {

                // Направляем на парсер списка матчей
                try
                {
                    parsed.matches = Matches(request.response_data);
                }
                catch { sendException("Неизвестная ошибка парсинга", "Router", request.response_data); return null; }

                // Отдаём распаршенную структуру с матчами
                return parsed;

            }

            // Получена страница одного матча
            else if (page_type == pageTypes.OneMatch && request.request_url.Contains("/live_ar.html?hl="))
            {

                // Направляем на парсер списка матчей
                try
                {
                    parsed.matches = Matches(request.response_data);
                }
                catch { sendException("Неизвестная ошибка парсинга", "Router", request.response_data); return null; }

                // Отдаём распаршенную структуру с одним матчем
                return parsed;

            }



            // Вернуть null, если парсинг не нужен
            return null;

        }

        #endregion

        #region "Контроль ошибок"

        /// <summary>
        /// Отправить ошибку парса в основную прогу немедленно [асинхронно]
        /// [реализовано через callback, чтобы редкие виды ошибок не приводили к неотправке самого списка ошибок]
        /// </summary>
        /// <param name="name">Название\описание ошибки</param>
        /// <param name="path">Путь до ошибки. Реализовано в виде строки можно было тонко описать момент её возникновения</param>
        /// <param name="data">Любые прикладные данные этой ошибки, которые вам нужно будет знать для её исправления. Будет сериализовано в json и записано.</param>
        public void sendException(string name, string path = "", object data = null)
        {
            if (ExceptionCallback != null)
            {
                ExceptionCallback.BeginInvoke(name, path, data, (iResult) =>
                {
                    ExceptionCallback.EndInvoke(iResult);
                },
                null);
            }
        }

        /// <summary>
        /// Коллбек для ловли ошибок в реалтайме в основной проге
        /// [присоединяется родительской программой]
        /// параметры: Имя ошибки, Точный путь к ошибке (необязательно), Данные (если нужны)
        /// </summary>
        public Action<string, string, object> ExceptionCallback { get; set; }

        #endregion
        
        #region "Парсер"

        /// <summary>
        /// Парсер матчей 
        /// * пример
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public ParsedMatches Matches(string data)
        {

            // Создаём выходной экземпляр и указываем режим, в котором парсится страница матчей
            ParsedMatches matches = new ParsedMatches { mode = ParserModes.FullMode };

            // ЗАПОЛНЯЕМ СТРУКТУРУ matches


            // Отдаём роутеру
            Debugger.Break();
            return matches;

        }

        /// <summary>
        /// Парсер кефов
        /// * пример
        /// </summary>
        /// <param name="data"></param>
        /// <param name="matchID">ID матча, если один запрос содержит один матч</param>
        /// <returns></returns>
        public ParsedKefs Kefs(string data, string matchID = "")
        {

            ParsedKefs kefs = new ParsedKefs()
            {
                mode = ParserModes.FullMode,
                matches = new List<ParsedKefs.Match>()
            };

            // Отдаём роутеру
            Debugger.Break();
            return kefs;

        }

        #endregion
    }
}
