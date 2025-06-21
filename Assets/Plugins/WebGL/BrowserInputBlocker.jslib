document.addEventListener("keydown", function (e) {
    if (e.ctrlKey || e.metaKey) {
        switch (e.key.toLowerCase()) {
            case 'w':
            case 'a':
            case 's':
            case 'd':
            case ' ':
            case 'space':
                e.preventDefault();
                break;
        }
    }

    if (e.code === "Space") {
        e.preventDefault();
    }
}, true);
