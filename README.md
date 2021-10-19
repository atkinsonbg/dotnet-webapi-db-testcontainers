# Using Docker & TestContainers to Mock Depenedencies
This repo demonstrates how you can mock all your downstream dependencies including databases, external APIs and even AWS. It tackles running a Postgres database, that is seeded with data each time, providing you an exact copy on each run. It uses Simple Mock Server, a Docker container that can provide mocked responses to any external API call you need. Finally it uses LocalStack, an open-source container that contains a mocked AWS environment that can be seeded with resources. With this setup, you can more quickly develop and test locally against all your dependencies with ease.

This demo showcases the following functionality:

- You can call the API with a user's EID and get:
    - A report from GitHub on the number of PRs, Commits, and Comments the user has done
    - Save a generated report to S3
    - Retrieve a saved report from S3

#### Running The Stack
When running the stack via the Docker Compose, the API is run using a `dotnet watch` command. This will watch for any file changes in the container (all of the API code is mounted), and will restart the API in the container if anything is changed.

## Tools Used
- [TestContainers](https://www.testcontainers.org/): Testcontainers provides lightweight, throwaway instances of Docker containers that can be invoked during your tests.
- [Simple Mock Server](https://github.com/atkinsonbg/simple-mock-server): Simple Mock Server runs as a standalone Docker container, which you can call via your code in order to mock API calls.
- [LocalStack](https://github.com/localstack/localstack): LocalStack provides an easy-to-use test/mocking framework for developing AWS Cloud applications using Docker.
- [Xunit.DependencyInjection](https://github.com/pengweiqhca/Xunit.DependencyInjection): Provides a DI framework to allow for injection during XUnit tests.
- [Docker Compose](https://docs.docker.com/compose/): A tool for defining and running multi-container Docker applications.
- [Dozzle](https://github.com/amir20/dozzle): Simple, lightweight application that provides you with a web based interface to monitor your Docker container logs live.

## Benefits
- Speed: Being able to quickly mock down stream dependencies to develop and test against
- Reliability: Being able to quickly reproduce a known set of consditions in your mock environments
- Test Your Code, Not Your Mocks: Write tests that test your code, without the need for any mocking code

## Base Configuration
In order to get all your mocked resources to work correctly, you should configure the following:

- Postgres: This demo uses a `postgres:11.6-alpine` image, pre-download for speed, but no other container/environment setup is required. Postres runs on port 5432.
- Mock Server: This demo uses a `atkinsonbg/mock-server:0.1` image, pre-download for speed, but no other container/environment setup is required. Simple Mock Server runs on port 5000.
    - In order to target a mock URL versus a real URL, utilize a `BASE_URL` environment variable:
    ```
    environment:
      CONNECTION_STRING: Host=dotnet-testcontainers-poc-database;Database=testcontainer-db;Username=postgres
      DATABASE_URL: postgresql://postgres@dotnet-testcontainers-poc-database:5432/testcontainer-db
      AWS_PROFILE: stack-profile
      ENVIRONMENT: Development
      GITHUB_BASE_URL: http://domain_mockserver:5000
    ```

    Then in your code: `var response = _httpClient.GetStringAsync($"{Environment.GetEnvironmentVariable("GITHUB_BASE_URL")}/api/v3/users/{userid}/events").Result;`

    This way you can easily target the Simple Mock Server container, or other environments as you deploy your code.
- Localstack: This demo uses a `localstack/localstack:latest` image, pre-download for speed. LocalStack runs on port 4566. You will also need to pass an AWS profile to LocalStack, and this should not be your professional/enterprise profile. You will need to create a new "fake" profile using: `aws configure --profile stack-profile`. Running this command will prompt for additional information:
    - `aws_access_key_id` = "test"
    - `aws_secret_access_key` = "test"
    - `ouput` = None

LocalStack basically ignores this profile when you call it, but it does look for these settings. *You do not need to install LocalStack locally!*

## Mocked Data
In order to mock your dependencies, you need to store some data to recreate everything on each run. For instance, for your database you need schema and seed data. For your mocked API calls, you need responses you expect back. And for AWS, you need anything your code expects to be in the cloud: S3 buckets, etc. To facilitate this, we create a root level directory to hold all this data.

In this repo we call the directory `fixtures`, but the name is irrelavent. You can call it `local-development` or `mocked-stuff`, it does not matter. Inside it are three additional directories:
- `sql`
- `mocks`
- `localstack`

#### SQL
This demo uses an extremely simple database setup, a single table that gets populated with data. While this is the most simple use possible, it does not mean you can't have a more complex setup. No matter what you need, this folder will hold `.sql` files that get mounted into a Postgres instance during runtime. They are copied to the `/docker-entrypoint-initdb.d` folder in the container and will be run in sorted order when the container starts.

#### Mocks
This folder holds `.json` files that allow you to mock your 3rd party API calls. These files are mounted into a Simple Mock Server instance during runtime at the following internal folder: `/api/Mocks`. The JSON in the mock file contain the following:
- URL that the code will call minus the base URL.
- Method for the call: GET, POST, etc.
- Request Body (if required)
- Request Headers (if required)

Simple Mock Server matches JSON in the following order:
1. URL
2. Method
3. Request Body
4. Request Headers

This allows for very complex matching of mocks.

#### LocalStack
This folder holds a number of files needed to make LocalStack work. The following files are created by LocalStack and are required to connect to the container:
- `server.test.pem`
- `server.test.pem.crt`
- `server.test.pem.key`

These are in the `.gitignore` and are boilerplate files, you have to have them, but don't need to worry about them.

As you interact with the LocalStack container, it creates a directory called `data` and inside that are various `.json` files that record any PUT or DELETE calls made to the container. These recorded calls are then replayed each time the container is run, getting back to its previous state.

To properly call LocalStack we configure an `IAmazonS3` provider via the `Startup.cs` class (this demo only uses S3, but this config would be similar for any other resource required). If the code is in development mode, we configure a provider for calling the LocalStack container. PLEASE NOTE the `config.ServiceURL` matches the container name via the Compose file (see below in Docker Compose section).

```
services.AddSingleton<IAmazonS3>(provider =>
{
    var config = new AmazonS3Config { };

    if (provider.GetService<IHostEnvironment>().IsDevelopment())
    {
        // If in Dev mode, we have to setup the client with fake credentials
        // and configure the settings to talk to the LocalStack container
        var creds = new BasicAWSCredentials("xxx", "xxx");
        config.ForcePathStyle = true;
        // This url must match the name of the container in the compose file
        config.ServiceURL = "http://domain_localstack:4566";
        config.UseHttp = true;
        return new AmazonS3Client(creds, config);
    }
    else
    {
        // Otherwise, just return a vanilla Client with an empty config
        // or config as needed for deployment
        return new AmazonS3Client(config);
    }
});
```

## Docker Compose (Local Development)
For local development we spin up the following containers via Docker Compose:
- Postgres DB
- Simple Mock Server
- LocalStack
- Dozzle

All of our mocked files are mounted to their corresponding containers per their documentation. When running Compose, all containers get a hostname that you assign via the `container_name` property. As such, when making cross container calls, you would not call `localhost` but rather the `container_name` specified in your Compose file. In order to make these calls properly with LocalStack, you must create a network and have all your containers utilize it.

## TestContainers (Local Testing & Testing During CI/CD)
Running TestContainers locally is fairly standard and does not require much modification from the vanilla implementations you can find on the LocalStack site. However, there are a few main thiings to consider:

#### Running In Jenkins/Bogie
Jenkins/Bogie does expose a Docker socket via a TCP endpoint. By default Docker looks for the socket on `/var/run/docker.sock`, which is not exposed in Jenkins. As such your code needs to look for the environment variable `DOCKER_HOST`, if it is populated, use the value in that, otherwise use the default socket. You can check like this:

Getting the values:
```
public static string dockerHost = Environment.GetEnvironmentVariable("DOCKER_HOST");
public const string localDockerSocketPath = "unix:///var/run/docker.sock";
```

Then check and use the appropriate value:
```
var testcontainersBuilder = new TestcontainersBuilder<TestcontainersContainer>()
    .WithImage("postgres:11.6")
    .WithEnvironment("POSTGRES_DB", "testcontainer-db")
    .WithMount(Constants.sqlInitPath, "/docker-entrypoint-initdb.d/init.sql")
    .WithPortBinding(5432, 5432)
    .WithDockerEndpoint(!String.IsNullOrEmpty(Constants.dockerHost) ? Constants.dockerHost : Constants.localDockerSocketPath)
    .WithWaitStrategy(Wait.ForUnixContainer().UntilCommandIsCompleted("pg_isready"));
```

#### Bogie Managed Pipeline `build` vs `script`
When using the Bogie managed pipeline, you can build your application using a base `build` command or bring your own contianer using a `script` build. In either scenario, you will have access to the Docker socket:
- Using `build`, the Docker socket is injected and available
- Using `script`: need to include `withDockerSock` to get the socket injected.

#### Don't Use Port 80!
When running TestContainers in Bogie, you call them on the `localhost` hostname. As such, map any ports away from 80. 80 is not available, and your tests will hang.

#### Using Collection Fixtures with XUnit
In order to not spin up a new container on each test, you must use the Xunit Collection Fixture feature. This will ensure a single container is spun up, and all tests will run against that one container.

```
[CollectionDefinition("Mock Database collection")]
public class MockDatabaseCollection : ICollectionFixture<MockDatabaseFixture>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}

[Collection("Mock Database collection")]
public class TestDatabase
{
    [Fact]
    public async void TestCountOfUsers()
    {
        Console.WriteLine("STARTING DB TEST1");
        var ctx = new GithubUsersContext();
        var r = await ctx.GithubUsers.ToListAsync();
        Assert.Equal(3, r.Count);
    }

    ...
```

## Makefile
A few import things about the Makefile included:

- Local Network: On Compose Up, the following command is run: `docker network create localstack || true`. This is used for the AWS CLI to talk to Container, if the network has already neen created, this will simply log a statement saying as such.

A few helper commands have been setup. All of these should be run from the root of the project:

- `make up`: This will use the Docker Compose file and spin up the stack for you. It will also create the Docker network it requires if it has not already been created.
- `make test`: This will run the entire test suite.
- Various other commands: However, the two above are used most often.

## VERY IMPORTANT FAQs
The items listed here can 100%/absolutely stop your code from running properly!! Do not gloss over this information, read it carefully. Some of it is repeated from above, but only because its so important:

- Localstack Docker Compose network: In order to communicate with LocalStack when using Docker Compose, you have to create a network and have all containers use that network.
- Don't use Port 80: Simple Mock Server and LocalStack run on higher ports, but don't run your API on port 80 in Compose or Tests. If it runs on 80 or 8080, map it to somehting else, it won't work in Jenkins.
- Be careful with your commits on LocalStack recorded calls. As you test, you may be recording calls and updating the "recorded calls" JSON file. You may not want all those test runs in that file in GitHub. Be selective about what gets committed to avoid bloat.
