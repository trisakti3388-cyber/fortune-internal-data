# GitHub Upload Checklist

## Before Upload
- [ ] Review repository contents
- [ ] Remove temporary secrets
- [ ] Confirm appsettings does not contain production credentials
- [ ] Confirm upload paths are environment-appropriate
- [ ] Confirm bootstrap credentials are not committed

## Upload Steps
- [ ] Create GitHub repository
- [ ] Copy project package into repository
- [ ] Commit initial scaffold
- [ ] Push to main branch
- [ ] Add README screenshots later if desired

## For IT Team After Clone
- [ ] Install .NET 8 SDK
- [ ] Restore NuGet packages
- [ ] Configure MySQL connection string
- [ ] Add EF migrations
- [ ] Implement Identity integration details
- [ ] Implement parser logic
- [ ] Run staging deployment
- [ ] Execute UAT checklist
