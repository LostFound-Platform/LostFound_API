using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ObjectBusiness;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class HolderReminderService : BackgroundService
    {
        private readonly IServiceScopeFactory scopeFactory;

        public HolderReminderService(IServiceScopeFactory scopeFactory)
        {
            this.scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var timer = new PeriodicTimer(TimeSpan.FromHours(24)); // Each 24 hours

            while (await timer.WaitForNextTickAsync())
            {
                using var scope = scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<FBLADbContext>(); // Used to connect Entity/DB
                var emailSender = scope.ServiceProvider.GetRequiredService<EmailSender>();

                var twoDaysAgo = DateTime.UtcNow.AddDays(-2);

                var holders = await db.Posts
                              .Include(p => p.User)
                              .Where(p => p.TypePost == TypePost.Found &&
                                          p.CreatedAt <= twoDaysAgo)
                              .OrderByDescending(p => p.CreatedAt)
                              .ToListAsync();

                foreach (var holder in holders)
                {
                    string senderName = "Back2Me";
                    string senderEmail = "baoandng07@gmail.com";
                    string toName = holder.User?.FirstName + " " + holder.User?.LastName;
                    string toEmail = holder.User?.Email;
                    string subject = "⏰ Reminder: Found Item Pending Admin Action";
                    string content = $@"
                    <html>
                    <head>
                      <style>
                        body {{
                            font-family: 'Segoe UI', Arial, sans-serif;
                            background-color: #f9f9fb;
                            color: #072138;
                            margin: 0;
                            padding: 20px;
                            line-height: 1.6;
                        }}
                        .container {{
                            max-width: 600px;
                            margin: auto;
                            background: #ffffff;
                            border-radius: 12px;
                            box-shadow: 0 4px 12px rgba(0,0,0,0.08);
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
                            margin: 12px 0;
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
                            color: #fff !important;
                            font-weight: 600;
                            font-size: 16px;
                            padding: 12px 25px;
                            border-radius: 20px;
                            text-decoration: none;
                            transition: all 0.3s ease-in-out;
                            cursor: pointer;
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
                          <h2>⏰ Pending Found Item!</h2>
                        </div>
                        <div class='content'>
                          <p>Hi <strong>{holder.User?.FirstName} {holder.User?.LastName}</strong>,</p>
                          <p>A <strong>Found Item</strong> is currently held but <strong>has not been transferred to the admin</strong> yet. Please review it to ensure proper processing.</p>

                          <table class='details'>
                            <tr>
                              <th>Title</th>
                              <td>{holder.Title}</td>
                            </tr>
                            <tr>
                              <th>Posted On</th>
                              <td>{holder.CreatedAt:MMMM dd, yyyy}</td>
                            </tr>
                          </table>

                          <div class='highlight'>
                            ⚡ Reminder: Please review the found item and transfer it to admin promptly.
                          </div>

                          <p style='text-align: center;'>
                             <a href='https://back2me.vercel.app/detail-post/{holder.PostId}' class='btn'>🔎 Review Found Item</a>
                          </p>

                          <p>Act quickly to ensure lost items are properly handled!</p>
                        </div>
                        <div class='footer'>
                          <p>Thanks for using <strong>Back2me</strong>!<br/>We prioritize proper handling of found items.</p>
                        </div>
                      </div>
                    </body>
                    </html>
                    ";

                    await emailSender.SendEmail(senderName, senderEmail, toName, toEmail, subject, content);
                }
            }
        }
    }
}
