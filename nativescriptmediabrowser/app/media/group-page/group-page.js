const observableModule = require("tns-core-modules/data/observable");
const ObservableArray = require("tns-core-modules/data/observable-array").ObservableArray;
const keys = require("../keys");
const http = require("http");
const WebView = require("tns-core-modules/ui/web-view").WebView;

const viewModel = new observableModule.fromObject({
    photos: new ObservableArray([]),
    isLoading: false,
    showDetails:true,
    weburl:"",
    directory:""
});

let SAS = "";

function onNavigatingTo(args) {
    /* ***********************************************************
    * The "onNavigatingTo" event handler lets you detect if the user navigated with a back button.
    * Skipping the re-initialization on back navigation means the user will see the
    * page in the same data state that he left it in before navigating.
    *************************************************************/
    // if (args.isBackNavigation) {
    //     return;
    // }
    viewModel.set("showDetails", true);

    const page = args.object;
    console.log("GROUP");
    console.log(page.navigationContext.group);
    console.log(page.navigationContext);

    //const viewModel = new GroupPageViewModel(page.navigationContext.group);

    page.bindingContext = viewModel;
    //viewModel.load();
    viewModel.set("isLoading", true);
    viewModel.set("directory", page.navigationContext.group);

    viewModel.set("photos", new ObservableArray([]));
    const Directory = encodeURI(page.navigationContext.group);

    http.request({
        url: "https://lammediafunctions.azurewebsites.net/api/getSAS",
        method: "GET",
        headers:{
            "content-type":"application/json",
            "X-ZUMO-AUTH": keys.loadKey()
        }
    }).then((response) => {
        SAS = response.content.toString();

        http.request({
            url: "https://lammediafunctions.azurewebsites.net/api/newsearchdetail?event=" + Directory,
            method: "GET",
            headers:{
                "content-type":"application/json",
                "X-ZUMO-AUTH": keys.loadKey()
            }
        }).then((response) => {
            viewModel.set("isLoading", false);
            const arr1 = response.content.toJSON();
            for (let index = 0; index < arr1.length; index++) {
                if (arr1[index].extension.toUpperCase() !== ".JPG" && arr1[index].extension.toUpperCase() !== ".PNG") {
                    arr1.splice(index, 1);
                } else {
                    arr1[index].imageSrc = "https://lamfamily.blob.core.windows.net/thumbs320/" + arr1[index].id + SAS;  
                    //arr1[index].imageSrc = "https://lamfamily.blob.core.windows.net/thumbs160/" + arr1[index].id;
                    try {
                        const a = JSON.parse(arr1[index].exif);
                        arr1[index].time = a["Date/Time Original"];
                    } catch (error) {
                        arr1[index].time = "";
                    }
                }
            }
            viewModel.set("photos", new ObservableArray(arr1));
        }, (error) => {
            console.log("error");
            viewModel.set("isLoading", false);
        });

    }, (error) => {
        console.log("error getting SAS");
        viewModel.set("isLoading", false);
    });

}

function onPhotoTap(args) {
    // alert ("Tapped");
    // console.log ( args.view.bindingContext );
    // console.log ( args.view.bindingContext.id );
    viewModel.set("showDetails", false);
    viewModel.set("weburl", "https://lamfamily.blob.core.windows.net/thumbs1000/" + args.view.bindingContext.id + SAS);

//     topmost().navigate({
//         moduleName: "./group-page.xml",
// //        moduleName: "../photo-page/photo-page",
//         context: args.view.bindingContext,
//         animated: true,
//         transition: {
//             name: "slide",
//             duration: 200,
//             curve: "ease"
//         }
//     });
}

/* ***********************************************************
* The back button is essential for a master-detail feature.
*************************************************************/
function onBackButtonTap() {
    console.log("back");
    //     topmost().goBack();
}

function onCloseButtonTap() {
    viewModel.set("showDetails", true);
}

exports.onNavigatingTo = onNavigatingTo;
exports.onBackButtonTap = onBackButtonTap;
exports.onPhotoTap = onPhotoTap;
exports.onCloseButtonTap = onCloseButtonTap;
