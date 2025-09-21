using Backend.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace Backend.Services
{
    public interface IChatService
    {
        Task<List<ChatMessage>> GetMessagesBetweenAsync(string sender, string receiver);
        Task SendMessageAsync(ChatMessage message);
        Task<DeleteResult> DeleteMessageAsync(string id);
        Task<UpdateResult> UpdateMessageAsync(string id, string newText);
        Task<ChatMessage?> GetMessageByIdAsync(string id);
    }
}