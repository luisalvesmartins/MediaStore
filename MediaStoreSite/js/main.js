function Init() {
    StorageManager.init();
    Search.Refresh();
}

var Search = {
    Detail: null,
    FullRefresh: function () {
        StorageManager.hasStorage = "Refresh";
        Search.Refresh();
    },
    Refresh: function () {
        Busy(true);
        if (StorageManager.hasStorage == "Yes") {
            try {
                Search.Detail = JSON.parse(localStorage.getItem("Detail"));
            } catch (e) {
                Search.Detail == null;
            }
            if (Search.Detail != null) {
                Search.DisplayResults();
            }
        }
        if (Search.Detail == null || StorageManager.hasStorage != "Yes") {
            $.get(
                URL.SearchMain,
                function (data) {
                    Search.Detail = data;
                    if (StorageManager.hasStorage != "No") {
                        try {
                            localStorage.setItem("Detail", JSON.stringify(Search.Detail));
                        } catch (error) {
                            localStorage.setItem("Detail", "");
                        }
                    }
                    Search.DisplayResults();
                });
        }
    },
    DisplayResults: function () {
        var s = "";
        var lastYear = "";
        for (var f = 0; f < Search.Detail.length; f++) {
            var lastG = Search.Detail[f].group;
            if (lastG == null)
                lastG = "";
            if (lastG.length > 4) {
                if (lastG.substr(0, 4) != lastYear) {
                    if (lastG.substr(0, 1) == "1" || lastG.substr(0, 1) == "2") {
                        lastYear = lastG.substr(0, 4);
                        s += "<div style='clear:both'></div><div class=titleh2>" + lastYear + "</div>";
                    }
                    else {
                        if (lastYear != "OTHER") {
                            s += "<div style='clear:both'></div><div class=titleh2>OTHER</div>";
                            lastYear = "OTHER";
                        }
                    }
                }
            }

            var id = Search.Detail[f].id;

            s += "<div class='lazy item' onclick=\"Search.go(" + f + ")\" " +
                " data-src=\"" + URL.Thumbs160 + id + "\"><div class=t>" + Search.Detail[f].group +
                "</div><div class=n>" + Search.Detail[f].numMedia + "</div></div>";

        }
        $("#grouppanel").html(s);

        $('.lazy').Lazy({
            scrollDirection: 'vertical',
            effect: 'fadeIn',
            visibleOnly: true,
            onError: function (element) {
                console.log('error loading ' + element.data('src'));
            }
        });
        Busy(false);
    },
    go: function (n) {
        var group = Search.Detail[n].group;
        window.location = "detail.html?s=" + group;
    }
}
