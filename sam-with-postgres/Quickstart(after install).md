# After first time setup you can use the following commands to start
**All commands should be executed in order** and **within the sam-with-postgres folder**

Docker-Desktop must be running somewhere to compose the project
```bash
docker-compose up -d
sam build
sam local start-api --docker-network host
```
This starts a local server hosting all our API endpoints on localhost:3000. You can call these with a normal HTTP request. (make sure you're using HTTP, not HTTPS if you're having trouble)
I recommend using an HTTP request manager like POSTMAN to send these requests (more info in installation readme)
*Remember to open your new terminal in the ```\sam-with-postgres``` directory not the git repo root directory*

and *always rebuild if you've changed your function!*

```
sam build
```

The github should contain a DB file that come pre filled with some test data. If the database is empty you can use the ```docker/dynamodb/KasishDM.json``` file with amazons ```noSQL workbench``` app to fill the database with the appropriate values.

## DEPLOYMENT.
To deploy the application you will need to set your proper AWS credentials in the AWS cli.