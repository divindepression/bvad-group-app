/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./src/**/*.{html,ts}",
  ],
  theme: {
    extend: {
      colors: {
        // 🎨 BVAD GROUP
        'bvad-primary': '#1e3a8a',
        'bvad-secondary': '#f59e0b',
        'bvad-accent': '#10b981',
        'bvad-dark': '#0f172a',
        'bvad-light': '#f8fafc',

        // 🏢 Filiales
        'bvad-agro': '#16a34a',
        'bvad-tech': '#0891b2',
        'bvad-school': '#ea580c',
        'bvad-conseil': '#7c3aed',
      },
      fontFamily: {
        sans: ['Segoe UI', 'system-ui', 'sans-serif'],
      }
    },
  },
  plugins: [],
}