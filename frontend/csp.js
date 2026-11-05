// Sicherheitsköpfe der Oberfläche. Die Werte gelten für die Vorschau (Vite) und stehen
// gleichlautend in nginx-sicherheit.conf für den Betrieb im Container.

// Die Oberfläche lädt nichts von fremden Servern: Schriften, Skripte, Bilder und Schnittstelle
// kommen alle vom eigenen Ursprung. Auch Stile stammen nur aus Dateien (Tailwind); die Oberfläche setzt
// keine Inline-Stile im Quelltext.
export const inhaltssicherheitsrichtlinie = [
  "default-src 'self'",
  "script-src 'self'",
  "style-src 'self'",
  "img-src 'self' data:",
  "font-src 'self'",
  "connect-src 'self'",
  "manifest-src 'self'",
  "worker-src 'self'",
  "object-src 'none'",
  "base-uri 'self'",
  "form-action 'self'",
  "frame-ancestors 'none'"
].join('; ');

export const sicherheitskoepfe = {
  'Content-Security-Policy': inhaltssicherheitsrichtlinie,
  'X-Content-Type-Options': 'nosniff',
  'X-Frame-Options': 'DENY',
  'Referrer-Policy': 'strict-origin-when-cross-origin',
  'Permissions-Policy': 'geolocation=(self), camera=(), microphone=()',
  // Trennung vom Ursprung: kein gemeinsamer Browserkontext mit fremden Seiten, Ressourcen nur für den eigenen Ursprung
  'Cross-Origin-Opener-Policy': 'same-origin',
  'Cross-Origin-Resource-Policy': 'same-origin',
  'Cross-Origin-Embedder-Policy': 'require-corp'
};
