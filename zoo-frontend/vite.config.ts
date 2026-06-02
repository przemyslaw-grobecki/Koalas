import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig({
  plugins: [react()],
  server: {
    port: 3000,
    proxy: {
      '/api/koalas': {
        target: 'http://localhost:5230',
        changeOrigin: true,
      },
      '/api/bamboo': {
        target: 'http://localhost:5010',
        changeOrigin: true,
      },
    },
  },
})
