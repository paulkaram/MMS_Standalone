<template>
  <Teleport to="body">
    <div class="mrl-overlay">
      <!-- Animated Background -->
      <div class="mrl-bg">
        <div class="mrl-grid"></div>
        <div class="mrl-particles">
          <span v-for="i in 15" :key="i" class="mrl-particle" :style="getParticleStyle(i)"></span>
        </div>
        <div class="mrl-glow"></div>
      </div>

      <!-- Main Content -->
      <div class="mrl-content">
        <!-- Icon Container -->
        <div class="mrl-icon-wrap">
          <div class="mrl-icon-glow"></div>
          <div class="mrl-ring mrl-ring-outer">
            <div class="mrl-ring mrl-ring-middle">
              <div class="mrl-ring mrl-ring-inner">
                <svg class="mrl-icon" viewBox="0 0 64 64" fill="none">
                  <ellipse cx="32" cy="44" rx="22" ry="7" fill="rgba(255,255,255,0.1)" stroke="rgba(255,255,255,0.3)" stroke-width="1"/>
                  <circle cx="32" cy="18" r="7" fill="white" class="mrl-user-main"/>
                  <path d="M24 30 Q32 35 40 30" stroke="white" stroke-width="2.5" fill="none" stroke-linecap="round"/>
                  <circle cx="14" cy="32" r="5" fill="white" class="mrl-user-left"/>
                  <circle cx="50" cy="32" r="5" fill="white" class="mrl-user-right"/>
                  <circle cx="23" cy="24" r="2" fill="#198b5f" class="mrl-dot mrl-dot-1"/>
                  <circle cx="41" cy="24" r="2" fill="#198b5f" class="mrl-dot mrl-dot-2"/>
                </svg>
              </div>
            </div>
          </div>
          <!-- Orbits -->
          <div class="mrl-orbit mrl-orbit-1"><span class="mrl-orbit-dot"></span></div>
          <div class="mrl-orbit mrl-orbit-2"><span class="mrl-orbit-dot"></span></div>
        </div>

        <!-- Title -->
        <h2 class="mrl-title">{{ $t('MeetingRoom') }}</h2>

        <!-- Progress -->
        <div class="mrl-progress">
          <div class="mrl-progress-track">
            <div class="mrl-progress-bar"></div>
          </div>
        </div>

        <!-- Status -->
        <div class="mrl-status">
          <span class="mrl-status-dot"></span>
          <span class="mrl-status-text">{{ loadingText || $t('LoadingMeetingData') }}</span>
        </div>

        <!-- Steps -->
        <div class="mrl-steps">
          <div class="mrl-step mrl-step-active">
            <div class="mrl-step-icon">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <rect x="3" y="4" width="18" height="18" rx="2"/>
                <path d="M16 2v4M8 2v4M3 10h18"/>
              </svg>
            </div>
            <span class="mrl-step-label">{{ $t('LoadingData') }}</span>
          </div>
          <div class="mrl-step-line"></div>
          <div class="mrl-step">
            <div class="mrl-step-icon">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/>
                <circle cx="9" cy="7" r="4"/>
                <path d="M23 21v-2a4 4 0 0 0-3-3.87M16 3.13a4 4 0 0 1 0 7.75"/>
              </svg>
            </div>
            <span class="mrl-step-label">{{ $t('Attendance') }}</span>
          </div>
          <div class="mrl-step-line"></div>
          <div class="mrl-step">
            <div class="mrl-step-icon">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <path d="M21 15a2 2 0 0 1-2 2H7l-4 4V5a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2z"/>
              </svg>
            </div>
            <span class="mrl-step-label">{{ $t('Connection') }}</span>
          </div>
        </div>
      </div>
    </div>
  </Teleport>
</template>

<script setup lang="ts">
defineProps<{
  loadingText?: string
}>()

function getParticleStyle(index: number) {
  const size = 3 + Math.random() * 5
  const x = Math.random() * 100
  const y = Math.random() * 100
  const delay = Math.random() * 4
  const duration = 4 + Math.random() * 4
  return {
    '--p-size': `${size}px`,
    '--p-x': `${x}%`,
    '--p-y': `${y}%`,
    '--p-delay': `${delay}s`,
    '--p-duration': `${duration}s`
  }
}
</script>

<style>
/* ═══════════════════════════════════════════════════════════════════════════
   MEETING ROOM LOADING - All classes prefixed with mrl-
   ═══════════════════════════════════════════════════════════════════════════ */

