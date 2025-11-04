using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Windows.Forms;

namespace CheckPosition
{
    internal class YandexMetricChecker
    {
        // для получения тоекна
        private static readonly string utl_to_create_token = "https://oauth.yandex.ru/authorize?response_type=token&client_id=4a9fc3668b6c4a869269f2e3e6ebc89d";

        private static readonly string apiUrl = "https://api-metrika.yandex.net/stat/v1/data";
        private static readonly string token = "y0_AgAAAAAAG7UXAAucVgAAAAEUnQ9kAAAfRBozUchJIIQFuWLTlMtsOyLLog";  // Здесь вставьте ваш OAuth токен  

/*        static async Task Main(string[] args)
        {
            var visitorsWithoutBots = await GetVisitorsWithoutBots();
            Console.WriteLine($"Количество посетителей без роботов: {visitorsWithoutBots}");
        }
*/

        public static async Task<int> GetVisitorsWithoutBots(string counterId, int countDays = 30)
        {
            // Определяем даты начала и окончания периода
            DateTime now = DateTime.Now;
            
            
            string date1 = now.AddDays(-countDays).ToString("yyyy-MM-dd");
            string date2 = now.AddDays(-1).ToString("yyyy-MM-dd");

            // Параметры запроса
            var queryParams = $"?id={counterId}&metrics=ym:s:visits&filters=ym:s:isRobot==\'No\'&date1={date1}&date2={date2}";


            using (var client = new HttpClient())
            {
                // Добавляем токен аутентификации
                client.DefaultRequestHeaders.Add("Authorization", $"OAuth {token}");

                // Параметры запроса
                // var queryParams = $"?id={counterId}&metrics=ym:s:visits&filters=ym:s:isRobot==\'No\'";

                // Выполняем GET-запрос к API
                var response = await client.GetAsync(apiUrl + queryParams);

                if (!response.IsSuccessStatusCode)
                    MessageBox.Show($"Ошибка получения данных по метрике {counterId}");

                // Проверяем успешность запроса
                response.EnsureSuccessStatusCode();

                // Читаем ответ
                var responseBody = await response.Content.ReadAsStringAsync();

                // Парсим JSON-ответ
                var jsonResponse = JObject.Parse(responseBody);

                // Извлекаем количество визитов (первое значение в массиве данных)
                int visitors = jsonResponse["data"][0]["metrics"][0].Value<int>();

                return visitors;
            }
        }
    }
}
