# Import Service Design

## Objective
Provide a reliable, reviewable, and scalable import workflow for CSV/XLSX files containing phone number data.

## Recommended Flow
1. User uploads file.
2. File is saved through `IFileStorageService`.
3. Import batch record is created.
4. File is parsed through `IImportFileParser`.
5. Each row is normalized and validated.
6. Duplicate-in-file detection is performed using an in-memory hash set.
7. Existing-in-database detection is performed using indexed phone lookup.
8. Validation results are stored in `import_batch_rows`.
9. User reviews preview page.
10. On confirm, only rows marked `new` are inserted into `phone_numbers`.
11. Activity log captures the confirmation event.

## Parser Strategy
### CSV
Use CsvHelper.

### XLSX
Use ClosedXML or EPPlus.

A clean production design is to expose one parser interface and choose implementation by file extension.

## Validation Rules
- required phone number field
- digits only
- length 10 to 14
- duplicate file rows should be marked `duplicate_file`
- existing database rows should be marked `existing`
- invalid rows should be marked `invalid`

## Performance Notes
- For very large uploads, parse and validate in chunks.
- Avoid one query per row in final production implementation; prefer batched existence checks.
- Use transaction during confirm step.
- Use bulk insert for new rows where practical.

## Recommended Future Enhancement
Move large import validation to a background queue if upload sizes become operationally heavy.
