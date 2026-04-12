/** @type {import('tailwindcss').Config} */
export default {
  content: [
    './index.html',
    './src/**/*.{vue,js,ts,jsx,tsx}'
  ],
  darkMode: 'class',
  theme: {
    extend: {
      colors: {
        // MISA Theme Colors
        primary: {
          DEFAULT: '#006d4b',
          50: '#e8f5ef',
          100: '#d1ebdf',
          200: '#a3d7bf',
          300: '#75c39f',
          400: '#47af7f',
          500: '#198b5f',
          600: '#006d4b',
          700: '#005a3e',
          800: '#004730',
          900: '#003423'
        },
        secondary: {
          DEFAULT: '#63a58f',
          50: '#eef6f3',
          100: '#dceee7',
          200: '#b9dccf',
          300: '#96cbb7',
          400: '#73b99f',
          500: '#63a58f',
          600: '#4f8472',
          700: '#3b6356',
          800: '#284239',
          900: '#14211d'
        },
        // MISA surface colors
        navy: '#111318',
        accent: '#006d4b',
        'background-light': '#fafafa',
        'surface-dark': '#161b22',
        'surface-dark-hover': '#1c2333',
        'border-dark': '#21262d',
        'background-dark': '#0d1117',
        navigation: {
          DEFAULT: '#F2F2F2',
          dark: '#e5e5e5'
        },
        // Semantic colors
        success: {
          DEFAULT: '#22c55e',
          50: '#f0fdf4',
          100: '#dcfce7',
          200: '#bbf7d0',
          500: '#22c55e',
          600: '#16a34a',
          700: '#15803d'
        },
        warning: {
          DEFAULT: '#f59e0b',
          50: '#fffbeb',
          100: '#fef3c7',
          200: '#fde68a',
          500: '#f59e0b',
          600: '#d97706',
          700: '#b45309'
        },
        error: {
          DEFAULT: '#ef4444',
          50: '#fef2f2',
          100: '#fee2e2',
          200: '#fecaca',
          500: '#ef4444',
          600: '#dc2626',
          700: '#b91c1c'
        },
        info: {
          DEFAULT: '#3b82f6',
          50: '#eff6ff',
          100: '#dbeafe',
          200: '#bfdbfe',
          500: '#3b82f6',
          600: '#2563eb',
          700: '#1d4ed8'
        }
      },
      fontFamily: {
        tajawal: ['Tajawal', 'sans-serif'],
        sans: ['"Segoe UI"', '-apple-system', 'BlinkMacSystemFont', 'Roboto', '"Helvetica Neue"', 'Arial', 'sans-serif']
      },
      spacing: {
        'sidebar': '260px',
        'sidebar-collapsed': '64px',
        'header': '60px'
      },
      zIndex: {
        'header': '40',
        'sidebar': '50',
        'modal': '10100',
        'toast': '10200'
      },
      transitionProperty: {
        'spacing': 'margin, padding'
      },
      backgroundImage: {
        'brand-gradient': 'linear-gradient(135deg, #111318, #006d4b)'
      }
    }
  },
  plugins: [
    require('@tailwindcss/forms')({
      strategy: 'class' // Only apply form styles when .form-* classes are used
    }),
    require('tailwindcss-rtl')
  ]
}
