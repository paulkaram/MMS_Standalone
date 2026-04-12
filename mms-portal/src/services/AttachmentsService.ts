import { mainApiAxios as axios } from '@/plugins/axios'

export interface AttachmentInfo {
  id: string
  fileName: string
  fileType: string
  fileSize: number
  contentType: string
  url?: string
  downloadUrl?: string
}

const AttachmentsService = {
  getAttachment(attachmentId: string | number): Promise<any> {
    return axios.get(`attachments/${attachmentId}`)
  },

  downloadAttachment(attachmentId: string): Promise<Blob> {
    return axios.get(`attachments/${attachmentId}/download`, {
      responseType: 'blob'
    })
  },

  // Download using query string (with auth tokens)
  download(query: string): Promise<Blob> {
    return axios.get(`attachments?${query}`, {
      responseType: 'blob'
    })
  },

  uploadAttachment(formData: FormData): Promise<AttachmentInfo> {
    return axios.post('attachments', formData, {
      headers: {
        'Content-Type': 'multipart/form-data'
      }
    })
  },

  deleteAttachment(attachmentId: string): Promise<void> {
    return axios.delete(`attachments/${attachmentId}`)
  }
}

export default AttachmentsService
