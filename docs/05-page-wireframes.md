# Page Wireframes

This document describes practical wireframes for the MVP pages. The goal is a clean, business-oriented interface suitable for internal operations.

## 1. Landing Page

```text
+-----------------------------------------------------------+
| Fortune Internal Data                                     |
| Internal phone data storage and management platform       |
|                                                           |
| [ Login ]                                                 |
+-----------------------------------------------------------+
```

## 2. Login Page

```text
+-----------------------------------------------------------+
| Fortune Internal Data                                     |
| Sign in to continue                                       |
|                                                           |
| Email        [____________________________]               |
| Password     [____________________________]               |
|                                                           |
|                [ Login ]                                  |
+-----------------------------------------------------------+
```

## 3. Two-Factor Setup Page

```text
+-----------------------------------------------------------+
| Set Up Google Authenticator                               |
|                                                           |
| Scan this QR code with Google Authenticator               |
|                                                           |
| [ QR CODE ]                                               |
|                                                           |
| Manual Key: ABCD-EFGH-IJKL                                |
| Verification Code [____________]                          |
|                                                           |
| [ Verify and Enable ]                                     |
+-----------------------------------------------------------+
```

## 4. OTP Verification Page

```text
+-----------------------------------------------------------+
| Two-Factor Authentication                                 |
| Enter the 6-digit code from Google Authenticator          |
|                                                           |
| Code [____________]                                       |
|                                                           |
| [ Verify ]                                                |
+-----------------------------------------------------------+
```

## 5. Main Layout

```text
+-------------------+---------------------------------------+
| Sidebar           | Top Bar                               |
| - Dashboard       | User Name / Role / Logout             |
| - Phone Data      +---------------------------------------+
| - Upload Import   | Page Content                          |
| - Import History  |                                       |
| - User Mgmt*      |                                       |
|                   |                                       |
| *Superadmin only  |                                       |
+-------------------+---------------------------------------+
```

## 6. Dashboard

```text
+-----------------------------------------------------------+
| Total Phone Data | Active | Inactive | WA 1day | WA 3day  |
| WA 7day | WA Active | WA Inactive                         |
+-----------------------------------------------------------+
| Recent Uploads                                              |
| Batch | File | Uploader | Date | New | Existing | Status   |
+-----------------------------------------------------------+
| Recent Updates                                              |
| Phone Number | Updated By | Modified Date                  |
+-----------------------------------------------------------+
```

## 7. Phone Data List

```text
+-----------------------------------------------------------+
| Search [________________] [ Search ]                      |
| Status [active v] WA Status [all v] Date From [ ] To [ ]  |
|                                                           |
| Seq | Phone Number | Remark | Status | WA Status | Action |
| 1   | 081234...    | note   | active | 1day      | Edit   |
| ...                                                       |
|                                                           |
| Pagination: << 1 2 3 4 >>                                 |
+-----------------------------------------------------------+
```

## 8. Edit Phone Data Modal

```text
+-------------------------------------------+
| Edit Phone Data                           |
|-------------------------------------------|
| Phone Number: 081234567890                |
| Status:          [active v]               |
| WhatsApp Status: [1day v]                 |
| Remark:                                   |
| [_____________________________________]   |
| [_____________________________________]   |
|                                           |
| [ Save ]   [ Cancel ]                     |
+-------------------------------------------+
```

## 9. Upload Import Page

```text
+-----------------------------------------------------------+
| Upload Phone Data File                                    |
| Supported formats: .csv, .xlsx                            |
| Required column: phone_number                             |
| Optional columns: seq, remark                             |
|                                                           |
| [ Choose File ]                                           |
|                                                           |
| [ Upload and Validate ]                                   |
| [ Download Sample Template ]                              |
+-----------------------------------------------------------+
```

## 10. Upload Preview Page

```text
+-----------------------------------------------------------+
| Import Preview                                             |
| Total | New | Existing | Invalid | Duplicate File         |
+-----------------------------------------------------------+
| Seq | Raw Phone | Normalized | Remark | Result | Message  |
| 1   | 0812...   | 0812...    | note   | new    | OK       |
| 2   | 08 12...  | null       | note   | invalid| non-digit|
| ...                                                       |
+-----------------------------------------------------------+
| [ Confirm Import ]   [ Cancel ]                           |
+-----------------------------------------------------------+
```

## 11. Import History Page

```text
+-----------------------------------------------------------+
| Import History                                             |
| Batch | File | Uploaded By | Date | Total | New | Status  |
| 101   | a.csv| Admin A     | ...  | 1000  | 200 | confirmed|
| ...                                                       |
+-----------------------------------------------------------+
| Action: [ View Detail ]                                   |
+-----------------------------------------------------------+
```

## 12. Import Detail Page

```text
+-----------------------------------------------------------+
| Import Batch Detail                                        |
| File Name: customers.csv                                  |
| Uploaded By: Admin A                                      |
| Status: confirmed                                         |
| Summary: Total 1000 / New 200 / Existing 700 / Invalid 50 |
| Duplicate 50                                              |
+-----------------------------------------------------------+
| Row detail table                                           |
+-----------------------------------------------------------+
```

## 13. User Management Page

```text
+-----------------------------------------------------------+
| Users                                                      |
| [ Add User ]                                               |
|                                                           |
| Name | Email | Role | 2FA | Status | Action              |
| John | ...   | Admin| Yes | Active | Edit                |
+-----------------------------------------------------------+
```
