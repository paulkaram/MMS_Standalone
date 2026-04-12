import { fileURLToPath, URL } from 'node:url';
import { defineConfig, loadEnv } from 'vite';
import vue from '@vitejs/plugin-vue';
export default defineConfig(function (_a) {
    var mode = _a.mode;
    var env = loadEnv(mode, process.cwd(), '');
    return {
        plugins: [vue()],
        resolve: {
            alias: {
                '@': fileURLToPath(new URL('./src', import.meta.url))
            }
        },
        optimizeDeps: {
            include: ['@microsoft/signalr']
        },
        server: {
            port: 1472,
            proxy: {
                '/api': {
                    target: 'http://localhost:1010',
                    changeOrigin: true
                }
            }
        },
        build: {
            rollupOptions: {
                output: {
                    manualChunks: {
                        'vendor-vue': ['vue', 'vue-router', 'pinia'],
                        'vendor-ui': ['@headlessui/vue', 'primevue'],
                        'vendor-utils': ['axios', 'moment', 'moment-hijri']
                    }
                }
            }
        }
    };
});
