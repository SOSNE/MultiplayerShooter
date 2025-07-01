mergeInto(LibraryManager.library, {
    BlockBrowserShortcuts: function () {
        if (typeof window !== 'undefined') {
            window.addEventListener("keydown", function (e) {
                const code = e.code;
                if (
                    code === 'KeyW' ||
                    code === 'KeyA' ||
                    code === 'KeyS' ||
                    code === 'KeyD' ||
                    code === 'Space'
                ) {
                    e.preventDefault();
                }
                // extra: block dangerous Ctrl/Cmd combos:
                if (e.ctrlKey || e.metaKey) {
                    switch (code) {
                        case 'KeyW':
                        case 'KeyA':
                        case 'KeyS':
                        case 'KeyD':
                        case 'KeyR':
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
