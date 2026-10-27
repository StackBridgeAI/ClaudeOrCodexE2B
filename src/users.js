const USERS = [
  { id: 1, email: "ana@acme.com", name: "Ana" },
  { id: 2, email: "jane+billing@acme.com", name: "Jane (billing)" },
  { id: 3, email: "ana+test@acme.com", name: "Ana (test)" },
]

export function findUserByEmail(email) {
  return USERS.find((u) => u.email === email.toLowerCase())
}
