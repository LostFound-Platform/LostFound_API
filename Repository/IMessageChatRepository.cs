using ObjectBusiness;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public interface IMessageChatRepository
    {
        public Task<MessageChat> SendMessage(MessageChat messageChat);
    }
}
