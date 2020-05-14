dotnet publish -c Debug -o out ./Starter.csproj

docker build --rm -f "Dockerfile.amd64.debug" -t localhost:5000/module-host-sample:0.0.1-amd64.debug "./";

if ($?) {
    docker push localhost:5000/module-host-sample:0.0.1-amd64.debug;
}
