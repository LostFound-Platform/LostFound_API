using Microsoft.EntityFrameworkCore;
using ObjectBusiness;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public class NotificationsDAO
    {
        #region Variables
        private readonly FBLADbContext db;
        private readonly UsersDAO usersDAO;
        #endregion

        #region Constructor
        public NotificationsDAO(FBLADbContext db, UsersDAO usersDAO)
        {
            this.db = db;
            this.usersDAO = usersDAO;
        }
        #endregion

        #region All notifications match image by user ID
        public async Task<List<Notifications>> AllNotificationsMatchImageByUserId(int userId)
        {
            try
            {
                var listNotifications = await (from n in db.Notifications
                                               join po in db.Posts
                                               on n.PostOriginalId equals po.PostId
                                               join pm in db.Posts
                                               on n.PostMatchedId equals pm.PostId
                                               join uo in db.Users
                                               on po.UserId equals uo.UserId
                                               join um in db.Users
                                               on pm.UserId equals um.UserId
                                               where uo.UserId == userId && n.NotificationType == NotificationType.MatchImage
                                               orderby n.CreatedAt descending
                                               select new Notifications
                                               {
                                                   PostMatchedId = n.PostMatchedId,
                                                   PostOriginalId = n.PostOriginalId,
                                                   NotificationContent = n.NotificationContent,
                                                   NotificationId = n.NotificationId,
                                                   NotificationType = n.NotificationType,
                                                   IsRead = n.IsRead,
                                                   FirstNameOriginal = uo.FirstName,
                                                   LastNameOriginal = uo.LastName,
                                                   FirstNameMatched = um.FirstName,
                                                   LastNameMatched = um.LastName,
                                                   TitlePostMatched = pm.Title,
                                                   AvatarUserMatched = um.Avatar,
                                                   ImagePostMatched = pm.Image,
                                                   DescriptionPostMatched = pm.Description,
                                                   TypePost = pm.TypePost
                                               }).ToListAsync();
                return listNotifications;
            }
            catch (Exception ex)
            {

            }
            return null;
        }
        #endregion

        #region All notifications match description by user ID
        public async Task<List<Notifications>> AllNotificationsMatchDescriptionByUserId(int userId)
        {
            var listNotifications = await (from n in db.Notifications
                                           join po in db.Posts
                                           on n.PostOriginalId equals po.PostId
                                           join pm in db.Posts
                                           on n.PostMatchedId equals pm.PostId
                                           join uo in db.Users
                                           on po.UserId equals uo.UserId
                                           join um in db.Users
                                           on pm.UserId equals um.UserId
                                           where uo.UserId == userId && n.NotificationType == NotificationType.MatchDescription
                                           orderby n.CreatedAt descending
                                           select new Notifications
                                           {
                                               PostMatchedId = n.PostMatchedId,
                                               PostOriginalId = n.PostOriginalId,
                                               NotificationContent = n.NotificationContent,
                                               NotificationId = n.NotificationId,
                                               NotificationType = n.NotificationType,
                                               IsRead = n.IsRead,
                                               FirstNameOriginal = uo.FirstName,
                                               LastNameOriginal = uo.LastName,
                                               FirstNameMatched = um.FirstName,
                                               LastNameMatched = um.LastName,
                                               TitlePostMatched = pm.Title,
                                               AvatarUserMatched = um.Avatar,
                                               ImagePostMatched = pm.Image,
                                               DescriptionPostMatched = pm.Description,
                                               TypePost = pm.TypePost
                                           }).ToListAsync();
            return listNotifications;
        }
        #endregion

        #region Create Notification
        public async Task<bool> CreateNotification(Notifications notification)
        {
            notification.NotificationId = new Random().Next();
            var isAdded = db.Notifications.Add(notification);
            if (isAdded != null)
            {
                try
                {
                    await db.SaveChangesAsync();
                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
            return false;
        }
        #endregion

        #region Update Notification
        public async Task<bool> UpdateNotification(Posts post)
        {
            post.PostId = new Random().Next();
            var isAdded = db.Posts.Add(post);
            if (isAdded != null)
            {
                try
                {
                    await db.SaveChangesAsync();
                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
            return false;
        }
        #endregion

        #region All Notifications By Post ID
        public async Task<List<Notifications>> AllNotificationsByPostId(int postId)
        {
            var result = await db.Notifications.Where(r => r.PostMatchedId == postId ||
                                                r.PostOriginalId == postId).ToListAsync();
            return result;
        }
        #endregion

        #region Delete All Notifications Of Post
        public async Task<bool> DeleteNotifications(List<Notifications> notifications)
        {
            if (notifications == null)
            {
                return false;
            }

            try
            {
                db.Notifications.RemoveRange(notifications);
                await db.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        #endregion
    }
}
