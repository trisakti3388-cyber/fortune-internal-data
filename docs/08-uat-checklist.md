# UAT Checklist

## Authentication
- [ ] User can open login page.
- [ ] Valid email/password login works.
- [ ] Invalid password is rejected.
- [ ] User can set up Google Authenticator.
- [ ] Valid OTP is accepted.
- [ ] Invalid OTP is rejected.
- [ ] Logout works.

## Authorization
- [ ] Superadmin sees all menus.
- [ ] Admin cannot access user management.
- [ ] Manager cannot confirm import.
- [ ] Staff has only allowed pages.

## Phone Data
- [ ] Search by exact phone number works.
- [ ] Filter by status works.
- [ ] Filter by WhatsApp status works.
- [ ] Edit status works.
- [ ] Edit WhatsApp status works.
- [ ] Edit remark works.
- [ ] Modified date updates after edit.

## Upload and Import
- [ ] CSV upload works.
- [ ] XLSX upload works.
- [ ] Invalid file type is rejected.
- [ ] Invalid phone numbers are marked invalid.
- [ ] Existing numbers are marked existing.
- [ ] Duplicate rows in same file are marked duplicate_file.
- [ ] Preview summary counts are correct.
- [ ] Confirm import inserts only new rows.
- [ ] Cancel import inserts nothing.

## History and Audit
- [ ] Import history shows uploaded batches.
- [ ] Import detail shows row results.
- [ ] Activity log captures key edit and import actions.

## Performance
- [ ] Phone list pagination loads quickly.
- [ ] Exact number search performs acceptably.
- [ ] Large import validation completes acceptably for MVP.
