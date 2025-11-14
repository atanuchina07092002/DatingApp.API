using API.DTO;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class MessageController(IMessageRepository messageRepository, IUserRepository userRepository, IMapper mapper) : BaseApiController
    {
        [HttpPost]
        public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessage)
        {

            var username = User.GetUserAsync();
            if (username == createMessage.RecipientUsername.ToLower())
            {
                return BadRequest("You not message yourself");
            }
            var sender = await userRepository.GetUserByUsernameAsync(username);
            var recipient = await userRepository.GetUserByUsernameAsync(createMessage.RecipientUsername);
            if (sender == null || recipient == null) { return BadRequest("Error Occured"); }

            var message = new Message
            {
                Sender = sender,
                Recipient = recipient,
                Content = createMessage.Content,
                SenderUsername = sender.UserName!,
                RecipientUsername = recipient.UserName!,
            };
            messageRepository.AddMessage(message);

            if (await messageRepository.SaveAllAsync()) { return Ok(mapper.Map<MessageDto>(message)); }
            return BadRequest("Message Not sent");

        }
        [HttpGet]
        public async Task<ActionResult<MessageDto>> GetMessagesForuser([FromQuery] MessageParams messageParams)
        {
            messageParams.Username = User.GetUserAsync();

            var message = await messageRepository.GetMessagesForUser(messageParams);

            Response.AddPaginationHeader(message);

            return Ok(message);
        }
        [HttpGet("thread/{username}")]
        public async Task<ActionResult<MessageDto>> GetMessageThread(string username)
        {
            string currentusername = User.GetUserAsync();

            return Ok(await messageRepository.GetMessageThread(currentusername, username));
        }
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMessge(int id)
        {
            var username = User.GetUserAsync();
            var message = await messageRepository.GetMessage(id);
            if (message == null) return BadRequest();
            if (message.SenderUsername != username && message.RecipientUsername != username)
            {
                return Forbid();//If user not authorize then redirect to login page
            }
            if (message.SenderUsername == username) message.SenderDeleted = true;
            if (message.RecipientUsername == username) message.RecipientDeleted = true;

            if (message is { SenderDeleted: true, RecipientDeleted: true })
            {
                messageRepository.DeleteMessage(message);
            }
            if (await messageRepository.SaveAllAsync()) return Ok();

            return BadRequest("Message is not deleting");
        }
    }
}
