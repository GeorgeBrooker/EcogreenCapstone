# After first time setup you can use the following commands to start
**All commands should be executed in order** and **within the sam-with-postgres folder**

Docker-Desktop must be running somewhere to compose the project
```bash
docker-compose up -d
sam build
sam local start-api
```
This starts a local server hosting all our API endpoints on localhost:3000. You can call these with a normal HTTP request. (make sure you're using HTTP, not HTTPS if you're having trouble)
I recommend using an HTTP request manager like POSTMAN to send these requests (more info in installation readme)
*Remember to open your new terminal in the ```\sam-with-postgres``` directory not the git repo root directory*

and *always rebuild if you've changed your function!*

```
sam build
```

