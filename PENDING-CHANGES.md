# Pending Changes - Fortune Internal Data (2026-03-16 Round 4)

## From Facai:

1. **Pagination - first/last page + jump to page** - Phone data page needs first page, last page buttons, and ability to jump to a specific page number
2. **Batch update expand fields** - Add: remarks, reference to batch update modal (already has status, whatsapp, agent_name)
3. **Batch update by file upload** - Upload Excel/CSV to update existing records (match by phone_number, update fields)
4. **Batch delete by file upload** - Upload Excel/CSV with phone numbers to delete
5. **Phone data column visibility toggle** - User can show/hide columns on phone data page
6. **Add web1-web10 fields (YES/NO)** - 10 new fields on phone data. Dashboard summary of web usage. New summary/export page. Upload import for web status (new import tab). Import history for web updates.
7. **Permission: hide web1-10 for non-superadmin** - Only superadmin can see web1-10 fields
8. **IP whitelist** - Website access restricted by IP. Whitelist: 172.188.217.112, 52.237.88.249, 20.6.33.33. Superadmin can manage whitelist. Permission-based IP management.
9. **WhatsApp status - free text search** - Remove dropdown, make it a text input search
10. **Assign phone numbers to specific users** - Superadmin can assign phone numbers to users. Staff can only view their assigned numbers.
11. **FIX: Permission enforcement for dashboard** - Staff with no dashboard permission can still see it. Need to actually enforce permission checks in controllers.
12. **Export permission separate** - Split "Export" as its own permission in the permission matrix
13. **Strip spaces from phone numbers on import** - Auto-clean phone numbers during import
14. **Export with column split** - Export large datasets with max 500 rows per column, numbers as text format in Excel
