var StorageManager = {
    hasStorage: 'No',
    init: function () {
        if (typeof (Storage) !== "undefined") {
            StorageManager.hasStorage = "Yes";
        } else {
            StorageManager.hasStorage = "No";
        }
    }
}
String.prototype.replaceAll = function (search, replacement) {
    var target = this;
    return target.replace(new RegExp(escapeRegExp(search), 'g'), replacement);
};
function escapeRegExp(str) {
    return str.replace(/[.*+?^${}()|[\]\\]/g, "\\$&"); // $& means the whole matched string
}
function formatDate(date) {
    var d = new Date(date),
        month = '' + (d.getMonth() + 1),
        day = '' + d.getDate(),
        year = d.getFullYear(),
        hour = d.getHours(),
        minute = d.getMinutes();

    if (month.length < 2) month = '0' + month;
    if (day.length < 2) day = '0' + day;

    if (hour.length < 2) hour = '0' + hour;
    if (minute.length < 2) minute = '0' + minute;

    return [year, month, day].join('-') + " " + hour + ":" + minute;
}

//SCREEN
function Busy(mode) {
    if (mode)
        $('#busy').show();
    else
        $('#busy').hide();
}

function toggleDisplay(id, mode) {
    if (mode == null)
        if (document.getElementById(id).style.display == "block")
            mode = true;
        else
            mode = false;
    if (mode)
        document.getElementById(id).style.display = "block";
    else
        document.getElementById(id).style.display = "none";
}