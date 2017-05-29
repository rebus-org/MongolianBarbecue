using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongolianBarbecue.Model;

namespace MongolianBarbecue.Tests.Extensions
{
    public static class ConfigExtensions
    {
        public static async Task<List<Message>> GetAllMessagesFrom(this Config config, string queueName)
        {
            var consumer = new Consumer(config, queueName);
            var messages = new List<Message>();
            var lastMessage = DateTime.UtcNow;

            while (true)
            {
                var message = await consumer.GetNextAsync();

                if (message != null)
                {
                    messages.Add(message);
                    await message.Ack();
                    lastMessage = DateTime.UtcNow;
                    continue;
                }

                await Task.Delay(100);

                if (DateTime.UtcNow - lastMessage > TimeSpan.FromSeconds(2)) break;
            }

            return messages;
        }

    }
}