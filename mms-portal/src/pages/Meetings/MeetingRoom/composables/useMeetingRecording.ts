import { ref, onUnmounted } from 'vue'
import MeetingsService from '@/services/MeetingsService'

export function useMeetingRecording(meetingId: () => number) {
  const isRecording = ref(false)
  const isPaused = ref(false)
  const recordingDuration = ref(0)
  const isUploading = ref(false)
  const isTranscribing = ref(false)
  const transcripts = ref<any[]>([])
  const micPermissionDenied = ref(false)

  let mediaRecorder: MediaRecorder | null = null
  let audioChunks: Blob[] = []
  let mediaStream: MediaStream | null = null
  let durationTimer: ReturnType<typeof setInterval> | null = null

  const startRecording = async () => {
    try {
      micPermissionDenied.value = false
      mediaStream = await navigator.mediaDevices.getUserMedia({ audio: true })

      const mimeType = MediaRecorder.isTypeSupported('audio/webm;codecs=opus')
        ? 'audio/webm;codecs=opus'
        : MediaRecorder.isTypeSupported('audio/webm')
          ? 'audio/webm'
          : 'audio/ogg'

      mediaRecorder = new MediaRecorder(mediaStream, { mimeType })
      audioChunks = []
      recordingDuration.value = 0

      mediaRecorder.ondataavailable = (e) => {
        if (e.data.size > 0) audioChunks.push(e.data)
      }

      mediaRecorder.start(1000)
      isRecording.value = true
      isPaused.value = false

      durationTimer = setInterval(() => {
        if (!isPaused.value) recordingDuration.value++
      }, 1000)

      return true
    } catch (err: any) {
      console.error('[Recording] Failed to start:', err)
      micPermissionDenied.value = true
      return false
    }
  }

  const pauseRecording = () => {
    if (mediaRecorder?.state === 'recording') {
      mediaRecorder.pause()
      isPaused.value = true
    }
  }

  const resumeRecording = () => {
    if (mediaRecorder?.state === 'paused') {
      mediaRecorder.resume()
      isPaused.value = false
    }
  }

  const stopRecording = (): Promise<Blob | null> => {
    return new Promise((resolve) => {
      if (!mediaRecorder || mediaRecorder.state === 'inactive') {
        resolve(null)
        return
      }

      mediaRecorder.onstop = () => {
        const blob = new Blob(audioChunks, { type: mediaRecorder!.mimeType })
        stopTimerAndStream()
        resolve(blob)
      }

      mediaRecorder.stop()
    })
  }

  const stopAndUpload = async (language?: string, attendeeName?: string) => {
    const blob = await stopRecording()
    if (!blob || blob.size === 0) return null

    isUploading.value = true
    isTranscribing.value = true

    try {
      console.log(`[Recording] Uploading ${(blob.size / 1024).toFixed(1)}KB audio for ${attendeeName || 'unknown'}...`)
      const res = await MeetingsService.uploadAudioRecording(
        meetingId(), blob, { language, attendeeName }
      )
      console.log('[Recording] Upload response:', res?.data)
      const transcript = res?.data?.result || res?.data
      if (transcript) {
        transcripts.value.unshift(transcript)
      }
      return transcript
    } catch (err: any) {
      console.error('[Recording] Failed to upload:', err?.response?.data || err.message || err)
      return null
    } finally {
      isUploading.value = false
      isTranscribing.value = false
    }
  }

  const loadTranscripts = async () => {
    try {
      const res = await MeetingsService.getTranscripts(meetingId())
      transcripts.value = res?.data?.result || res?.data || []
    } catch (err) {
      console.error('Failed to load transcripts:', err)
    }
  }

  const generateSummary = async (transcriptId: number) => {
    try {
      const res = await MeetingsService.generateTranscriptSummary(meetingId(), transcriptId)
      const updated = res?.data?.result || res?.data
      if (updated) {
        const idx = transcripts.value.findIndex((t: any) => t.id === transcriptId)
        if (idx >= 0) transcripts.value[idx] = updated
      }
      return updated
    } catch (err) {
      console.error('Failed to generate summary:', err)
      return null
    }
  }

  const generateCombinedSummary = async () => {
    try {
      const res = await MeetingsService.generateCombinedSummary(meetingId())
      const result = res?.data?.result || res?.data
      if (result?.attendeeTranscripts) {
        transcripts.value = result.attendeeTranscripts
      }
      return result
    } catch (err) {
      console.error('Failed to generate combined summary:', err)
      return null
    }
  }

  const formatDuration = (seconds: number) => {
    const m = Math.floor(seconds / 60).toString().padStart(2, '0')
    const s = (seconds % 60).toString().padStart(2, '0')
    return `${m}:${s}`
  }

  const stopTimerAndStream = () => {
    if (durationTimer) {
      clearInterval(durationTimer)
      durationTimer = null
    }
    if (mediaStream) {
      mediaStream.getTracks().forEach(t => t.stop())
      mediaStream = null
    }
    isRecording.value = false
    isPaused.value = false
    mediaRecorder = null
    audioChunks = []
  }

  const cleanup = () => {
    stopTimerAndStream()
  }

  onUnmounted(() => cleanup())

  return {
    isRecording,
    isPaused,
    recordingDuration,
    isUploading,
    isTranscribing,
    transcripts,
    micPermissionDenied,
    startRecording,
    pauseRecording,
    resumeRecording,
    stopRecording,
    stopAndUpload,
    loadTranscripts,
    generateSummary,
    generateCombinedSummary,
    formatDuration,
    cleanup
  }
}
