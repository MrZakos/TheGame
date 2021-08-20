# The Game 
[![Build Status](https://travis-ci.org/joemccann/dillinger.svg?branch=master)](https://travis-ci.org/joemccann/dillinger)

## Solution Strcture

Dillinger is currently extended with the following plugins.
Instructions on how to use them in your own application are linked below.

| Plugin | README |
| ------ | ------ |
| Dropbox | [plugins/dropbox/README.md][PlDb] |
| GitHub | [plugins/github/README.md][PlGh] |
| Google Drive | [plugins/googledrive/README.md][PlGd] |
| OneDrive | [plugins/onedrive/README.md][PlOd] |
| Medium | [plugins/medium/README.md][PlMe] |
| Google Analytics | [plugins/googleanalytics/README.md][PlGa] |


Login Request
```sh
{
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
    "Event": "Login",
    "LoginRequest": {
        "DeviceId": "00000000-0000-0000-0000-000000000002"
    }
}
```

UpdateResources Response
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

SendGift Request
```sh
{
    "Event": "Login",
    "LoginRequest": {
        "DeviceId": "00000000-0000-0000-0000-000000000002"
    }
}
```

SendGift Response
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