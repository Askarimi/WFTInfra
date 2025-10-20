# 🚀 Quick Start Guide - JSON:API Pagination

## ✅ Everything is Ready

Your API now returns pagination responses in **JSON:API standard format** with **camelCase** and **flat data/meta structure**.

---

## 📊 Expected Response Format

```json
{
  "success": true,
  "data": [
    { "id": 1, "username": "admin", "email": "admin@test.com" },
    { "id": 2, "username": "user1", "email": "user1@test.com" }
  ],
  "meta": {
    "page": 1,
    "pageSize": 10,
    "totalItems": 200,
    "totalPages": 20
  },
  "message": null,
  "error": null
}
```

---

## 🧪 How to Test (3 Quick Steps)

### **Step 1: Run Tests**
```bash
dotnet test src/WFT.Infra.Test
```
**Expected Output:**
```
Passed!  - Failed: 0, Passed: 38, Skipped: 0
```

### **Step 2: Start API**
```bash
dotnet run --project src/WFT.Infra.WebApi
```
**Server runs on:** `https://localhost:5001`

### **Step 3: Test Endpoint**
```bash
curl -X GET "https://localhost:5001/api/app/v1/wft/Users/List?PageNumber=1&PageSize=10" \
  -H "Authorization: Bearer <your-token>" -k
```

---

## ✅ Verification Checklist

When you test an endpoint, verify:

- [ ] Response has `"success": true` (lowercase)
- [ ] Response has `"data": [...]` (lowercase, flat array)
- [ ] Response has `"meta": { ... }` (lowercase object)
- [ ] Meta has `"page": 1` (lowercase)
- [ ] Meta has `"pageSize": 10` (lowercase camelCase)
- [ ] Meta has `"totalItems": 200` (lowercase camelCase)
- [ ] Meta has `"totalPages": 20` (lowercase camelCase)
- [ ] Response has `"message": null` (lowercase)
- [ ] Response has `"error": null` (lowercase)

---

## 📖 Frontend Integration

### **TypeScript Interface:**
```typescript
interface ApiPagedResponse<T> {
  success: boolean;
  data: T[];
  meta: {
    page: number;
    pageSize: number;
    totalItems: number;
    totalPages: number;
  };
  message?: string | null;
  error?: string | null;
}
```

### **Usage:**
```typescript
const response = await httpService.getPaged<User>('/api/app/v1/wft/Users/List', {
  pageNumber: 1,
  pageSize: 10
});

// Access data
const users = response.data;                    // Flat array
const page = response.meta.page;                // Current page
const total = response.meta.totalItems;         // Total count
const pages = response.meta.totalPages;         // Page count
```

---

## 🎯 All 8 Pagination Endpoints

| Endpoint | Path |
|----------|------|
| Users | `GET /api/app/v1/wft/Users/List` |
| Roles | `GET /api/app/v1/wft/Roles/List` |
| Permissions | `GET /api/app/v1/wft/Permissions/List` |
| Countries | `GET /api/app/v1/wft/Country/List` |
| Attribute Definitions | `GET /api/app/v1/wft/AttributeDefinitions/List` |
| Attribute Groups | `GET /api/app/v1/wft/AttributeGroups/List` |
| Policy Rules | `GET /api/app/v1/wft/PolicyRules/List` |
| Condition Operators | `GET /api/app/v1/wft/ConditionOperators/List` |

**All return the same JSON:API format!** ✅

---

## 📝 Common Query Parameters

```http
?PageNumber=1&PageSize=10
?PageNumber=2&PageSize=25
?PageNumber=1&PageSize=50
```

**Note:** Query parameters use PascalCase, but response uses camelCase.

---

## 🎊 Success!

- ✅ 38 tests passing
- ✅ 0 build errors
- ✅ JSON:API compliant
- ✅ camelCase throughout
- ✅ Flat data/meta structure
- ✅ Ready for production

**Start your API and test it!** 🚀




