using DataAccess;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using ObjectBusiness;
using Services;
using SignalRLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public class ChatRepository : IChatRepository
    {
        #region Variables
        private readonly PostDAO postDAO;
        private readonly UsersDAO userDAO;
        private readonly ChatDAO chatDAO;
        private readonly EmailSender emailSender;
        private readonly IHubContext<SystemHub> hubContext;
        #endregion

        #region Constructor
        public ChatRepository(PostDAO postDAO,
                              MatchDAO matchDAO,
                              IHubContext<SystemHub> hubContext,
                              UsersDAO userDAO,
                              EmailSender emailSender,
                              PickUpRequestDAO pickUpRequestDAO,
                              VerificationCodeDAO verificationCodeDAO,
                              ChatDAO chatDAO)
        {
            this.postDAO = postDAO;
            this.hubContext = hubContext;
            this.userDAO = userDAO;
            this.emailSender = emailSender;
            this.chatDAO = chatDAO;
        }
        #endregion

        #region All Chats by user ID
        public async Task<List<Chat>> AllChatsByUserId(int userId)
        {
            var chats = await chatDAO.AllChatsByUserId(userId);
            return chats;
        }
        #endregion

        #region Get Chat By Id
        public async Task<Chat> GetChatById(int chatId)
        {
            var chat = await chatDAO.GetChatById(chatId);
            return chat;
        }
        #endregion

        #region Create Chat
        public async Task<Chat> CreateChat(Chat chat)
        {
            var chatAdded = await chatDAO.CreateChat(chat);
            return chatAdded;
        }
        #endregion

        #region All Messages by chat ID
        public async Task<List<MessageChat>> AllMessagesByChatId(int chatId)
        {
            var chatAdded = await chatDAO.AllMessagesByChatId(chatId);
            return chatAdded;
        }
        #endregion

        #region Check Pair user existed
        public async Task<Chat> CheckPairUserExisted(int userSendId, int userReceiveId)
        {
            var chatExisted = await chatDAO.CheckPairUserExisted(userSendId, userReceiveId);
            return chatExisted;
        }
        #endregion
    }
}
