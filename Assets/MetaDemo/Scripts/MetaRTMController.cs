using Agora.Rtm;
using UnityEngine;
using System.Threading.Tasks;

namespace Agora.Spaces.Controller
{
    /// <summary>
    ///   The MetaRTMController controls functionalities using the Signal SDK.
    /// It does not work on any game logics.
    /// </summary>
    public class MetaRTMController : MonoBehaviour
    {
        IRtmClient _rtmClient = null;
        IStreamChannel _streamChannel = null;
        IRtmPresence _presence = null;

        public uint PresenceTimeout = 1;

        public string TransformTopic = "TransformData";
        public static MetaRTMController Instance;

        /// <summary>
        ///   Remembers whether the Transform topic is joined
        /// </summary>
        bool _inTransformSyncTopic = false;

        string UserID;
        string ChannelName;

        MetaGameController GameController;

        /* Event handlers */
        public event System.Action OnLoginComplete;
        public event System.Action OnJoinStreamChannel;
        public event System.Action OnLeaveStreamChannel;
        public event System.Action<string> OnUserLeftStreamChannel;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(Instance);
            }

            Instance = this;
        }

        private void OnDestroy()
        {
            DeInit();
        }

        /// <summary>
        ///   Destructor
        /// </summary>
        internal void DeInit()
        {
            if (_rtmClient != null)
            {
                LeaveStreamChannel();
                ReleaseStreamChannel();
                LogoutAsync();
                _rtmClient.Dispose();
                _rtmClient = null;
            }
        }

        /// <summary>
        ///  The InitClient method controls the start of rtmClient; it will
        /// join the stream channel for transform sync in this step.
        /// </summary>
        /// <param name="game"></param>
        public async void InitClient(MetaGameController game)
        {
            GameController = game;
            UserID = GameApplication.Instance.UserName;
            ChannelName = GameApplication.Instance.RTMChannelName;

            RtmConfig config = new RtmConfig();
            config.appId = GameApplication.Instance.AppInfoInput.appID;
            config.userId = UserID;
            config.presenceTimeout = PresenceTimeout;
            config.useStringUserId = true;
            config.logConfig = new RtmLogConfig() { filePath = Application.persistentDataPath + "/" + GameController.GetLogName() };
            Debug.Log("Log path:" + config.logConfig.filePath);

            IRtmClient rtmClient = null;
            try
            {
                rtmClient = RtmClient.CreateAgoraRtmClient(config);
            }
            catch (RTMException e)
            {
                Debug.LogError($"rtmClient.init error: {e.Status.ErrorCode} reason: {e.Status.Reason}");
            }


            if (rtmClient != null)
            {
                //add observer
                rtmClient.OnMessageEvent += this.OnMessageEvent;
                //rtmClient.OnPresenceEvent += this.OnPresenceEvent;
                rtmClient.OnTopicEvent += this.OnTopicEvent;
                rtmClient.OnStorageEvent += this.OnStorageEvent;
                rtmClient.OnConnectionStateChanged += this.OnConnectionStateChanged;
                rtmClient.OnTokenPrivilegeWillExpire += this.OnTokenPrivilegeWillExpire;

                // rtmClient.SetParameters("{\"rtm.sync_ap_address\":[\"114.236.137.40\", 8443]}");
                _rtmClient = rtmClient;
                _presence = _rtmClient.GetPresence();
                await LoginAsync();
                InitStreamChannel(ChannelName);
            }
        }

        void InitStreamChannel(string channel)
        {
            Debug.Log("InitStreamChannel " + channel);
            _streamChannel = _rtmClient.CreateStreamChannel(channel);
            JoinStreamChannel();
        }

        /// <summary>
        ///   Join a stream channel with options and handle errors. 
        /// Invoke JoinTopic next.
        /// </summary>
        async void JoinStreamChannel()
        {
            if (this._streamChannel == null)
            {
                return;
            }
            var infoInput = GameApplication.Instance.AppInfoInput;
            string token = infoInput.rtmToken == "" ? infoInput.appID : infoInput.rtmToken;
            JoinChannelOptions joptions = new JoinChannelOptions()
            {
                token = token,
                withMetadata = true,
                withPresence = true
            };

            Debug.Log("JoinStreamChannel " + _streamChannel.GetChannelName());
            var result = await _streamChannel.JoinAsync(joptions);
            if (result.Status.Error)
            {
                Debug.LogError(string.Format("JoinStream Status.ErrorCode:{0} reason:{1}", result.Status.ErrorCode, result.Status.Reason));
            }
            else
            {
                string str = string.Format("JoinStream result.Response: channelName:{0} userId:{1}",
                    result.Response.ChannelName, result.Response.UserId);
                Debug.Log(str);
                _rtmClient.OnPresenceEvent += this.OnPresenceEvent;
            }

            JoinTopic(TransformTopic);
        }

        /// <summary>
        ///    Join a topic.
        /// </summary>
        /// <param name="topic"></param>
        public async void JoinTopic(string topic)
        {
            JoinTopicOptions toptions = new JoinTopicOptions()
            {
                qos = RTM_MESSAGE_QOS.ORDERED,
                priority = RTM_MESSAGE_PRIORITY.HIGH,
                meta = "",
                syncWithMedia = false
            };

            var result = await _streamChannel.JoinTopicAsync(topic, toptions);

            if (result.Status.Error)
            {
                Debug.LogError(string.Format("StreamChannel.JoinTopic Status.ErrorCode:{0} reason:{1}", result.Status.ErrorCode, result.Status.Reason));
            }
            else
            {
                string str = string.Format("StreamChannel.JoinTopic result.Response: channelName:{0} userId:{1} topic:{2} meta:{3}",
                  result.Response.ChannelName, result.Response.UserId, result.Response.Topic, result.Response.Meta);
                Debug.Log(str);
                _inTransformSyncTopic = true;

                //SubscribeTopic(topic);
                OnJoinStreamChannel?.Invoke();
            }
        }

        /// <summary>
        ///  Leave a topic
        /// </summary>
        /// <param name="topic"></param>
        public async void LeaveTopic(string topic)
        {
            var result = await _streamChannel.LeaveTopicAsync(topic);
            if (result.Status.Error)
            {
                Debug.LogError(string.Format("StreamChannel.LeaveTopic Status.ErrorCode:{0} reason:{1}", result.Status.ErrorCode, result.Status.Reason));
            }
            else
            {
                if (topic == TransformTopic)
                {

                    _inTransformSyncTopic = false;
                }
                OnLeaveStreamChannel?.Invoke();
            }
        }


        /// <summary>
        ///   Subscribe a topic
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="users"></param>
        public async void SubscribeTopic(string topic, string[] users)
        {
            TopicOptions options = new TopicOptions(users);

            var result = await _streamChannel.SubscribeTopicAsync(topic, options);

            if (result.Status.Error)
            {
                Debug.LogError(string.Format("StreamChannel.SubscribeTopic Status.ErrorCode:{0} ", result.Status.ErrorCode));
            }
            else
            {
                string successUsers = string.Join(',', result.Response.SucceedUsers);
                Debug.Log("Subscribed to topic " + topic + " +Users:" + successUsers);

                if (result.Response.FailedUsers.Length > 0)
                {
                    string failed = string.Join(',', result.Response.FailedUsers);
                    Debug.LogWarning("topic " + topic + " failed Users:" + failed);
                }

                //GameController.SyncSubscriptions(result.Response.SucceedUsers);
            }
        }

        /// <summary>
        ///   Publish the transfrom data by setting a state on Presence.
        /// </summary>
        /// <param name="json">JSON string of a serialized structure.</param>
        public async void PublishTransformState(string json)
        {
            StateItem posItem = new StateItem { key = "TransformData", value = json };
            await _presence?.SetStateAsync(ChannelName, RTM_CHANNEL_TYPE.STREAM,
                new StateItem[] { posItem }
            );
        }

        /// <summary>
        ///   LeaveStreamChannel
        /// </summary>
        public async void LeaveStreamChannel()
        {
            if (_streamChannel == null)
            {
                return;
            }

            _rtmClient.OnPresenceEvent -= this.OnPresenceEvent;

            var result = await _streamChannel.LeaveAsync();

            if (result.Status.Error)
            {
                Debug.LogError(string.Format("StreamChannel.Leave Status.ErrorCode:{0} reason:{1}", result.Status.ErrorCode, result.Status.Reason));
            }
            else
            {
                string str = string.Format("StreamChannel.Leave result.Response: channelName:{0} userId:{1}",
                    result.Response.ChannelName, result.Response.UserId);
                Debug.Log(str);
            }

            LeaveTopic(TransformTopic);
        }

        /// <summary>
        ///    Release Stream Channel
        /// </summary>
        void ReleaseStreamChannel()
        {
            _streamChannel.Dispose();
            _streamChannel = null;
        }

        /// <summary>
        ///    Publish transform data by posting 
        /// </summary>
        /// <param name="data"></param>
        public async void PublishTransformSync(byte[] data)
        {
            if (_inTransformSyncTopic)
            {
                TopicMessageOptions options = new TopicMessageOptions() { sendTs = default, customType = "binary" };
                var result = await _streamChannel.PublishTopicMessageAsync(TransformTopic, data, options);
                if (result.Status.Error)
                {
                    Debug.LogError("Login failed, error = " + result.Status.Reason);
                }
                else
                {
                    Debug.Log("PublishTransformSync, any error?" + result.Status.Error);

                }
            }
        }

        /// <summary>
        ///   Publish the transfrom data by setting a state on Presence.
        /// </summary>
        /// <param name="json"></param>
        public async void PublishTransformSync(string json)
        {
            if (_inTransformSyncTopic)
            {
                TopicMessageOptions options = new TopicMessageOptions() { sendTs = default, customType = "json" };

                var result = await _streamChannel.PublishTopicMessageAsync(TransformTopic, json, options);
                if (result.Status.Error)
                {
                    Debug.LogError("Publish topic message failed, error = " + result.Status.Reason);
                }
                else
                {
                    //Debug.Log("Sending TransformData OK.  Json = " + json);
                }
            }
        }

        /// <summary>
        ///   Login the the RTM Client
        /// </summary>
        /// <returns></returns>
        async Task<RtmStatus> LoginAsync()
        {
            // assume test mode AppID for empty token. In this case, AppID is the token.
            var infoInput = GameApplication.Instance.AppInfoInput;
            string token = infoInput.rtmToken == "" ? infoInput.appID : infoInput.rtmToken;
            var result = await _rtmClient.LoginAsync(token);

            if (result.Status.Error)
            {
                Debug.LogError("Login failed, error = " + result.Status.Reason);
            }
            else
            {
                Debug.Log("Login OK.");
                OnLoginComplete?.Invoke();
            }
            return result.Status;
        }

        /// <summary>
        ///   Logout the RTM Client
        /// </summary>
        async void LogoutAsync()
        {
            var result = await _rtmClient.LogoutAsync();

            if (result.Status.Error)
            {
                Debug.LogError("Login failed, error = " + result.Status.Reason);
            }
        }

        #region --- RTM Event Handlers ---
        /// <summary>
        ///    A stream message is received.
        /// </summary>
        /// <param name="event"></param>
        void OnMessageEvent(MessageEvent @event)
        {
            string str = string.Format("OnMessageEvent channelName:{0} channelTopic:{1} channelType:{2} publisher:{3} message:{4} customType:{5}",
              @event.channelName, @event.channelTopic, @event.channelType, @event.publisher, @event.message.GetData<string>(), @event.customType);
            //            Debug.Log(str);

            if (@event.channelName == ChannelName && @event.channelTopic == TransformTopic)
            {
                if (@event.customType == "binary")
                {
                    byte[] bytes = @event.message.GetData<byte[]>();
                    GameController.SyncManager.OnTransformUpdate(bytes);
                }
                else
                {
                    string json = @event.message.GetData<string>();
                    GameController.SyncManager.OnTransformUpdate(json);
                }
            }
        }

        /// <summary>
        ///   This is the core logic that does the bookkeeping of remote users
        /// </summary>
        /// <param name="event"></param>
        void OnPresenceEvent(PresenceEvent @event)
        {
            string str = string.Format("OnPresenceEvent: type:{0} channelType:{1} channelName:{2} publisher:{3} states:{4} snapshot:{5}" + $" count:{@event.stateItems.Length} ",
                @event.type, @event.channelType, @event.channelName, @event.publisher, @event.stateItems.ToMyString(), @event.snapshot.ToMyString());
            //Debug.Log(str);

            switch (@event.type)
            {
                case RTM_PRESENCE_EVENT_TYPE.REMOTE_JOIN:
                    if (@event.channelName == ChannelName)
                    {
                        // subscribe to the newly added user. Original group didn't include them
                        // SubscribeTopic(TransformTopic);
                        GetPresenceUser(HandleUserStateList);
                    }
                    break;
                case RTM_PRESENCE_EVENT_TYPE.REMOTE_LEAVE:
                    OnUserLeftStreamChannel(@event.publisher);
                    break;
                case RTM_PRESENCE_EVENT_TYPE.REMOTE_STATE_CHANGED:

                    foreach (var state in @event.stateItems)
                    {
                        if (state.key == "TransformData")
                        {
                            if (GameController.HasPlayer(@event.publisher))
                            {
                                GameController.ValidateScriptions(@event.publisher, state.value);
                            }
                            else
                            {
                                // in case snapshot or REMOTE_JOIN didn't get this
                                GameController.SpawnAvatar(@event.publisher, false, state.value);
                            }
                        }
                    }

                    break;
                case RTM_PRESENCE_EVENT_TYPE.SNAPSHOT:
                    //Debug.LogWarning("SNAPSHOT:" + @event.snapshot.ToMyString());
                    // GetState();
                    HandleUserStateList(@event.snapshot.userStateList);
                    break;
            }
        }

        /// <summary>
        ///   A handler that respond to the user state list
        /// </summary>
        /// <param name="userStates"></param>
        void HandleUserStateList(UserState[] userStates)
        {
            foreach (var user in userStates)
            {
                // if this is a remote user
                if (user.userId != UserID)
                {
                    foreach (var state in user.states)
                    {
                        if (state.key == "TransformData" && !GameController.HasPlayer(user.userId))
                        {
                            GameController.SpawnAvatar(user.userId, false, state.value);
                        }
                    }
                }
            }
        }

        /// <summary>
        ///   The Storage event callback
        /// </summary>
        /// <param name="event"></param>
        void OnStorageEvent(StorageEvent @event)
        {
            string str = string.Format("OnStorageEvent: channelType:{0} storageType:{1} eventType:{2} target:{3}",
                @event.channelType, @event.storageType, @event.eventType, @event.target);
            Debug.Log(str);
            if (@event.data != null)
            {
            }
        }

        /// <summary>
        ///   Topic event callback
        /// </summary>
        /// <param name="event"></param>
        void OnTopicEvent(TopicEvent @event)
        {
            string str = string.Format("OnTopicEvent: channelName:{0} publisher:{1}", @event.channelName, @event.publisher);
            Debug.Log(str);
        }

        /// <summary>
        ///  Handling connection states
        /// </summary>
        /// <param name="channelName"></param>
        /// <param name="state"></param>
        /// <param name="reason"></param>
        void OnConnectionStateChanged(string channelName, RTM_CONNECTION_STATE state, RTM_CONNECTION_CHANGE_REASON reason)
        {
            string str = string.Format("OnConnectionStateChange channelName {0}: state:{1} reason:{2}", channelName, state, reason);
            Debug.Log(str);
        }

        /// <summary>
        ///   Handle token expiration.  
        /// </summary>
        /// <param name="channelName"></param>
        void OnTokenPrivilegeWillExpire(string channelName)
        {
            string str = string.Format("OnTokenPrivilegeWillExpire channelName {0}", channelName);
            Debug.Log(str);

        }
        #endregion
        /// <summary>
        ///    Get the state of the presence
        /// </summary>
        /// <param name="player"></param>
        async void GetState(string player)
        {
            RTM_CHANNEL_TYPE channelType = RTM_CHANNEL_TYPE.STREAM;


            var result = await _presence.GetStateAsync(ChannelName, channelType, player);
            if (result.Status.Error)
            {
                Debug.LogError(string.Format("GetState Status.ErrorCode:{0} ", result.Status.ErrorCode));
            }
            else
            {
                var statesCount = result.Response.State.states == null ? 0 : result.Response.State.states.Length;
                string info2 = string.Format("GetState: userStateList userId:{0}, stateCount:{1}",
                    result.Response.State.userId, statesCount);
                //RtmScene.AddMessage(info2, Message.MessageType.Info);
                Debug.Log(info2);
                foreach (var stateItem in result.Response.State.states)
                {
                    string info3 = string.Format("key:{0},value:{1}", stateItem.key, stateItem.value);
                    Debug.Log(info3);
                    //RtmScene.AddMessage(info3, Message.MessageType.Info);
                    if (stateItem.key == "TransformData")
                    {
                        Debug.LogWarning("Getting new player " + player);
                        GameController.ValidateScriptions(player, stateItem.value);
                    }
                }
            }
        }

        /// <summary>
        ///   The the users from the Presence.  
        /// </summary>
        /// <param name="processUsers">Handler to process the user info</param>
        async void GetPresenceUser(System.Action<UserState[]> processUsers)
        {
            GetOnlineUsersOptions options = new GetOnlineUsersOptions()
            {
                includeUserId = true,
                includeState = true,
            };

            var result = await _presence.GetOnlineUsersAsync(ChannelName, RTM_CHANNEL_TYPE.STREAM, options);
            if (result.Status.Error)
            {
                Debug.LogError(string.Format("WhoNow Status.ErrorCode:{0} ", result.Status.ErrorCode));
            }
            else
            {
                var count = result.Response.UserStateList.Length;
                string info = string.Format("WhoNow result.Response : count:{0},nextPage:{1}",
                    count, result.Response.NextPage);
                Debug.Log(info);
                if (count > 0)
                {
                    for (int i = 0; i < result.Response.UserStateList.Length; i++)
                    {
                        var userState = result.Response.UserStateList[i];
                        var statesCount = userState.states == null ? 0 : userState.states.Length;
                        string info2 = string.Format("userStateList userId:{0}, stateCount:{1} states:{2}",
                            userState.userId, statesCount, userState.states.ToMyString());
                        Debug.Log(info2);
                    }
                }
                processUsers?.Invoke(result.Response.UserStateList);
            }
        }
    }
}