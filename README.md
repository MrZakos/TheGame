# The Game 
[![Build Status](https://travis-ci.org/joemccann/dillinger.svg?branch=master)](https://travis-ci.org/joemccann/dillinger)


A websocket game implementation using .NET Core 5 (ASP Kestral)

## Installation

```sh
cd dillinger
npm i
node app
```

For production environments...

```sh
npm install --production
NODE_ENV=production node app
```

## Server API
| API/Event | Input | Response | Notes |
| ------ | ------ |------| ------ |
| Login | DeviceId(UUID) | PlayerId | if a deviceId is already connected , disconnect it |
| UpdateResources | ResourceType ResourceValue | resource balance ||
| SendGift | PlayerId ResourceType ResourceValue |success message to sender + GiftEvent message to receiver if online  | |
| GiftEvent | FromPlayerId ResourceType ResourceValue PreviousBalance CurrentBalance  | - |  event by server

# Solution Strcture

- All projects are .NET Core 5 Runtime
- Shared/sharedSettings.json is shared and used for all executable applciations (Server/ClientConsoleRunner/UnitTests).
  - Avoid configurations duplications
  - Contains configurations for Sqlite / Serilog / Client 

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
  
 

| Solution | Info |
| ------ | ------ |
| TheGame.Server | (ASP.NET CORE) The WebsSocket server , using Kestral which is the basic   |
| TheGame.ClientConsole | (library) The client (console) , using https://github.com/Marfusios/websocket-client for websocket handling|
| TheGame.ClientConsoleRunner | (console application) runs the client |
| TheGame.Common | (library) shared models,interface,constants |
| TheGame.BootstrapService | (library) initiation for non web application (configurations & logger loading) and shared DI registration for both web and non web projects
| TheGame.WebSocketService | (library) contain ```WebSocketConnectionManager``` class which handles web socket connections |
| TheGame.DataService | (library) Handles Sqlite database with entity framework (code first) , repository & unit of work patterns  |
| TheGame.DAL| (library) Data Access Layer - database operations logic on top of TheGame.DatService   |
| TheGame.BLL| (library) Business Logic Layer - contains ```WebSocketConnectionsHandler``` which handles and process ```TheGame``` workflow |
| TheGame.UnitTests | NUnit |


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

```flow
st=>start: Login
op=>operation: Login operation
cond=>condition: Successful Yes or No?
e=>end: To admin

st->op->cond
cond(yes)->e
cond(no)->op
``` 

# Server
![""](1.JPG?raw=true "")




# Client example
![""](2.png?raw=true "")


## License

MIT