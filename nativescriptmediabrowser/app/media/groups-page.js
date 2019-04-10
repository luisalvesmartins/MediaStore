const topmost = require("tns-core-modules/ui/frame").topmost;
const keys = require("./keys");
const observableModule = require("tns-core-modules/data/observable");
const ObservableArray = require("tns-core-modules/data/observable-array").ObservableArray;
const http = require("http");
const dialogs = require("tns-core-modules/ui/dialogs");

const viewModel = observableModule.fromObject({
    groups: new ObservableArray([]),
    isLoading: false
});

/* ***********************************************************
* This is the master list code behind in the master-detail structure.
* This code behind gets the data, passes it to the master view and displays it in a list.
* It also handles the navigation to the details page for each item.
*************************************************************/

/* ***********************************************************
* Use the "onNavigatingTo" handler to initialize the page binding context.
* Call any view model data initialization load here.
*************************************************************/
function onNavigatingTo(args) {
    /* ***********************************************************
    * The "onNavigatingTo" event handler lets you detect if the user navigated with a back button.
    * Skipping the re-initialization on back navigation means the user will see the
    * page in the same data state that he left it in before navigating.
    *************************************************************/
    if (args.isBackNavigation) {
        return;
    }

    const page = args.object;
    page.bindingContext = viewModel;
    load();
}

function load() {
    viewModel.set("isLoading", true);
    viewModel.set("groups", new ObservableArray([]));

    console.log("KEYS");
    console.log(keys.loadKey());
    http.request({
        url: "https://lammediafunctions.azurewebsites.net/api/newsearchmain",
        method: "GET",
        headers:{
            "content-type":"application/json",
            "X-ZUMO-AUTH": keys.loadKey()
        }
    }).then((response) => {
        viewModel.set("isLoading", false);
        const arr1 = response.content.toJSON();
        for (let index = 0; index < arr1.length; index++) {
            arr1[index].imageSrc = "https://lamfamily.blob.core.windows.net/thumbs160/" + arr1[index].id;    
        }
        viewModel.set("groups", new ObservableArray(arr1));
    }, (error) => {
        console.log("error");
        viewModel.set("isLoading", false);
    });
}

/* ***********************************************************
* Use the "itemTap" event handler of the <RadListView> to navigate to the
* item details page. Retrieve a reference for the data item and pass it
* to the item details page, so that it can identify which data item to display.
* Learn more about navigating and passing context in this documentation article:
* https://docs.nativescript.org/core-concepts/navigation#navigate-and-pass-context
*************************************************************/
function onGroupTap(args) {
    topmost().navigate({
        moduleName: "media/group-page/group-page",
        context: args.view.bindingContext,
        animated: true,
        transition: {
            name: "slide",
            duration: 200,
            curve: "ease"
        }
    });
}

function onEnterKey() {
    // Add proper facebook authentication somewhere in the future
    dialogs.prompt("Your key", "authkeyfromfacebook").then(function (r) {
        console.log("Dialog result: " + r.result + ", text: " + r.text);
        keys.saveKey(r.text);

        load();
    });

}

exports.onNavigatingTo = onNavigatingTo;
exports.onGroupTap = onGroupTap;
exports.onEnterKey = onEnterKey;
