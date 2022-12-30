using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameFramework.Utils;
using WebSocketSharp;
using WebSocketSharp.Net;

namespace GameFramework.Runtime.Network
{
    public sealed class WebSocketChannel : IChannel
    {
        private WebSocket webSocket;
        private IChannelHandler channelHandler;
        private IChannelContext channelContext;
        private TaskCompletionSource connectTask;
        private TaskCompletionSource closedTask;
        private Queue<WritedTask> waitingSendMessages;


        public bool active
        {
            get;
            private set;
        }

        public string name
        {
            get;
            private set;
        }

        public string url { get; private set; }

        public async Task Connect(string name, string url, IChannelHandler channelHandler)
        {
            this.url = url;
            connectTask = new TaskCompletionSource();
            this.name = name;
            this.active = false;
            this.webSocket = new WebSocket(url);
            this.channelHandler = channelHandler;
            this.webSocket.OnOpen += OnConnectCompletedCallback;
            this.webSocket.OnMessage += OnMessageReceived;
            this.webSocket.OnClose += OnClosed;
            this.webSocket.OnError += OnError;
            this.webSocket.Connect();
            await connectTask.Task;
        }

        private void OnConnectCompletedCallback(object sender, EventArgs e)
        {
            if (connectTask == null)
            {
                return;
            }
            connectTask.Complete();
            connectTask = null;
            if (!webSocket.IsConnected)
            {
                this.active = false;
                return;
            }
            this.active = true;
            waitingSendMessages = new Queue<WritedTask>();
            this.channelContext = new DefaultChannelContext(this);
            this.channelHandler.ChannelActive(channelContext);
        }

        private void OnMessageReceived(object sender, MessageEventArgs args)
        {
            if (this.channelHandler == null)
            {
                return;
            }
            UnityEngine.Debug.Log("web socket recv:" + args.RawData.Length);
            this.channelHandler.ChannelRead(channelContext, args.RawData);
        }

        private void OnClosed(object sender, CloseEventArgs args)
        {
            if (this.channelHandler == null)
            {
                return;
            }
            UnityEngine.Debug.Log(args.Reason);
            this.channelHandler.ChannelInactive(this.channelContext);
            closedTask.Complete();
            closedTask = null;
        }

        private void OnError(object sender, ErrorEventArgs args)
        {
            if (this.channelHandler == null)
            {
                return;
            }
            this.channelHandler.ChannelErrored(this.channelContext, args.Message);
        }

        public Task Disconnect()
        {
            if (active == false)
            {
                return Task.FromException(new Exception("the channel is not connected"));
            }
            closedTask = new TaskCompletionSource();
            webSocket.Close();
            return closedTask.Task;
        }

        public void Dispose()
        {
            webSocket = null;
            channelContext.Dispose();
            channelContext = null;
            channelHandler.Dispose();
            channelHandler = null;
        }

        public void Flush()
        {
            while (waitingSendMessages.TryDequeue(out WritedTask writed))
            {
                webSocket.Send(writed.data);
                writed.taskCompletionSource.Complete();
                UnityEngine.Debug.Log("web socket send:" + writed.data.Length);
            }
            waitingSendMessages.Clear();
        }

        public Task WriteAsync(byte[] stream)
        {
            if (active == false)
            {
                return Task.FromException(new Exception("the channel is not connected"));
            }
            WritedTask writed = new WritedTask(stream);
            waitingSendMessages.Enqueue(writed);
            return writed.taskCompletionSource.Task;
        }

        class WritedTask
        {
            public byte[] data { get; }
            public TaskCompletionSource taskCompletionSource { get; }

            public WritedTask(byte[] dataStream)
            {
                this.data = dataStream;
                this.taskCompletionSource = new TaskCompletionSource();
            }
        }
    }
}

