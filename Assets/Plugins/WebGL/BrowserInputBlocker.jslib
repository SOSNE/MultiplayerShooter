mergeInto(LibraryManager.library, {
    BlockBrowserShortcuts: function () {
        if (typeof window !== 'undefined') {
            window.addEventListener("keydown", function (e) {
                if (e.ctrlKey || e.metaKey) {
                    switch (e.code) {
                        case 'KeyW':
                        case 'KeyA':
                        case 'KeyS':
                        case 'KeyD':
                        case 'Space':
                            e.preventDefault();
                            break;
                    }
                }
            }, true);
        }
    }
});
