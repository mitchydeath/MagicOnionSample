using MagicOnion;
using MagicOnion.Server.Hubs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MagicOnionServer
{
    public interface IChatHubReceiver
    {
        void OnJoin(string name);
        void OnLeave(string name);
        void OnSendMessage(string name, string message);
    }

    public interface IChatHub : IStreamingHub<IChatHub, IChatHubReceiver>
    {
        Task JoinAsync(string userName);
        Task LeaveAsync();
        Task SendMessageAsync(string message);
    }

    public class ChatHub : StreamingHubBase<IChatHub, IChatHubReceiver>, IChatHub
    {
        IGroup room;
        string me;
        IInMemoryStorage<string> storage;

        public async Task JoinAsync(string userName)
        {
            //ルーム名は固定
            const string roomName = "SampleRoom";
            (room, storage) = await this.Group.AddAsync(roomName, userName);
            me = userName;
            this.Broadcast(room).OnJoin(userName);
        }

        public async Task LeaveAsync()
        {
            await room.RemoveAsync(this.Context);
            this.Broadcast(room).OnLeave(me);
        }


        public async Task SendMessageAsync(string message)
        {
            this.Broadcast(room).OnSendMessage(me, message);
        }

        protected override ValueTask OnDisconnected()
        {
            return CompletedTask;
        }
    }
}
