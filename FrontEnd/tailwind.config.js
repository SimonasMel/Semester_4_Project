/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./Components/**/*.razor",
    "./wwwroot/**/*.html",
    "./wwwroot/**/*.js",
  ],
  darkMode: 'class',
  theme: {
    extend: {
      colors: {
        primary: {
          DEFAULT: '#22c55e',
          soft: 'rgba(34, 197, 94, 0.18)',
          blue: '#3b82f6',
        },
        danger: '#ef4444',
        'bg-dark': {
          DEFAULT: '#020617',
          elevated: '#0f172a',
        },
        'bg-light': {
          DEFAULT: '#f5f7fa',
          soft: '#e5f0ff',
        },
        surface: {
          dark: '#0f172a',
          light: '#ffffff',
          'dark-elevated': '#1e293b',
        },
        text: {
          dark: '#0f172a',
          light: '#e5e7eb',
          muted: '#64748b',
        }
      },
      boxShadow: {
        'soft': '0 18px 45px rgba(15, 23, 42, 0.65)',
        'light-soft': '0 10px 25px rgba(148, 163, 184, 0.15)',
      }
    },
  },
  plugins: [],
}
