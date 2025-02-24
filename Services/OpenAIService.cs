using Data_Organizer_Server.Interfaces;
using Data_Organizer_Server.Models;
using System.Net.Http.Headers;
using System.Text.Json;
using Message = Data_Organizer_Server.Models.Message;

namespace Data_Organizer_Server.Services
{
    public class OpenAIService : IOpenAIService
    {
        private readonly HttpClient _httpClient;
        private readonly List<Message> _messages;

        public const string ENDPOINT = "https://api.openai.com/v1/chat/completions";

        public OpenAIService()
        {
            _httpClient = new HttpClient();
            _messages = new List<Message>();
        }

        public async Task<string> GetSummary(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                throw new ArgumentException("Пусті вхідні дані були відправлені на обробку ШІ!");

            string instruction = "Зроби короткі тези для цього тексту, використовуючи мову, якою надано вхідні дані.\n\n";
            content = instruction + content;

            var message = new Message() { Role = "user", Content = content };
            _messages.Add(message);

            var requestData = new Request()
            {
                ModelId = "gpt-4o-mini",
                Messages = _messages
            };

            var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");

            if (string.IsNullOrWhiteSpace(apiKey))
                throw new InvalidOperationException("API ключ OpenAI не заданий!");

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, ENDPOINT)
                {
                    Content = new StringContent(JsonSerializer.Serialize(requestData))
                };

                request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

                using var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"OpenAI API помилка: {(int)response.StatusCode} - {errorResponse}");
                }

                var responseData = await response.Content.ReadFromJsonAsync<ResponseData>();

                if (responseData == null || responseData.Choices == null || responseData.Choices.Count == 0)
                    throw new NullReferenceException("ШІ нічого не повернув!");

                var responseMessage = responseData.Choices[0].Message;

                if (responseMessage == null || string.IsNullOrWhiteSpace(responseMessage.Content))
                    throw new NullReferenceException("Отримане повідомлення від OpenAI пусте!");

                _messages.Add(responseMessage);
                return responseMessage.Content.Trim();
            }
            catch (HttpRequestException ex)
            {
                throw new Exception("Помилка під час запиту до OpenAI API!", ex);
            }
            catch (JsonException ex)
            {
                throw new Exception("Помилка десеріалізації відповіді OpenAI!", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Невідома помилка у OpenAIService!", ex);
            }
        }
    }
}
