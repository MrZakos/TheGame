using System;
using System.Text.Json;
using TheGame.Common.Models;

namespace TheGame.ClientConsole
{
    public class CommandParser
    {
        public readonly string Command;
        public WebSocketServerClientDTO Model { get; set; }
        public bool IsValid => Model != null;
        public string ConvertToServerJSONRequest => JsonSerializer.Serialize(Model, new JsonSerializerOptions { IgnoreNullValues = true, WriteIndented = true });

        public CommandParser(string inputCommand)
        {
            Command = inputCommand.Trim();
            Parse();
        }

        public void Parse()
        {
            Guid guid;
            int friendId;
            double resourceValue;
            var split = Command.Split(' ');
            var eventName = split[0];

            switch (eventName)
            {
                case "login":
                    if (split.Length == 2 && Guid.TryParse(split[1], out guid))
                    {
                        Model = new WebSocketServerClientDTO
                        {
                            Event = WebSocketServerClientEventCode.Login.ToString(),
                            LoginRequest = new LoginRequest
                            {
                                DeviceId = guid
                            }
                        };
                    }
                    break;
                case "update":
                    if (split.Length == 3 &&
                        (split[1] == ResourceType.Coin.ToString().ToLower() || split[1] == ResourceType.Roll.ToString().ToLower()) &&
                        double.TryParse(split[2], out resourceValue))
                    {
                        Model = new WebSocketServerClientDTO
                        {
                            Event = WebSocketServerClientEventCode.UpdateResources.ToString(),
                            UpdateResourcesRequest = new UpdateResourcesRequest
                            {
                                ResourceType = split[1],
                                ResourceValue = resourceValue
                            }
                        };
                    }
                    break;
                case "gift":
                    if (split.Length == 4 &&
                        int.TryParse(split[1], out friendId) &&
                        (split[2] == ResourceType.Coin.ToString().ToLower() || split[2] == ResourceType.Roll.ToString().ToLower()) &&
                        double.TryParse(split[3], out resourceValue))
                    {
                        Model = new WebSocketServerClientDTO
                        {
                            Event = WebSocketServerClientEventCode.SendGift.ToString(),
                            SendGiftRequest = new SendGiftRequest
                            {
                                FriendPlayerId = friendId,
                                ResourceType = split[2],
                                ResourceValue = resourceValue
                            }
                        };
                    }
                    break;
                default:
                    break;
            }


        }
    }
}
