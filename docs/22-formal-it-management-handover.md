# Fortune Internal Data — Formal IT Management Handover

**Document type:** Professional handover brief  
**Prepared for:** IT Management  
**Date:** 15 March 2026  
**Project:** Fortune Internal Data  
**Technology:** ASP.NET Core MVC (.NET 8) + MySQL 8+

---

## 1. Executive Summary

Fortune Internal Data is an internal web-based system designed to replace spreadsheet-based phone number storage with a secure, scalable, and auditable platform capable of managing more than 3,000,000 phone number records.

A professional handover package has been prepared, containing:
- complete business and technical documentation,
- a MySQL database schema draft,
- a .NET solution with layered project structure,
- starter code scaffolding across all application layers,
- role and permission definitions,
- import workflow design,
- identity and two-factor authentication design,
- deployment and bootstrap guidance,
- UAT checklist,
- and operational checklists for the IT team.

This package is designed to enable the IT team to begin implementation immediately without spending time rediscovering business requirements or initial architecture decisions.

---

## 2. Business Justification

| Current state | Target state |
|---|---|
| Phone numbers stored in Microsoft Excel | Centralized web-based database |
| No access control | Role-based access (Superadmin, Admin, Manager, Staff) |
| No duplicate protection | Automated validation before import |
| No audit trail | Activity logging and import history |
| No authentication security | Password + Google Authenticator 2FA |
| Manual file management | Structured CSV/XLSX upload with preview and confirmation |

---

## 3. Scope of Delivery

### In scope (MVP)
- Secure authentication with Google Authenticator TOTP
- Role-based access control (4 roles)
- CSV and XLSX bulk upload
- Pre-import validation and duplicate detection
- Import preview and confirmation workflow
- Phone number master data search, filter, and edit
- Dashboard for operational visibility
- Import history and activity logging
- User management for privileged roles

### Out of scope (MVP)
- Public-facing portal
- Third-party WhatsApp API integration
- Advanced analytics or BI
- Multi-tenant architecture
- Localization

---

## 4. Role Model

| Role | User management | Upload & import | Edit records | View data | Dashboard |
|---|---|---|---|---|---|
| Superadmin | ✅ | ✅ | ✅ | ✅ | ✅ |
| Admin | — | ✅ | ✅ | ✅ | ✅ |
| Manager | — | — | ✅ | ✅ | ✅ |
| Staff | — | — | ✅ | ✅ | Limited |

---

## 5. Technology Stack

| Component | Technology |
|---|---|
| Web framework | ASP.NET Core MVC (.NET 8) |
| Database | MySQL 8+ |
| ORM | Entity Framework Core |
| Authentication | ASP.NET Identity |
| Two-factor auth | Google Authenticator compatible TOTP |
| CSV parsing | CsvHelper |
| XLSX parsing | ClosedXML or EPPlus |
| Version control | GitHub |

---

## 6. Handover Package Contents

The repository includes the following documentation and starter materials:

### Documentation (21 documents)
| # | Document | Purpose |
|---|---|---|
| 01 | Executive Summary | Business objectives and scope |
| 02 | MVP Functional Specification | Detailed feature and acceptance criteria |
| 03 | Technical Architecture | System design and layering |
| 04 | Database Schema (MySQL SQL) | Table definitions and indexes |
| 05 | Page Wireframes | UI layout guidance |
| 06 | Development Task Breakdown | Sprint-level task list |
| 07 | Deployment and Handover Guide | Environment and release guidance |
| 08 | UAT Checklist | Testing scenarios |
| 09 | Implementation Notes for IT | Technical guidance for developers |
| 10 | Role Permission Matrix | Access control reference |
| 11 | Bootstrap Superadmin Guide | First admin account creation |
| 12 | Import Service Design | Upload/validate/confirm workflow |
| 13 | Identity and 2FA Design | Authentication architecture |
| 14 | GitHub Upload Checklist | Repository publishing steps |
| 15 | Repository Map | Code structure guide |
| 16 | Implementation Status | What is done vs remaining |
| 17 | Next Steps for IT | Sprint recommendations |
| 18 | Professional Handover Summary | Executive summary for handover |
| 19 | Final IT Handover and Operations | Comprehensive IT guide with 3-job model |
| 20 | IT Action Checklist | Step-by-step implementation checklist |
| 21 | Deployment-Ready Checklist | Go/no-go criteria before release |

