#  MeridianDownload
Download MST data from data.meridianproject.ac.cn
### url
Website
https://data.meridianproject.ac.cn/

Page
https://data.meridianproject.ac.cn/science-data/download-list/?file_type=file&ift_id=130&datetime1=20120101000000&datetime2=20211222235959&page_num=1

### Publish

```bash
dotnet publish -c Release -r win-x64
dotnet publish -c Release -r linux-x64 -p:PublishSingleFile=true -p:PublishTrimmed=true
dotnet publish -c Release --no-self-contained -r linux-x64 -p:PublishSingleFile=true
dotnet publish -c Release -r win-x64 -p:PublishSingleFile=true -p:PublishTrimmed=true -p:IncludeNativeLibrariesForSelfExtract=true -p:PublishReadyToRun=true -p:PublishReadyToRunComposite=true
dotnet publish -c Release -r osx.11.0-arm64 -p:PublishSingleFile=true -p:PublishTrimmed=true
```
