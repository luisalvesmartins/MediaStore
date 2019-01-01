# MediaStore
Store, browse and share your digital media in Azure

The main project is divided in four components:
- [PhotoUploader](./PhotoUploader) - Crawls and uploads all the media files it finds - not just photos.
- Azure Functions - will process the media files uploaded and will build smart thumbnails using cognitive services.
- [Site](./MediaStoreSite) - Site to browse the uploaded media and to manage media, tags and permissions.

## PhotoUploader

Is a command line app that crawls a given folder tree.
It's a C# app, compile & run it.

## Media Store Site

The site is HTML and Javascript site deployed in Azure Storage and accessed thru the proxy defined in Azure functions.

## Azure Functions

C# functions that access azure storage. 
(Coming soon)

## Instalation instructions

(comming soon)