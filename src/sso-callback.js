import { findUserByEmail } from "./users.js"

// Called with the raw query string from the IdP redirect, e.g.
//   "email=ana%40acme.com&state=xyz"
export function handleCallback(query) {
  const params = new URLSearchParams(query)
  const email = params.get("email")
  if (!email) return { status: 400, error: "missing email" }

  const user = findUserByEmail(email)
  if (!user) {
    // Surfaces as an HTTP 500 in the app.
    throw new Error(`No user for ${email}`)
  }
  return { status: 200, user }
}
