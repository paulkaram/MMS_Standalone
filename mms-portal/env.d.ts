/// <reference types="vite/client" />

declare module '*.vue' {
  import type { DefineComponent } from 'vue'
  const component: DefineComponent<{}, {}, any>
  export default component
}

declare module 'moment-hijri' {
  import moment from 'moment'

  interface HijriMoment extends moment.Moment {
    iYear(): number
    iYear(y: number): HijriMoment
    iMonth(): number
    iMonth(m: number): HijriMoment
    iDate(): number
    iDate(d: number): HijriMoment
    iDaysInMonth(): number
    iWeek(): number
    iWeekYear(): number
    startOf(unit: string): HijriMoment
    endOf(unit: string): HijriMoment
    add(amount: number, unit: string): HijriMoment
    subtract(amount: number, unit: string): HijriMoment
    clone(): HijriMoment
    format(format?: string): string
  }

  function momentHijri(input?: moment.MomentInput): HijriMoment
  namespace momentHijri {
    export function iDaysInMonth(year: number, month: number): number
  }

  export = momentHijri
}

declare module '@tiptap/extension-underline' {
  import { Extension } from '@tiptap/core'
  const Underline: Extension
  export default Underline
}

interface ImportMetaEnv {
  readonly VITE_MAIN_API: string
  readonly VITE_COOKIE_TIMEOUT: string
  readonly VITE_DEFAULT_LANGUAGE: string
  readonly VITE_SECRET_KEY: string
  readonly VITE_IV: string
  readonly VITE_IAM_AUTHORITY: string
  readonly VITE_IAM_CLIENT_ID: string
  readonly VITE_IAM_REDIRECT: string
}

interface ImportMeta {
  readonly env: ImportMetaEnv
}
