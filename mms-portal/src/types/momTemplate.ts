export interface MomTemplateListItem {
  id: number
  branchId: number | null
  branchNameAr: string | null
  branchNameEn: string | null
  templateType: number
  templateTypeName: string
  nameAr: string
  nameEn: string
  isActive: boolean
  isDefault: boolean
  createdDate: string
}

export interface MomTemplate {
  id: number
  branchId: number | null
  branchNameAr: string | null
  branchNameEn: string | null
  templateType: number
  templateTypeName: string
  nameAr: string
  nameEn: string
  config: MomTemplateConfig
  htmlTemplate: string | null
  isActive: boolean
  isDefault: boolean
  createdDate: string
  createdBy: string
  modifiedDate: string | null
  modifiedBy: string | null
}

export interface MomTemplateConfig {
  colors: MomTemplateColors
  fonts: MomTemplateFonts
  pageLayout: MomTemplatePageLayout
  sections: Record<string, MomTemplateSection>
  labels: Record<string, string>
  roles: Record<string, string>
  tableColumns: MomTemplateTableColumns
}

export interface MomTemplateColors {
  primary: string
  secondary: string
  border: string
  text: string
  mutedText: string
  success: string
  danger: string
  white: string
}

export interface MomTemplateFonts {
  arabicFont: string
  fallbackFont: string
  titleSize: number
  headingSize: number
  bodySize: number
  smallSize: number
}

export interface MomTemplatePageLayout {
  topMargin: number
  bottomMargin: number
  leftMargin: number
  rightMargin: number
  rtl: boolean
}

export interface MomTemplateSection {
  visible: boolean
  order: number
}

export interface MomTemplateTableColumns {
  attendeesIndexWidth: number
  attendeesNameWidth: number
  attendeesJobTitleWidth: number
  attendeesRoleWidth: number
  attendeesStatusWidth: number
  recommendationsIndexWidth: number
  recommendationsTextWidth: number
  recommendationsAgendaWidth: number
  recommendationsOwnerWidth: number
  recommendationsDueDateWidth: number
}

export interface MomTemplateCreate {
  branchId: number | null
  templateType: number
  nameAr: string
  nameEn: string
  config: MomTemplateConfig
  htmlTemplate: string | null
  isActive: boolean
  isDefault: boolean
}

export interface MomTemplateUpdate {
  id: number
  branchId: number | null
  templateType: number
  nameAr: string
  nameEn: string
  config: MomTemplateConfig
  htmlTemplate: string | null
  isActive: boolean
  isDefault: boolean
}

export interface MomTemplateType {
  id: number
  nameAr: string
  nameEn: string
}

export interface BranchListItem {
  id: number
  nameAr: string
  nameEn: string
}

// Default configuration for new templates
export const defaultMomTemplateConfig: MomTemplateConfig = {
  colors: {
    primary: '#803580',
    secondary: '#F5F5FA',
    border: '#C085C0',
    text: '#000000',
    mutedText: '#646464',
    success: '#228B22',
    danger: '#DC3545',
    white: '#FFFFFF'
  },
  fonts: {
    arabicFont: 'Sakkal Majalla',
    fallbackFont: 'Arial',
    titleSize: 24,
    headingSize: 14,
    bodySize: 12,
    smallSize: 10
  },
  pageLayout: {
    topMargin: 40,
    bottomMargin: 40,
    leftMargin: 40,
    rightMargin: 40,
    rtl: true
  },
  sections: {
    header: { visible: true, order: 1 },
    meetingInfo: { visible: true, order: 2 },
    attendees: { visible: true, order: 3 },
    agenda: { visible: true, order: 4 },
    voting: { visible: true, order: 5 },
    recommendations: { visible: true, order: 6 },
    summary: { visible: true, order: 7 },
    signatures: { visible: true, order: 8 }
  },
  labels: {
    meetingMinutes: 'محضر اجتماع',
    referenceNumber: 'رقم المرجع',
    meetingNumber: 'رقم الاجتماع',
    meetingInfo: 'معلومات الاجتماع',
    date: 'التاريخ',
    time: 'الوقت',
    location: 'المكان',
    duration: 'المدة',
    attendance: 'الحضور',
    session: 'الدورة',
    attendeesAndAbsence: 'الحضور والغياب',
    name: 'الاسم',
    jobTitle: 'المسمى الوظيفي',
    role: 'الصفة',
    attendanceStatus: 'الحضور',
    present: 'حاضر',
    absent: 'غائب',
    agenda: 'جدول الأعمال',
    agendaItem: 'البند',
    plannedDuration: 'المدة المخططة',
    actualDuration: 'المدة الفعلية',
    minutes: 'دقيقة',
    notes: 'الملاحظات',
    votingResults: 'نتائج التصويت',
    option: 'الخيار',
    votes: 'الأصوات',
    percentage: 'النسبة',
    voters: 'المصوتون',
    decision: 'القرار',
    recommendations: 'التوصيات',
    recommendationsSummary: 'ملخص التوصيات والمهام',
    recommendation: 'التوصية',
    responsible: 'المسؤول',
    dueDate: 'الموعد',
    notSpecified: 'غير محدد',
    meetingSummary: 'ملخص الاجتماع',
    signatures: 'التوقيعات',
    quorumMet: 'النصاب مكتمل',
    quorumNotMet: 'النصاب غير مكتمل',
    outOf: 'من أصل',
    from: 'من',
    to: 'إلى',
    version: 'الإصدار',
    generatedAt: 'تم الإنشاء بتاريخ'
  },
  roles: {
    chairman: 'رئيس',
    vice_chairman: 'نائب الرئيس',
    secretary: 'أمين السر',
    member: 'عضو',
    guest: 'ضيف',
    observer: 'مراقب'
  },
  tableColumns: {
    attendeesIndexWidth: 30,
    attendeesNameWidth: 150,
    attendeesJobTitleWidth: 150,
    attendeesRoleWidth: 70,
    attendeesStatusWidth: 60,
    recommendationsIndexWidth: 25,
    recommendationsTextWidth: 200,
    recommendationsAgendaWidth: 70,
    recommendationsOwnerWidth: 80,
    recommendationsDueDateWidth: 80
  }
}
