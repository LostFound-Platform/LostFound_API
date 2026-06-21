using ObjectBusiness;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public interface IChatRepository
    {
        public Task<List<Chat>> AllChatsByUserId(int userId);
        public Task<Chat> GetChatById(int chatId);
        public Task<Chat> CreateChat(Chat chat);
        public Task<List<MessageChat>> AllMessagesByChatId(int chatId);
        public Task<Chat> CheckPairUserExisted(int userSendId, int userReceiveId);
    }
}
