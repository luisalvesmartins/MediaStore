const topmost = require("tns-core-modules/ui/frame").topmost;

// const WebView = require("tns-core-modules/ui/web-view").WebView;

// const webViewSrc = "https://docs.nativescript.org/";

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
    console.log("PHOTO");
    console.log(page.navigationContext.id);


    // const viewModel = new GroupPageViewModel(page.navigationContext.group);
    // page.bindingContext = viewModel;
    // viewModel.load();

}

function onBackButtonTap() {
    topmost().goBack();
}

exports.onNavigatingTo = onNavigatingTo;
exports.onBackButtonTap = onBackButtonTap;
