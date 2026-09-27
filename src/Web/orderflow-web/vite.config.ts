import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'

const ordersApiProxyTarget = process.env.VITE_ORDERS_API_PROXY_TARGET ?? 'http://orders-api:8080'

export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
      '/api': {
        target: ordersApiProxyTarget,
        changeOrigin: true,
      },
    },
  },
})
