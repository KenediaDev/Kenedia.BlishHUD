using System;

namespace Kenedia.BlishHUD.API.Requests
{
    public enum RequestStatus
    {
        Pending,
        InProgress,
        Completed,
        Failed
    }

    public class Response
    {
        public RequestStatus Status { get; set; }
        
        public string Message { get; set; }

        public DateTime Timestamp { get; set; }
    }

    public class Request
    {

        public enum Module
        {
            SwapCharacter
        }

        public RequestStatus Status { get; set; } = RequestStatus.Pending;

        public Module RequestModule { get; set; }
    }

    public class SwapCharacterRequest : Request
    {
        public SwapCharacterRequest(string characterName)
        {
            CharacterName = characterName;
        }

        public string CharacterName { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
