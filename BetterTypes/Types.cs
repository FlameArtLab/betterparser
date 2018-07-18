using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterTypes
{

    #region "ЗАПРОСЫ"

    /// <summary>
    /// Один обычный запрос к серверу (Get\Post\Fetch\итд)
    /// </summary>
    public class NormalRequest
    {

        /// <summary>
        /// Полная строка запроса (пример: https://www.site.com/info/request.php?id=5)
        /// </summary>
        public string request_url;

        /// <summary>
        /// Строка с заголовками, разделёнными по CrLf, из ответа сервера
        /// (необязательно указывать при симуляции, если инфа не нужна)
        /// </summary>
        public string response_headers;

        /// <summary>
        /// Строка с текстовым ответом сервера, раскодированная по указанной кодировке (в Encoding_Matches или Encoding_OneMatch соответственно)
        /// </summary>
        public string response_data;

    }

    /// <summary>
    /// Один фрейм WebSocket`а
    /// </summary>
    public class WebSocketRequest
    {

        /// <summary>
        /// Путь к websocket`у (пример: https://www.site.com/info/request.php?id=5)
        /// </summary>
        public string WebSocket_url;

        /// <summary>
        /// Тип фрейма
        /// </summary>
        public WebSocketRequestFrameType FrameType;

        /// <summary>
        /// Содержимое текстового фрейма (0x1), раскодированного по указанной кодировке (в Encoding_Matches или Encoding_OneMatch соответственно)
        /// </summary>
        public string StringFrame;

        /// <summary>
        /// Содержимое двоичного фрейма (0x1)
        /// </summary>
        public byte[] BinaryFrame;

        /// <summary>
        /// Тип фрейма
        /// </summary>
        public enum WebSocketRequestFrameType : byte { Text = 1, Binary = 2 }

    }


    /// <summary>
    /// Тип поступившего запроса
    /// </summary>
    public enum requestTypes : byte
    {
        /// <summary>
        /// Любой обычный запрос (get\post\fetch\etc)
        /// </summary>
        Normal = 1,

        /// <summary>
        /// Один фрейм WebSocket`а
        /// </summary>
        WebSocket = 2
    }

    /// <summary>
    /// Тип страницы
    /// </summary>
    public enum pageTypes : byte
    {
        /// <summary>
        /// Страница со списком матчей
        /// </summary>
        MatchesList = 1,

        /// <summary>
        /// Страница с конкретным матчем
        /// </summary>
        OneMatch = 2
    }


    #endregion

    /// <summary>
    /// Результат парсинга всех страниц целиком описывается этим классом
    /// Если запрос отдаёт и матчи и кефы, то их всё-равно надо разделить по двум вложенным структурам
    /// </summary>
    public class Parsed
    {

        /// <summary>
        /// Если на запросе была распаршена страница со списком матчей - создаём этот объект и заполняем его
        /// </summary>
        public ParsedMatches matches;

        /// <summary>
        /// Если была распаршена страницы с одним матчем - заполняем структуру с кефами
        /// </summary>
        public ParsedKefs kefs;

        /// <summary>
        /// ID букмекера
        /// </summary>
        public uint BookmakerID;

    }

    /// <summary>
    /// Спаршенный список матчей со страницы матчей
    /// </summary>
    public class ParsedMatches
    {

        /// <summary>
        /// Режим парсера матчей: распаршена вся страница или обновлён только её кусочек
        /// </summary>
        public ParserModes mode;

        /// <summary>
        /// [FullMode] Список распаршенных матчей
        /// [LiveMode] Список новых матчей
        /// </summary>
        public List<Match> matches = new List<Match>();

        /// <summary>
        /// [LiveMode] Список обновлённых матчей
        /// </summary>
        public List<UpdatedMatch> updatedMatches = new List<UpdatedMatch>();

        /// <summary>
        /// Уникальные ID матчей (Match.ID) на странице-источнике, которые были удалены в режиме LiveMode
        /// </summary>
        public List<string> deletedMatches = new List<string>();

        /// <summary>
        /// Один матч со страницы матчей
        /// </summary>
        public class Match
        {
            /// <summary>
            /// Любой уникальный текстовый идентификатор вида спорта на сайте источнике
            /// Важно, чтобы из ID было понятно, что это за вид спорта
            /// Лучший вариант: прямое название типа "Волейбол"
            /// если названия нет, может быть ссылка на иконку "FOOTBALL.png" или что-нибудь такое
            /// </summary>
            public string Sport;

            /// <summary>
            /// Уникальный ID матча на сайте-источнике
            /// </summary>
            public string ID;

            /// <summary>
            /// Имена команд 
            /// [избавление от тегов и нормализация над ними будет проведена в основной программе, можно не парится]
            /// </summary>
            public string Team1Name, Team2Name;

            /// <summary>
            /// Полная ссылка на матч, если существует
            /// </summary>
            public string link;

            /// <summary>
            /// Название турнира, лига, дивизион или другая подкатегория в этом виде спорта.
            /// Если содержит много уровней категорий, то привести к одной строке, разделяя точкой.
            /// </summary>
            public string category;

            /// <summary>
            /// Счёт в виде цельной строки (без парса её составляющих), если указан
            /// </summary>
            public string score;

            /// <summary>
            /// Время матча, как оно указано на сайте, если есть
            /// </summary>
            public string time;

            /// <summary>
            /// Если есть видео, указать прямую ссылку на его iframe (src=...)
            /// </summary>
            public string video;

            /// <summary>
            /// * Техническое поле, заполнять не надо
            /// Список всех линий с вложенными кефами
            /// </summary>
            public List<ParsedKefs.Line> lines = new List<ParsedKefs.Line>();

        }

        /// <summary>
        /// [LiveMode] Обновление статуса матча
        /// </summary>
        public class UpdatedMatch
        {

            /// <summary>
            /// Уникальный ID матча на сайте-источнике
            /// </summary>
            public string ID;

            /// <summary>
            /// Счёт в виде цельной строки (без парса её составляющих), если указан
            /// </summary>
            public string score;

            /// <summary>
            /// Время матча, как оно указано на сайте, если есть
            /// </summary>
            public string time;

        }

    }

    /// <summary>
    /// Коэффициенты одного спаршенного матча
    /// </summary>
    public class ParsedKefs
    {

        /// <summary>
        /// Режим парсера кефов: перепаршены все кефы целиком или только отдельные из них
        /// </summary>
        public ParserModes mode;

        /// <summary>
        /// Список матчей, для которых были распаршены линии и кефы
        /// Обычно со страницы матча парсится 1 матч, и массив будет из одного элемента
        /// но есть Фонбет и мб. другие буки, где матчи в запросе приходят сразу все
        /// </summary>
        public List<Match> matches = new List<Match>();

        /// <summary>
        /// Один матч
        /// </summary>
        public class Match
        {

            /// <summary>
            /// Уникальный ID матча на странице источнике 
            /// равен тому, что парсится со страницы матчей ParsedMatches.Match.ID
            /// </summary>
            public string ID;

            /// <summary>
            /// Матч заблокирован целиком
            /// * если эта информация предоставляется со страницы матча
            /// </summary>
            public bool blocked = false;

            /// <summary>
            /// Матч окончен и закрыт
            /// * если эта информация предоставляется со страницы матча
            /// </summary>
            public bool closed = false;

            /// <summary>
            /// [FullMode] Список всех линий с вложенными кефами, режим распарса всех кефов целиком
            /// </summary>
            public List<Line> lines;

            /// <summary>
            /// [LiveMode] Список новых линий БЕЗ вложенных кефов, только описание линии
            /// </summary>
            public List<Line> NewLines = new List<Line>();

            /// <summary>
            /// [LiveMode] Список уникальных айдишников линий Line.ID, которые были заблокированы, на сайте-источнике
            /// Т.е. если линия была заблокирована целиком, если сайт такую инфу предоставляет
            /// </summary>
            public List<string> BlockedLines = new List<string>();

            /// <summary>
            /// [LiveMode] Список уникальных айдишников линий Line.ID, которые были удалены, на сайте-источнике
            /// </summary>
            public List<string> DeletedLines = new List<string>();

            /// <summary>
            /// [LiveMode] Список новых кефов
            /// </summary>
            public List<Kef> NewKefs = new List<Kef>();

            /// <summary>
            /// [LiveMode] Обновлённые кефы
            /// При блокировке кефов их нужно вносить сюда и указывать отрицательное значение Kef.value
            /// </summary>
            public List<Kef> UpdatedKefs = new List<Kef>();

            /// <summary>
            /// [LiveMode] Удалённые кефы
            /// </summary>
            public List<Kef> DeletedKefs = new List<Kef>();

        }

        /// <summary>
        /// Одна линия [т.е. группа коэффициентов]
        /// </summary>
        public class Line
        {

            /// <summary>
            /// Уникальный ID линии на сайте источнике
            /// </summary>
            public string ID;

            /// <summary>
            /// Имя линии
            /// Если линия имеет несколько уровней названий, то объединить их в одну строку, разделяя ". "
            /// (подробнее см. в README.md)
            /// </summary>
            public string name;

            /// <summary>
            /// [FullMode] Список кефов этой линии
            /// </summary>
            public List<Kef> kefs = new List<Kef>();

        }

        /// <summary>
        /// Один коэффициент
        /// </summary>
        public class Kef
        {

            /// <summary>
            /// Уникальный ID линии (Line.ID) сайта-источника, к которой кеф принадлежит.
            /// </summary>
            public string LineID;

            /// <summary>
            /// Уникальный ID конкретно этого кефа в этом матче на сайте-источнике
            /// </summary>
            public string ID;

            /// <summary>
            /// Название кефа
            /// [Не требуется при обновлении кефов в LiveMode, только при добавлении новых]
            /// </summary>
            public string name;

            /// <summary>
            /// Значение кефа
            /// Если кеф заблокирован, то отрицательное значение
            /// На вход может приниматься строка, которая будет нормализована и распаршена как double
            /// </summary>
            public object value { get { return _value; } set { _value = normalizeKef(value); }} public double _value;
            


            /// <summary>
            /// Нормализовать кеф
            /// </summary>
            /// <param name="kef"></param>
            /// <returns></returns>
            private double normalizeKef(object kef)
            {

                // Если на вход пришло уже цифровое значение, сразу его отдаём
                if (kef is double || kef is int) return Convert.ToDouble(kef);

                // Если на вход пришла строка - нормализуем её
                if (kef is string)
                {

                    string TestString = kef.ToString().Replace(".", ",").Replace("—", "").Replace("\0", "");

                    // Пробуем распарсить как double
                    double result = 0;
                    if (!double.TryParse(TestString.ToString(), out result))
                    {
                        // Распарсть кеф не удалось - какая-то неизвестная ситуация
                        System.Diagnostics.Debugger.Break();

                        // Возвращаем нулевой кеф в любом случае
                        return 0;

                    }

                    // Возвращаем результат в любом случае
                    return result;

                }

                // Распарсить кеф не удалось
                return 0;

            }

        }

    }

    /// <summary>
    /// Режим работы парсера
    /// 
    /// Есть два формата работы букмекеров с лайв-данными: 
    /// Одни просто перерисовывают всю страницу и каждый раз отсылают цельный HTML или цельный JSON, 
    /// который надо заново распарсить. 
    /// И есть те, кто высылает только изменения: кэф добавлен, изменён, удалён, заблокирован 
    /// с момента последнего запроса.
    /// </summary>
    public enum ParserModes : byte
    {

        /// <summary>
        /// Парсер в этом запросе получил страницу целиком и распарсил абсолютно все данные, а не их часть
        /// </summary>
        FullMode = 1,

        /// <summary>
        /// Парсер получил только кусочек данных и сказал что с ними надо сделать: добавить\изменить\удалить\заблокировать
        /// </summary>
        LiveMode = 2,

        /// <summary>
        /// Парсер получил одну цельную линию со всеми кефами и распарсил её полностью
        /// </summary>
        LineMode = 3

    }

    /// <summary>
    /// Тип сайта: одностраничный или многостраничный
    /// </summary>
    public enum ParserPageModes:byte {
        
        /// <summary>
        /// Мультистраничный сайт: есть страница со списком матчей, и отдельная страница для каждого матча
        /// </summary>
        MultiPage = 1,

        /// <summary>
        /// Одностраничный сайт: страница матчей и страница кефов одна, для парсинга кефов надо загружать только страницу матчей
        /// </summary>
        SinglePage = 2

    }

}
