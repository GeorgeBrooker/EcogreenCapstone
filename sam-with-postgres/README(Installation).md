# How to properly install dev environment.
### IMPORTANT NOTE
This guide sets up a dev environment, so you don't have to think about AWS as you are developing. In setting up this environment I have configured all the weird AWS shit, so we don't have to think about it anymore.

If you are ever confused about how to use AWS, try to ignore it. Build the project using ```sam build```, run the project using ```sam local start-api --docker-network host``` (and make sure the DB is running using docker_desktop).
Then just develop the project like any other C# backend. Our Lambda function is the entire backend, which AWS calls by running ```src/ShopRepository/Program.cs```. To create helper functions, classes, or controllers just add those files in the ShopRepository folder. The entire project gets pushed to AWS. 

To call functions from the frontend just send HTTP requests to the API gateway. In development this is ```localhost:3000/{function_endpoint_url}```. In production these URLs will be given to us by Amazon (so don't worry about it yet). 


Whenever I refer to the *root* or *project* directory I am referring to the sam-with-postgres directory inside the main git repo.
## Requirements.
- AWS CLI (Note when setting up the AWS CLI you will have to add your _Session Token_ as well as your _Access_ and _Secret_ key. These can be found in the AWS access portal link sent to use by Anna **[Images in the docs folder]**)
- .NET core 
- Docker *(Docker-Desktop is the recommended installer)*
- AWS SAM CLI.

To add a session token to the AWS CLI (you need this to deploy to AWS) you will first need to set up your AWS credentials with 

```aws configure``` 

then manually configure your session token with 

```aws configure set aws_session_token {your_session_token_here}``` ```(do not include the {})```

more information about setting up the AWS CLI can be found here:  [Set and view configuration settings using commands](https://docs.aws.amazon.com/cli/latest/userguide/cli-configure-files.html#cli-configure-files-methods)



**NOTE: If developing / using stripe applications you will also need to install the stripe API**
### HIGHLY RECOMENDED
- Postman https://www.postman.com/downloads/ (this will let you send custom Http requests to the server very easily).
**Join the postman project [here](https://app.getpostman.com/join-team?invite_code=e234a329218bc9ff1c4bd68d70e74060&target_code=e0bcae68abfa2e97cfe11343d421aad4). This project contains some pre-configured test commands.**

You may need to restart you computer after installing everything (I had to in windows but not WSL)

The dev environment consists of 2 docker containers. If you've never worked with docker before you can consider these as small virtual machines. AWS SAM runs in docker and the database does too. **YOU WILL NEED TO HAVE DOCKER RUNNING TO BUILD OR TEST THE APPLICATION.** Having the database running at all times is not essential but obviously DB methods won't work without it running.


To install you will need to install the AWS CLI, .NET core, Docker and the AWS SAM CLI. Instructions and links can be found in the ```AWS_DOCS.md``` in this directory. <br><br>
I installed everything in Windows subsystem for linux but **because everything runs in docker it doesn't matter where you install everything.**

**If you're also going to use WSL there are some important things to note so make sure to follow the WSL specific instructions.**

## USING WSL *\*Optional*\* 
I dont recommend using WSL unless you like linux as a dev environment. Because everything is dockerized it really doesn't matter where the app is running so using WSL just introduces more points of failure.
#### Install Docker-Desktop in windows not your WSL linux distribution.
- Docker has native support for WSL, it will link into any linux distributions natively. Installing in WSL can get messy fast.
#### Enable WSL 2 Backend in docker-desktop settings, Turn on "Add the *.docker.internal names to the hosts etc/hosts file" setting.
- Enable WSL 2 engine and add *.docker.internal in General settings, Enable WSL 2 integration in resources -> WSL integration -> tick the box in the menu, you can also set your default distro.
#### Turn on network mirroring for WSL 2 (off by default) - _THE PROGRAM WILL NOT WORK WITHOUT THIS_
- Our database is hosted on your main computers localhost, but each docker container & WSL has its own localhost. Turning on network mirroring lets us connect to the server from inside of WSL / containers.
To enable network mirroring create a .wslconfig file in your home directory ```[/localdisk/users/%username%]```. Inside this file paste <br><br>```[wsl2]```<br>```networkingMode=mirrored```<br><br> restart WSL and this should work.
#### You can use remote development with jetbrains to very easily manage your project inside of WSL (jetbrains is also free with a university email)
- Open the new projects tab select ```remote development -> WSL -> new project / open existing project```. Recent projects should show up here too. When run for the first time the IDE will install a remote env into WSL, after this is done performance is near native. This is my preferred way to manage the filesystem without using the terminal. 

## Building the project
- Install all requirements including docker-desktop.


- Clone the GitHub repo into an appropriate location. (if using WSL I recommend using jetbrains remote dev features to do this)


- Initialise the database:
    - Navigate to the project directory in the terminal  (i.e ```\capstone-project-2024-s1-team-techtitans\sam-with-postgres```).
    - run ```docker-compose up -d``` This creates a docker container for the local database. 
    - you should now see a container in your docker_desktop application. The container should also show as started. You can now start and stop the database from docker (or with docker-compose)


- Initialise AWS
    - Confirm you are in the project root directory. This is the folder containing the readme files.
    - In a new terminal run ```sam local start-api --docker-network host``` this starts a docker container that emulates an AWS server locally. This server takes control of the terminal, so I recommend doing it in one you won't be using. (so not the jetbrains terminal)<br>
## Development methodology.
The AWS server will listen for REST HTTP requests and route them to the appropriate endpoint using the structure defined in ```template.yml```. This has already been set up for you.

**YOU NO LONGER NEED TO CONSIDER AWS LAMBDA**, You can develop the app as you would any other backend server. The environment will manage that all AWS interactions for you.
### To develop new functions
Go to the ShopRepository project in ```sam-with-postgres/src``` and create any classes or controllers as normal using standard .NET conventions. 

To make a method callable from the outside put it inside a **.net API controller**. **For example:** the database API is hosted in ```/ShopRepository/Controllers/ShopController.cs``` and is reachable on ```localhost:3000/api/shop``` when running ```sam local start-api --docker-network host```.
You can then use any HTTP api tester to call your code and check the response is correct. 

I **HIGHLY** recommend using postman for this as it's really nice to use, and we have a pre-configured test environment on there.

### To test (run) your code
To run any of your code it must either be in a controller method or called by a controller method.

- run ```sam local start-api --docker-network host```. The command ```--docker-network host``` connects the sam api to your computers host. Without this AWS SAM will not be able to connect to the offline database (although you can still connect to the prod database)

- Either manually send an HTTP packet to ```localhost:3000/{YourMethodUri}``` or use an HTTP tester like postman to do so.

- Check the response from the server matches what you expect.

### Testing API's using postman.
**To test a method using postman:**
- Create an account and all that BS
- Join the project [here](https://app.getpostman.com/join-team?invite_code=e234a329218bc9ff1c4bd68d70e74060&target_code=e0bcae68abfa2e97cfe11343d421aad4) to get all the previously created methods.

**To create a new test:**
- Find or create an appropriate folder for your test request (make sure you are in the ```cs399-techtitans-p41``` workspace).
- Hover over your collection name, right click, click "add request" in the drop-down menu
- Select the type of request (GET,POST,ETC..)
- Enter the API endpoint url. (i.e ```http://localhost:3000/api/shop```) Which calls the default method of ShopController (```GetAlive()```)
- The full HTTP response is now visible. You can also define tests and view the status of those.
- An example of a successful postman request is in /docs/img



#### MISC

Builds are stored in .aws-sam. If you want to check your built app is up-to-date you can look in here and see if anything has changed. 

### Happy coding!

