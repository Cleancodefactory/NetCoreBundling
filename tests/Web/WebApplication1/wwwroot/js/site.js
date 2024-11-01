//if ('serviceWorker' in navigator) {
//    navigator.serviceWorker
//        .register('/sw.js?@@@version@@@') // Loading from root in order to make it encompass the whole thing
//        .then(_ => console.log("SW: Our service worker is registered"));
//}

function Test (value) {
    if (typeof value != "string") {
        return value;
    }

    return value
        .replace(/([\\])([^"])/g, function (regValue, firstGroup, secondGroup) { return "\\" + firstGroup + secondGroup; })
        .replace(/[\/]/g, "\\/")
        .replace(/[\b]/g, "\\b")
        .replace(/[\f]/g, "\\f")
        .replace(/[\n]/g, "\\n")
        .replace(/[\r]/g, "\\r")
        .replace(/[\t]/g, "\\t");
};

\