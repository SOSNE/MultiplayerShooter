mergeInto(LibraryManager.library, {
    
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
