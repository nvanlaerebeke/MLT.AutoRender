using AutoRender.Messaging.Request;
using AutoRender.Messaging.Response;
using AutoRender.Server.Config;
using Mitto.IMessaging;
using Mitto.Messaging.Action;

namespace AutoRender.Messaging.Action.Request {

    public class GetSettingsRequestAction : RequestAction<GetSettingsRequest, GetSettingsResponse> {

        public GetSettingsRequestAction(IClient pClient, GetSettingsRequest pRequest) : base(pClient, pRequest) {
        }

        public override GetSettingsResponse Start() {
            return new GetSettingsResponse(Request) {
                BasePath = Settings.BasePath,
                FinalDirectory = Settings.FinalDirectory,
                LogDirectory = Settings.LogDirectory,
                MeltPath = Settings.MeltPath,
                NewDirectory = Settings.NewDirectory,
                ProjectDirectory = Settings.ProjectDirectory,
                Threads = Settings.Threads
            };
        }
    }
}