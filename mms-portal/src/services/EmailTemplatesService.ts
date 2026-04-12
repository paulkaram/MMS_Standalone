import api from '@/plugins/axios'

export interface EmailTemplate {
  id: number
  appCode: string
  name: string
  sendTo: string | null
  subject: string | null
  body: string
}

export interface UpdateEmailTemplateDto {
  subject: string | null
  body: string
  sendTo: string | null
}

const EmailTemplatesService = {
  list(): Promise<any> {
    return api.get('email-templates')
  },
  getById(id: number): Promise<any> {
    return api.get(`email-templates/${id}`)
  },
  update(id: number, dto: UpdateEmailTemplateDto): Promise<any> {
    return api.put(`email-templates/${id}`, dto)
  }
}

export default EmailTemplatesService
