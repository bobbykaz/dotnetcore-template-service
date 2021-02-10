 param (
	[Parameter(Mandatory=$false)]
	[switch]$push,
    [Parameter(Mandatory=$false)]
    [string]$tagExtra = "",
    [Parameter(Mandatory=$false)]
    [string]$awsRegion = "us-west-1",
    [Parameter(Mandatory=$false)]
    [string]$ecr = "127.0.0.1/not-a-real-ecr-repo"
 )

$pathToDockerfile = ".\src\bk-dotnet-template\bk-dotnet-template\Dockerfile"
$dockerDir = ".\src\bk-dotnet-template"
$imageName = "bk-template"


$tag =  Get-Date -Format "yy.MM.dd"
$tag = "0.1." + $tag

if ($tagExtra -ne "")
{
	$tag = $tag + "." + $tagExtra
}

$buildTag = $imageName + ":" + $tag

Write-Host "Local build tag is: $buildTag" -ForegroundColor Yellow

docker build -t  $buildTag -f $pathToDockerfile $dockerDir

Write-Host "Done building and tagging." -ForegroundColor Green

if ($push -eq $true)
{
	$ecrTag = $ecr + "/" + $imageName + ":" + $tag
	Write-Host "ECR tag is: $ecrTag" -ForegroundColor Yellow
	
	docker tag $buildTag $ecrTag

	aws ecr get-login-password --region $awsRegion | docker login --username AWS --password-stdin $ecr
	
	docker push $ecrTag
	
	Write-Host "Done pushing." -ForegroundColor Green
}