import react from '@vitejs/plugin-react';
import { defineConfig } from 'vite';

export default defineConfig({
    plugins: [react()],
    build: {
        outDir: '../wwwroot/dist',
        emptyOutDir: true,
        rollupOptions: {
            output: {
                entryFileNames: 'main.js',
                chunkFileNames: 'chunks/[name].js',
                assetFileNames: '[name].[ext]',
            },
        },
    },
    server: {
        proxy: {
            '/api': 'http://localhost:5080',
        },
    },
});
