using MagicOnion;
using MagicOnion.Server.Hubs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MagicOnionServer
{
    /// <summary>
    /// Server -> ClientのAPI
    /// </summary>
    public interface IChatHubReceiver
    {
        /// <summary>
        /// 誰かがチャットに参加したことをクライアントに伝える。
        /// </summary>
        /// <param name="name">参加した人の名前</param>
        void OnJoin(string name);
        /// <summary>
        /// 誰かがチャットから退室したことをクライアントに伝える。
        /// </summary>
        /// <param name="name">退室した人の名前</param>
        void OnLeave(string name);
        /// <summary>
        /// 誰かが発言した事をクライアントに伝える。
        /// </summary>
        /// <param name="name">発言した人の名前</param>
        /// <param name="message">メッセージ</param>
        void OnSendMessage(string name, string message);
    }

    /// <summary>
    /// CLient -> ServerのAPI
    /// </summary>
    public interface IChatHub : IStreamingHub<IChatHub, IChatHubReceiver>
    {
        /// <summary>
        /// 参加することをサーバに伝える
        /// </summary>
        /// <param name="userName">参加者の名前</param>
        /// <returns></returns>
        Task JoinAsync(string userName);
        /// <summary>
        /// 退室することをサーバに伝える
        /// </summary>
        /// <returns></returns>
        Task LeaveAsync();
        Task SendMessageAsync(string message);
    }

    public class ChatHub : StreamingHubBase<IChatHub, IChatHubReceiver>, IChatHub
    {
        IGroup room;
        string me;

        public async Task JoinAsync(string userName)
        {
            //ルームは全員固定
            const string roomName = "SampleRoom";
            //ルームに参加&ルームを保持
            this.room = await this.Group.AddAsync(roomName);
            //自分の名前も保持
            me = userName;
            //参加したことをルームに参加している全メンバーに通知
            this.Broadcast(room).OnJoin(userName);
        }

        public async Task LeaveAsync()
        {
            //ルーム内のメンバーから自分を削除
            await room.RemoveAsync(this.Context);
            //退室したことを全メンバーに通知
            this.Broadcast(room).OnLeave(me);
        }


        public async Task SendMessageAsync(string message)
        {
            //発言した内容を全メンバーに通知
            this.Broadcast(room).OnSendMessage(me, message);
        }

        protected override ValueTask OnDisconnected()
        {
            //nop
            return CompletedTask;
        }
    }
}
