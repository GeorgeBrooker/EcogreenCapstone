# How to properly install dev environment.
### IMPORTANT NOTE
Whenever I refer to the *root* or *project* directory I am referring to the sam-with-postgres directory inside the main git repo.
## Requirements.
- AWS CLI 
- .NET core 
- Docker *(Docker-Desktop is the recommended installer)*
- AWS SAM CLI.

You may need to restart you computer after installing everything (I had to in windows but not WSL)

The dev environment consists of 2-3 docker containers. If you've never worked with docker before you can consider these as small virtual machines. AWS SAM requires docker and postgresql also should be run inside a container before deployment.<br><br>
To install you will need to install the AWS CLI, .NET core, Docker and the AWS SAM CLI. Instructions and links can be found in the README in this directory. <br><br>
I installed everything in a Windows subsystem for linux environment but because everything runs in docker you should be able to run this on Mac or windows fine.

**If you're also going to use WSL there are some important things to note so make sure to follow the WSL specific instructions.**

## USING WSL
#### Install Docker-Desktop in windows not your WSL linux distribution.
- Docker has native support for WSL, it will link into any linux distributions natively. Installing in WSL can get messy fast.
#### Enable WSL 2 Backend in docker-desktop settings, Turn on "Add the *.docker.internal names to the hosts etc/hosts file" setting.
- Enable WSL 2 engine and add *.docker.internal in General settings, Enable WSL 2 integration in resources -> WSL integration -> tick the box in the menu, you can also set your default distro.
#### Turn on network mirroring for WSL 2 (off by default)
- Our database is hosted on your main computers localhost, but each docker container & WSL has its own localhost. Turning on network mirroring lets us connect to the server from inside of WSL / containers.
To enable network mirroring create a .wslconfig file in your home directory ```[/localdisk/users/%username%]```. Inside this file paste <br><br>```[wsl2]```<br>```networkingMode=mirrored```<br><br> restart WSL and this should work.
#### You can use remote development with jetbrains to very easily manage your project inside of WSL (jetbrains is also free with a university email)
- Open the new projects tab select ```remote development -> WSL -> new project / open existing project```. Recent projects should show up here too. When run for the first time the IDE will install a remote env into WSL, after this is done performance is near native. This is my preferred way to manage the filesystem without using the terminal. 

## Building the project
- Install all requirements including docker desktop.<br><br>
- Clone the GitHub repo into an appropriate location. (if using WSL I recommend using jetbrains remote dev features to do this)<br><br>
- Initialise the database:<br><br>
    - Navigate to the project directory in the terminal.<br><br>
    - run ```docker-compose up -d``` This creates a docker container using the schema in docker-compose.yml. The schema initialises a postgresql server from the init.sql file.<br><br>
    - You can check for successful deployment by running ```docker exec -it sam-postgres-db psql -U postgres``` "docker exec -it sam-postgres-db" enters our container (named sam-postgres-db) and opens a terminal, "psql -U postgres" connects to the database using the psql management engine.<br><br>
    - You should see your terminal text change to ```postgres=#``` run ```select * from hello_table;``` (do not forget the semicolon) and you should see a small table output. If so the DB is working.<br><br>
    - Quit the terminal with the ```\q``` command<br><br>


- Initialise AWS<br><br>
    - Confirm you are in the project root directory. This is the folder containing the readme files.<br><br>
    - In a new terminal run ```sam local start-lambda``` this starts a docker container that emulates an AWS server locally. This server takes control of the terminal, so I recommend doing it in one you won't be using. (so not the jetbrains terminal)<br><br>
    - To confirm successful install you can run ```sam local invoke HelloWorldFunction --event events/event.json --docker-network host``` This will invoke the lambda function "HelloWorldFunction" and pass it the Json information in "events/event.json", it will do this in a new docker container and "--docker-network host" ensures this container gets connected to your PC's root network (so it can see postgres, very important).<br><br>
    - This should return about 10 lines of data, you're looking for the json return ```{"statusCode": 200, "headers": {"Content-Type": "application/json"}, "body": "\"Jobs done\"", "isBase64Encoded": false}``` near the end to confirm success.<br><br>
    - You can now start developing your backend functions locally!
 
## Development methodology.
In our deployed application we will call lambda functions from the frontend by sending Json events to the AWS server. We can emulate these events by adding .json files into the /events folder. Coordinate with the frontend to decide on your preferred Json format for data transfer and then create some test json files you can use.
### To develop new functions
#### With jebrains (or other ide)
 - If you're using jetbrains you can create a new lambda function easily by right-clicking on the project root directory and selecting ```Add -> New Project``` Select ```Console App``` and set the root folder of your solution to be the ```/src``` folder in the ```sam-with-postgres``` directory. Once done you should see a new solution folder right next to the ```HelloWorld``` solution folder.

#### Manual creation
- Lambda functions are stored in the ```/src``` folder. Create a new folder in ```/src``` with your code. 

Whatever method you choose **you will need to add this function to the resources tab in the root directories template.yml**. You can see an example of this will HelloWorldFunction.
#### Adding functions to template.yml (this lets AWS see our function and inject environment settings).
For the most part you should be able to copy the HelloWorldFunction syntax and change the CodeUri and Names. The **Handler:** property lists the file and method we want to run. In the HelloWorld method we can see it calls Function.cs in the HelloWorld folder (```HelloWorld::HelloWorld.Function```) and runs the FunctionHandler method (```::FunctionHandler```)
<br><br>You can also use this template file to set up environment variables. We will often use environment variables in the final application for things like DB login info so make sure to make use of this.
#### Setting environment variables
If ```Enviroment:``` does not exist in ```Properties:``` create it. under ```Enviroment:``` create ```Variables:``` add each variable under variables with the syntax ```VARIABLE_NAME: variable_value```
for example:<br>
```
Properties:
    Environment:
        Variables:
            EVIRONMENT_VARIABLE_1: WeeWeeWaaWaa
```

Defines an enviroment variable named ```ENVIRONMENT_VARIABLE_1``` with the value ```WeeWeeWaaWaa```. <br>You can get this value in your program using ```string variable = Environment.GetEnvironmentVariable("ENVIRONMENT_VARIABLE_1");```


The template file won't be used in the final application, it is used to substitute all the values AWS will fill in automatically when we deploy (I think). It is imortant to use it for any static info like DB connection strings that will change when we deploy on AWS. Otherwise we will have to refactor our code every deployment.



### To test (run) your Lambda function
**BEFORE CALLING YOUR METHOD MAKE SURE TO REBUILD THE APPLICATION**<br>

to rebuild the app run ```sam build``` in the root directory

To run your function use the *invoke* command<br> ```sam local invoke [FUNCTION_NAME] --event events/[EVENT_FILE_NAME] --docker-network host```

```FUNCTION_NAME``` is the name in the template.yml file *not the name of the folder or method*.

```EVENT_FILE_NAME``` is the name of whatever JSON event file we want to inject into our app. 
- Every call requires some kind of event. If your backend function doesn't need specific information in the event, feel free to use the standard event.json event for testing.

#### MISC

Builds are stored in .aws-sam. If you want to check your built app is up-to-date you can look in here and see if anything has changed. 

### Happy coding!

