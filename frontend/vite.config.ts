import { defineConfig } from 'vite'
import { devtools } from '@tanstack/devtools-vite'
import viteReact from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'

import { tanstackRouter } from '@tanstack/router-plugin/vite'
import { fileURLToPath, URL } from 'node:url'

// https://vitejs.dev/config/
export default defineConfig({
    base: "/RFIDAttendanceSystemPages/",
    plugins: [
        devtools(),
        tanstackRouter({
            target: 'react',
            autoCodeSplitting: true,
        }),
        viteReact(),
        tailwindcss(),
    ],
    resolve: {
        alias: {
            '@': fileURLToPath(new URL('./src', import.meta.url)),
        },
    },
    server: {
        proxy: {
            '/User': {
                target: 'http://localhost:5075',
                changeOrigin: true,
                secure: false,
            },
            '/Session': {
                target: 'http://localhost:5075',
                changeOrigin: true,
                secure: false,
            },
            '/Attendance': {
                target: 'http://localhost:5075',
                changeOrigin: true,
                secure: false,
            },
            '/Device': {
                target: 'http://localhost:5075',
                changeOrigin: true,
                secure: false,
            },
            '/Classroom': {
                target: 'http://localhost:5075',
                changeOrigin: true,
                secure: false,
            },
            '/ws': {
                target: 'http://localhost:5075',
                ws: true,
                changeOrigin: true,
                secure: false,
            }
        }
    },
})
