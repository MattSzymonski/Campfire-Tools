using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OpenAI_API;
using OpenAI_API.Chat;
using OpenAI_API.Models;

namespace CampfireTools.OpenAI
{
    public class OpenAIHelper  
    {
        public static string GetOpenAiKey(string credentialsFilePath) 
        { 
            string jsonContent = File.ReadAllText(credentialsFilePath);
            dynamic? json = JsonConvert.DeserializeObject(jsonContent);
            return json != null ? json.api_key : "";  
        }

        public static async Task<string> SendPromptToChatGPTAsync(OpenAIAPI api, string prompt)
        {
            try
            {
                var result = await api.Chat.CreateChatCompletionAsync(new ChatRequest()
                {
                    Model = Model.ChatGPTTurbo,
                    Temperature = 1.0,
                    MaxTokens = 2000,
                    Messages = new ChatMessage[] { new ChatMessage(ChatMessageRole.User, prompt) }
                });

                return result.Choices[0].Message.Content;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"OpenAI Error: {ex.Message}");
                return string.Empty;
            }
        }

        public static async Task<T> CallOpenAiAndDeserializeToClass<T>(OpenAIAPI? openAiApi, string prompt, int retries = 0) 
        {
            if (retries == 4)
            {
                throw new Exception($"CallOpenAiAndDeserializeToClass {typeof(T).FullName} error! Number of retries exceeded");
            }

            if (openAiApi == null)
            {
                throw new Exception("No OpenAI initialized");
            }

            string answer = await OpenAIHelper.SendPromptToChatGPTAsync(openAiApi, prompt);

            // Try to deserialize to class
            try
            {
                T? serializedClass = JsonConvert.DeserializeObject<T>(answer);
                if (serializedClass == null)
                {
                    throw new Exception($"Failed to parse JSON and serialize it to {typeof(T).FullName}");
                }
                return serializedClass;
            }
            catch (JsonException error)
            {
                Console.WriteLine($"CallOpenAiAndDeserializeToClass {typeof(T).FullName} error! Retrying...\n({error})\n{answer}");
                return await CallOpenAiAndDeserializeToClass<T>(openAiApi, prompt, retries++);
            }
        }
    }
}