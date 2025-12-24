# crm-utility-hubspot-salesforce
CRM Utility integrating HubSpot &amp; Salesforce

##  Overview
This project is a **CRM Utility** developed during an internship to integrate **HubSpot** and **Salesforce** using **OAuth 2.0**.  
It demonstrates real-world CRM integration, secure authentication, modular backend design, and a simple frontend dashboard.

The application allows users to:
- Authenticate with HubSpot and Salesforce
- Fetch (Pull) Contacts, Companies, and Deals
- Create and Update CRM records
- Handle token expiration and API errors
- View latest CRM data in a React dashboard

---

##  Project Objectives
- Implement OAuth 2.0 authentication for HubSpot and Salesforce
- Build modular, scalable backend services
- Support Create, Update, and Fetch operations
- Fetch latest records reliably
- Document APIs using Swagger
- Deliver a working frontend + backend demo

---

## 🏗 System Architecture
### Frontend
- React.js
- Connect, Create, Update, Pull buttons
- Tables displaying latest 10 records

### Backend
- ASP.NET Core Web API
- Controllers for Auth, Push, and CRM operations
- Separate service layers for HubSpot and Salesforce
- Secure token management

### Database
- Stores OAuth access tokens and refresh tokens
- Stores Salesforce instance URL
- Enables seamless API calls without re-login

---

##  OAuth Authentication Flow

### HubSpot OAuth Flow
1. User clicks **Connect HubSpot**
2. Redirected to HubSpot login & consent screen
3. HubSpot redirects back with authorization code
4. Backend exchanges code for access & refresh token
5. Tokens stored securely in database

### Salesforce OAuth Flow
1. User clicks **Connect Salesforce**
2. Redirected to Salesforce login page
3. Salesforce returns authorization code and instance URL
4. Backend stores access token and instance URL
5. Instance URL is used for all Salesforce API calls

---

##  CRM Operations Implemented

### Contacts
- Create Contact
- Update Contact
- Fetch latest 10 contacts

### Companies
- Create Company
- Update Company
- Fetch latest 10 companies

### Deals
- Create Deal
- Update Deal
- Fetch latest 10 deals

**Note:**
- HubSpot uses Search APIs for reliable sorting by creation time
- Salesforce uses SOQL queries to fetch recent records

---

##  Error Handling & Edge Cases
Handled scenarios include:
- Expired access tokens
- Missing CRM connection
- Invalid input data
- Duplicate record creation (409 Conflict)
- API rate limits (429 errors)
- Invalid SOQL queries (Salesforce)

Errors are returned from backend APIs and shown in the frontend.

---

##  API Documentation (Swagger)

Swagger UI is enabled for all APIs.

**Swagger URL:**
https://localhost:7265/swagger

markdown
Copy code

### Authentication APIs
- `GET /auth/hubspot/login`
- `GET /auth/hubspot/callback`
- `GET /auth/salesforce/login`
- `GET /auth/salesforce/callback`

### Pull (Fetch) APIs
- `GET /hubspot/contacts`
- `GET /hubspot/companies`
- `GET /hubspot/deals`
- `GET /salesforce/contacts`
- `GET /salesforce/companies`
- `GET /salesforce/deals`

### Push (Create / Update) APIs
- `POST /push/contact`
- `PUT /push/contact`
- `POST /push/company`
- `PUT /push/company`
- `POST /push/deal`
- `PUT /push/deal`

---

## Project Folder Structure
CrmUtility.Backend
│
├── Controllers
│ ├── AuthController.cs
│ ├── HubSpotController.cs
│ ├── SalesforceController.cs
│ ├── PushController.cs
│
├── Services
│ ├── HubSpot
│ │ ├── HubSpotAuthService.cs
│ │ ├── HubSpotCrmService.cs
│ │
│ ├── Salesforce
│ │ ├── SalesforceAuthService.cs
│ │ ├── SalesforceCrmService.cs
│
├── Models
│ ├── OAuthConnection.cs
│ ├── StandardContactDto.cs
│ ├── StandardCompanyDto.cs
│ ├── StandardDealDto.cs
│ ├── CrmType.cs
│
├── Data
│ └── AppDbContext.cs
│
├── Program.cs
└── appsettings.json

yaml
Copy code

---

##  Key Learnings
- Real-world OAuth 2.0 implementation
- Differences between HubSpot and Salesforce APIs
- Modular service-based backend design
- Secure token management
- Handling third-party API limits and errors
- Debugging real production-style issues

---

##  Challenges Faced & Solutions
**Challenge:** Latest records not appearing  
**Solution:** Switched to HubSpot Search APIs and SOQL ordering

**Challenge:** Salesforce instance URL handling  
**Solution:** Stored instance URL during OAuth and reused it

**Challenge:** Token expiration  
**Solution:** Used refresh tokens and token validation logic

---

##  Final Outcome
✔ HubSpot authentication working  
✔ Salesforce authentication working  
✔ Create, Update, Pull operations implemented  
✔ APIs documented with Swagger  
✔ Frontend dashboard functional  
✔ Code modular and extensible  

---

##  Conclusion
This project provided hands-on experience with enterprise CRM integrations and backend system design.  
It closely reflects real-world SaaS integrations and strengthened understanding of OAuth security,
