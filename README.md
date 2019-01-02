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
    - Create a container named "site" and copy the site files inside. Use your favourite Azure Storage Explorer to do it.
    - Ensure the container has public access. In the azure functions we will create a proxy to this container.
3. Deploy the [Azure Functions](./Functions)
    - Edit proxies.json and change yourblob name
    - Edit localsettings.json to add the setting keys
    - Deploy to Azure Functions
4. Start uploading your media by running PhotoUploader
5. While it's running, configure Facebook authentication and point it to your functions site. In the Facebook Login settings configure the "Valid OAuth Redirect URIs to https://yourfunctionname.azurewebsites.net/.auth/login/facebook/callback
6. Init the  usersecurity storage table with your facebook email address.
7. Browse your site by navigating to: https://yourfunctionname.azurewebsites.net/
