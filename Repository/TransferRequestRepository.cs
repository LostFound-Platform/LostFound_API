using Azure.Core;
using DataAccess;
using Microsoft.AspNetCore.SignalR;
using ObjectBusiness;
using SignalRLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public class TransferRequestRepository : ITransferRequestRepository
    {
        #region Variables
        private readonly TransferRequestDAO transferRequestDAO;
        private readonly PostDAO postDAO;
        private readonly UsersDAO userDAO;
        private readonly IHubContext<SystemHub> hubContext;
        #endregion

        #region Constructor
        public TransferRequestRepository(TransferRequestDAO transferRequestDAO,
                                         IHubContext<SystemHub> hubContext,
                                         PostDAO postDAO,
                                         UsersDAO usersDAO)
        {
            this.hubContext = hubContext;
            this.transferRequestDAO = transferRequestDAO;
            this.postDAO = postDAO;
            this.userDAO = usersDAO;
        }
        #endregion

        #region Create Request
        public async Task<bool> CreateRequest(TransferRequests request)
        {
            var isAdded = await transferRequestDAO.CreateRequest(request);

            if (isAdded)
            {
                var user = await userDAO.GetUserByID(request.UserId);

                if (user != null)
                {
                    try
                    {
                        // Send to a specific student
                        await hubContext.Clients.User(user.Email).SendAsync("ReceiveStatusPost", new
                        {
                            Status = request.Status,
                            PostId = request.PostId,
                        });

                        var post = await postDAO.GetPostById(request.PostId);

                        await hubContext.Clients.Group("Admin").SendAsync("ReceiveNewRequest", new
                        {
                            Requests = new
                            {
                                NameItem = post.Title,
                                FirstName = user.FirstName,
                                LastName = user.LastName,
                                CreatedAt = request.CreatedAt,
                                Status = request.Status,
                                Role = user.Role,
                                RequestId = request.RequestId,
                                PostId = post.PostId,
                            },
                            Message = "An item handover is waiting for your confirmation",
                            CreatedAt = request.CreatedAt,
                        });
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
            return isAdded;
        }
        #endregion

        #region Cancel Request
        public async Task<TransferRequests> CancelRequest(int requestId)
        {
            var requestCancelled = await transferRequestDAO.CancelRequest(requestId);

            if (requestCancelled != null)
            {
                var user = await userDAO.GetUserByID(requestCancelled.UserId);

                if (user != null)
                {
                    // Send to a specific student
                    await hubContext.Clients.User(user.Email).SendAsync("ReceiveStatusPostCancelled", new
                    {
                        Status = requestCancelled.Status,
                        PostId = requestCancelled.PostId,
                    });
                }

                await hubContext.Clients.Group("Admin").SendAsync("ReceivedStatusRequestCancelled", new
                {
                    RequestId = requestId,
                    Status = requestCancelled.Status,
                });
            }

            return requestCancelled;
        }
        #endregion

        #region Cancel Request
        public async Task<TransferRequests> MarkReceived(int requestId)
        {
            var requestMarked = await transferRequestDAO.MarkReceived(requestId);

            if (requestMarked != null)
            {
                await hubContext.Clients.Group("Admin").SendAsync("ReceivedStatusRequestMarked", new
                {
                    RequestId = requestId,
                    Status = requestMarked.Status,
                });
            }

            return requestMarked;
        }
        #endregion

        #region All Requests
        public IQueryable<TransferRequests> AllRequests()
        {
            return transferRequestDAO.AllRequests();
        }
        #endregion

        #region Check Status Request Post
        public async Task<TransferRequests> CheckStatusRequestPost(int postId)
        {
            return await transferRequestDAO.CheckStatusRequestPost(postId);
        }
        #endregion

        #region Search Request
        public IQueryable<TransferRequests> SearchRequest(string query)
        {
            return transferRequestDAO.SearchRequest(query);
        }
        #endregion
    }
}
