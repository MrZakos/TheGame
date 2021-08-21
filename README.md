# The Game 
[![Build Status](https://travis-ci.org/joemccann/dillinger.svg?branch=master)](https://travis-ci.org/joemccann/dillinger)


A websocket game implementation using .NET Core 5 (ASP Kestral)

# Run
you need to run the server(TheGame.Server) and then run many clients(TheGame.ClientConsoleRunner) as you wish
- option A : Visual Studio
- option B : using CLI
```sh
cd TheGame.Server
dotnet run
```

```sh
cd TheGame.ClientConsoleRunner
dotnet run
```

# Solution Strcture

| Solution | Info |
| ------ | ------ |
| TheGame.Server | (ASP.NET CORE) The WebsSocket server |
| TheGame.ClientConsole | (library) The client (console) ,  https://github.com/Marfusios/websocket-client is used for websocket handling|
| TheGame.ClientConsoleRunner | (console application) Runs the client |
| TheGame.Common | (library) Shared models,interfaces,constants |
| TheGame.BootstrapService | (library) Responsible of initiation(configurations & logger setup) for a console application , also has common DI registration function for both console & web projects
| TheGame.WebSocketService | (library) Expose ```WebSocketConnectionManager``` class which handles web socket connections |
| TheGame.DataService | (library) Sqlite database handler with entity framework (code first) , repository & unit of work pattern  |
| TheGame.DAL| (library) Data Access Layer - database operations logic on top of TheGame.DatService   |
| TheGame.BLL| (library) Business Logic Layer - contains ```WebSocketConnectionsHandler``` which handles and process ```TheGame``` workflow |
| TheGame.UnitTests | NUnit |

- All projects are .NET Core 5 Runtime
- appsettings configure Sqlite/Serilog/Client  settings

  ```sh
  {
  "ConnectionStrings": {
    "Sqlite": "Filename=TheGameDB.sqlite"
  },
  "Serilog": {
    "Using": [
      "Serilog.Sinks.Console",
      "Serilog.Sinks.File"
    ],
    "MinimumLevel": {
      "Default": "Verbose",
      "Override": {
        "Microsoft": "Error",
        "Microsoft.Extensions": "Information",
        "System": "Debug",
        "Microsoft.EntityFrameworkCore.Database.Command": "Error"
      }
    },
    "WriteTo": [
      {
        "Name": "File",
        "Args": {
          "path": "logs/log.txt",
          "rollingInterval": "Day",
          "rollOnFileSizeLimit": true,
          "fileSizeLimitBytes": "512000",
          "retainedFileCountLimit": 3,
          "formatter": "Serilog.Formatting.Compact.CompactJsonFormatter, Serilog.Formatting.Compact"
        }
      }
    ]
  },
  "ClientOptions": {
    "WebSocketURL": "wss://localhost:5001/connect",
    "KeepAliveIntervalSeconds": 5
   }
  }
  ```


## Server API

| API/Event | Input | Response | Notes |
| ------ | ------ |------| ------ |
| Login | DeviceId(UUID) | PlayerId | if a deviceId is already connected , disconnect it |
| UpdateResources | ResourceType ResourceValue | resource balance | updates resource value,need to be loggged in |
| SendGift | PlayerId ResourceType ResourceValue |success message to sender + GiftEvent message to receiver if online  | need to be loggged in |
| GiftEvent | FromPlayerId ResourceType ResourceValue PreviousBalance CurrentBalance  | - |  event by server
- ResourceType = coins/rolls
- ResourceValue = number
- PlayerId = number

# DB Browser for SQLite
[DB Browser for SQLite](https://sqlitebrowser.org/) can be used for exploring database data
# Server Web Socket Connection URL

```sh
wss://localhost:5001/connect
```

# Request/Response Payloads Examples

Login Request
```sh
{
    "RequestId": "fd5b6bf9-1582-4178-97a0-151bfa18ce9c",
    "Event": "Login",
    "LoginRequest": {
        "DeviceId": "00000000-0000-0000-0000-000000000002"
    }
}
```

Login Response
```sh
{
    "RequestId": "fd5b6bf9-1582-4178-97a0-151bfa18ce9c",
    "Event": "Login",
    "Success": true,
    "LoginResponse": {
        "PlayerId": 2
    }
}
```

UpdateResources Request
```sh
{
    "RequestId": "d30744f1-b6be-484f-a9f9-9b9442dba1a1",
    "Event": "UpdateResources",
    "UpdateResourcesRequest": {
        "ResourceType": "Roll",
        "ResourceValue": 101
    }
}
```

UpdateResources Response
```sh
{
  "RequestId": "d30744f1-b6be-484f-a9f9-9b9442dba1a1",
  "Event": "UpdateResources",
  "Success": true,
  "UpdateResourcesResponse": {
    "Balance": 101
  }
}

```

UpdateResources when not logged in
```sh
{
  "RequestId": "4694a6be-5701-4655-ad69-20c03ab5256c",
  "Event": "UpdateResources",
  "Success": false,
  "Message": "logged in required to exceute this command"
}
```


SendGift Request
```sh
{
    "RequestId": "a518bfe4-092b-4bc5-895f-3bb5b747142d",
    "Event": "SendGift",
    "SendGiftRequest": {
        "FriendPlayerId": 6,
        "ResourceType": "Coin",
        "ResourceValue": 100
    }
}
```

SendGift Response
```sh
{
  "RequestId": "a518bfe4-092b-4bc5-895f-3bb5b747142d",
  "Event": "SendGift",
  "Success": true,
  "SendGiftResponse": {
    "Message": "gift sent ! your friend is online"
  }
}
```

SendGift to non existent player
```sh
{
  "RequestId": "a518bfe4-092b-4bc5-895f-3bb5b747142d",
  "Event": "SendGift",
  "Success": false,
  "Message": "player(id=22223) does not exists"
}
```

SendGift when not logged in
```sh
{
  "RequestId": "c44a6451-b63a-4c2a-a14b-88484d94973f",
  "Event": "SendGift",
  "Success": false,
  "Message": "logged in required to exceute this command"
}
```

Invalid Request (invalid model)
```sh
{
  "RequestId": "3313eea1-dccf-464f-ab29-4dab3f8a6e65",
  "Event": "SendGifttttttttttttttttttt",
  "Success": false,
  "Message": "invalid request"
}
```

GiftEvent
```sh
{
  "RequestId": "2f83080d-ad4d-4651-ab82-af609a942b30",
  "Event": "GiftEvent",
  "Success": true,
  "GiftEvent": {
    "FromPlayerId": 2,
    "ResourceType": "Roll",
    "ResourceValue": 40000,
    "PreviousResourceBalance": 0,
    "CurrentResourceBalance": 40000
  }
}
```



# Server
![""](1.JPG?raw=true "")




# Client
![""](3.JPG?raw=true "")

![""](2.png?raw=true "")


## License

MIT