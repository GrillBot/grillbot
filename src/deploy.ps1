git pull --rebase;
if(!($?)) {
    Write-Error -Message "Git pull (Rebase) failed. Check repository.";
    exit;
}

git push;
if(!($?)) {
    Write-Error -Message "Git push failed. Check repository";
    git push;
}

docker build -t ghcr.io/grillbot/grillbot .
docker push ghcr.io/grillbot/grillbot
