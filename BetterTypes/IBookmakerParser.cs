using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterTypes
{

    /// <summary>
    /// Базовый интерфейс, который должен реализовывать каждый парсер
    /// </summary>
    public interface IBookmakerParser
    {

        /// <summary>
        /// * ID букмекера
        /// </summary>
        uint BookmakerID { get; set; }

        /// <summary>
        /// * Кодировка страницы матчей
        /// 65001 = utf8; 1251 = win-1251; 20866 = KOI8-R;
        /// </summary>
        int Encoding_Matches { get; set; }

        /// <summary>
        /// * Кодировка страницы одного матча
        /// </summary>
        int Encoding_OneMatch { get; set; }

        /// <summary>
        /// * Тип сайта: многостраничник (страница матчей и отдельно страницы кефов) или одностраничник (только страница кефов)
        /// </summary>
        ParserPageModes pageMode { get; set; }

        /// <summary>
        /// Ссылка на страницу матчей для многостраничников, или на главную страницу с матчами и кефами для одностраничников
        /// </summary>
        string MainPageURL { get; set; }

        /// <summary>
        /// Список разрешённых URL, которые должны приходить на роутер
        /// Для понимания: фильтроваться записи будут через request_url.Contains( approvedUrls[i] ), так что можно вводить только части адреса
        /// [Нужно для быстродействия, чтобы браузер не отгружал ненужные URL]
        /// </summary>
        List<string> approvedUrls { get; set; }

        /// <summary>
        /// Инициализация при первичной подгрузке парсера
        /// </summary>
        /// <returns>Возвращает результат инициализации, т.е. готовность парсера к работе</returns>
        bool init();


        /// <summary>
        /// Точка входа в библиотеку. Отделяет полезные запросы от бесполезных и направляет каждый на свои парсеры
        /// </summary>
        /// <param name="page_type">Тип страницы: список матчей или страница кефа. Если страницы кефов нет - по умолчанию страница матчей</param>
        /// <param name="request_type">Тип запроса: обычный или фрейм WebSocket`а</param>
        /// <param name="request">Инфо с обычного запроса, либо NULL</param>
        /// <param name="wsFrame">Инфо о фрейме WebSocket`а, либо NULL</param>
        /// <returns>Вернуть распаршенную структуру Parsed или NULL, если запросу парсинг не требуется</returns>
        Parsed Router(pageTypes page_type, requestTypes request_type, ref NormalRequest request, ref WebSocketRequest wsFrame);

        /// <summary>
        /// Запустить парсер нужной страницы, отправляя запросы вручную
        /// Предпочтение отдавать асинхронным запросам и сохранению куков через WebClientCookie
        /// </summary>
        /// <param name="page">Какую страницу парсить: список матчей или одного матча</param>
        /// <param name="urlMatches">Ссылка на страницу списка матчей, обязательно указывать даже если парсится страница матча</param>
        /// <param name="urlOneMatch">Ссылка на страницу конкретного матча, не нужно указывать, если парсится список матчей</param>
        /// <param name="speed_kef">Коэффициент ускорения отправки запросов</param>
        /// <param name="callback">Коллбек, который надо вызвать со структурой Parsed, которую вернёт роутер, если нужно</param>
        Task Start(pageTypes page, string urlMatches, string urlOneMatch, double speed_kef = 1, Action<Parsed> callback = null);



        /// <summary>
        /// Отправить ошибку парса в основную прогу немедленно [асинхронно]
        /// [реализовано через callback, чтобы редкие виды ошибок не приводили к неотправке самого списка ошибок]
        /// </summary>
        /// <param name="name">Название\описание ошибки</param>
        /// <param name="path">Путь до ошибки. Реализовано в виде строки можно было тонко описать момент её возникновения</param>
        /// <param name="data">Любые прикладные данные этой ошибки, которые вам нужно будет знать для её исправления. Будет сериализовано в json и записано.</param>
        void sendException(string name, string path = "", object data = null);

        /// <summary>
        /// Ссылка на коллбек ошибки
        /// Параметры: Имя, Путь, Данные
        /// </summary>
        Action<string,string,object> ExceptionCallback { get; set; }

        /// <summary>
        /// Глобальный куки контейнер, прикрепляемый извне
        /// </summary>
        System.Net.CookieContainer GlobalCookies { get; set; }

        /// <summary>
        /// Семафор закрытия парсера
        /// </summary>
        bool needStop { get; set; }

    }
}
