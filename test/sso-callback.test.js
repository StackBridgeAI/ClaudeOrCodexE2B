import assert from "node:assert/strict"
import { test } from "node:test"

import { handleCallback } from "../src/sso-callback.js"

test("resolves a user from an encoded email", () => {
  const result = handleCallback("email=ana%40acme.com&state=xyz")
  assert.equal(result.status, 200)
  assert.equal(result.user.name, "Ana")
})

test("keeps a raw plus in the email (AC1)", () => {
  const result = handleCallback("email=ana+test%40acme.com&state=xyz")
  assert.equal(result.status, 200)
  assert.equal(result.user.name, "Ana (test)")
})

test("resolves the existing plus fixture (AC2)", () => {
  const result = handleCallback("email=jane+billing%40acme.com&state=xyz")
  assert.equal(result.status, 200)
  assert.equal(result.user.name, "Jane (billing)")
})

test("decodes a percent-encoded plus (AC3)", () => {
  const result = handleCallback("email=ana%2Btest%40acme.com&state=xyz")
  assert.equal(result.status, 200)
  assert.equal(result.user.name, "Ana (test)")
})

test("unknown plus email error shows the plus (FR2.3)", () => {
  assert.throws(
    () => handleCallback("email=nobody+x%40acme.com&state=xyz"),
    /No user for nobody\+x@acme\.com/,
  )
})

test("rejects a callback without an email", () => {
  assert.equal(handleCallback("state=xyz").status, 400)
})

test("resolves the exact reported callback with a literal plus", () => {
  assert.deepEqual(handleCallback("email=ana+test%40acme.com&state=xyz"), {
    status: 200,
    user: { id: 3, email: "ana+test@acme.com", name: "Ana (test)" },
  })
})

test("preserves a plus in the unknown-user error", () => {
  assert.throws(
    () => handleCallback("email=ghost+test%40acme.com&state=xyz"),
    { name: "Error", message: "No user for ghost+test@acme.com" },
  )
})

test("resolves the existing plus-address user", () => {
  assert.deepEqual(handleCallback("email=jane+billing%40acme.com&state=xyz"), {
    status: 200,
    user: { id: 2, email: "jane+billing@acme.com", name: "Jane (billing)" },
  })
})

test("resolves a percent-encoded plus identically to a literal plus", () => {
  assert.deepEqual(
    handleCallback("email=jane%2Bbilling%40acme.com&state=xyz"),
    handleCallback("email=jane+billing%40acme.com&state=xyz"),
  )
})
