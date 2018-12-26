# PhotoUploader

Command Line app to crawl the folder structure and upload media to Azure.

Usage:

```
  PhotoUploader <media root folder>
```

Example:
```
  PhotoUploader c:\Photos
```

The Azure configuration is stored in app.config:

```xml
  <appSettings>
    <add key="StorageConnectionString" value="DefaultEndpointsProtocol=https;AccountName=<youraccount>;AccountKey=<yourkey>;EndpointSuffix=core.windows.net" />
    <add key="notallowedextensions" value=".url.3gp.ini.thm.mht.jbf.db.dat.tmp.doc.pub.html.xls.doc.docx.xlsx" />
    <add key="Container.Media" value="media" />
    <add key="Container.Thumbnails" value="thumbnails" />
    <add key="Container.Thumbnails320" value="thumbs320" />
    <add key="Container.Thumbnails1000" value="thumbs1000" />
    <add key="Container.EXIF" value="exif" />
    <add key="Table.Media" value="media" />
    <add key="Table.Tag" value="Tag" />
    <add key="Table.TagReverse" value="TagIndex" />

</appSettings>
```

For each file found the following records are created:
- One record in the media table - defined by Table.Media - master record with all the file information
- One record in the tag table - defined by Table.Tag - tag the file with Event equal to folder name
- One record in the reverse tag table - defined by Table.TagReverse - reverse tag to index the file

This blobs are uploaded:
- Copy of original file to "Container.Media"
- Thumbnail of size 1000x1000 px in "Container.Thumbnails1000"
- Thumbnail of size 320x320 px in "Container.Thumbnails320"
- File EXIF information blob in "Container.EXIF"
