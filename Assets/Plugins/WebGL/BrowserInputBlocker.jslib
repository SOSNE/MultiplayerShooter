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
    },
    
    CopyWebGL: function(strPtr) {
        var str = UTF8ToString(strPtr);
        if (navigator.clipboard) {
          navigator.clipboard.writeText(str)
            .then(console.log)
            .catch(console.error);
        } else {
          document.addEventListener("copy", function listener(e) {
            e.clipboardData.setData("text/plain", str);
            e.preventDefault();
            document.removeEventListener("copy", listener);
          });
          document.execCommand("copy");
        }
      }
});
