namespace InformaticaCloudClient
{
    /// <summary>
    /// DTO and stub for session data
    /// </summary>
    public class IodClientSession
    {
        public static string BaseAddress = "https://app.informaticaondemand.com";
        public string serverUrl { get; private set; }
        public string icSessionId { get; private set; }
        public bool IsConnected { get; private set; }

        public IodClientSession(string _serverUrl, string _sessionId, bool _isConnected)
        {
            serverUrl = _serverUrl;
            icSessionId = _sessionId;
            IsConnected = _isConnected;
        }

    }

    public class IodSessionStub
    {
        public string icSessionId;
        public string serverUrl;
    }
}
