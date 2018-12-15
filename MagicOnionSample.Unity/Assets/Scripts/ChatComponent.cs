using Grpc.Core;
using MagicOnion;
using MagicOnion.Client;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class ChatComponent : MonoBehaviour, IChatHubReceiver
{
    private IChatHub _chutHub;
    private bool _isJoin;

    //受信メッセージ
    public Text ChatText;

    //入室・退室
    public Button JoinOrLeaveButton;
    public Text JoinOrLeaveButtonText;

    //テキスト送信
    public Button SendMessageButton;
    public InputField Input;

    // Start is called before the first frame update
    void Start()
    {
        this._isJoin = false;

        //Client側のHubの初期化
        var channel = new Channel("localhost:12345", ChannelCredentials.Insecure);
        this._chutHub = StreamingHubClient.Connect<IChatHub, IChatHubReceiver>(channel, this);

        //メッセージ送信ボタンはデフォルト非表示
        this.SendMessageButton.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

    }

    #region Client -> Server

    /// <summary>
    /// 参加してなかったら参加する。
    /// 参加してたら退室する。
    /// </summary>
    public async void JoinOrLeave()
    {
        if (this._isJoin)
        {
            await this._chutHub.LeaveAsync();
            this._isJoin = false;
            this.JoinOrLeaveButtonText.text = "入室する";
            //メッセージ送信ボタンを非表示に
            this.SendMessageButton.gameObject.SetActive(false);
        }
        else
        {
            await this._chutHub.JoinAsync(this.Input.text);
            this._isJoin = true;
            this.JoinOrLeaveButtonText.text = "退室する";
            //メッセージ送信ボタンを表示
            this.SendMessageButton.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// メッセージを送信
    /// </summary>
    public async void SendMessage()
    {
        //入室してなかったら何もしない
        if (!this._isJoin)
            return;

        await this._chutHub.SendMessageAsync(this.Input.text);
    }


    #endregion  

    #region Client <- Server

    public void OnJoin(string name)
    {
        this.ChatText.text += $"\n{name}さんが入室しました";
    }

    public void OnLeave(string name)
    {
        this.ChatText.text += $"\n{name}さんが退室しました";
    }

    public void OnSendMessage(string name, string message)
    {
        this.ChatText.text += $"\n{name}：{message}";
    }
    #endregion
}

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
