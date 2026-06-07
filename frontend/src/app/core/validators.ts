/**
 * Email must be local@domain.tld with a 2+ letter TLD (rejects "a@b").
 * Pragmatic, not full RFC 5322 — overly strict email regexes reject valid addresses.
 * Mirrored on the backend (ValidationPatterns.Email).
 */
export const EMAIL_PATTERN = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;
