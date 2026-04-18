import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import App from './App.tsx'
import { ThemeProvider } from './components/theme-provider.tsx'
import { TimezoneProvider } from './components/timezone-provider.tsx'
import './index.css'

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <TimezoneProvider>
      <ThemeProvider
        attribute="class"
        defaultTheme="system"
        enableSystem
        storageKey="logcenter-theme"
      >
        <App />
      </ThemeProvider>
    </TimezoneProvider>
  </StrictMode>,
)
