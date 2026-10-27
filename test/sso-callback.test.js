import assert from "node:assert/strict"
import { test } from "node:test"

import { handleCallback } from "../src/sso-callback.js"

test("resolves a user from an encoded email", () => {
  const result = handleCallback("email=ana%40acme.com&state=xyz")
  assert.equal(result.status, 200)
  assert.equal(result.user.name, "Ana")
})

test("rejects a callback without an email", () => {
  assert.equal(handleCallback("state=xyz").status, 400)
})
