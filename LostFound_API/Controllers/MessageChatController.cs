using FBLA_API.DTOs.Chat;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Linq;
using ObjectBusiness;
using Repository;
using Services;
using SignalRLayer;
using System;
using System.Security.Claims;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FBLA_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessageChatController : ControllerBase
    {
        #region Variables
        private readonly IPostRepository postRepository;
        private readonly IUsersRepository userRepository;
        private readonly IChatRepository chatRepository;
        private readonly IMessageChatRepository messageChatRepository;
        private readonly IHubContext<SystemHub> hubContext;
        private readonly EmailSender emailSender;
        #endregion

        #region Constructor
        public MessageChatController(IPostRepository postRepository,
                               IUsersRepository userRepository,
                               IWebHostEnvironment webHost,
                               IChatRepository chatRepository,
                               IHubContext<SystemHub> hubContext,
                               IMessageChatRepository messageChatRepository,
                               EmailSender emailSender)
        {
            this.postRepository = postRepository;
            this.userRepository = userRepository;
            this.chatRepository = chatRepository;
            this.hubContext = hubContext;
            this.messageChatRepository = messageChatRepository;
            this.emailSender = emailSender;
        }
        #endregion

        // GET: api/<MessageChatController>
        [Authorize]
        [HttpGet("my-chats")]
        public async Task<ActionResult<List<MessageChat>>> AllChatsByUserId()
        {
            var userEmail = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userEmail == null)
            {
                return Unauthorized(new
                {
                    Message = "User unauthorized"
                });
            }

            var user = await userRepository.GetUserByEmail(userEmail);

            if (user == null)
            {
                return NotFound(new
                {
                    Message = "User does not found"
                });
            }

            var chats = await chatRepository.AllChatsByUserId(user.UserId);

            var result = chats.Select(c => new
            {
                ChatId = c.ChatId,
                UserAId = c.UserAId,
                UserBId = c.UserBId,
                MessageContent = c.MessageContent,
                IsRead = c.IsRead,
                FirstNameUserA = c.UserA?.FirstName,
                LastNameUserA = c.UserA?.LastName,
                AvatarUserA = c.UserA?.Avatar,
                UrlAvatarUserA = $"{Request.Scheme}://{Request.Host}/Uploads/{c.UserA?.Avatar}",
                FirstNameUserB = c.UserB?.FirstName,
                LastNameUserB = c.UserB?.LastName,
                DateSendMessage = c.DateSendMessage,
                PostId = c.PostId,
                Title = c.Title,
                AvatarUserB = c.UserB?.Avatar,
                UrlAvatarUserB = $"{Request.Scheme}://{Request.Host}/Uploads/{c.UserB?.Avatar}",
            });
            return Ok(result);
        }

        [Authorize]
        [HttpGet("chat/{chatId}")]
        public async Task<ActionResult<List<MessageChat>>> AllMessagesByChatId(int chatId)
        {
            var chats = await chatRepository.AllMessagesByChatId(chatId);

            var result = chats.Select(c => new
            {
                MessageChatId = c.MessageChatId,
                UserAId = c.UserAId,
                UserBId = c.UserBId,
                MessageContent = c.MessageContent,
                FirstNameUserA = c.FirstNameUserA,
                LastNameUserA = c.LastNameUserA,
                AvatarUserA = c.AvatarUserA,
                UrlAvatarUserA = $"{Request.Scheme}://{Request.Host}/Uploads/{c.AvatarUserA}",
                FirstNameUserB = c.LastNameUserB,
                LastNameUserB = c.LastNameUserB,
                UserSenderId = c.UserSenderId,
                PostId = c.PostId,
                AvatarUserB = c.AvatarUserB,
                UrlAvatarUserB = $"{Request.Scheme}://{Request.Host}/Uploads/{c.AvatarUserB}",
            });
            return Ok(result);
        }

        // GET api/<MessageChatController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<MessageChatController>
        [Authorize]
        [HttpPost]
        public async Task<ActionResult> SendMessage([FromBody] MessageChatDTO messageChatDTO)
        {
            var userEmail = User.FindFirst(ClaimTypes.NameIdentifier)?.Value; // User email currently signed in

            if (userEmail == null)
            {
                return Unauthorized(new
                {
                    Message = "User unauthorized"
                });
            }

            var user = await userRepository.GetUserByEmail(userEmail);

            // Check user if existed
            if (user == null)
            {
                return NotFound(new
                {
                    Message = "User does not found"
                });
            }

            // Check user if verified email
            if (!user.IsVerifiedEmail)
            {
                return Forbid();
            }

            var chatExisted = await chatRepository.GetChatById(messageChatDTO.ChatId);

            if (chatExisted == null)
            {
                var chat = new Chat
                {
                    ChatId = messageChatDTO.ChatId,
                    UserAId = messageChatDTO.UserAId,
                    UserBId = messageChatDTO.UserBId,
                    PostId = messageChatDTO.PostId,
                };
                var chatAdded = await chatRepository.CreateChat(chat); // Create Chat

                // Then create message
                var messageChat = new MessageChat
                {
                    UserSenderId = user.UserId,
                    ChatId = chatAdded.ChatId,
                    MessageContent = messageChatDTO.MessageContent,
                };
                var messageAdded = await messageChatRepository.SendMessage(messageChat);
                if (messageAdded != null)
                {
                    var userReceive = await userRepository.GetUserByID(
                        messageAdded.UserSenderId == chatAdded.UserAId ?
                        chatAdded.UserBId : chatAdded.UserAId);

                    // To send post title real time
                    var post = await postRepository.GetPostById(chat.PostId);

                    // Send email notification new chat
                    string senderName = "Back2Me";
                    string senderEmail = "baoandng07@gmail.com";
                    string toName = user.FirstName + " " + user.LastName;
                    string toEmail = userReceive.Email;
                    string subject = "💬 You have a new chat on Back2me";
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
                        .highlight {{
                            background: #fff6ee;
                            border-left: 4px solid #ec7207;
                            padding: 12px 15px;
                            margin: 15px 0;
                            border-radius: 6px;
                        }}
                        .sender {{
                            font-weight: 600;
                            color: #ec7207;
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
                          <h2>💬 New Chat Received</h2>
                        </div>

                        <div class='content'>
                          <p>Hi <strong>{chat.UserB.FirstName} {chat.UserB.LastName}</strong>,</p>

                          <p>You have received a new message on <strong>Back2me</strong>.</p>

                          <div class='highlight'>
                            <p>
                              <span class='sender'>{user.FirstName} {user.LastName}</span> has sent you a message.
                            </p>
                            <p style='margin-top: 8px; font-style: italic; color: #555;'>
                              “{messageAdded.MessageContent}”
                            </p>
                          </div>

                          <p style='text-align: center;'>
                            <a href='https://back2me.vercel.app/' class='btn'>
                              💬 Open Chat
                            </a>
                          </p>

                          <p style='font-size: 0.95em; color: #666;'>
                            If you are not available right now, you can reply later.
                          </p>
                        </div>

                        <div class='footer'>
                          <p>
                            Best regards,<br/>
                            <strong>Back2me Team</strong>
                          </p>
                          <p>
                            This is an automated message. Please do not reply to this email.
                          </p>
                        </div>
                      </div>
                    </body>
                    </html>
                    ";

                    await emailSender.SendEmail(senderName, senderEmail, toName, toEmail, subject, content);

                    // Send realtime new chat
                    await hubContext.Clients.User(userReceive.Email).SendAsync("ReceiveNewChat", new
                    {
                        Chat = new
                        {
                            ChatId = chat.ChatId,
                            UserAId = chat.UserAId,
                            UserBId = chat.UserBId,
                            MessageContent = messageAdded.MessageContent,
                            IsRead = chat.IsRead,
                            FirstNameUserA = chat.UserA?.FirstName,
                            LastNameUserA = chat.UserA?.LastName,
                            UrlAvatarUserA = $"{Request.Scheme}://{Request.Host}/Uploads/{chat.UserA?.Avatar}",
                            FirstNameUserB = chat.UserB?.FirstName,
                            LastNameUserB = chat.UserB?.LastName,
                            DateSendMessage = chat.CreatedAt,
                            PostId = chat.PostId,
                            Title = post.Title,
                            UrlAvatarUserB = $"{Request.Scheme}://{Request.Host}/Uploads/{chat.UserB?.Avatar}",
                        },
                        SendStatus = "Sent"
                    });

                    return Ok(new
                    {
                        Message = "Sent message successfully"
                    });
                }
            }
            else
            {
                // Create only message
                var messageChat = new MessageChat
                {
                    UserSenderId = user.UserId,
                    ChatId = chatExisted.ChatId,
                    MessageContent = messageChatDTO.MessageContent,
                };
                var messageAdded = await messageChatRepository.SendMessage(messageChat);
                if (messageAdded != null)
                {
                    var userReceive = await userRepository.GetUserByID(
                        messageAdded.UserSenderId == chatExisted.UserAId ?
                        chatExisted.UserBId : chatExisted.UserAId);

                    // Send email notification new message
                    string senderName = "Back2Me";
                    string senderEmail = "baoandng07@gmail.com";
                    string toName = user.FirstName + " " + user.LastName;
                    string toEmail = userReceive.Email;
                    string subject = "💬 You have a new message on Back2me";
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
                        .highlight {{
                            background: #fff6ee;
                            border-left: 4px solid #ec7207;
                            padding: 12px 15px;
                            margin: 15px 0;
                            border-radius: 6px;
                        }}
                        .sender {{
                            font-weight: 600;
                            color: #ec7207;
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
                          <h2>💬 New Message Received</h2>
                        </div>

                        <div class='content'>
                          <p>Hi <strong>{userReceive.FirstName} {userReceive.LastName}</strong>,</p>

                          <p>You have received a new message on <strong>Back2me</strong>.</p>

                          <div class='highlight'>
                            <p>
                              <span class='sender'>{user.FirstName} {user.LastName}</span> has sent you a message.
                            </p>
                            <p style='margin-top: 8px; font-style: italic; color: #555;'>
                              “{messageAdded.MessageContent}”
                            </p>
                          </div>

                          <p style='text-align: center;'>
                            <a href='https://back2me.vercel.app/' class='btn'>
                              💬 Open Chat
                            </a>
                          </p>

                          <p style='font-size: 0.95em; color: #666;'>
                            If you are not available right now, you can reply later.
                          </p>
                        </div>

                        <div class='footer'>
                          <p>
                            Best regards,<br/>
                            <strong>Back2me Team</strong>
                          </p>
                          <p>
                            This is an automated message. Please do not reply to this email.
                          </p>
                        </div>
                      </div>
                    </body>
                    </html>
                    ";

                    await emailSender.SendEmail(senderName, senderEmail, toName, toEmail, subject, content);

                    // Send realtime new message
                    await hubContext.Clients.Users(user.Email, userReceive.Email).SendAsync("ReceiveNewMessage", new
                    {
                        Message = new
                        {
                            MessageChatId = messageAdded.MessageChatId,
                            UserAId = messageAdded.UserAId,
                            UserBId = messageAdded.UserBId,
                            MessageContent = messageAdded.MessageContent,
                            FirstNameUserA = messageAdded.FirstNameUserA,
                            LastNameUserA = messageAdded.LastNameUserA,
                            UrlAvatarUserA = $"{Request.Scheme}://{Request.Host}/Uploads/{messageAdded.AvatarUserA}",
                            FirstNameUserB = messageAdded.LastNameUserB,
                            LastNameUserB = messageAdded.LastNameUserB,
                            UserSenderId = messageAdded.UserSenderId,
                            UrlAvatarUserB = $"{Request.Scheme}://{Request.Host}/Uploads/{messageAdded.AvatarUserB}",
                        },
                        SendStatus = "Sent"
                    });

                    return Ok(new
                    {
                        Message = "Sent message successfully"
                    });
                }
            }

            return BadRequest(new
            {
                Message = "Send message failed"
            });
        }

        // PUT api/<MessageChatController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<MessageChatController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
