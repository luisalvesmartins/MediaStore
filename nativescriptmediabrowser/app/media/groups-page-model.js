const observableModule = require("tns-core-modules/data/observable");
const ObservableArray = require("tns-core-modules/data/observable-array").ObservableArray;
const keys = require("./keys");
const http = require("http");

/* ***********************************************************
 * This is the master list view model.
 *************************************************************/
function CarsListViewModel() {
    const viewModel = observableModule.fromObject({
        groups: new ObservableArray([]),
        isLoading: false,

        load: function () {
            this.set("isLoading", true);

            this.set("groups", new ObservableArray([]));

            http.request({
                url: "https://lammediafunctions.azurewebsites.net/api/newsearchmain",
                method: "GET",
                headers:{
                    "content-type":"application/json",
                    "X-ZUMO-AUTH": keys.loadKey()
                }
            }).then((response) => {
                this.set("isLoading", false);
                const arr1 = response.content.toJSON();
                for (let index = 0; index < arr1.length; index++) {
                    arr1[index].imageSrc = "https://lamfamily.blob.core.windows.net/thumbs160/" + arr1[index].id;    
                }
                this.set("groups", new ObservableArray(arr1));
        }, (error) => {
            console.log("error");
            this.set("isLoading", false);
        });
        }
    });

    return viewModel;
}

module.exports = CarsListViewModel;
