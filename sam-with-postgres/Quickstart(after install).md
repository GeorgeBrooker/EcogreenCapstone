﻿# After first time setup you can use the following commands to start
**All commands should be executed in order** and **within the sam-with-postgres folder**

Docker-Desktop must be running somewhere to compose the project
```bash
docker-compose up -d
sam build
sam local start-api --docker-network host
```
Just a note for _ONLY Linux_ users, --docker-network host works a bit different on linux
to connect use:
```bash
sam local start-api --add-host host.docker.internal:host-gateway
```
Ignore this last line if you're using any other OS.

This starts a local server hosting all our API endpoints on localhost:3000. You can call these with a normal HTTP request. (make sure you're using HTTP, not HTTPS if you're having trouble)
I recommend using an HTTP request manager like POSTMAN to send these requests (more info in installation readme)
*Remember to open your new terminal in the ```\sam-with-postgres``` directory not the git repo root directory*

and *always rebuild if you've changed your function!*

```
sam build
```

The github should contain a DB file that come pre filled with some test data. If the database is empty you can use the ```docker/dynamodb/KasishDM.json``` file with amazons ```noSQL workbench``` app to fill the database with the appropriate values.

## DEPLOYMENT.
To deploy the application you will need to set your proper AWS credentials in the AWS cli. AWS credentials regularly expire, so you will most likely net to reset them every deployment.

To reset your AWS credentials:
- Go to the login portal and get your access keys & session token
- run ```aws configure``` in the terminal and put in your access key and secret key.
- run ```aws configure set aws_session_token {your session token}``` to set your session token.
- confirm sucessful credentials change by running ```aws sts get-caller-identity```. If it returns a userId, account, and Arn, you're in.