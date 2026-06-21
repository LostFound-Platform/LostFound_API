using DataAccess;
using Microsoft.AspNetCore.SignalR;
using ObjectBusiness;
using Services;
using SignalRLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Repository
{
    public class PickUpRequestRepository : IPickUpRequestRepository
    {
        #region Variables
        private readonly PickUpRequestDAO pickUpRequestDAO;
        private readonly PostDAO postDAO;
        private readonly UsersDAO usersDAO;
        private readonly EmailSender emailSender;
        private readonly IHubContext<SystemHub> hubContext;
        #endregion

        #region Constructor
        public PickUpRequestRepository(PickUpRequestDAO pickUpRequestDAO,
                                       IHubContext<SystemHub> hubContext,
                                       EmailSender emailSender,
                                       PostDAO postDAO,
                                       UsersDAO usersDAO)
        {
            this.pickUpRequestDAO = pickUpRequestDAO;
            this.hubContext = hubContext;
            this.emailSender = emailSender;
            this.postDAO = postDAO;
            this.usersDAO = usersDAO;
        }
        #endregion

        #region Create Request
        public async Task<bool> CreateRequest(PickUpRequest request)
        {
            var isAdded = await pickUpRequestDAO.CreateRequest(request);

            if (isAdded)
            {
                var post = await postDAO.GetPostById(request.PostId);

                if (post != null)
                {
                    string senderName = "Back2Me";
                    string senderEmail = "baoandng07@gmail.com";
                    string toName = post.User?.FirstName + " " + post.User?.LastName;
                    string toEmail = post.User?.Email;
                    string subject = "Your pickup request is pending confirmation";
                    string content = $@"
                    <html>
                    <head>
                      <style>
                        body {{
                          font-family: 'Segoe UI', Arial, sans-serif;
                          background-color: #f5f7fb;
                          padding: 20px;
                          color: #1f2a44;
                        }}
                        a {{
                          text-decoration: none;
                        }}
                        .container {{
                          max-width: 600px;
                          margin: auto;
                          background: #ffffff;
                          border-radius: 14px;
                          box-shadow: 0 6px 18px rgba(0,0,0,0.08);
                          overflow: hidden;
                        }}
                        .header {{
                          background: linear-gradient(135deg, #ff8c1a, #ec7207);
                          color: #fff;
                          padding: 24px;
                          text-align: center;
                        }}
                        .header h2 {{
                          margin: 0;
                          font-size: 22px;
                        }}
                        .content {{
                          padding: 24px;
                        }}
                        .content p {{
                          margin: 12px 0;
                          font-size: 15px;
                        }}
                        .status-box {{
                          background: #fff7e6;
                          border-left: 5px solid #ff9900;
                          padding: 16px 18px;
                          border-radius: 10px;
                          margin: 20px 0;
                        }}
                        .status-box h4 {{
                          margin: 0 0 6px 0;
                          font-size: 16px;
                          color: #cc7a00;
                        }}
                        .info-card {{
                          background: #f9fafc;
                          border-radius: 10px;
                          padding: 16px;
                          margin: 18px 0;
                        }}
                        .info-row {{
                          display: flex;
                          justify-content: space-between;
                          padding: 6px 0;
                          font-size: 14px;
                        }}
                        .info-row span:first-child {{
                          color: #6b7280;
                        }}
                        .btn {{
                          display: inline-block;
                          background-color: #ec7207;
                          color: #fff !important;
                          font-weight: 600;
                          font-size: 15px;
                          padding: 12px 26px;
                          border-radius: 22px;
                          margin-top: 16px;
                        }}
                        .footer {{
                          background: #f1f3f7;
                          padding: 16px;
                          text-align: center;
                          font-size: 13px;
                          color: #6b7280;
                        }}
                      </style>
                    </head>
                    <body>
                      <div class='container'>
    
                        <div class='header'>
                          <h2>⏳ Pickup Request Received</h2>
                        </div>

                        <div class='content'>
                          <p>Hi <strong>{post.User?.FirstName} {post.User?.LastName}</strong>,</p>

                          <p>
                            We’ve received your request to pick up the found item.  
                            At the moment, your request is <strong>waiting for admin confirmation</strong>.
                          </p>

                          <div class='status-box'>
                            <h4>🔔 Current Status: Pending Confirmation</h4>
                            <p>
                              Our admin is reviewing your pickup time.  
                              You’ll receive another email as soon as the pickup schedule is approved.
                            </p>
                          </div>

                          <div class='info-card'>
                            <div class='info-row'>
                              <span>📦 Item <strong>{post.Title}</strong></span>
                            </div>
                            <div class='info-row'>
                              <span>🧑‍💼 Managed by Admin</span>
                            </div>
                            <div class='info-row'>
                              <span>🗓 Requested on {post.CreatedAt}</span>
                            </div>
                          </div>

                          <p style='text-align:center;'>
                            <a href='https://back2me.vercel.app/detail-post/{post.PostId}' class='btn'>
                              🔎 View Item Details
                            </a>
                          </p>

                          <p>
                            Please wait for our confirmation before coming to pick up the item.
                            Thank you for your patience 💙
                          </p>
                        </div>

                        <div class='footer'>
                          <p>
                            Best regards,<br/>
                            <strong>Back2me Team</strong><br/>
                            This is an automated notification — please do not reply.
                          </p>
                        </div>

                      </div>
                    </body>
                    </html>
                    ";

                    await emailSender.SendEmail(senderName, senderEmail, toName, toEmail, subject, content);
                }

            }

            return isAdded;
        }
        #endregion

        #region Check Contains Post
        public async Task<PickUpRequest> CheckContainsPost(int postId)
        {
            var result = await pickUpRequestDAO.CheckContainsPost(postId);
            return result;
        }
        #endregion

        #region All Requests
        public IQueryable<PickUpRequest> AllRequests()
        {
            var result = pickUpRequestDAO.AllRequests();
            return result;
        }
        #endregion

        #region Accept Time
        public async Task<PickUpRequest> AcceptTime(int requestId)
        {
            var result = await pickUpRequestDAO.AcceptTime(requestId);

            if (result != null)
            {
                var post = await postDAO.GetPostById(result.PostId);

                if (post != null)
                {
                    string senderName = "Back2Me";
                    string senderEmail = "baoandng07@gmail.com";
                    string toName = post.User?.FirstName + " " + post.User?.LastName;
                    string toEmail = post.User?.Email;
                    string subject = "Pickup Time Confirmed by Admin!";
                    string content = $@"
                    <html>
                    <head>
                      <style>
                        body {{
                          font-family: 'Segoe UI', Arial, sans-serif;
                          background-color: #f5f7fb;
                          padding: 20px;
                          color: #1f2a44;
                        }}
                        .container {{
                          max-width: 600px;
                          margin: auto;
                          background: #ffffff;
                          border-radius: 14px;
                          box-shadow: 0 6px 18px rgba(0,0,0,0.08);
                          overflow: hidden;
                        }}
                        .header {{
                          background: linear-gradient(135deg, #22c55e, #16a34a);
                          color: #fff;
                          padding: 24px;
                          text-align: center;
                        }}
                        .header h2 {{
                          margin: 0;
                          font-size: 22px;
                        }}
                        .content {{
                          padding: 24px;
                        }}
                        .content p {{
                          margin: 12px 0;
                          font-size: 15px;
                        }}
                        .status-box {{
                          background: #ecfdf5;
                          border-left: 5px solid #22c55e;
                          padding: 16px 18px;
                          border-radius: 10px;
                          margin: 20px 0;
                        }}
                        .info-card {{
                          background: #f9fafc;
                          border-radius: 10px;
                          padding: 16px;
                          margin: 18px 0;
                        }}
                        .info-row {{
                          display: flex;
                          justify-content: space-between;
                          padding: 6px 0;
                          font-size: 14px;
                        }}
                        .info-row span:first-child {{
                          color: #6b7280;
                        }}
                        .btn {{
                          display: inline-block;
                          background-color: #16a34a;
                          color: #fff !important;
                          font-weight: 600;
                          font-size: 15px;
                          padding: 12px 26px;
                          border-radius: 22px;
                          margin-top: 16px;
                        }}
                        .footer {{
                          background: #f1f3f7;
                          padding: 16px;
                          text-align: center;
                          font-size: 13px;
                          color: #6b7280;
                        }}
                      </style>
                    </head>

                    <body>
                      <div class='container'>
                        <div class='header'>
                          <h2>✅ Pickup Time Confirmed by Admin</h2>
                        </div>

                        <div class='content'>
                          <p>Hi <strong>{post.User?.FirstName} {post.User?.LastName}</strong>,</p>

                          <p>
                            Good news! Your pickup request has been <strong>approved</strong>.
                            You can come to pick up the item at the confirmed time below.
                          </p>

                          <div class='status-box'>
                            <strong>📍 Pickup Schedule Confirmed</strong>
                            <p>Please arrive on time and bring any required identification if needed.</p>
                          </div>

                          <div class='info-card'>
                            <div class='info-row'>
                              <span>📦 Item <strong>{post.Title}</strong></span>
                            </div>
                            <div class='info-row'>
                              <span>🗓 Pickup Time <strong>{result.PickUpDate}</strong></span>
                            </div>
                            <div class='info-row'>
                              <span>📍 Location <strong>Media Center</strong></span>
                            </div>
                          </div>

                          <p style='text-align:center;'>
                            <a href='https://back2me.vercel.app/detail-post/{post.PostId}' class='btn'>
                              📄 View Pickup Details
                            </a>
                          </p>

                          <p>
                            If you cannot make it on time, please contact us as soon as possible.
                          </p>
                        </div>

                        <div class='footer'>
                          <p>
                            Best regards,<br/>
                            <strong>Back2me Team</strong><br/>
                            This is an automated notification — please do not reply.
                          </p>
                        </div>
                      </div>
                    </body>
                    </html>
                    ";

                    await emailSender.SendEmail(senderName, senderEmail, toName, toEmail, subject, content);
                }
            }

            return result;
        }
        #endregion

        #region Accept Time Rescheduled
        public async Task<PickUpRequest> AcceptTimeRescheduled(int requestId)
        {
            var result = await pickUpRequestDAO.AcceptTime(requestId);

            if (result != null)
            {
                var post = await postDAO.GetPostById(result.PostId);
                var admin = usersDAO.GetAdmin();

                if (post != null && admin != null)
                {
                    string senderName = "Back2Me";
                    string senderEmail = "baoandng07@gmail.com";
                    string toName = admin.FirstName + " " + admin.LastName;
                    string toEmail = admin.Email;
                    string subject = "Pickup Time Accepted by User!";
                    string content = $@"
                    <html>
                    <head>
                      <style>
                        body {{
                          font-family: 'Segoe UI', Arial, sans-serif;
                          background-color: #f5f7fb;
                          padding: 20px;
                          color: #1f2a44;
                        }}
                        .container {{
                          max-width: 600px;
                          margin: auto;
                          background: #ffffff;
                          border-radius: 14px;
                          box-shadow: 0 6px 18px rgba(0,0,0,0.08);
                          overflow: hidden;
                        }}
                        .header {{
                          background: linear-gradient(135deg, #22c55e, #16a34a);
                          color: #fff;
                          padding: 24px;
                          text-align: center;
                        }}
                        .header h2 {{
                          margin: 0;
                          font-size: 22px;
                        }}
                        .content {{
                          padding: 24px;
                        }}
                        .content p {{
                          margin: 12px 0;
                          font-size: 15px;
                        }}
                        .status-box {{
                          background: #ecfdf5;
                          border-left: 5px solid #22c55e;
                          padding: 16px 18px;
                          border-radius: 10px;
                          margin: 20px 0;
                        }}
                        .info-card {{
                          background: #f9fafc;
                          border-radius: 10px;
                          padding: 16px;
                          margin: 18px 0;
                        }}
                        .info-row {{
                          display: flex;
                          justify-content: space-between;
                          padding: 6px 0;
                          font-size: 14px;
                        }}
                        .info-row span:first-child {{
                          color: #6b7280;
                        }}
                        .btn {{
                          display: inline-block;
                          background-color: #16a34a;
                          color: #fff !important;
                          font-weight: 600;
                          font-size: 15px;
                          padding: 12px 26px;
                          border-radius: 22px;
                          margin-top: 16px;
                        }}
                        .footer {{
                          background: #f1f3f7;
                          padding: 16px;
                          text-align: center;
                          font-size: 13px;
                          color: #6b7280;
                        }}
                      </style>
                    </head>

                    <body>
                      <div class='container'>
                        <div class='header'>
                          <h2>✅ Pickup Time Accepted by {result.Post?.User?.FirstName} {result.Post?.User?.LastName}</h2>
                        </div>

                        <div class='content'>
                          <p>Hi <strong>{admin.FirstName} {admin.LastName}</strong>,</p>

                          <p>
                            The user has <strong>accepted</strong> the updated pickup time for the following item.
                            The pickup schedule is now fully confirmed.
                          </p>

                          <div class='status-box'>
                            <strong>📍 Pickup Schedule Confirmed</strong>
                            <p>No further action is required at this time.</p>
                          </div>

                          <div class='info-card'>
                            <div class='info-row'>
                              <span>📦 Item <strong>{post.Title}</strong></span>
                            </div>
                            <div class='info-row'>
                              <span>🗓 Pickup Time <strong>{result.PickUpDate}</strong></span>
                            </div>
                            <div class='info-row'>
                              <span>📍 Location <strong>Media Center</strong></span>
                            </div>
                          </div>

                          <p style='text-align:center;'>
                            <a href='https://back2me.vercel.app/detail-post/{post.PostId}' class='btn'>
                              📄 View Pickup Details
                            </a>
                          </p>

                          <p>
                            The user will arrive according to the confirmed schedule.
                          </p>
                        </div>

                        <div class='footer'>
                          <p>
                            <p>
                            Best regards,<br/>
                            <strong>Back2me Team</strong><br/>
                            This is an automated notification — please do not reply.
                          </p>
                          </p>
                        </div>
                      </div>
                    </body>
                    </html>
                    ";

                    await emailSender.SendEmail(senderName, senderEmail, toName, toEmail, subject, content);
                }
            }

            return result;
        }
        #endregion

        #region Change Time
        public async Task<PickUpRequest> ChangeTime(int requestId, DateTime date)
        {
            var result = await pickUpRequestDAO.ChangeTime(requestId, date);

            if (result != null)
            {
                var post = await postDAO.GetPostById(result.PostId);

                if (post != null)
                {
                    string senderName = "Back2Me";
                    string senderEmail = "baoandng07@gmail.com";
                    string toName = post.User?.FirstName + " " + post.User?.LastName;
                    string toEmail = post.User?.Email;
                    string subject = "Pickup Time Updated";
                    string content = $@"
                    <html>
                    <head>
                      <style>
                        body {{
                          font-family: 'Segoe UI', Arial, sans-serif;
                          background-color: #f5f7fb;
                          padding: 20px;
                          color: #1f2a44;
                        }}
                        .container {{
                          max-width: 600px;
                          margin: auto;
                          background: #ffffff;
                          border-radius: 14px;
                          box-shadow: 0 6px 18px rgba(0,0,0,0.08);
                          overflow: hidden;
                        }}
                        .header {{
                          background: linear-gradient(135deg, #3b82f6, #2563eb);
                          color: #fff;
                          padding: 24px;
                          text-align: center;
                        }}
                        .content {{
                          padding: 24px;
                        }}
                        .status-box {{
                          background: #eff6ff;
                          border-left: 5px solid #3b82f6;
                          padding: 16px;
                          border-radius: 10px;
                          margin: 20px 0;
                        }}
                        .info-card {{
                          background: #f9fafc;
                          border-radius: 10px;
                          padding: 16px;
                        }}
                        .info-row {{
                          display: flex;
                          justify-content: space-between;
                          padding: 6px 0;
                          font-size: 14px;
                        }}
                        .btn {{
                          display: inline-block;
                          background-color: #2563eb;
                          color: #fff !important;
                          font-weight: 600;
                          padding: 12px 26px;
                          border-radius: 22px;
                          margin-top: 16px;
                        }}
                        .footer {{
                          background: #f1f3f7;
                          padding: 16px;
                          text-align: center;
                          font-size: 13px;
                          color: #6b7280;
                        }}
                      </style>
                    </head>

                    <body>
                      <div class='container'>
                        <div class='header'>
                          <h2>🔄 Pickup Time Updated</h2>
                        </div>

                        <div class='content'>
                          <p>Hi <strong>{post.User?.FirstName} {post.User?.LastName}</strong>,</p>

                          <p>
                            The admin has updated your pickup schedule.
                            Please check the new pickup time below.
                          </p>

                          <div class='status-box'>
                            <strong>🕒 Updated Pickup Time</strong>
                            <p>This change helps ensure a smoother pickup process.</p>
                          </div>

                          <div class='info-card'>
                            <div class='info-row'>
                              <span>📦 Item <strong>{post.Title}</strong></span>
                            </div>
                            <div class='info-row'>
                              <span>🗓 New Pickup Time <strong>{result.PickUpDate}</strong></span>
                            </div>
                          </div>

                          <p style='text-align:center;'>
                            <a href='https://back2me.vercel.app/detail-post/{post.PostId}' class='btn'>
                              View Updated Details
                            </a>
                          </p>
                        </div>

                        <div class='footer'>
                        <p>
                            Best regards,<br/>
                            <strong>Back2me Team</strong><br/>
                            This is an automated notification — please do not reply.
                          </p>
                        </div>
                      </div>
                    </body>
                    </html>
                    ";

                    await emailSender.SendEmail(senderName, senderEmail, toName, toEmail, subject, content);
                }
            }

            return result;
        }
        #endregion

        #region Search Request
        public IQueryable<PickUpRequest> SearchRequest(string query)
        {
            return pickUpRequestDAO.SearchRequest(query);
        }
        #endregion

        #region Delete Pick Up Request
        public async Task<bool> DeletePickUpRequest(int postId)
        {
            var isDeleted = await pickUpRequestDAO.DeletePickUpRequest(postId);
            return isDeleted;
        }
        #endregion
    }
}
