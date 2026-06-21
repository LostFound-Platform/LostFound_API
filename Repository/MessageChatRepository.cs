using DataAccess;
using Microsoft.AspNetCore.SignalR;
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
    public class MessageChatRepository : IMessageChatRepository
    {
        #region Variables
        private readonly PostDAO postDAO;
        private readonly UsersDAO userDAO;
        private readonly MessageChatDAO messageChatDAO;
        private readonly EmailSender emailSender;
        private readonly IHubContext<SystemHub> hubContext;
        #endregion

        #region Constructor
        public MessageChatRepository(PostDAO postDAO,
                              MatchDAO matchDAO,
                              IHubContext<SystemHub> hubContext,
                              UsersDAO userDAO,
                              EmailSender emailSender,
                              PickUpRequestDAO pickUpRequestDAO,
                              VerificationCodeDAO verificationCodeDAO,
                              MessageChatDAO messageChatDAO)
        {
            this.postDAO = postDAO;
            this.hubContext = hubContext;
            this.userDAO = userDAO;
            this.emailSender = emailSender;
            this.messageChatDAO = messageChatDAO;
        }
        #endregion

        #region Create Message (Send Message)
        public async Task<MessageChat> SendMessage(MessageChat messageChat)
        {
            var chatAdded = await messageChatDAO.SendMessage(messageChat);
            return chatAdded;
        }
        #endregion
    }
}
