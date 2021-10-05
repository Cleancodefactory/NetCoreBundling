var pwaCacheKey = '@@@version@@@';

self.addEventListener('install', function (e) {
    e.waitUntil(
        caches.open(pwaCacheKey).then(function (cache) {
            return cache.addAll([
                '/'
            ]);
        })
    );
});

// self.addEventListener('fetch', function (event) {
//     event.respondWith(async function () {

//         console.log(event.request.url + " - " + event.request.method);
//         if (/\/account\/signin/gi.test(event.request.url) || /\/account\/signout/gi.test(event.request.url)) {
//             caches.delete(pwaCacheKey);
//             return fetch(event.request);
//         }
//         // Try to get the response from a cache.
//         const cachedResponse = await caches.match(event.request);
//         // Return it if we found one.
//         if (cachedResponse) {
//             console.log(event.request.url + " - " + event.request.method + " CACHED");
//             return cachedResponse;
//         }
//         console.log(event.request.url + " - " + event.request.method + " AFTER");
//         // If we didn't find a match in the cache, use the network.
//         return fetch(event.request);
//     }());
// });

self.addEventListener('sync', (event) => {
    if (event.id == 'update-leaderboard') {
        event.waitUntil(async function () {
            const cache = await caches.open('mygame-dynamic');
            await cache.add('/leaderboard.json');
        }());
    }
});

self.addEventListener('fetch', (event) => {
    event.respondWith(async function () {
        console.log(event.request.url + " - " + event.request.method);
        if (/\/account\/signin/gi.test(event.request.url) || /\/account\/signout/gi.test(event.request.url)) {
            caches.delete(pwaCacheKey);
            return fetch(event.request);
        }
        const cache = await caches.open(pwaCacheKey);
        const cachedResponse = await cache.match(event.request);
        if (cachedResponse) {
            console.log(event.request.url + " - " + "FROM CACHE");
            return cachedResponse;
        }
        const networkResponsePromise = fetch(event.request);

        event.waitUntil(async function () {
            console.log(event.request.url + " - " + event.request.method + " NETWORK");
            const networkResponse = await networkResponsePromise;
            if (event.request.method === "GET") {
                await cache.put(event.request, networkResponse.clone());
            }
        }());

        // Returned the cached response if we have one, otherwise return the network response.
        return cachedResponse || networkResponsePromise;
    }());
});