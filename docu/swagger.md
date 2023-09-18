## Swagger
If you use the [AspNetCore.Simple.Sdk](https://www.nuget.org/packages/AspNetCore.Simple.Sdk) all is `ready to use` but you have to go sure that `<GenerateDocumentationFile>true</GenerateDocumentationFile>` is activated. `SwaggerInfo` is optional

### AppSettings.json
| Name | Description |
|--|--|
| IncludeOnlyVersionedPaths | Means that all paths with versiosn will be shown in swagger |
| SwaggerInfosByVersion| There you can define swagger document infos per version |

```json
"SwaggerInfos": {
    "IncludeOnlyVersionedPaths": true,
    "SwaggerInfosByVersion": [
      {
        "Version": "1",
        "Id": "5064d915-f010-4223-956f-8f18e3ac43d0", // Must be a GUID
        "Title": "Our new cool API",
        "Description": "This Api will do very cool stuff",
        "ContactName": "Opportunity API Team",
        "ContactEmail": "cool-api@outlook.de",
        "ContactUrl": "https://renepeuser.visualstudio.com/AspNetCore.Simple.Sdk",
        "Audience": "SimpleSdk"
      },
      {
        "Version": "2",
        "Id": "5686555f-8d18-4ba1-a728-1b6cdbcd231d", // Must be a GUID
        "Title": "Our new cool API",
        "Description": "API for opportunities",
        "ContactName": "Opportunity API Team",
        "ContactEmail": "cool-api@outlook.de",
        "ContactUrl": "https://renepeuser.visualstudio.com/AspNetCore.Simple.Sdk",
        "Audience": "SimpleSdk"
      }
    ]
  }
```

Hint: as Version is allowed: 
* 1
* 1.0
* v1
* V1
* v1.0
* V1.0

## Environment variables / Pipeline
```
SwaggerInfos__SwaggerInfosByVersion__0__Audience
SwaggerInfos__IncludeOnlyVersionedPaths                          
SwaggerInfos__SwaggerInfosByVersion__0__Audience     
SwaggerInfos__SwaggerInfosByVersion__0__ContactEmail 
SwaggerInfos__SwaggerInfosByVersion__0__ContactName  
SwaggerInfos__SwaggerInfosByVersion__0__ContactUrl   
SwaggerInfos__SwaggerInfosByVersion__0__Description  
SwaggerInfos__SwaggerInfosByVersion__0__Id           
SwaggerInfos__SwaggerInfosByVersion__0__Title  
```

### Project file of your API
Important is to use `<GenerateDocumentationFile>true</GenerateDocumentationFile>` that you have all your commemts and summaries inside your swagger document
```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
    <PropertyGroup>    
        <GenerateDocumentationFile>true</GenerateDocumentationFile>
    </PropertyGroup>
</Project>
```