# MediaStore
Store, browse and share your digital media in Azure

The main project is divided in four components:
- [PhotoUploader](./PhotoUploader) - Crawls and uploads all the media files it finds - not just photos.
- [Azure Functions](./Functions) - will process the media files uploaded and will build smart thumbnails using cognitive services.
- [Site](./MediaStoreSite) - Site to browse the uploaded media and to manage media, tags and permissions.

## PhotoUploader

Is a command line app that crawls a given folder tree.
It's a C# app, compile & run it.

```
PhotoUploader <path media root folder>
```

## Media Store Site

The site is HTML and Javascript site deployed in Azure Storage and accessed thru the proxy defined in Azure functions.

## Azure Functions

C# functions that access azure storage to be deployed in Azure Functions

## Instalation instructions

1. Create an Azure blob storage account
2. Deploy the [Site](./MediaStoreSite):
    - Change the "yourblob" and "yourfunctions" string inside js/URL.js
    - Create a container and copy the site files inside. Use your favourite Azure Storage Explorer to do it.
3. Deploy the [Azure Functions](./Functions)
    - Edit proxies.json and change yourblob name
    - Edit localsettings.json to add the setting keys
    - Deploy to Azure Functions
4. Run PhotoUploader
5. While it's running, configure Facebook authentication and point it to your site
6. Edit the userpermissions to add your facebook email address
7. Browse your site
