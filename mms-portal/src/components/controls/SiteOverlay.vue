<template>
  <div class="site-overlay">
    <div class="loader-grid"></div>
    <div class="loader-glow"></div>
    <div class="loader-scan"></div>

    <div class="loader-container">
      <!-- Animated Icon -->
      <div class="loader-icon-ring">
        <div class="loader-icon-inner">
          <svg width="32" height="32" viewBox="0 0 24 24" fill="none">
            <rect x="3" y="1" width="18" height="20" rx="2.5" stroke="white" stroke-width="1.8" fill="none"/>
            <path d="M7 1V4H17V1" stroke="white" stroke-width="1.8" stroke-linecap="round" fill="none"/>
            <line x1="7.5" y1="9" x2="16.5" y2="9" stroke="white" stroke-width="1.5" stroke-linecap="round" opacity="0.7"/>
            <line x1="7.5" y1="12.5" x2="14" y2="12.5" stroke="white" stroke-width="1.5" stroke-linecap="round" opacity="0.5"/>
            <line x1="7.5" y1="16" x2="12" y2="16" stroke="white" stroke-width="1.5" stroke-linecap="round" opacity="0.3"/>
          </svg>
        </div>
        <svg class="loader-spinner" width="88" height="88" viewBox="0 0 88 88">
          <defs>
            <linearGradient id="overlayLoaderGrad" x1="0%" y1="0%" x2="100%" y2="0%">
              <stop offset="0%" stop-color="#006d4b"/>
              <stop offset="100%" stop-color="rgba(0,174,140,0.1)"/>
            </linearGradient>
          </defs>
          <circle cx="44" cy="44" r="42" fill="none" stroke="rgba(0,174,140,0.08)" stroke-width="1.5"/>
          <circle cx="44" cy="44" r="42" fill="none" stroke="url(#overlayLoaderGrad)" stroke-width="2" stroke-linecap="round" stroke-dasharray="66 198"/>
        </svg>
      </div>

      <!-- Brand Text -->
      <div class="loader-text">
        <span class="loader-brand">INTALIO</span>
        <span class="loader-sub">Loading workspace</span>
      </div>

      <!-- Progress Bar -->
      <div class="loader-progress">
        <div class="loader-progress-bar"></div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
// No props needed - this is a self-contained loading screen
</script>

<style scoped>
.site-overlay {
  position: fixed;
  inset: 0;
  z-index: 9999;
  background: linear-gradient(170deg, #0a1019 0%, #0e1f2f 40%, #0b2a2a 70%, #0a1019 100%);
  display: flex;
  align-items: center;
  justify-content: center;
  overflow: hidden;
}

.loader-grid {
  position: absolute;
  inset: 0;
  background-image:
    linear-gradient(rgba(0,174,140,0.03) 1px, transparent 1px),
    linear-gradient(90deg, rgba(0,174,140,0.03) 1px, transparent 1px);
  background-size: 32px 32px;
  mask-image: radial-gradient(ellipse at center, rgba(0,0,0,0.5) 20%, transparent 65%);
  -webkit-mask-image: radial-gradient(ellipse at center, rgba(0,0,0,0.5) 20%, transparent 65%);
  pointer-events: none;
}

.loader-glow {
  position: absolute;
  width: 350px;
  height: 350px;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  background: radial-gradient(circle, rgba(0,174,140,0.08) 0%, transparent 60%);
  pointer-events: none;
}

.loader-scan {
  position: absolute;
  left: 0;
  right: 0;
  height: 1px;
  background: linear-gradient(90deg, transparent, rgba(0,174,140,0.2), transparent);
  animation: loaderScan 4s ease-in-out infinite;
  pointer-events: none;
}

@keyframes loaderScan {
  0% { top: 20%; opacity: 0; }
  10% { opacity: 1; }
  90% { opacity: 1; }
  100% { top: 80%; opacity: 0; }
}

.loader-container {
  position: relative;
  z-index: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 28px;
}

.loader-icon-ring {
  position: relative;
  width: 88px;
  height: 88px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.loader-icon-inner {
  width: 52px;
  height: 52px;
  border-radius: 14px;
  background: linear-gradient(135deg, #004730 0%, #006d4b 100%);
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: 0 8px 24px rgba(0, 109, 75, 0.3), 0 2px 8px rgba(0, 0, 0, 0.3);
  animation: iconPulse 2s ease-in-out infinite;
  z-index: 1;
}

.loader-spinner {
  position: absolute;
  top: 0;
  left: 0;
  animation: spinLoader 1.5s linear infinite;
}

.loader-text {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 6px;
}

.loader-brand {
  font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
  font-size: 18px;
  font-weight: 800;
  color: #ffffff;
  letter-spacing: 5px;
  text-shadow: 0 0 30px rgba(0, 109, 75, 0.3);
}

.loader-sub {
  font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
  font-size: 12px;
  color: rgba(255, 255, 255, 0.3);
  font-weight: 400;
  letter-spacing: 1px;
}

.loader-progress {
  width: 140px;
  height: 2px;
  background: rgba(255, 255, 255, 0.06);
  border-radius: 3px;
  overflow: hidden;
}

.loader-progress-bar {
  width: 40%;
  height: 100%;
  background: linear-gradient(90deg, rgba(0,174,140,0.3), #006d4b);
  border-radius: 3px;
  animation: progressSlide 1.8s ease-in-out infinite;
}

@keyframes spinLoader {
  from { transform: rotate(0deg); }
  to { transform: rotate(360deg); }
}

@keyframes iconPulse {
  0%, 100% { transform: scale(1); box-shadow: 0 8px 24px rgba(0, 109, 75, 0.3), 0 2px 8px rgba(0, 0, 0, 0.3); }
  50% { transform: scale(1.04); box-shadow: 0 12px 32px rgba(0, 109, 75, 0.4), 0 4px 12px rgba(0, 0, 0, 0.4); }
}

@keyframes progressSlide {
  0% { transform: translateX(-100%); }
  50% { transform: translateX(200%); }
  100% { transform: translateX(350%); }
}
</style>
