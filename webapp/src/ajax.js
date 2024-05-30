
// load data and call function:
export function ajax(method, url, callback, data, headers, max_retry) {
    var r = new XMLHttpRequest();
    r.open(method, url, true);
    if (typeof(max_retry) === "undefined") {
        max_retry = 5;
    }

    r.onreadystatechange = function () {
        if (r.readyState == 4) {
            if (r.status != 200) {
                if (max_retry > 0) {
                    // request again:
                    setTimeout(function () {
                        ajax(method, url, callback, data, headers, max_retry - 1);
                    }, 1000);
                }
            } else {
                try {
                    callback(r.responseText);
                } catch {
                    // Do nothing :(
                }
            }
        }
    };

    if (typeof(headers) !== "undefined") {
        for (var nm in headers) {
            r.setRequestHeader(nm, headers[nm]);
        }
    }

    r.send(data);
}

export function ajaxGet(url, callback) {
    ajax("GET", url, callback);
}

export function ajaxPost(url, callback, data, headers) {
    ajax("POST", url, callback, data, headers);
}

export function ajaxGetJSON(url, callback) {
    ajaxGet(url, function (data) {
        var d = JSON.parse(data);
        if (typeof(d.error) === "undefined") {
            callback(d.result);
        } else {
            console.log("Request failed: " + d.error.toString());
        }
    });
}

export function ajaxRPC(url, callback) {
    var args = [];
    for (var i = 2; i < arguments.length; i++) {
        args.push(arguments[i]);
    }

    var req = {
        "arguments": args,
    };

    ajaxPost(url, function (data) {
        var d = JSON.parse(data);
        if (typeof(d.error) === "undefined") {
            callback(d.result);
        } else {
            console.log("Request failed: " + d.error.toString());
        }
    }, JSON.stringify(req));
}
