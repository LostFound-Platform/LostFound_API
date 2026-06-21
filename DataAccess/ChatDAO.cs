using Azure.Core;
using Microsoft.EntityFrameworkCore;
using ObjectBusiness;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public class ChatDAO
    {
        #region Variables
        private readonly FBLADbContext db;
        #endregion

        #region Constructor
        public ChatDAO(FBLADbContext db)
        {
            this.db = db;
        }
        #endregion

        #region Create Chat
        public async Task<Chat> CreateChat(Chat chat)
        {
            try
            {

                var isAdded = db.Chat.Add(chat);
                if (isAdded != null)
                {
                    await db.SaveChangesAsync();
                    return chat;
                }
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        #endregion

        #region All Chats by user ID
        public async Task<List<Chat>> AllChatsByUserId(int postId)
        {
            var result = await (from c in db.Chat
                                join m in db.MessageChat
                                on c.ChatId equals m.ChatId
                                join ua in db.Users
                                on c.UserAId equals ua.UserId
                                join ub in db.Users
                                on c.UserBId equals ub.UserId
                                where c.UserAId == postId || c.UserBId == postId
                                group new { c, m, ua, ub } by c.ChatId into g // Group because 1 chat can have messages
                                select g
                                    .OrderByDescending(x => x.m.CreatedAt) // order by one newest message from each chat
                                    .Select(x => new Chat
                                    {
                                        ChatId = x.c.ChatId,
                                        CreatedAt = x.c.CreatedAt,
                                        UserAId = x.c.UserAId,
                                        UserBId = x.c.UserBId,
                                        UserA = x.ua,
                                        UserB = x.ub,
                                        PostId = x.c.PostId,
                                        Title = x.c.Post.Title,
                                        MessageContent = x.m.MessageContent,
                                        DateSendMessage = x.m.CreatedAt,
                                        UserALastReadMessageId = x.c.UserALastReadMessageId,
                                        UserBLastReadMessageId = x.c.UserBLastReadMessageId,
                                    })
                                    .FirstOrDefault())
                                    .AsNoTracking()
                                    .ToListAsync();
            return result;
        }
        #endregion

        #region All Chats by post ID
        public async Task<List<Chat>> AllChatsByPostId(int postId)
        {
            var result = await db.Chat.Where(c => c.PostId == postId).ToListAsync();
            return result;
        }
        #endregion

        #region All Messages by chat ID
        public async Task<List<MessageChat>> AllMessagesByChatId(int chatId)
        {
            var result = await (from c in db.Chat
                                join m in db.MessageChat
                                on c.ChatId equals m.ChatId
                                join ua in db.Users
                                on c.UserAId equals ua.UserId
                                join ub in db.Users
                                on c.UserBId equals ub.UserId
                                where c.ChatId == chatId
                                select new MessageChat
                                {
                                    MessageChatId = m.MessageChatId,
                                    ChatId = c.ChatId,
                                    CreatedAt = m.CreatedAt,
                                    UserAId = c.UserAId,
                                    UserBId = c.UserBId,
                                    UserSenderId = m.UserSenderId,
                                    MessageContent = m.MessageContent,
                                    FirstNameUserA = ua.FirstName,
                                    LastNameUserA = ua.LastName,
                                    FirstNameUserB = ub.FirstName,
                                    LastNameUserB = ub.LastName,
                                    AvatarUserA = ua.Avatar,
                                    AvatarUserB = ub.Avatar,
                                    PostId = c.PostId
                                })
                               .OrderBy(m => m.CreatedAt)
                               .AsNoTracking()
                               .ToListAsync();
            return result;
        }
        #endregion

        #region Get Chat By Id
        public async Task<Chat> GetChatById(int chatId)
        {
            var chat = await db.Chat
                         .FirstOrDefaultAsync(c => c.ChatId == chatId);
            return chat;
        }
        #endregion

        #region Get Chat By Post Id
        public async Task<Chat> GetChatByPostId(int postId)
        {
            var chat = await db.Chat
                         .FirstOrDefaultAsync(c => c.PostId == postId);
            return chat;
        }
        #endregion

        #region Delete All Messages Of Post
        public async Task<bool> DeleteMessages(List<MessageChat> messages)
        {
            if (!messages.Any())
            {
                return false;
            }

            try
            {
                db.MessageChat.RemoveRange(messages);
                await db.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        #endregion

        #region Delete All Chat Of Post
        public async Task<bool> DeleteChats(List<Chat> chats)
        {
            if (chats == null)
            {
                return false;
            }

            try
            {
                db.Chat.RemoveRange(chats);
                await db.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        #endregion

        #region Check Pair user existed
        public async Task<Chat> CheckPairUserExisted(int userSendId, int userReceiveId)
        {
            var isExisted = await db.Chat
                         .FirstOrDefaultAsync(c => c.UserAId == userSendId &&
                                              c.UserBId == userReceiveId);
            return isExisted;
        }
        #endregion
    }
}
