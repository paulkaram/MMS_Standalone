using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using MMS.DAL.Enumerations;
using MMS.DTO;
using MMS.DTO.Meetings;
using StackExchange.Redis;
using System.Security.Claims;
using IDatabase = StackExchange.Redis.IDatabase;

namespace MMS.API.Common.Hubs
{
    [Authorize]
    public class IntalioHub : Hub, IMainHub
    {
        private readonly IHubContext<IntalioHub> hubContext;

        public static Dictionary<int, HashSet<string>> meetingsAttendees = new Dictionary<int, HashSet<string>>();
        private readonly IDatabase redisDatabase; // Redis database instance


        public IntalioHub(IHubContext<IntalioHub> hubContext, IConnectionMultiplexer redis)
        {
            this.hubContext = hubContext;
			redisDatabase = redis.GetDatabase(); // Get Redis database

		}

		public override async Task OnConnectedAsync()
        {
            if (Context.User != null)
            {
                var userClaim = Context.User.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier);
                if (userClaim != null)
                {
                    if (userClaim.Value != null)
                    {
						await redisDatabase.SetAddAsync("ConnectedUsers", userClaim.Value);

					    await hubContext.Clients.All.SendAsync("UserOnline", userClaim.Value);
						
                    }
                }
            }
			await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (Context.User != null)
            {
                var userClaim = Context.User.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier);
                if (userClaim != null)
                {
					await redisDatabase.SetRemoveAsync("ConnectedUsers", userClaim.Value);
					await hubContext.Clients.All.SendAsync("UserOnline", userClaim.Value);

                    // Clean up meeting attendance when user disconnects
                    var meetingsToUpdate = meetingsAttendees
                        .Where(m => m.Value.Contains(userClaim.Value))
                        .Select(m => m.Key)
                        .ToList();

                    foreach (var meetingId in meetingsToUpdate)
                    {
                        meetingsAttendees[meetingId].Remove(userClaim.Value);
                        await Clients.All.SendAsync("NotifyMeetingAttendanceChange", meetingId, meetingsAttendees[meetingId]);
                    }
                }
            }
           await base.OnDisconnectedAsync(exception);
        }

        public async Task NotifyAllClients()
        {
            await hubContext.Clients.All.SendAsync("ReceiveChanges");
        }

        public async Task NotifyUser(string username)
        {
            await hubContext.Clients.User(username).SendAsync("ReceiveChanges");
        }

        public async Task NotifyUsers(string[] usernames)
        {
            await hubContext.Clients.Users(usernames).SendAsync("ReceiveChanges");
        }

        public async Task ClaimTask(string username)
        {
            await hubContext.Clients.User(username).SendAsync("ReceiveClaim");
        }

        public async Task UserOnline()
        {
            await hubContext.Clients.All.SendAsync("UserOnline");
        }

        public async Task<List<string>> GetConnectedIdsAsync()
        {
			var userIds = await redisDatabase.SetMembersAsync("ConnectedUsers");

			// Convert the Redis values to a List<string>
			return userIds.Select(u => u.ToString()).Distinct().ToList();
        }

        public async Task NotifyChatUsers(string[] usernames)
        {
            await hubContext.Clients.Users(usernames).SendAsync("ChatChanges");
        }

        public async Task NotifyMeetingAgendaChange(string[] usernames,int meetingId, List<LiveMeetingAgendaDto> meetingAgenda)
        {
            // Broadcast to all clients - meetingId in event name ensures only relevant clients react
            await hubContext.Clients.All.SendAsync($"meetingagendachanges{meetingId}", meetingAgenda);
        }
        public async Task NotifyMeetingStatusChange(string[] usernames,int meetingId, MeetingStatusDbEnum meetingStatusDbEnum)
        {
            await hubContext.Clients.Users(usernames).SendAsync($"MeetingStatusChange{meetingId}", (int)meetingStatusDbEnum);
        }

        public async Task NotifyChatMessagesCount(string[] usernames)
        {
            await hubContext.Clients.Users(usernames).SendAsync("ChatMessagesCountChanges");
        }
        public async Task NotifyMeetingChatUsers(int meetingId, string[] usernames)
        {
            // Broadcast to all clients - the meetingId in event name ensures only relevant clients react
            await hubContext.Clients.All.SendAsync($"meetingchatchanges{meetingId}");
        }
        public async Task ChangeMeetingAttendanceStatus(int MeetingId, bool InMeeting)
        {
            try
            {
                if (Context.User != null)
                {

                    var userClaim = Context.User.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier);

                    if (userClaim != null)
                    {

                        if (!meetingsAttendees.ContainsKey(MeetingId))
                        {
                            meetingsAttendees[MeetingId] = new HashSet<string>();

                        }
                        if (InMeeting)
                        {
                            meetingsAttendees[MeetingId].Add(userClaim.Value);
                        }
                        else
                        {
                            meetingsAttendees[MeetingId].Remove(userClaim.Value);
                        }

                        await Clients.All.SendAsync(method: "NotifyMeetingAttendanceChange", MeetingId, meetingsAttendees[MeetingId]);
                    }
                }
            }
            catch (Exception e)
            {


            }

        }

        public Task<string[]> GetMeetingOnlineAttendees(int MeetingId)
        {
            if (meetingsAttendees.ContainsKey(MeetingId))
            {
                return Task.FromResult(meetingsAttendees[MeetingId].ToArray());
            }
            return Task.FromResult(Array.Empty<string>());
        }

		public async Task NotifyMeetingAttachmentsChange(string[] usernames,int meetingId, List<AttachmentListItemDto> meetingAttachments)
		{
			await hubContext.Clients.Users(usernames).SendAsync($"MeetingAttachmentsChanges{meetingId}", meetingAttachments);
		}
		public async Task NotifyNewMeetingAgendaBegins(string[] usernames, LiveMeetingAgendaDto meetingAgenda , string meetingTitle)
		{
			await hubContext.Clients.Users(usernames).SendAsync("NewMeetingAgendaBegins", meetingAgenda,meetingTitle);
		}

		public async Task NotifyMeetingAgendaNotesChange(int meetingId, int agendaId)
		{
			// Broadcast to all clients in the meeting - the meetingId in event name ensures only relevant clients react
			await hubContext.Clients.All.SendAsync($"meetingagendanoteschanges{meetingId}", agendaId);
		}
	}
}
