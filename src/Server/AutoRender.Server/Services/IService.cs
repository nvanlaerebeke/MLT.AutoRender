namespace AutoRender.Server.Services {

    internal interface IService {

        void Start();

        void Stop();

        byte Priority { get; }
    }
}