### Starter Code
| Project | Layer | Contents |
|---|---|---|
| FortuneInternalData.Web | Presentation | Controllers, Views, ViewModels, Security policies, DI extensions |
| FortuneInternalData.Application | Application | DTOs, Service interfaces, Service implementations, Validators |
| FortuneInternalData.Domain | Domain | Entities, Constants (roles, statuses, import) |
| FortuneInternalData.Infrastructure | Infrastructure | DbContext, Repositories, File storage, Import parser, Identity scaffold |

### Supporting Materials
- Sample import template (`samples/import-template.csv`)
- Solution file (`FortuneInternalData.sln`)
- SDK target (`global.json`)

---

## 7. The 3 Core Operational Jobs

The system is structured around three main operational workflows that IT should implement and own:

### Job 1 — Import Intake
Receive uploaded CSV/XLSX files, parse and validate phone numbers, detect duplicates, and prepare a preview summary for user review.

### Job 2 — Validation and Confirm Import
Allow authorized users to review the import preview, then confirm insertion of only valid new records into the master database within a controlled transaction. All actions are logged for audit.

### Job 3 — Master Data and Administration
Provide daily operational capability: search, filter, and edit phone records; manage users and roles; view dashboard summaries; and review import history.

---

## 8. Implementation Roadmap

| Phase | Focus | Estimated effort |
|---|---|---|
| Phase 1 | Solution setup, database wiring, Identity integration, first Superadmin | Sprint 1 |
| Phase 2 | Phone data list/search/edit, dashboard, activity logging | Sprint 2 |
| Phase 3 | CSV/XLSX parser, import preview, confirm import | Sprint 3 |
| Phase 4 | Import history, user management, hardening, staging deployment, UAT | Sprint 4 |

---

## 9. Key Data Rules

| Rule | Specification |
|---|---|
| Storage format | Text (VARCHAR), not numeric |
| Allowed length | 10 to 14 digits |
| Allowed characters | Digits only |
| Spaces / symbols | Not allowed |
| Duplicate logic | Normalized text comparison |

---

## 10. Security Design

| Area | Approach |
|---|---|
| Authentication | ASP.NET Identity with password + TOTP |
| Two-factor auth | Google Authenticator compatible |
| Authorization | Role-based policies (4 roles) |
| Bootstrap | Environment-variable-based first admin, disabled after use |
| Credentials | Never hardcoded in repository |
| Sessions | Secure cookie with HTTPS |

---

## 11. Risk and Mitigation

| Risk | Mitigation |
|---|---|
| Scaffold is not production-complete code | Clear documentation of what IT must finish; checklists provided |
| Large data volume (3M+ records) | Indexed phone column, batched queries, performance testing in staging |
| Import errors or duplicates | Pre-import validation with preview; transaction-based confirm |
| Credential exposure | Environment variables for secrets; bootstrap disabled after first use |
| Knowledge transfer gaps | 21 documents covering business rules, architecture, and implementation |

---

## 12. Recommended Immediate Actions for IT Management

1. **Assign project owner** within IT team.
2. **Upload repository** to GitHub (internal or private).
3. **Schedule kickoff meeting** to walk through business rules, role model, and import workflow.
4. **Allocate development resource** for 4 sprints of implementation.
5. **Provision staging environment** (server with .NET 8 runtime + MySQL 8+).
6. **Review checklists** (docs 20 and 21) as the team's delivery tracking tool.
7. **Execute UAT** using the provided checklist (doc 08).
8. **Sign off staging** before production deployment.

---

## 13. Handover Deliverables Summary

| Deliverable | Status |
|---|---|
| Business specification | ✅ Complete |
| Technical architecture | ✅ Complete |
| Database schema draft | ✅ Complete |
| Role permission model | ✅ Complete |
| Import workflow design | ✅ Complete |
| Identity and 2FA design | ✅ Complete |
| .NET solution structure | ✅ Complete |
| Starter code scaffold | ✅ Complete |
| Documentation package (21 docs) | ✅ Complete |
| IT action checklist | ✅ Complete |
| Deployment-ready checklist | ✅ Complete |
| Sample import file | ✅ Complete |
| Production-ready application | ⬜ IT team to complete |

---

## 14. Conclusion

Fortune Internal Data has been prepared as a professional, well-documented handover package. The IT team receives a clear business scope, defined data rules, a structured .NET solution, starter code across all layers, and comprehensive implementation guidance.

The recommended path forward is to assign IT ownership, upload the repository, and begin the 4-phase implementation plan. With the documentation and checklists provided, the team should be able to move from handover to a working staging build efficiently.

This handover is designed to minimize rework, reduce ambiguity, and give IT a strong foundation for delivering a reliable internal system.
