using AutoRender.Messaging.Request;
using Mitto.IMessaging;
using Mitto.Messaging;

namespace AutoRender.Messaging.Response {

    public class GetSettingsResponse : ResponseMessage {
        public string BasePath { get; set; }
        public string FinalDirectory { get; set; }
        public string LogDirectory { get; set; }
        public string MeltPath { get; set; }
        public string NewDirectory { get; set; }
        public string ProjectDirectory { get; set; }
        public int Threads { get; set; }

        public GetSettingsResponse() {
        }

        public GetSettingsResponse(GetSettingsRequest pRequest) : base(pRequest) {
        }

        public GetSettingsResponse(RequestMessage pMessage, ResponseStatus pStatus) : base(pMessage, pStatus) {
        }
    }
}