# After first time setup you can use the following commands to start
**All commands should be executed in order** and **within the sam-with-postgres folder**

```bash
docker-compose up -d
sam build
sam local start-lambda
```
You can then run your commands in a **new terminal** with
```
sam local invoke [FUNCTION_NAME] --event events/[EVENT_FILE_NAME] --docker-network host
```
*Remember to open your new terminal in the ```\sam-with-postgres``` directory not the git repo root directory*

and *always rebuild if you've changed your function!*

```
sam build
```

