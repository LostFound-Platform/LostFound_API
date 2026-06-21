using ObjectBusiness;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public interface INotificationRepository
    {
        public Task<List<Notifications>> AllNotificationsMatchImageByUserId(int userId);
        public Task<List<Notifications>> AllNotificationsMatchDescriptionByUserId(int userId);
    }
}
