using MongoDB.Driver;
using Backend.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Backend.Services
{
    public class ChatService : IChatService
    {
        private readonly IMongoCollection<ChatMessage> _messages;

        public ChatService(IMongoDatabase database)
        {
            _messages = database.GetCollection<ChatMessage>("messages");
        }

        public async Task<List<ChatMessage>> GetMessagesBetweenAsync(string sender, string receiver)
        {
            var filter = Builders<ChatMessage>.Filter.Or(
                Builders<ChatMessage>.Filter.And(
                    Builders<ChatMessage>.Filter.Eq(m => m.Sender, sender),
                    Builders<ChatMessage>.Filter.Eq(m => m.Receiver, receiver)
                ),
                Builders<ChatMessage>.Filter.And(
                    Builders<ChatMessage>.Filter.Eq(m => m.Sender, receiver),
                    Builders<ChatMessage>.Filter.Eq(m => m.Receiver, sender)
                )
            );

            var encryptedMessages = await _messages.Find(filter)
                .SortBy(m => m.Timestamp)
                .ToListAsync();

            // Decrypt all messages safely
            foreach (var msg in encryptedMessages)
            {
                msg.Text = EncryptionHelper.SafeDecrypt(msg.Text);
            }

            return encryptedMessages;
        }

        public async Task SendMessageAsync(ChatMessage message)
        {
            // Ensure ID is generated if not set
            if (string.IsNullOrEmpty(message.Id))
            {
                message.Id = MongoDB.Bson.ObjectId.GenerateNewId().ToString();
            }
            
            // Encrypt the message text
            message.Text = EncryptionHelper.Encrypt(message.Text);
            
            // Ensure timestamp is set
            if (message.Timestamp == default)
            {
                message.Timestamp = DateTime.UtcNow;
            }
            
            // Insert or update the message
            var filter = Builders<ChatMessage>.Filter.Eq(m => m.Id, message.Id);
            var options = new ReplaceOptions { IsUpsert = true };
            await _messages.ReplaceOneAsync(filter, message, options);
        }

        public async Task<DeleteResult?> DeleteMessageAsync(string id)
        {
            if (!MongoDB.Bson.ObjectId.TryParse(id, out _))
                return null;

            var filter = Builders<ChatMessage>.Filter.Eq(m => m.Id, id);
            return await _messages.DeleteOneAsync(filter);
        }

        public async Task<UpdateResult?> UpdateMessageAsync(string id, string newText)
        {
            if (!MongoDB.Bson.ObjectId.TryParse(id, out _))
                return null;

            var encryptedText = EncryptionHelper.Encrypt(newText);
            var filter = Builders<ChatMessage>.Filter.Eq(m => m.Id, id);
            var update = Builders<ChatMessage>.Update
                .Set(m => m.Text, encryptedText)
                .Set(m => m.Timestamp, DateTime.UtcNow);

            return await _messages.UpdateOneAsync(filter, update);
        }

        public async Task<ChatMessage?> GetMessageByIdAsync(string id)
        {
            if (!MongoDB.Bson.ObjectId.TryParse(id, out _))
                return null;

            var filter = Builders<ChatMessage>.Filter.Eq(m => m.Id, id);
            var message = await _messages.Find(filter).FirstOrDefaultAsync();

            if (message != null)
                message.Text = EncryptionHelper.SafeDecrypt(message.Text);

            return message;
        }

        // Encrypt all plaintext messages safely
        public async Task EncryptPlaintextMessagesAsync()
        {
            var allMessages = await _messages.Find(_ => true).ToListAsync();

            foreach (var msg in allMessages)
            {
                // Only encrypt if text is not already Base64 (i.e. likely not encrypted)
                if (!IsBase64(msg.Text))
                {
                    Console.WriteLine($"Encrypting message ID: {msg.Id}, Text: {msg.Text}");
                    var encryptedText = EncryptionHelper.Encrypt(msg.Text);

                    var filter = Builders<ChatMessage>.Filter.Eq(m => m.Id, msg.Id);
                    var update = Builders<ChatMessage>.Update.Set(m => m.Text, encryptedText);

                    await _messages.UpdateOneAsync(filter, update);
                }
            }
        }

        // Helper method to check if a string is Base64 encoded
        private static bool IsBase64(string input)
        {
            if (string.IsNullOrEmpty(input))
                return false;

            // Check length is multiple of 4
            if (input.Length % 4 != 0)
                return false;

            Span<byte> buffer = new byte[input.Length];
            return Convert.TryFromBase64String(input, buffer, out _);
        }
    }
}
