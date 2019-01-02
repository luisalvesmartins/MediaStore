var imageBaseUrl= "https://yourblob.blob.core.windows.net/";
var functionAppBaseUrl = 'https://yourfunctions.azurewebsites.net/';
//functionAppBaseUrl="http://localhost:7071/";

var URL = {
    Thumbs160: imageBaseUrl + "thumbs160/",
    Thumbs320: imageBaseUrl + "thumbs320/",
    Thumbs1000: imageBaseUrl + "thumbs1000/",
    Media: imageBaseUrl + "media/",
    Vision: imageBaseUrl + "vision/",

    Provider: functionAppBaseUrl + '.auth/login/facebook',

    SearchMain: functionAppBaseUrl + "api/newsearchmain",
    SearchDetail: functionAppBaseUrl + "api/newsearchdetail?event=",
    SAS: functionAppBaseUrl + "api/getSAS",

//ADMIN:
    TagSearch: functionAppBaseUrl + "api/tagSearch",
    TagSearchDetail: functionAppBaseUrl + "api/tagSearchDetail?group=",

    adminSaveMediaTags: functionAppBaseUrl + "api/saveMediaTags?id=",
    adminAddMediaTags: functionAppBaseUrl + "api/AddMediaTags?id=",
    adminRenameDirectory: functionAppBaseUrl + "api/admRenameDirectory?",
    adminMediaDelete: functionAppBaseUrl + "api/MediaDelete?id=",

    adminGetPermissions: functionAppBaseUrl + "api/admListUsers",
    admSaveUserPermissions: functionAppBaseUrl + "api/admSaveUserPermissions",

    getMediaInfo: functionAppBaseUrl + "api/getMediaInfo?id=",
    getAllTagValues: functionAppBaseUrl + "api/getAllTagValues",

    AuthMe: `${functionAppBaseUrl}.auth/me`,
    AuthLogout: `${functionAppBaseUrl}.auth/logout`,

    querystring: function (key) {
        key = key.replace(/[*+?^$.\[\]{}()|\\\/]/g, "\\$&"); // escape RegEx meta chars
        var match = location.search.match(new RegExp("[?&]" + key + "=([^&]+)(&|$)"));
        return match && decodeURIComponent(match[1].replace(/\+/g, " "));
    }
}