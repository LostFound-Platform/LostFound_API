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
    public class NotificationRepository : INotificationRepository
    {
        #region Variables
        private readonly PostDAO postDAO;
        private readonly MatchDAO matchDAO;
        private readonly UsersDAO userDAO;
        private readonly PickUpRequestDAO pickUpRequestDAO;
        private readonly NotificationsDAO notificationsDAO;
        private readonly VerificationCodeDAO verificationCodeDAO;
        private readonly EmailSender emailSender;
        private readonly IHubContext<SystemHub> hubContext;
        #endregion

        #region Constructor
        public NotificationRepository(PostDAO postDAO,
                              MatchDAO matchDAO,
                              IHubContext<SystemHub> hubContext,
                              UsersDAO userDAO,
                              EmailSender emailSender,
                              PickUpRequestDAO pickUpRequestDAO,
                              VerificationCodeDAO verificationCodeDAO,
                              NotificationsDAO notificationsDAO)
        {
            this.postDAO = postDAO;
            this.matchDAO = matchDAO;
            this.hubContext = hubContext;
            this.userDAO = userDAO;
            this.emailSender = emailSender;
            this.pickUpRequestDAO = pickUpRequestDAO;
            this.verificationCodeDAO = verificationCodeDAO;
            this.notificationsDAO = notificationsDAO;
        }
        #endregion

        #region All notifications match image by user ID
        public async Task<List<Notifications>> AllNotificationsMatchImageByUserId(int userId)
        {
            return await notificationsDAO.AllNotificationsMatchImageByUserId(userId);
        }
        #endregion

        #region All notifications match description by user ID
        public async Task<List<Notifications>> AllNotificationsMatchDescriptionByUserId(int userId)
        {
            var listNotifications = await notificationsDAO.AllNotificationsMatchDescriptionByUserId(userId);
            return listNotifications;
        }
        #endregion
    }
}
