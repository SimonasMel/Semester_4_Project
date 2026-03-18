window.themeManager = {
    setTheme: function (isDark) {
        console.log('Setting theme to:', isDark ? 'dark' : 'light');
        if (isDark) {
            document.documentElement.classList.add('dark');
            localStorage.setItem('theme', 'dark');
        } else {
            document.documentElement.classList.remove('dark');
            localStorage.setItem('theme', 'light');
        }
    },
    getTheme: function () {
        const storedTheme = localStorage.getItem('theme');
        console.log('Stored theme:', storedTheme);
        if (storedTheme) {
            return storedTheme === 'dark';
        }
        const systemDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
        console.log('System dark preference:', systemDark);
        return systemDark;
    }
};
