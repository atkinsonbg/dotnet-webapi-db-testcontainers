include .env
export

build:
	dotnet build ./api/dotnet-testcontainers-poc.csproj
	dotnet build ./tests/tests.csproj

run:
	dotnet run --project ./api/dotnet-testcontainers-poc.csproj

up : down
	docker network create localstack || true
	docker-compose -f ./api/docker-compose.yml up --build

down :
	docker-compose -f ./api/docker-compose.yml down

publish:
	rm -rf ./api/deploy
	dotnet publish ./api/dotnet-testcontainers-poc.csproj -r linux-musl-x64 -p:PublishSingleFile=true -c Release -o ./api/deploy

test:
	dotnet test ./tests/tests.csproj

test-apis:
	dotnet test ./tests/tests.csproj --filter dotnet_webapi_db_testcontainers.tests.TestAPICalls

test-db:
	dotnet test ./tests/tests.csproj --filter dotnet_webapi_db_testcontainers.tests.TestDatabase

test-aws:
	dotnet test ./tests/tests.csproj --filter dotnet_webapi_db_testcontainers.tests.TestAWSCalls

restore:
	dotnet tool restore ./api/dotnet-testcontainers-poc.csproj