.mrl-overlay {
  position: fixed !important;
  top: 0 !important;
  left: 0 !important;
  right: 0 !important;
  bottom: 0 !important;
  z-index: 99999 !important;
  display: flex !important;
  align-items: center !important;
  justify-content: center !important;
  background: linear-gradient(135deg, #0a0f0d 0%, #0d1912 50%, #0a1410 100%) !important;
  direction: rtl !important;
  overflow: hidden !important;
}

/* Background */
.mrl-bg {
  position: absolute;
  inset: 0;
  overflow: hidden;
}

.mrl-grid {
  position: absolute;
  inset: 0;
  background-image:
    linear-gradient(rgba(0, 109, 75, 0.04) 1px, transparent 1px),
    linear-gradient(90deg, rgba(0, 109, 75, 0.04) 1px, transparent 1px);
  background-size: 50px 50px;
  animation: mrl-grid-pulse 3s ease-in-out infinite;
}

.mrl-particles {
  position: absolute;
  inset: 0;
}

.mrl-particle {
  position: absolute;
  width: var(--p-size);
  height: var(--p-size);
  left: var(--p-x);
  top: var(--p-y);
  background: radial-gradient(circle, #006d4b, transparent);
  border-radius: 50%;
  opacity: 0.5;
  animation: mrl-float var(--p-duration) ease-in-out var(--p-delay) infinite;
}

.mrl-glow {
  position: absolute;
  top: 50%;
  left: 50%;
  width: 500px;
  height: 500px;
  transform: translate(-50%, -50%);
  background: radial-gradient(circle, rgba(0, 109, 75, 0.2) 0%, transparent 70%);
  animation: mrl-glow-pulse 3s ease-in-out infinite;
}

/* Content */
.mrl-content {
  position: relative;
  z-index: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  padding: 2rem;
}

/* Icon */
.mrl-icon-wrap {
  position: relative;
  margin-bottom: 2rem;
}

.mrl-icon-glow {
  position: absolute;
  inset: -50px;
  background: radial-gradient(circle, rgba(0, 109, 75, 0.4) 0%, transparent 70%);
  animation: mrl-glow-pulse 2s ease-in-out infinite;
}

.mrl-ring {
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 50%;
}

.mrl-ring-outer {
  width: 180px;
  height: 180px;
  background: rgba(0, 109, 75, 0.05);
  border: 1px solid rgba(0, 109, 75, 0.15);
  animation: mrl-rotate 20s linear infinite;
}

.mrl-ring-middle {
  width: 145px;
  height: 145px;
  background: rgba(0, 109, 75, 0.1);
  border: 1px solid rgba(0, 109, 75, 0.25);
  animation: mrl-rotate 15s linear infinite reverse;
}

.mrl-ring-inner {
  width: 110px;
  height: 110px;
  background: linear-gradient(135deg, #006d4b 0%, #198b5f 100%);
  box-shadow: 0 0 60px rgba(0, 109, 75, 0.5), 0 0 100px rgba(0, 109, 75, 0.3);
}

.mrl-icon {
  width: 65px;
  height: 65px;
}

.mrl-user-main {
  animation: mrl-pulse 2s ease-in-out infinite;
}

.mrl-user-left {
  opacity: 0;
  animation: mrl-appear 0.5s ease-out 0.3s forwards;
}

.mrl-user-right {
  opacity: 0;
  animation: mrl-appear 0.5s ease-out 0.6s forwards;
}

.mrl-dot {
  animation: mrl-dot-ping 1.5s ease-in-out infinite;
}

.mrl-dot-1 { animation-delay: 0s; }
.mrl-dot-2 { animation-delay: 0.5s; }

/* Orbits */
.mrl-orbit {
  position: absolute;
  border: 1px dashed rgba(0, 109, 75, 0.2);
  border-radius: 50%;
  top: 50%;
  left: 50%;
}

.mrl-orbit-1 {
  width: 220px;
  height: 220px;
  margin: -110px 0 0 -110px;
  animation: mrl-orbit-spin 8s linear infinite;
}

.mrl-orbit-2 {
  width: 260px;
  height: 260px;
  margin: -130px 0 0 -130px;
  animation: mrl-orbit-spin 12s linear infinite reverse;
}

.mrl-orbit-dot {
  position: absolute;
  width: 8px;
  height: 8px;
  background: #006d4b;
  border-radius: 50%;
  top: 0;
  left: 50%;
  margin-left: -4px;
  box-shadow: 0 0 15px #006d4b;
}

/* Title */
.mrl-title {
  font-family: 'Tajawal', sans-serif;
  font-size: 2rem;
  font-weight: 700;
  color: #fff;
  margin: 0 0 2rem 0;
  text-shadow: 0 0 40px rgba(0, 109, 75, 0.6);
  animation: mrl-title-glow 2s ease-in-out infinite;
}

/* Progress */
.mrl-progress {
  width: 250px;
  margin-bottom: 1.5rem;
}

.mrl-progress-track {
  height: 4px;
  background: rgba(255, 255, 255, 0.1);
  border-radius: 4px;
  overflow: hidden;
}

.mrl-progress-bar {
  height: 100%;
  width: 40%;
  background: linear-gradient(90deg, #006d4b, #198b5f);
  border-radius: 4px;
  animation: mrl-progress-move 1.5s ease-in-out infinite;
}

/* Status */
.mrl-status {
  display: flex;
  align-items: center;
  gap: 10px;
  margin-bottom: 2rem;
}

.mrl-status-dot {
  width: 10px;
  height: 10px;
  background: #006d4b;
  border-radius: 50%;
  box-shadow: 0 0 15px #006d4b;
  animation: mrl-blink 1s ease-in-out infinite;
}

.mrl-status-text {
  font-family: 'Tajawal', sans-serif;
  font-size: 1rem;
  color: rgba(255, 255, 255, 0.8);
}

/* Steps */
.mrl-steps {
  display: flex;
  align-items: center;
  gap: 8px;
}

.mrl-step {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 8px;
  opacity: 0.4;
}

.mrl-step-active {
  opacity: 1;
}

.mrl-step-icon {
  width: 40px;
  height: 40px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: rgba(0, 109, 75, 0.15);
  border: 1px solid rgba(0, 109, 75, 0.3);
  border-radius: 12px;
  color: #006d4b;
}

.mrl-step-icon svg {
  width: 20px;
  height: 20px;
}

.mrl-step-label {
  font-family: 'Tajawal', sans-serif;
  font-size: 0.8rem;
  color: rgba(255, 255, 255, 0.6);
}

.mrl-step-line {
  width: 30px;
  height: 2px;
  background: rgba(0, 109, 75, 0.3);
  margin-bottom: 30px;
}

/* ═══════════════════════════════════════════════════════════════════════════
   ANIMATIONS
   ═══════════════════════════════════════════════════════════════════════════ */

@keyframes mrl-grid-pulse {
  0%, 100% { opacity: 1; }
  50% { opacity: 0.5; }
}

@keyframes mrl-float {
  0%, 100% { transform: translateY(0); opacity: 0.5; }
  50% { transform: translateY(-25px); opacity: 0.8; }
}

@keyframes mrl-glow-pulse {
  0%, 100% { opacity: 0.8; transform: translate(-50%, -50%) scale(1); }
  50% { opacity: 1; transform: translate(-50%, -50%) scale(1.1); }
}

@keyframes mrl-rotate {
  from { transform: rotate(0deg); }
  to { transform: rotate(360deg); }
}

@keyframes mrl-pulse {
  0%, 100% { transform: scale(1); }
  50% { transform: scale(1.1); }
}

@keyframes mrl-appear {
  from { opacity: 0; transform: scale(0.5); }
  to { opacity: 1; transform: scale(1); }
}

@keyframes mrl-dot-ping {
  0%, 100% { r: 2; opacity: 1; }
  50% { r: 3; opacity: 0.5; }
}

@keyframes mrl-orbit-spin {
  from { transform: rotate(0deg); }
  to { transform: rotate(360deg); }
}

@keyframes mrl-title-glow {
  0%, 100% { text-shadow: 0 0 40px rgba(0, 109, 75, 0.6); }
  50% { text-shadow: 0 0 60px rgba(0, 109, 75, 0.9); }
}

@keyframes mrl-progress-move {
  0% { margin-left: -40%; }
  100% { margin-left: 100%; }
}

@keyframes mrl-blink {
  0%, 100% { opacity: 1; }
  50% { opacity: 0.4; }
}

/* ═══════════════════════════════════════════════════════════════════════════
   RESPONSIVE
   ═══════════════════════════════════════════════════════════════════════════ */

@media (max-width: 480px) {
  .mrl-ring-outer { width: 150px; height: 150px; }
  .mrl-ring-middle { width: 120px; height: 120px; }
  .mrl-ring-inner { width: 90px; height: 90px; }
  .mrl-icon { width: 55px; height: 55px; }
  .mrl-orbit-1 { width: 180px; height: 180px; margin: -90px 0 0 -90px; }
  .mrl-orbit-2 { width: 210px; height: 210px; margin: -105px 0 0 -105px; }
  .mrl-title { font-size: 1.5rem; }
  .mrl-progress { width: 200px; }
  .mrl-steps { flex-wrap: wrap; justify-content: center; }
  .mrl-step-line { display: none; }
}
</style>
