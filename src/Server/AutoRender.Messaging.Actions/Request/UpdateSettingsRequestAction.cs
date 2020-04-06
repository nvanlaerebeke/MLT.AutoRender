using AutoRender.Messaging.Request;
using AutoRender.Server.Config;
using Mitto.IMessaging;
using Mitto.Messaging.Action;
using Mitto.Messaging.Response;

namespace AutoRender.Messaging.Action.Request {

    public class UpdateSettingsRequestAction : RequestAction<UpdateSettingsRequest, ACKResponse> {

        public UpdateSettingsRequestAction(IClient pClient, UpdateSettingsRequest pRequest) : base(pClient, pRequest) {
        }

        public override ACKResponse Start() {
            Settings.FinalDirectory = Request.Settings.FinalDirectory;
            Settings.LogDirectory = Request.Settings.LogDirectory;
            Settings.MeltPath = Request.Settings.MeltPath;
            Settings.NewDirectory = Request.Settings.NewDirectory;
            Settings.ProjectDirectory = Request.Settings.ProjectDirectory;
            Settings.Threads = Request.Settings.Threads;
            return new ACKResponse(Request);
        }
    }
}