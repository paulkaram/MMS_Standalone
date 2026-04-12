import CryptoJS from 'crypto-js'

/**
 * Encrypts a password using AES-CBC encryption
 * The backend expects passwords to be encrypted with this format
 */
export function encryptPassword(password: string): string {
  const secretKey = CryptoJS.enc.Utf8.parse(import.meta.env.VITE_SECRET_KEY || '')
  const vector = CryptoJS.enc.Utf8.parse(import.meta.env.VITE_IV || '')

  // Pad plaintext manually to a multiple of 16 bytes using zero padding
  const blockSize = 16
  let plaintext = CryptoJS.enc.Utf8.parse(password)

  const padLength = blockSize - (plaintext.sigBytes % blockSize)
  const padding = CryptoJS.lib.WordArray.create(new Uint8Array(padLength))
  plaintext = plaintext.concat(padding)

  // Encrypt with no internal padding
  const encrypted = CryptoJS.AES.encrypt(plaintext, secretKey, {
    iv: vector,
    mode: CryptoJS.mode.CBC,
    padding: CryptoJS.pad.NoPadding
  })

  return encrypted.toString()
}

export default {
  encryptPassword
}
