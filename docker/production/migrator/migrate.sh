export PATH="$PATH:/root/.dotnet/tools"
cp ./config.json ./Tools/Christofel.Design/bin/Debug/net6.0

dotnet ef database --startup-project=./Tools/Christofel.Design/ update --context ChristofelBaseContext -p ./Core/Christofel.Common
dotnet ef database --startup-project=./Tools/Christofel.Design/ update --context ManagementContext -p ./Plugins/Christofel.Management
dotnet ef database --startup-project=./Tools/Christofel.Design/ update --context ReactHandlerContext -p ./Plugins/Christofel.ReactHandler
dotnet ef database --startup-project=./Tools/Christofel.Design/ update --context ApiCacheContext -p ./Plugins/Christofel.Api
dotnet ef database --startup-project=./Tools/Christofel.Design/ update --context CoursesContext -p ./Libs/Christofel.CoursesLib