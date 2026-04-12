/**
 * Downloads a file from a Blob or Response object
 */
export function downloadFile(response: Blob | Response, filename?: string): void {
  let downloadFilename = filename || 'download'

  if (response instanceof Response) {
    // Extract filename from Content-Disposition header if available
    const contentDisposition = response.headers.get('Content-Disposition')
    if (contentDisposition) {
      const filenameMatch = contentDisposition.match(/filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/)
      if (filenameMatch && filenameMatch[1]) {
        downloadFilename = decodeURIComponent(filenameMatch[1].replace(/['"]/g, ''))
      }
    }

    response.blob().then((responseBlob) => {
      triggerDownload(responseBlob, downloadFilename)
    })
  } else if (response instanceof Blob) {
    triggerDownload(response, downloadFilename)
  }
}

/**
 * Triggers the browser download for a Blob
 */
function triggerDownload(blob: Blob, filename: string): void {
  const url = window.URL.createObjectURL(blob)
  const link = document.createElement('a')
  link.href = url
  link.download = filename
  document.body.appendChild(link)
  link.click()
  document.body.removeChild(link)
  window.URL.revokeObjectURL(url)
}

/**
 * Downloads a file from a URL
 */
export function downloadFromUrl(url: string, filename?: string): void {
  const link = document.createElement('a')
  link.href = url
  link.download = filename || 'download'
  document.body.appendChild(link)
  link.click()
  document.body.removeChild(link)
}

/**
 * Converts a base64 string to a Blob
 */
export function base64ToBlob(base64: string, mimeType: string = 'application/octet-stream'): Blob {
  const byteCharacters = atob(base64.split(',')[1] || base64)
  const byteNumbers = new Array(byteCharacters.length)

  for (let i = 0; i < byteCharacters.length; i++) {
    byteNumbers[i] = byteCharacters.charCodeAt(i)
  }

  const byteArray = new Uint8Array(byteNumbers)
  return new Blob([byteArray], { type: mimeType })
}
