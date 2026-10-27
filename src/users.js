const USERS = [
  { id: 1, email: "ana@acme.com", name: "Ana" },
  { id: 2, email: "jane+billing@acme.com", name: "Jane (billing)" },
]

export function findUserByEmail(email) {
  return USERS.find((u) => u.email === email.toLowerCase())
}
