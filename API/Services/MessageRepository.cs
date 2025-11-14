using API.Data;
using API.DTO;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace API.Services
{
    public class MessageRepository(DataContext context, IMapper mapper) : IMessageRepository
    {
        public void AddMessage(Message message)
        {
            context.Messages.Add(message);
        }

        public void DeleteMessage(Message message)
        {
            context.Messages.Remove(message);
        }

        public async Task<Message?> GetMessage(int id)
        {
            return await context.Messages.FindAsync(id);
        }

        public async Task<PagedList<MessageDto>> GetMessagesForUser(MessageParams messageParams)
        {
            var query = context.Messages.OrderByDescending(x => x.MessageSent).AsQueryable();

            query = messageParams.Container switch
            {
                "Inbox" => query.Where(x => x.RecipientUsername == messageParams.Username && x.RecipientDeleted == false),
                "Outbox" => query.Where(x => x.Sender.UserName == messageParams.Username && x.SenderDeleted == false),//here match the sender name and current user name to display the current user outbox
                _ => query.Where(x => x.RecipientUsername == messageParams.Username && x.DateRead == null && x.RecipientDeleted == false),
            };

            var messages = query.ProjectTo<MessageDto>(mapper.ConfigurationProvider);
            return await PagedList<MessageDto>.CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);


        }

        //this is return the list of messagedto
        public async Task<IEnumerable<MessageDto>> GetMessageThread(string currentUserName, string reciverUserName)
        {
            var messages = await context.Messages
                .Include(x => x.Sender).ThenInclude(x => x.photos)
                .Include(x => x.Recipient).ThenInclude(x => x.photos)
                .Where(
                x => x.RecipientUsername == currentUserName && x.SenderUsername == reciverUserName && x.RecipientDeleted == false
                ||
                x.SenderUsername == currentUserName && x.RecipientUsername == reciverUserName && x.SenderDeleted == false
                )
                .OrderBy(x => x.MessageSent)
                .ToListAsync();
            var unreadMessages = messages
                                .Where(x => x.DateRead == null && x.RecipientUsername == currentUserName).ToList();
            if (unreadMessages.Count() != 0)
            {
                foreach (var item in unreadMessages)
                {
                    var indiaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"); //as per Indian time jone
                    item.DateRead = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, indiaTimeZone);
                }
                await context.SaveChangesAsync();
            }
            return mapper.Map<IEnumerable<MessageDto>>(messages);
        }

        public async Task<bool> SaveAllAsync()
        {
            return await context.SaveChangesAsync() > 0;
        }
    }
}
