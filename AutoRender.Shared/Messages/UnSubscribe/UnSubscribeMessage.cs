namespace WebSocketMessaging.UnSubscribe {
    public abstract class UnSubscribeMessage : RequestMessage {
        public UnSubscribeMessage(): base(MessageType.UnSubscribe) { }
    }
}
