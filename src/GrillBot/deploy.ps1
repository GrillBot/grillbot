git pull --rebase;
if(!($?)) {
    Write-Error -Message "Git pull (Rebase) failed. Check repository.";
    exit;
}

docker build -t grillbot-test-image -f ./Dockerfile.Test .
if(!($?)) {
    Write-Error -Message "Tests failed. Push and deployment cancelled.";
    exit;
}

docker rmi $(docker images --format "{{.Repository}}:{{.Tag}}" | findstr 'grillbot-test-image')
git push;
if(!($?)) {
    Write-Error -Message "Git push failed. Check repository";
    git push;
}

docker build -t ghcr.io/grillbot/grillbot .
docker push ghcr.io/grillbot/grillbot
