import { mainApiAxios as axios } from '@/plugins/axios'

// Match backend AnnotationTypeEnum
export enum AnnotationType {
  Text = 3,
  Draw = 15,
  Stamp = 21,
  Signature = 23
}

export interface AnnotationPath {
  bezier?: number[]
  points?: number[]
}

export interface StampDto {
  annotationType: AnnotationType
  color?: number[]
  fontSize?: number
  value?: string
  pageIndex: number
  rect?: number[]
  rotation?: number
  stampType?: number
  stampData?: string  // base64 image data
  language?: string
  thickness?: number
  opacity?: number
  paths?: AnnotationPath[]
}

export interface SaveAnnotationsRequest {
  attachmentId: number
  taskId: number
  token: string
  hashValidation: string
  actions?: string
  stamps: StampDto[]
}

const AnnotationsService = {
  /**
   * Get existing annotations for an attachment
   */
  async getAnnotations(
    attachmentId: number,
    taskId: number,
    token: string,
    hashValidation: string,
    actions?: string
  ): Promise<StampDto[]> {
    const params = new URLSearchParams({
      att: attachmentId.toString(),
      task: taskId.toString(),
      tk: token,
      hvd: hashValidation
    })
    if (actions) {
      params.append('act', actions)
    }

    const response = await axios.get(`attachments/annotations?${params.toString()}`)
    return response.data || []
  },

  /**
   * Save annotations to an attachment (burns them into the PDF)
   */
  async saveAnnotations(
    attachmentId: number,
    taskId: number,
    token: string,
    hashValidation: string,
    stamps: StampDto[],
    actions?: string
  ): Promise<any> {
    const params = new URLSearchParams({
      att: attachmentId.toString(),
      task: taskId.toString(),
      tk: token,
      hvd: hashValidation
    })
    if (actions) {
      params.append('act', actions)
    }

    const url = `attachments/annotations?${params.toString()}`

    const response = await axios.post(url, stamps)
    return response
  },

  /**
   * Remove signature from an attachment (unsign)
   */
  async removeSignature(
    attachmentId: number,
    taskId: number,
    token: string,
    hashValidation: string,
    actions?: string
  ): Promise<any> {
    const params = new URLSearchParams({
      att: attachmentId.toString(),
      task: taskId.toString(),
      tk: token,
      hvd: hashValidation
    })
    if (actions) {
      params.append('act', actions)
    }

    const url = `attachments/remove-signature?${params.toString()}`

    const response = await axios.post(url)
    return response
  },

  /**
   * Convert frontend annotation to backend StampDto format
   */
  convertToStampDto(annotation: {
    id: string
    type: 'signature' | 'stamp' | 'text' | 'draw'
    page: number
    x: number
    y: number
    width: number
    height: number
    data?: string
    text?: string
    color?: string
    fontSize?: number
    thickness?: number
    opacity?: number
    paths?: { x: number; y: number }[][]
  }): StampDto {
    const typeMap: Record<string, AnnotationType> = {
      signature: AnnotationType.Signature,
      stamp: AnnotationType.Stamp,
      text: AnnotationType.Text,
      draw: AnnotationType.Draw
    }

    const stamp: StampDto = {
      annotationType: typeMap[annotation.type] || AnnotationType.Stamp,
      pageIndex: annotation.page - 1, // Backend uses 0-based index
      rect: [annotation.x, annotation.y, annotation.x + annotation.width, annotation.y + annotation.height],
      rotation: 0
    }

    // Handle signature/stamp image data
    if (annotation.data && (annotation.type === 'signature' || annotation.type === 'stamp')) {
      // Remove data URL prefix if present
      stamp.stampData = annotation.data.replace(/^data:image\/\w+;base64,/, '')
      stamp.stampType = annotation.type === 'signature' ? 23 : 21
    }

    // Handle text annotation
    if (annotation.type === 'text' && annotation.text) {
      stamp.value = annotation.text
      stamp.fontSize = annotation.fontSize || 14
      if (annotation.color) {
        stamp.color = hexToRgb(annotation.color)
      }
    }

    // Handle draw annotation
    if (annotation.type === 'draw' && annotation.paths) {
      stamp.thickness = annotation.thickness || 2
      stamp.opacity = annotation.opacity || 100
      stamp.paths = annotation.paths.map(path => ({
        points: path.flatMap(p => [p.x, p.y])
      }))
      if (annotation.color) {
        stamp.color = hexToRgb(annotation.color)
      }
    }

    return stamp
  }
}

/**
 * Convert hex color to RGB array
 */
function hexToRgb(hex: string): number[] {
  const result = /^#?([a-f\d]{2})([a-f\d]{2})([a-f\d]{2})$/i.exec(hex)
  return result
    ? [parseInt(result[1], 16), parseInt(result[2], 16), parseInt(result[3], 16)]
    : [0, 0, 0]
}

export default AnnotationsService
