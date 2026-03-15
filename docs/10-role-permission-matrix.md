# Role Permission Matrix

| Module / Action | Superadmin | Admin | Manager | Staff |
|---|---|---|---|---|
| Login | Yes | Yes | Yes | Yes |
| Google Authenticator 2FA | Yes | Yes | Yes | Yes |
| Dashboard | Yes | Yes | Yes | Optional/Limited |
| View Phone Data | Yes | Yes | Yes | Yes |
| Edit Status / WhatsApp / Remark | Yes | Yes | Yes | Yes |
| Upload File | Yes | Yes | No | No |
| Confirm Import | Yes | Yes | No | No |
| View Import History | Yes | Yes | Yes | Limited/No |
| User Management | Yes | No | No | No |
| System Bootstrap / First Admin | Yes | No | No | No |

## Notes
- Superadmin is the only role allowed to manage users.
- Admin can manage imports but not users.
- Manager can monitor and update operational data but cannot import.
- Staff access can be reduced further during implementation if desired.
