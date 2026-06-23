using ObjectBusiness;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public class MessageChatDAO
    {
        #region Variables
        private readonly BackToMeDbContext db;
        #endregion

        #region Constructor
        public MessageChatDAO(BackToMeDbContext db)
        {
            this.db = db;
        }
        #endregion

        #region Create Message (Send Message)
        public async Task<MessageChat> SendMessage(MessageChat messageChat)
        {

            messageChat.MessageChatId = new Random().Next();
            try
            {
                var isAdded = db.MessageChat.Add(messageChat);
                if (isAdded != null)
                {
                    await db.SaveChangesAsync();
                    return messageChat;
                }
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        #endregion
    }
}
