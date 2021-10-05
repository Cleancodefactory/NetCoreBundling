if ('serviceWorker' in navigator) {
    navigator.serviceWorker
        .register('/sw.js?@@@version@@@') // Loading from root in order to make it encompass the whole thing
        .then(_ => console.log("SW: Our service worker is registered"));
}