const applicationSettings = require("application-settings");

function loadKey() {
    console.log("KEY");
    if (applicationSettings.hasKey("auth"))
    {
        console.log("HAS KEY");
        console.log(applicationSettings.getString("auth", ""));    
    }

    return applicationSettings.getString("auth", "");
}

function saveKey (key) {
    applicationSettings.setString("auth", key);
}

exports.loadKey = loadKey;
exports.saveKey = saveKey;
