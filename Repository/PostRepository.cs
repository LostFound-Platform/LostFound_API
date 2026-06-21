using Azure.Core;
using DataAccess;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using ObjectBusiness;
using Services;
using SignalRLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Repository
{
    public class PostRepository : IPostRepository
    {
        #region Variables
        private readonly PostDAO postDAO;
        private readonly MatchDAO matchDAO;
        private readonly UsersDAO userDAO;
        private readonly PickUpRequestDAO pickUpRequestDAO;
        private readonly ChatDAO chatDAO;
        private readonly NotificationsDAO notificationsDAO;
        private readonly TransferRequestDAO transferRequestDAO;
        private readonly VerificationCodeDAO verificationCodeDAO;
        private readonly EmailSender emailSender;
        private readonly IHubContext<SystemHub> hubContext;
        #endregion

        #region Constructor
        public PostRepository(PostDAO postDAO,
                              MatchDAO matchDAO,
                              IHubContext<SystemHub> hubContext,
                              UsersDAO userDAO,
                              EmailSender emailSender,
                              PickUpRequestDAO pickUpRequestDAO,
                              VerificationCodeDAO verificationCodeDAO,
                              NotificationsDAO notificationsDAO,
                              ChatDAO chatDAO,
                              TransferRequestDAO transferRequestDAO)
        {
            this.postDAO = postDAO;
            this.transferRequestDAO = transferRequestDAO;
            this.chatDAO = chatDAO;
            this.matchDAO = matchDAO;
            this.hubContext = hubContext;
            this.userDAO = userDAO;
            this.emailSender = emailSender;
            this.pickUpRequestDAO = pickUpRequestDAO;
            this.verificationCodeDAO = verificationCodeDAO;
            this.notificationsDAO = notificationsDAO;
        }
        #endregion

        #region Get Found Posts
        public IQueryable<Posts> GetFoundPosts()
        {
            return postDAO.GetFoundPosts();
        }
        #endregion

        #region Get Lost Posts Per Month
        public IQueryable<object> GetLostPostsPerMonth()
        {
            return postDAO.GetLostPostsPerMonth();
        }
        #endregion

        #region Get Found Posts Per Month
        public IQueryable<object> GetFoundPostsPerMonth()
        {
            return postDAO.GetFoundPostsPerMonth();
        }
        #endregion

        #region Get Received Posts Per Month
        public IQueryable<object> GetReceivedPostsPerMonth()
        {
            return postDAO.GetReceivedPostsPerMonth();
        }
        #endregion

        #region Sort by status
        public IQueryable<Posts> SortByStatus(TypePost typePost, int userId)
        {
            return postDAO.SortByStatus(typePost, userId);
        }
        #endregion

        #region Get Found Posts Not Received
        public IQueryable<object> GetFoundPostsNotReceived()
        {
            return postDAO.GetFoundPostsNotReceived();
        }
        #endregion

        #region All Lost Post Codes
        public IQueryable<Posts> AllLostPostCodes()
        {
            return postDAO.AllLostPostCodes();
        }
        #endregion

        #region Get Newest Posts
        public async Task<List<Posts>> GetNewestPosts()
        {
            return await postDAO.GetNewestPosts();
        }
        #endregion

        #region Get Pick 30 Lost Posts
        public async Task<List<Posts>> GetPick60LostPosts()
        {
            return await postDAO.GetPick60LostPosts().ToListAsync();
        }
        #endregion

        #region Get Pick 60 Received Posts
        public async Task<List<Posts>> GetPick60ReceivedPosts()
        {
            return await postDAO.GetPick60ReceivedPosts().ToListAsync();
        }
        #endregion

        #region Regular Search
        public async Task<List<Posts>> RegularSearch(string? status, int? categoryId, string? nameItem)
        {
            return await postDAO.RegularSearch(status, categoryId, nameItem).ToListAsync();
        }
        #endregion

        #region Get All Posts
        public IQueryable<Posts> AllPosts()
        {
            return postDAO.AllPosts();
        }
        #endregion

        #region All posts by user ID
        public IQueryable<Posts> AllPostsByUserId(int userId)
        {
            return postDAO.AllPostsByUserId(userId);
        }
        #endregion

        #region Get Post By Id
        public async Task<Posts> GetPostById(int postId)
        {
            return await postDAO.GetPostById(postId);
        }
        #endregion

        #region Update post
        public async Task<bool> UpdatePost(Posts post)
        {
            var isUpdated = await postDAO.UpdatePost();

            // Send suggestions notification for lost posts, which similar to found
            if (isUpdated && post.TypePost == TypePost.Found && post.Vector != null)
            {
                // All posts
                var posts = await postDAO.GetLostPosts().ToListAsync();

                // Check description
                foreach (var lostPost in posts)
                {
                    if (!string.IsNullOrEmpty(lostPost.Description) && !string.IsNullOrEmpty(post.Description))
                    {
                        var score = CalculateSimilarity(lostPost.Description, post.Description);
                        if (score >= 65) // If have 3 or more common words
                        {
                            var postUser = await userDAO.GetUserByID(lostPost.UserId);
                            if (postUser != null)
                            {
                                string senderName = "Back2Me";
                                string senderEmail = "baoandng07@gmail.com";
                                string toName = postUser.FirstName + " " + postUser.LastName;
                                string toEmail = postUser.Email;
                                string subject = "🔔 Potential Match for Your Lost Item!";
                                string content = $@"
                                    <html>
                                    <head>
                                      <style>
                                        body {{
                                            font-family: 'Segoe UI', Arial, sans-serif;
                                            line-height: 1.6;
                                            color: #072138;
                                            background-color: #f9f9fb;
                                            padding: 20px;
                                        }}
                                        a {{
                                            text-decoration: none !important;
                                        }}
                                        .container {{
                                            max-width: 600px;
                                            margin: auto;
                                            background: #ffffff;
                                            border-radius: 12px;
                                            box-shadow: 0 4px 12px rgba(0,0,0,0.1);
                                            overflow: hidden;
                                        }}
                                        .header {{
                                            background-color: #ec7207;
                                            color: #fff;
                                            padding: 20px;
                                            text-align: center;
                                        }}
                                        .header h2 {{
                                            margin: 0;
                                            font-size: 22px;
                                        }}
                                        .content {{
                                            padding: 20px;
                                        }}
                                        .content p {{
                                            margin: 10px 0;
                                        }}
                                        .details {{
                                            width: 100%;
                                            border-collapse: collapse;
                                            margin: 15px 0;
                                        }}
                                        .details th, .details td {{
                                            border: 1px solid #ddd;
                                            padding: 10px;
                                        }}
                                        .details th {{
                                            background-color: #fdcc4b;
                                            text-align: left;
                                            color: #072138;
                                        }}
                                        .highlight {{
                                            background: #fffae6;
                                            border-left: 4px solid #ff9900;
                                            padding: 10px 15px;
                                            margin: 15px 0;
                                            border-radius: 6px;
                                        }}
                                        .btn {{
                                            display: inline-block;
                                            background-color: #ec7207;
                                            border: none;
                                            color: #fff !important;
                                            font-weight: 600;
                                            cursor: pointer;
                                            font-size: 16px;
                                            padding: 12px 25px;
                                            border-radius: 20px;
                                            margin-top: 15px;
                                            transition: all 0.3s ease-in-out;
                                        }}
                                        .btn:hover {{
                                            transform: scale(1.05);
                                        }}
                                        .footer {{
                                            background: #f4f6f9;
                                            padding: 15px;
                                            text-align: center;
                                            font-size: 0.9em;
                                            color: #666;
                                        }}
                                      </style>
                                    </head>
                                    <body>
                                      <div class='container'>
                                        <div class='header'>
                                          <h2>🔔 Potential Match Found!</h2>
                                        </div>
                                        <div class='content'>
                                          <p>Hi <strong>{postUser.FirstName} {postUser.LastName}</strong>,</p>
                                          <p>We found a new <strong>Found Item</strong> that might match one of your lost items.</p>

                                          <table class='details'>
                                            <tr>
                                              <th>Title</th>
                                              <td>{post.Title}</td>
                                            </tr>
                                            <tr>
                                              <th>Posted By</th>
                                              <td>{postUser.FirstName} {postUser.LastName}</td>
                                            </tr>
                                            <tr>
                                              <th>Posted On</th>
                                              <td>{post.CreatedAt:MMMM dd, yyyy}</td>
                                            </tr>
                                          </table>

                                          <div class='highlight'>
                                            ⚡ Tip: Click the button below to check if this item is yours.
                                          </div>

                                          <p style='text-align: center;'>
                                             <a href='https://back2me.vercel.app/detail-post/{post.PostId}' class='btn'>🔎 View Item</a>
                                          </p>

                                          <p>We recommend checking as soon as possible to reclaim your lost item.</p>
                                        </div>
                                        <div class='footer'>
                                          <p>Thanks for using <strong>Back2me</strong>!<br/>Your lost items are our priority.</p>
                                        </div>
                                      </div>
                                    </body>
                                    </html>
                                    ";

                                await emailSender.SendEmail(senderName, senderEmail, toName, toEmail, subject, content);

                                // Add notification
                                var notification = new Notifications
                                {
                                    PostOriginalId = lostPost.PostId,
                                    NotificationContent = $"A new found item titled '{post.Title}' might match your lost item. Click to view details.",
                                    NotificationType = NotificationType.MatchDescription,
                                    PostMatchedId = post.PostId,
                                    IsRead = false,
                                    CreatedAt = DateTime.Now
                                };
                                await notificationsDAO.CreateNotification(notification);
                            }
                        }
                    }
                }

                // Check image if found post has image
                var vector = System.Text.Json.JsonSerializer.Deserialize<List<double>>(post.Vector!);
                var postSearchWithImage = postDAO.SearchImageSimilarityForLost(vector!).ToList();

                foreach (var postWithImage in postSearchWithImage)
                {
                    var postUser = await userDAO.GetUserByID(postWithImage.Post.UserId);

                    string senderName = "Back2Me";
                    string senderEmail = "baoandng07@gmail.com";
                    string toName = postWithImage.Post.User?.FirstName + " " + postWithImage.Post.User?.LastName;
                    string toEmail = postWithImage.Post.User?.Email;
                    string subject = "📸 Potential Match Found for Your Lost Item!";
                    string content = $@"
                        <html>
                        <head>
                          <style>
                            body {{
                                font-family: 'Segoe UI', Arial, sans-serif;
                                line-height: 1.6;
                                color: #072138;
                                background-color: #f9f9fb;
                                padding: 20px;
                            }}
                            a {{
                                text-decoration: none !important;
                            }}
                            .container {{
                                max-width: 600px;
                                margin: auto;
                                background: #ffffff;
                                border-radius: 12px;
                                box-shadow: 0 4px 12px rgba(0,0,0,0.1);
                                overflow: hidden;
                            }}
                            .header {{
                                background-color: #ec7207;
                                color: #fff;
                                padding: 20px;
                                text-align: center;
                            }}
                            .header h2 {{
                                margin: 0;
                                font-size: 22px;
                            }}
                            .content {{
                                padding: 20px;
                            }}
                            .content p {{
                                margin: 10px 0;
                            }}
                            .details {{
                                width: 100%;
                                border-collapse: collapse;
                                margin: 15px 0;
                            }}
                            .details th, .details td {{
                                border: 1px solid #ddd;
                                padding: 10px;
                            }}
                            .details th {{
                                background-color: #fdcc4b;
                                text-align: left;
                                color: #072138;
                            }}
                            .highlight {{
                                background: #fffae6;
                                border-left: 4px solid #ff9900;
                                padding: 10px 15px;
                                margin: 15px 0;
                                border-radius: 6px;
                            }}
                            .btn {{
                                display: inline-block;
                                background-color: #ec7207;
                                border: none;
                                color: #fff !important;
                                font-weight: 600;
                                cursor: pointer;
                                font-size: 16px;
                                padding: 12px 25px;
                                border-radius: 20px;
                                margin-top: 15px;
                                transition: all 0.3s ease-in-out;
                            }}
                            .btn:hover {{
                                transform: scale(1.05);
                            }}
                            .footer {{
                                background: #f4f6f9;
                                padding: 15px;
                                text-align: center;
                                font-size: 0.9em;
                                color: #666;
                            }}
                          </style>
                        </head>
                        <body>
                          <div class='container'>
                            <div class='header'>
                              <h2>📸 Potential Image Match!</h2>
                            </div>
                            <div class='content'>
                              <p>Hi <strong>{postWithImage.Post.User?.FirstName} {postWithImage.Post.User?.LastName}</strong>,</p>
                              <p>We detected a new <strong>Found Item</strong> that has a <strong>similar image</strong> to one of your lost items.</p>

                              <table class='details'>
                                <tr>
                                  <th>Title</th>
                                  <td>{post.Title}</td>
                                </tr>
                                <tr>
                                  <th>Posted By</th>
                                  <td>{postUser.FirstName} {postUser.LastName}</td>
                                </tr>
                                <tr>
                                  <th>Posted On</th>
                                  <td>{post.CreatedAt:MMMM dd, yyyy}</td>
                                </tr>
                                <tr>
                                  <th>Similarity Score</th>
                                  <td>{Math.Floor(postWithImage.Score * 1000) / 10}%</td>
                                </tr>
                              </table>

                              <div class='highlight'>
                                ⚡ Tip: Click the button below to view the found item and check if it matches your lost item.
                              </div>

                              <p style='text-align: center;'>
                                 <a href='https://back2me.vercel.app/detail-post/{post.PostId}' class='btn'>🔎 View Found Item</a>
                              </p>

                              <p>Act quickly to reclaim your lost item before someone else does!</p>
                            </div>
                            <div class='footer'>
                              <p>Thanks for using <strong>Back2me</strong>!<br/>We prioritize helping you recover your lost items.</p>
                            </div>
                          </div>
                        </body>
                        </html>
                        ";

                    await emailSender.SendEmail(senderName, senderEmail, toName, toEmail, subject, content);

                    // Add notification
                    var notification = new Notifications
                    {
                        PostOriginalId = postWithImage.Post.PostId, // Lost
                        NotificationContent = $"A new found item titled '{post.Title}' might match your lost item. Click to view details.",
                        NotificationType = NotificationType.MatchImage,
                        PostMatchedId = post.PostId, // Found
                        IsRead = false,
                        CreatedAt = DateTime.Now
                    };
                    await notificationsDAO.CreateNotification(notification);
                }
            }

            return isUpdated;
        }
        #endregion

        #region Mark Received
        public async Task<Posts> MarkReceived(int postId)
        {
            var postMarked = await postDAO.MarkReceived(postId);

            if (postMarked != null)
            {
                var match = await matchDAO.GetMatchByPostId(postMarked.PostId);

                // Remove pick up request of the lost post
                var pickUpRequest = await pickUpRequestDAO.DeletePickUpRequest(postMarked.PostId);

                // Remove match
                if (match != null)
                {
                    // Remove verification code
                    var verificationCode = await verificationCodeDAO.GetVerificationCodeByMatchId(match.MatchId);
                    if (verificationCode != null)
                    {
                        await verificationCodeDAO.DeleteVerificationCode(verificationCode.VerificationCodeId);
                    }

                    // Mark found post as received
                    var foundPost = await postDAO.GetPostById(match.FoundPostId);

                    if (foundPost != null)
                    {
                        var postFoundMarked = await postDAO.MarkReceived(foundPost.PostId);
                    }

                    await matchDAO.DeleteMatch(match.MatchId); // Delete Match
                }

                // Send realtime
                await hubContext.Clients.Group("Admin").SendAsync("ReceiveStatusMarkPost", new
                {
                    PostId = postMarked.PostId,
                    Code = postMarked.Code,
                    TypePost = postMarked.TypePost,
                    User = new
                    {
                        FirstName = postMarked.User.FirstName,
                        LastName = postMarked.User.LastName,
                    },
                    CreatedAt = postMarked.CreatedAt,
                    IsReceived = postMarked.IsReceived,
                });
            }

            return postMarked;
        }
        #endregion

        #region Search Codes
        public IQueryable<Posts> SearchCodes(string query)
        {
            return postDAO.SearchCodes(query);
        }
        #endregion

        #region Create Post
        public async Task<bool> CreatePost(Posts post)
        {
            var isAdded = await postDAO.CreatePost(post);

            if (isAdded)
            {
                var user = await userDAO.GetUserByID(post.UserId);

                if (user != null)
                {
                    // Send realtime
                    if (post.TypePost == TypePost.Lost)
                    {
                        await hubContext.Clients.All.SendAsync("ReceiveNewLostPostCode", new
                        {
                            PostId = post.PostId,
                            Code = post.Code,
                            TypePost = post.TypePost,
                            User = new
                            {
                                FirstName = user.FirstName,
                                LastName = user.LastName,
                            },
                            CreatedAt = post.CreatedAt,
                            IsReceived = post.IsReceived,
                        });
                    }

                    // Send suggestions notification for lost posts, which similar to found
                    if (post.TypePost == TypePost.Found && post.Vector != null)
                    {
                        // All posts
                        var posts = await postDAO.GetLostPosts().ToListAsync();

                        // Check description
                        foreach (var lostPost in posts)
                        {
                            if (!string.IsNullOrEmpty(lostPost.Description) && !string.IsNullOrEmpty(post.Description))
                            {
                                var score = CalculateSimilarity(lostPost.Description, post.Description);
                                if (score >= 65) // If have 3 or more common words
                                {
                                    var postUser = await userDAO.GetUserByID(lostPost.UserId);
                                    if (postUser != null)
                                    {
                                        string senderName = "Back2Me";
                                        string senderEmail = "baoandng07@gmail.com";
                                        string toName = postUser.FirstName + " " + postUser.LastName;
                                        string toEmail = postUser.Email;
                                        string subject = "🔔 Potential Match for Your Lost Item!";
                                        string content = $@"
                                        <html>
                                        <head>
                                          <style>
                                            body {{
                                                font-family: 'Segoe UI', Arial, sans-serif;
                                                line-height: 1.6;
                                                color: #072138;
                                                background-color: #f9f9fb;
                                                padding: 20px;
                                            }}
                                            a {{
                                                text-decoration: none !important;
                                            }}
                                            .container {{
                                                max-width: 600px;
                                                margin: auto;
                                                background: #ffffff;
                                                border-radius: 12px;
                                                box-shadow: 0 4px 12px rgba(0,0,0,0.1);
                                                overflow: hidden;
                                            }}
                                            .header {{
                                                background-color: #ec7207;
                                                color: #fff;
                                                padding: 20px;
                                                text-align: center;
                                            }}
                                            .header h2 {{
                                                margin: 0;
                                                font-size: 22px;
                                            }}
                                            .content {{
                                                padding: 20px;
                                            }}
                                            .content p {{
                                                margin: 10px 0;
                                            }}
                                            .details {{
                                                width: 100%;
                                                border-collapse: collapse;
                                                margin: 15px 0;
                                            }}
                                            .details th, .details td {{
                                                border: 1px solid #ddd;
                                                padding: 10px;
                                            }}
                                            .details th {{
                                                background-color: #fdcc4b;
                                                text-align: left;
                                                color: #072138;
                                            }}
                                            .highlight {{
                                                background: #fffae6;
                                                border-left: 4px solid #ff9900;
                                                padding: 10px 15px;
                                                margin: 15px 0;
                                                border-radius: 6px;
                                            }}
                                            .btn {{
                                                display: inline-block;
                                                background-color: #ec7207;
                                                border: none;
                                                color: #fff !important;
                                                font-weight: 600;
                                                cursor: pointer;
                                                font-size: 16px;
                                                padding: 12px 25px;
                                                border-radius: 20px;
                                                margin-top: 15px;
                                                transition: all 0.3s ease-in-out;
                                            }}
                                            .btn:hover {{
                                                transform: scale(1.05);
                                            }}
                                            .footer {{
                                                background: #f4f6f9;
                                                padding: 15px;
                                                text-align: center;
                                                font-size: 0.9em;
                                                color: #666;
                                            }}
                                          </style>
                                        </head>
                                        <body>
                                          <div class='container'>
                                            <div class='header'>
                                              <h2>🔔 Potential Match Found!</h2>
                                            </div>
                                            <div class='content'>
                                              <p>Hi <strong>{postUser.FirstName} {postUser.LastName}</strong>,</p>
                                              <p>We found a new <strong>Found Item</strong> that might match one of your lost items.</p>

                                              <table class='details'>
                                                <tr>
                                                  <th>Title</th>
                                                  <td>{post.Title}</td>
                                                </tr>
                                                <tr>
                                                  <th>Posted By</th>
                                                  <td>{postUser.FirstName} {postUser.LastName}</td>
                                                </tr>
                                                <tr>
                                                  <th>Posted On</th>
                                                  <td>{post.CreatedAt:MMMM dd, yyyy}</td>
                                                </tr>
                                              </table>

                                              <div class='highlight'>
                                                ⚡ Tip: Click the button below to check if this item is yours.
                                              </div>

                                              <p style='text-align: center;'>
                                                 <a href='https://back2me.vercel.app/detail-post/{post.PostId}' class='btn'>🔎 View Item</a>
                                              </p>

                                              <p>We recommend checking as soon as possible to reclaim your lost item.</p>
                                            </div>
                                            <div class='footer'>
                                              <p>Thanks for using <strong>Back2me</strong>!<br/>Your lost items are our priority.</p>
                                            </div>
                                          </div>
                                        </body>
                                        </html>
                                        ";

                                        await emailSender.SendEmail(senderName, senderEmail, toName, toEmail, subject, content);

                                        // Add notification
                                        var notification = new Notifications
                                        {
                                            PostOriginalId = lostPost.PostId,
                                            NotificationContent = $"A new found item titled '{post.Title}' might match your lost item. Click to view details.",
                                            NotificationType = NotificationType.MatchDescription,
                                            PostMatchedId = post.PostId,
                                            IsRead = false,
                                            CreatedAt = DateTime.Now
                                        };
                                        await notificationsDAO.CreateNotification(notification);
                                    }
                                }
                            }
                        }

                        // Check image if found post has image
                        var vector = System.Text.Json.JsonSerializer.Deserialize<List<double>>(post.Vector!);
                        var postSearchWithImage = postDAO.SearchImageSimilarityForLost(vector!).ToList();

                        foreach (var postWithImage in postSearchWithImage)
                        {
                            var postUser = await userDAO.GetUserByID(postWithImage.Post.UserId);

                            string senderName = "Back2Me";
                            string senderEmail = "baoandng07@gmail.com";
                            string toName = postWithImage.Post.User?.FirstName + " " + postWithImage.Post.User?.LastName;
                            string toEmail = postWithImage.Post.User?.Email;
                            string subject = "📸 Potential Match Found for Your Lost Item!";
                            string content = $@"
                            <html>
                            <head>
                              <style>
                                body {{
                                    font-family: 'Segoe UI', Arial, sans-serif;
                                    line-height: 1.6;
                                    color: #072138;
                                    background-color: #f9f9fb;
                                    padding: 20px;
                                }}
                                a {{
                                    text-decoration: none !important;
                                }}
                                .container {{
                                    max-width: 600px;
                                    margin: auto;
                                    background: #ffffff;
                                    border-radius: 12px;
                                    box-shadow: 0 4px 12px rgba(0,0,0,0.1);
                                    overflow: hidden;
                                }}
                                .header {{
                                    background-color: #ec7207;
                                    color: #fff;
                                    padding: 20px;
                                    text-align: center;
                                }}
                                .header h2 {{
                                    margin: 0;
                                    font-size: 22px;
                                }}
                                .content {{
                                    padding: 20px;
                                }}
                                .content p {{
                                    margin: 10px 0;
                                }}
                                .details {{
                                    width: 100%;
                                    border-collapse: collapse;
                                    margin: 15px 0;
                                }}
                                .details th, .details td {{
                                    border: 1px solid #ddd;
                                    padding: 10px;
                                }}
                                .details th {{
                                    background-color: #fdcc4b;
                                    text-align: left;
                                    color: #072138;
                                }}
                                .highlight {{
                                    background: #fffae6;
                                    border-left: 4px solid #ff9900;
                                    padding: 10px 15px;
                                    margin: 15px 0;
                                    border-radius: 6px;
                                }}
                                .btn {{
                                    display: inline-block;
                                    background-color: #ec7207;
                                    border: none;
                                    color: #fff !important;
                                    font-weight: 600;
                                    cursor: pointer;
                                    font-size: 16px;
                                    padding: 12px 25px;
                                    border-radius: 20px;
                                    margin-top: 15px;
                                    transition: all 0.3s ease-in-out;
                                }}
                                .btn:hover {{
                                    transform: scale(1.05);
                                }}
                                .footer {{
                                    background: #f4f6f9;
                                    padding: 15px;
                                    text-align: center;
                                    font-size: 0.9em;
                                    color: #666;
                                }}
                              </style>
                            </head>
                            <body>
                              <div class='container'>
                                <div class='header'>
                                  <h2>📸 Potential Image Match!</h2>
                                </div>
                                <div class='content'>
                                  <p>Hi <strong>{postWithImage.Post.User?.FirstName} {postWithImage.Post.User?.LastName}</strong>,</p>
                                  <p>We detected a new <strong>Found Item</strong> that has a <strong>similar image</strong> to one of your lost items.</p>

                                  <table class='details'>
                                    <tr>
                                      <th>Title</th>
                                      <td>{post.Title}</td>
                                    </tr>
                                    <tr>
                                      <th>Posted By</th>
                                      <td>{postUser.FirstName} {postUser.LastName}</td>
                                    </tr>
                                    <tr>
                                      <th>Posted On</th>
                                      <td>{post.CreatedAt:MMMM dd, yyyy}</td>
                                    </tr>
                                    <tr>
                                      <th>Similarity Score</th>
                                      <td>{Math.Floor(postWithImage.Score * 1000) / 10}%</td>
                                    </tr>
                                  </table>

                                  <div class='highlight'>
                                    ⚡ Tip: Click the button below to view the found item and check if it matches your lost item.
                                  </div>

                                  <p style='text-align: center;'>
                                     <a href='https://back2me.vercel.app/detail-post/{post.PostId}' class='btn'>🔎 View Found Item</a>
                                  </p>

                                  <p>Act quickly to reclaim your lost item before someone else does!</p>
                                </div>
                                <div class='footer'>
                                  <p>Thanks for using <strong>Back2me</strong>!<br/>We prioritize helping you recover your lost items.</p>
                                </div>
                              </div>
                            </body>
                            </html>
                            ";

                            await emailSender.SendEmail(senderName, senderEmail, toName, toEmail, subject, content);

                            // Add notification
                            var notification = new Notifications
                            {
                                PostOriginalId = postWithImage.Post.PostId, // Lost
                                NotificationContent = $"A new found item titled '{post.Title}' might match your lost item. Click to view details.",
                                NotificationType = NotificationType.MatchImage,
                                PostMatchedId = post.PostId, // Found
                                IsRead = false,
                                CreatedAt = DateTime.Now
                            };
                            await notificationsDAO.CreateNotification(notification);
                        }
                    }
                }

                // Send Email to all students
                if (post.TypePost == TypePost.Found && user.Role == Role.Admin)
                {
                    var listUsers = userDAO.AllUsers().ToList();

                    foreach (var item in listUsers)
                    {
                        string senderName = "Back2Me";
                        string senderEmail = "baoandng07@gmail.com";
                        string toName = item.FirstName + " " + item.LastName;
                        string toEmail = item.Email;
                        string subject = "🚨 New Found Item Posted!";
                        string content = $@"
                            <html>
                            <head>
                              <style>
                                body {{
                                    font-family: 'Segoe UI', Arial, sans-serif;
                                    line-height: 1.6;
                                    color: #072138;
                                    background-color: #f9f9fb;
                                    padding: 20px;
                                }}
                                a {{
                                    text-decoration: none !important;
                                }}
                                .container {{
                                    max-width: 600px;
                                    margin: auto;
                                    background: #ffffff;
                                    border-radius: 12px;
                                    box-shadow: 0 4px 12px rgba(0,0,0,0.1);
                                    overflow: hidden;
                                }}
                                .header {{
                                    background-color: #ec7207;
                                    color: #fff;
                                    padding: 20px;
                                    text-align: center;
                                }}
                                .header h2 {{
                                    margin: 0;
                                    font-size: 22px;
                                }}
                                .content {{
                                    padding: 20px;
                                }}
                                .content p {{
                                    margin: 10px 0;
                                }}
                                .details {{
                                    width: 100%;
                                    border-collapse: collapse;
                                    margin: 15px 0;
                                }}
                                .details th, .details td {{
                                    border: 1px solid #ddd;
                                    padding: 10px;
                                }}
                                .details th {{
                                    background-color: #fdcc4b;
                                    text-align: left;
                                    color: #072138;
                                }}
                                .highlight {{
                                    background: #fffae6;
                                    border-left: 4px solid #ff9900;
                                    padding: 10px 15px;
                                    margin: 15px 0;
                                    border-radius: 6px;
                                }}
                                .btn {{
                                    display: inline-block;
                                    background-color: #ec7207;
                                    border: none;
                                    width: max-content;
                                    color: #fff !important;
                                    font-weight: 600;
                                    cursor: pointer;
                                    font-size: 16px;
                                    padding: 12px 25px;
                                    border-radius: 20px;
                                    margin-top: 10px;
                                    margin-bottom: 10px;
                                    transition: all 0.3s ease-in-out;
                                }}
                                .btn:hover {{
                                    transform: scale(1.05);
                                }}
                                .footer {{
                                    background: #f4f6f9;
                                    padding: 15px;
                                    text-align: center;
                                    font-size: 0.9em;
                                    color: #666;
                                }}
                              </style>
                            </head>
                            <body>
                              <div class='container'>
                                <div class='header'>
                                  <h2>🚨 New Found Item Posted!</h2>
                                </div>
                                <div class='content'>
                                  <p>Hi <strong>{item.FirstName} {item.LastName}</strong>,</p>
                                  <p>The admin has just posted a new <strong>Found Item</strong> that might match your lost items:</p>

                                  <table class='details'>
                                    <tr>
                                      <th>Title</th>
                                      <td>{post.Title}</td>
                                    </tr>
                                    <tr>
                                      <th>Posted By</th>
                                      <td>Admin</td>
                                    </tr>
                                    <tr>
                                      <th>Posted On</th>
                                      <td>{post.CreatedAt}</td>
                                    </tr>
                                  </table>

                                  <div class='highlight'>
                                    ⚡ Check the system to see if this item matches any of your lost items.
                                  </div>

                                  <p style='text-align: center;'>
                                     <a href='https://back2me.vercel.app/detail-post/{post.PostId}' class='btn'>🔎 View Found Item</a>
                                  </p>

                                  <p>Thank you for your quick attention!</p>
                                </div>
                                <div class='footer'>
                                  <p>Best regards,<br/><strong>Back2me Team</strong></p>
                                </div>
                              </div>
                            </body>
                            </html>
                            ";

                        await emailSender.SendEmail(senderName, senderEmail, toName, toEmail, subject, content);
                    }
                }
            }

            return isAdded;
        }
        #endregion

        #region Search Image similarity
        public IEnumerable<SearchResult> SearchImageSimilarity(List<double> vector)
        {
            return postDAO.SearchImageSimilarity(vector);
        }
        #endregion

        #region Hand Over to Admin
        public async Task<bool> HandOverAdmin(int postId, int oldUserId)
        {
            var isHandedOver = await postDAO.HandOverAdmin(postId, oldUserId);

            if (isHandedOver)
            {
                await hubContext.Clients.All.SendAsync("ReceivePostHandedOver", new
                {
                    PostId = postId,
                });
            }

            return isHandedOver;
        }
        #endregion

        #region Delete post
        public async Task<bool> DeletePost(int postId)
        {
            try
            {
                var match = await matchDAO.GetMatchByPostId(postId);

                // Remove pick up request of the lost post
                var pickUpRequest = await pickUpRequestDAO.DeletePickUpRequest(postId);

                // Remove match
                if (match != null)
                {
                    // Remove verification code
                    var verificationCode = await verificationCodeDAO.GetVerificationCodeByMatchId(match.MatchId);
                    if (verificationCode != null)
                    {
                        await verificationCodeDAO.DeleteVerificationCode(verificationCode.VerificationCodeId);
                    }

                    // Mark found post as received
                    var foundPost = await postDAO.GetPostById(match.FoundPostId);

                    if (foundPost != null)
                    {
                        var postFoundMarked = await postDAO.MarkReceived(foundPost.PostId);
                    }

                    await matchDAO.DeleteMatch(match.MatchId); // Delete Match
                }

                // Remove all transfer requests
                var transferRequests = await transferRequestDAO.AllRequestsByPostId(postId);
                if (transferRequests != null || transferRequests.Any())
                {
                    await transferRequestDAO.DeleteTransfers(transferRequests);
                }

                // Remove all notifications
                var notifications = await notificationsDAO.AllNotificationsByPostId(postId);
                if (notifications != null || notifications.Any())
                {
                    await notificationsDAO.DeleteNotifications(notifications);
                }

                // Remove chat has post
                var chats = await chatDAO.AllChatsByPostId(postId);

                if (chats != null || chats.Any())
                {
                    foreach (var chatItem in chats)
                    {
                        // Remove messages
                        var messages = await chatDAO.AllMessagesByChatId(chatItem.ChatId);

                        if (messages != null)
                        {
                            await chatDAO.DeleteMessages(messages);
                        }
                    }

                    // Delete chat
                    await chatDAO.DeleteChats(chats);
                }

                var isDeletedPost = await postDAO.DeletePost(postId);
                return isDeletedPost;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        #endregion

        // Funtion not related to Entity
        #region Get random string
        public string GetRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            Random random = new Random();

            char[] charsArray = new char[length];
            for (int i = 0; i < length; i++)
            {
                charsArray[i] = chars[random.Next(charsArray.Length)];
            }

            return new string(charsArray);
        }
        #endregion

        #region Function calculation similarity
        private int CalculateSimilarity(string lostDesc, string foundDesc)
        {
            var lostWords = NormalizeText(lostDesc);
            var foundWords = new HashSet<string>(NormalizeText(foundDesc)); // Faster search

            int matchCount = 0;

            foreach (var word in lostWords)
            {
                if (foundWords.Contains(word))
                {
                    matchCount++;
                }
            }

            if (lostWords.Count == 0)
            {
                return 0;
            }

            return (int)Math.Round((double)matchCount / lostWords.Count * 100);
        }
        #endregion

        #region Normalize Text
        private List<string> NormalizeText(string text)
        {
            text = text.ToLower();
            text = RemoveVietnameseTones(text);

            text = Regex.Replace(text, "<.*?>", ""); // Remove all html tags
            text = Regex.Replace(text, @"[^a-z0-9\s]", " ");

            #region Stop Words
            var stopWords = new HashSet<string>()
            {
                "la",
                "va",
                "tai",
                "toi",
                "bi",
                "lost",
                "found",
                "the",
                "am",
                "think",
                "are",
                "and",
                "holding",
                "was",
                "were",
                "is",
                "a",
                "an",
                "in",
                "on",
                "at",
                "of",
                "for",
                "with",
                "to",
                "by",
                "from",
                "cua",
                "co",
                "trong",
                "duoc",
                "mot",
                "nhung",
                "item",
                "object",
                "it",
                "thing",
                "they"
            };
            #endregion

            return text
                .Split(" ", StringSplitOptions.RemoveEmptyEntries)
                .Where(w => w.Length > 1 && !stopWords.Contains(w))
                .ToList();
        }
        #endregion

        #region Remove Vietnamese Tones
        private string RemoveVietnameseTones(string text)
        {
            string normalized = text.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();

            foreach (var c in normalized)
            {
                var unicodeCategory = Char.GetUnicodeCategory(c);
                if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(c);
                }
            }

            return sb.ToString().Normalize(NormalizationForm.FormC);
        }
        #endregion
    }
}
