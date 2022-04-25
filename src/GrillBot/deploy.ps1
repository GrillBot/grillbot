dotnet test -v quiet --nologo -l:"console;verbosity=normal";

if(!($?)) {
    Write-Error -Message "Unit tests failed. Push and deployment cancelled.";
    exit;
}

git push;
docker build -t registry.gitlab.com/grillbot/grillbot .
docker push registry.gitlab.com/grillbot/grillbot
