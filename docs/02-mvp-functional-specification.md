# MVP Functional Specification

## 1. Overview
Fortune Internal Data is an internal data management system for storing and managing large volumes of phone number records. The MVP focuses on secure access, bulk import, duplicate control, operational searching, controlled record maintenance, and clear auditability.

## 2. In Scope
- User authentication
- Google Authenticator 2FA
- Role-based access control
- Dashboard
- Phone number master data list
- Search and filtering
- Bulk CSV/XLSX upload
- Validation preview before import confirmation
- Import confirmation flow
- Import history
- User management
- Activity logging

## 3. Out of Scope for MVP
- Public-facing customer portal
- Third-party WhatsApp integration
- Automated external status synchronization
- Soft-delete and recovery bin
- Advanced analytics and BI dashboards
- Multi-tenant support
- Localization

## 4. Roles

### 4.1 Superadmin
- Full system access
- Manage users and roles
- Upload and confirm import
- Edit records
- View all history and logs

### 4.2 Admin
- Dashboard access
- Upload and confirm import
- Edit records
- View import history

### 4.3 Manager
- Dashboard access
- View and search records
- Edit status, WhatsApp status, and remark
- View import history

### 4.4 Staff
- View and search records
- Edit status, WhatsApp status, and remark, subject to policy

## 5. Master Data Rules

### 5.1 Phone Number
- Required
- Stored as text
- Digits only
- Length must be between 10 and 14
- No spaces and no symbols
- Must be unique in master table

### 5.2 Phone Status
Allowed values:
- active
- inactive

### 5.3 WhatsApp Status
Allowed values:
- 1day
- 3day
- 7day
- active
- inactive

## 6. Main Features

### 6.1 Authentication
- Login with email and password
- 2FA setup using QR code and secret key
- OTP verification page
- Logout
- Role-based page access

### 6.2 Dashboard
Display:
- Total phone data count
- Total active count
- Total inactive count
- WhatsApp status counts
- Recent upload batches
- Recent updates

### 6.3 Phone Data Management
- List page with pagination
- Search by phone number
- Filter by status
- Filter by WhatsApp status
- Filter by upload date and modified date
- Edit status, WhatsApp status, remark
- View detail page if desired

### 6.4 Upload and Import
- Upload CSV/XLSX file
- Parse rows
- Validate phone number format
- Detect duplicates inside file
- Detect existing phone numbers in system
- Show preview result before import
- Confirm import to insert valid new rows only
- Cancel import without inserting

### 6.5 Import History
- View list of import batches
- View batch summary
- View row-by-row detail
- Track uploader and date/time

### 6.6 User Management
- Create user
- Edit user
- Assign role
- Activate/deactivate user
- Reset password flow by admin policy

## 7. Upload Validation Rules
For each imported row:
1. Read raw phone number.
2. Trim surrounding spaces.
3. Reject if any non-digit remains.
4. Reject if length is outside 10 to 14.
5. Check whether same number appears earlier in same file.
6. Check whether same number already exists in master table.
7. Classify result as one of:
   - new
   - existing
   - invalid
   - duplicate_file

## 8. Import Confirmation Rules
- Only rows marked `new` may be inserted into master table.
- Existing, invalid, and duplicate_file rows must not be inserted.
- Import is not committed until a user with permission confirms it.
- Batch status must be updated after confirmation or cancellation.

## 9. Record Update Rules
- User can update Status, WhatsAppStatus, and Remark.
- ModifiedDate must update on every edit.
- UpdatedAt must update on every edit.
- Activity log should capture before/after values for sensitive changes.

## 10. Acceptance Criteria
1. Secure login works.
2. Google Authenticator 2FA works.
3. Role permissions are enforced.
4. Upload accepts CSV/XLSX.
5. Validation preview correctly separates new, existing, invalid, and duplicate-file rows.
6. Confirm import inserts only valid new rows.
7. Search and filtering work on large data volumes.
8. Manual edit updates status, WhatsApp status, and remark.
9. Import history is retained.
10. User management works for Superadmin.
