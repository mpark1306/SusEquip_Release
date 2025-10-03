window.sessionStorageHelper = {
    setItem: function (key, value) {
        sessionStorage.setItem(key, value);
    },
    getItem: function (key) {
        return sessionStorage.getItem(key);
    },
    removeItem: function (key) {
        sessionStorage.removeItem(key);
    },
    setUsername: function (username) {
        sessionStorage.setItem('username', username);
    },
    getUsername: function () {
        return sessionStorage.getItem('username');
    },
    setTheme: function (theme) {
        sessionStorage.setItem('theme', theme);
    },
    getTheme: function () {
        return sessionStorage.getItem('theme');
    }
};
