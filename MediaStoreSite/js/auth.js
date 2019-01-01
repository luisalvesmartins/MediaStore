window.fbAsyncInit = function () {
    FB.init({
        appId: '726050054443285',
        cookie: true,
        xfbml: true,
        version: 'v2.12'
    });
    onFacebookLogin();
};
(function (d, s, id) {
    var js, fjs = d.getElementsByTagName(s)[0];
    if (d.getElementById(id)) { return; }
    js = d.createElement(s); js.id = id;
    js.src = "https://connect.facebook.net/en_US/sdk.js";
    fjs.parentNode.insertBefore(js, fjs);
}(document, 'script', 'facebook-jssdk'));
function onFacebookLogin() {
    FB.getLoginStatus(function (response) {
        //console.log(`In facebook callback: ${JSON.stringify(response)}`);
        if (response === undefined || response.status.toLowerCase() === 'connected') {
            var accessToken = response.authResponse.accessToken;
            translateAuthToken('facebook', { 'access_token': accessToken });
        } else {
            clearEasyAuth('facebook');
        }
    });
}


var easyAuthInfo = null;


function clearEasyAuth(provider) {
    if (easyAuthInfo === null) {
        return; // no info to clear
    }
    if (easyAuthInfo.provider === provider) {
        easyAuthInfo = null;
    }
    // else - not currently logged in with the specified provider
}
function getEasyAuthToken() {
    return easyAuthInfo === null ? null : easyAuthInfo.token;
}

function translateAuthToken(provider, body) {
    // Call function app to translate provider token to easyAuthInfo
    sendRequest(
        {
            method: 'POST',
            url: URL.Provider,
            headers: {
                'accept': 'application/json',
                'content-type': 'application/json',
            },
            body: body
        },
        function () {
            //console.log(`EasyAuth response (${provider}): ${this.responseText}`);
            var easyAuthResponse = JSON.parse(this.responseText);
            easyAuthInfo =
                {
                    provider: provider,
                    token: easyAuthResponse.authenticationToken,
                };
            $.ajaxSetup({
                beforeSend: function (xhr) {
                    xhr.setRequestHeader('accept', "application/json");
                    xhr.setRequestHeader('content-type', "application/json");
                    xhr.setRequestHeader('X-ZUMO-AUTH', getEasyAuthToken());
                }
            });
            Init();
        }
    );
}

function callIsAuthenticated() {
    sendRequest(
        {
            method: 'GET',
            url: URL.IsAuthenticated,
            headers: {
                'accept': 'application/json',
                'content-type': 'application/json',
                'X-ZUMO-AUTH': getEasyAuthToken()
            },
        },
        function () {
            log(`Response from IsAuthenticated: ${this.responseText}`);
        }
    );
}
function callGetClaims() {
    sendRequest(
        {
            method: 'GET',
            url: URL.GetClaims,
            headers: {
                'accept': 'application/json',
                'content-type': 'application/json',
                'X-ZUMO-AUTH': getEasyAuthToken()
            },
        },
        function () {
            log(`Response from GetClaims: ${this.responseText}`);
        }
    );
}
function callGetAuthInfo() {
    sendRequest(
        {
            method: 'GET',
            url: URL.GetAuthInfo,
            headers: {
                'accept': 'application/json',
                'content-type': 'application/json',
                'X-ZUMO-AUTH': getEasyAuthToken()
            },
        },
        function () {
            log(`Response from GetAuthInfo: ${this.responseText}`);
        }
    );
}
function callGetEmail() {
    sendRequest(
        {
            method: 'GET',
            url: URL.GetEmailClaim,
            headers: {
                'accept': 'application/json',
                'content-type': 'application/json',
                'X-ZUMO-AUTH': getEasyAuthToken()
            },
        },
        function () {
            log(`Response from GetEmail: ${this.responseText}`);
        }
    );
}
function callAuthMe() {
    sendRequest(
        {
            method: 'GET',
            url: URL.AuthMe,
            headers: {
                'accept': 'application/json',
                'content-type': 'application/json',
                'X-ZUMO-AUTH': getEasyAuthToken()
            },
        },
        function () {
            log(`Response from .auth/me: ${this.responseText}`);
        }
    );
}
function callAuthLogout() {
    sendRequest(
        {
            method: 'GET',
            url: URL.AuthLogout,
            headers: {
                'accept': 'application/json',
                'content-type': 'application/json',
                'X-ZUMO-AUTH': getEasyAuthToken()
            },
        },
        function () {
            log('called .auth/logout');
        }
    );
}
function sendRequest(options, handler) {
    var xhr = new XMLHttpRequest();
    xhr.open(options.method, options.url);
    for (header in options.headers) {
        xhr.setRequestHeader(header, options.headers[header]);
    }
    xhr.onload = handler
    var body = options.body;
    if (body !== undefined && body !== null && typeof (body) !== 'string') {
        body = JSON.stringify(body);
    }
    xhr.send(body);
}
