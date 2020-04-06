using AutoRender.Data;
using Mitto.Messaging;

namespace AutoRender.Messaging.Request {

    public class UpdateSettingsRequest : RequestMessage {
        public ServerSettings Settings;

        public UpdateSettingsRequest(ServerSettings pSettings) {
            Settings = pSettings;
        }

        public UpdateSettingsRequest() : base() {
        }
    }
}