using Microsoft.AspNetCore.Mvc;
using Backend.Models;
using Backend.Hubs;
using Backend.Services;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/chat")]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;
        private readonly IHubContext<ChatHub> _hubContext;

        public ChatController(IChatService chatService, IHubContext<ChatHub> hubContext)
        {
            _chatService = chatService;
            _hubContext = hubContext;
        }

        [HttpGet("messages")]
        public async Task<ActionResult<List<ChatMessage>>> GetMessages([FromQuery] string sender, [FromQuery] string receiver)
        {
            var messages = await _chatService.GetMessagesBetweenAsync(sender, receiver);
            return Ok(messages);
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] ChatMessage message)
        {
            if (message == null || string.IsNullOrWhiteSpace(message.Text))
                return BadRequest("Invalid message");

            // Set timestamp
            message.Timestamp = DateTime.UtcNow;
            
            // Save to database first - this will generate an ID if needed
            await _chatService.SendMessageAsync(message);

            // Get the message with the generated ID from the database
            var savedMessage = await _chatService.GetMessageByIdAsync(message.Id);
            if (savedMessage == null)
                return StatusCode(500, "Failed to save message");

            // Broadcast the saved message (with ID) to all clients via SignalR
            await _hubContext.Clients.All.SendAsync("ReceiveMessage", savedMessage);

            return Ok(savedMessage);
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteMessage(string id)
        {
            // Get the message before deleting to have the data for broadcasting
            var messageToDelete = await _chatService.GetMessageByIdAsync(id);
            if (messageToDelete == null)
                return NotFound("Message not found");

            var result = await _chatService.DeleteMessageAsync(id);

            if (result.DeletedCount == 0)
                return NotFound("Message not found");

            // Broadcast deletion to all clients via SignalR
            await _hubContext.Clients.All.SendAsync("MessageDeleted", messageToDelete);

           return Ok(new { message = "Message deleted successfully" });
        }

        [HttpPut("edit/{id}")]
        public async Task<IActionResult> EditMessage(string id, [FromBody] EditMessageRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Text))
                return BadRequest("Message text cannot be empty");

            var result = await _chatService.UpdateMessageAsync(id, request.Text);

            if (result.ModifiedCount == 0)
                return NotFound("Message not found or no changes");

            // Get the updated message from DB
            var updatedMessage = await _chatService.GetMessageByIdAsync(id);
            if (updatedMessage != null)
            {
                // ✅ Send event using correct SignalR event name and format
                await _hubContext.Clients
                    .Users(new[] { updatedMessage.Sender, updatedMessage.Receiver })
                    .SendAsync("ReceiveMessageEvent", new
                    {
                        type = "edit",
                        message = updatedMessage
                    });
            }

            return Ok(new { message = "Message updated successfully" });

        }

    }

    public class EditMessageRequest
    {
        public string Text { get; set; } = string.Empty;
    }
}