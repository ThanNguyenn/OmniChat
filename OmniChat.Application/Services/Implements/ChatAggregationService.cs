using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Models;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Application.Services.Implements
{
    public class ChatAggregationService : IChatAggregationService
    {
        private readonly IConnectionMultiplexer _redis;

        public ChatAggregationService(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        public async Task AddMessageRedisAsync(Guid customerId, string message, Guid providerId)
        {
            var db = _redis.GetDatabase();
            var key = $"chat:{providerId}:{customerId}";

            await db.ListRightPushAsync(key, message);
            await db.KeyExpireAsync(key, TimeSpan.FromSeconds(60));

          
            await db.SetAddAsync("chat_keys", key);
        }
    }
}
