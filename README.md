# WFT Infrastructure - ABAC System Documentation

## فهرست مطالب
- [معرفی ABAC](#معرفی-abac)
- [مقایسه ABAC و RBAC](#مقایسه-abac-و-rbac)
- [اجزای اصلی ABAC](#اجزای-اصلی-abac)
- [مدل داده‌ای پیاده‌سازی شده](#مدل-داده‌ای-پیاده‌سازی-شده)
- [جداول پایگاه داده](#جداول-پایگاه-داده)
- [ترتیب ورود اطلاعات](#ترتیب-ورود-اطلاعات)
- [مثال عملی](#مثال-عملی)
- [ترکیب ABAC با RBAC](#ترکیب-abac-با-rbac)
- [API Endpoints](#api-endpoints)

## معرفی ABAC

**ABAC (Attribute-Based Access Control)** یا کنترل دسترسی مبتنی بر ویژگی، یک مدل امنیتی پیشرفته است که تصمیم‌گیری‌های دسترسی را بر اساس ویژگی‌های مختلف انجام می‌دهد. برخلاف RBAC که فقط بر اساس نقش‌ها عمل می‌کند، ABAC امکان تعریف قوانین پیچیده‌تر و انعطاف‌پذیرتری را فراهم می‌آورد.

### مزایای ABAC:
- **انعطاف‌پذیری بالا**: امکان تعریف قوانین پیچیده بر اساس ویژگی‌های مختلف
- **مقیاس‌پذیری**: عدم نیاز به تعریف نقش‌های متعدد برای شرایط مختلف
- **دقت بالا**: کنترل دقیق‌تر بر روی دسترسی‌ها
- **پویایی**: امکان تغییر قوانین بدون تغییر ساختار

## مقایسه ABAC و RBAC

| ویژگی | RBAC | ABAC |
|-------|------|------|
| **مبنای تصمیم‌گیری** | نقش‌ها | ویژگی‌ها |
| **انعطاف‌پذیری** | محدود | بالا |
| **پیچیدگی پیاده‌سازی** | ساده | متوسط |
| **مقیاس‌پذیری** | محدود | بالا |
| **مثال** | "مدیران می‌توانند فایل‌ها را حذف کنند" | "کاربران با سطح دسترسی بالا در ساعات کاری می‌توانند فایل‌های محرمانه را ویرایش کنند" |

## اجزای اصلی ABAC

### 1. Subject (فاعل)
- **تعریف**: کاربر یا سیستم درخواست‌کننده دسترسی
- **ویژگی‌ها**: سطح دسترسی، دپارتمان، موقعیت جغرافیایی، سابقه کاری

### 2. Object (شیء)
- **تعریف**: منبع یا داده‌ای که دسترسی به آن کنترل می‌شود
- **ویژگی‌ها**: نوع فایل، سطح محرمانگی، مالک، تاریخ ایجاد

### 3. Action (عملیات)
- **تعریف**: عملیاتی که کاربر می‌خواهد انجام دهد
- **مثال‌ها**: خواندن، نوشتن، حذف، ویرایش

### 4. Environment (محیط)
- **تعریف**: شرایط محیطی که در آن درخواست انجام می‌شود
- **ویژگی‌ها**: زمان، مکان، نوع دستگاه، پروتکل امنیتی

## مدل داده‌ای پیاده‌سازی شده

سیستم ABAC ما بر اساس Clean Architecture طراحی شده و شامل لایه‌های زیر است:

```
┌─────────────────────────────────────────────────────────────┐
│                    Web API Layer                            │
├─────────────────────────────────────────────────────────────┤
│                  Application Layer                          │
├─────────────────────────────────────────────────────────────┤
│                Infrastructure Layer                         │
├─────────────────────────────────────────────────────────────┤
│                    Core Layer                               │
└─────────────────────────────────────────────────────────────┘
```

## جداول پایگاه داده

### 1. AttributeDefinitions (تعاریف ویژگی‌ها)
**هدف**: تعریف ویژگی‌های قابل استفاده در سیستم ABAC

```sql
CREATE TABLE AttributeDefinitions (
    Id BIGINT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(100) NOT NULL UNIQUE,
    DisplayName NVARCHAR(200) NOT NULL,
    DataType NVARCHAR(50) NOT NULL, -- String, Number, Boolean, Date
    Source NVARCHAR(50) NOT NULL,   -- User, Resource, Environment, Action
    IsRequired BIT DEFAULT 0,
    DefaultValue NVARCHAR(500),
    IsActive BIT DEFAULT 1,
    Description NVARCHAR(500),
    AttributeGroupId BIGINT NULL,
    CreatedUserId BIGINT,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2
);
```

**مثال‌های ویژگی‌ها**:
- `UserLevel` (سطح کاربر): Number, User Source
- `Department` (دپارتمان): String, User Source
- `DocumentType` (نوع سند): String, Resource Source
- `ConfidentialityLevel` (سطح محرمانگی): Number, Resource Source
- `CurrentTime` (زمان فعلی): Date, Environment Source

### 2. AttributeGroups (گروه‌های ویژگی)
**هدف**: دسته‌بندی ویژگی‌ها برای مدیریت بهتر

```sql
CREATE TABLE AttributeGroups (
    Id BIGINT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(100) NOT NULL UNIQUE,
    DisplayName NVARCHAR(200) NOT NULL,
    Description NVARCHAR(500),
    IsActive BIT DEFAULT 1,
    SortOrder INT DEFAULT 0,
    Icon NVARCHAR(100),
    Color NVARCHAR(20),
    CreatedUserId BIGINT,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2
);
```

**مثال‌های گروه‌ها**:
- **اطلاعات کاربری**: UserLevel, Department, Position
- **اطلاعات منبع**: DocumentType, ConfidentialityLevel, Owner
- **اطلاعات محیطی**: CurrentTime, Location, DeviceType

### 3. AttributeValues (مقادیر ویژگی‌ها)
**هدف**: ذخیره مقادیر واقعی ویژگی‌ها برای کاربران و منابع

```sql
CREATE TABLE AttributeValues (
    Id BIGINT PRIMARY KEY IDENTITY(1,1),
    AttributeDefinitionId BIGINT NOT NULL,
    UserId BIGINT NULL,
    ResourceId BIGINT NULL,
    ResourceType NVARCHAR(100) NULL,
    Value NVARCHAR(500) NOT NULL,
    ValidFrom DATETIME2 DEFAULT GETDATE(),
    ValidTo DATETIME2 NULL,
    IsActive BIT DEFAULT 1,
    CreatedUserId BIGINT,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2
);
```

**مثال‌ها**:
- کاربر با ID=1 دارای سطح دسترسی 5 است
- سند با ID=100 از نوع "محرمانه" است
- کاربر با ID=2 در دپارتمان "مالی" کار می‌کند

### 4. ConditionOperators (عملگرهای شرط)
**هدف**: تعریف عملگرهای مقایسه‌ای برای شرایط

```sql
CREATE TABLE ConditionOperators (
    Id BIGINT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(50) NOT NULL UNIQUE,
    DisplayName NVARCHAR(100) NOT NULL,
    Symbol NVARCHAR(10) NOT NULL,
    DataTypes NVARCHAR(200) NOT NULL, -- String,Number,Boolean,Date
    IsActive BIT DEFAULT 1,
    CreatedUserId BIGINT,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2
);
```

**عملگرهای موجود**:
- `Equals` (=): String, Number, Boolean, Date
- `NotEquals` (!=): String, Number, Boolean, Date
- `Contains` (LIKE): String
- `GreaterThan` (>): Number, Date
- `LessThan` (<): Number, Date
- `Between` (BETWEEN): Number, Date
- `In` (IN): String, Number

### 5. PolicyRules (قوانین سیاست)
**هدف**: تعریف قوانین ABAC

```sql
CREATE TABLE PolicyRules (
    Id BIGINT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(100) NOT NULL UNIQUE,
    DisplayName NVARCHAR(200) NOT NULL,
    Description NVARCHAR(500),
    IsActive BIT DEFAULT 1,
    Priority INT DEFAULT 0,
    Effect NVARCHAR(10) NOT NULL CHECK (Effect IN ('Allow', 'Deny')),
    CreatedUserId BIGINT,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2
);
```

### 6. PolicyConditions (شرایط سیاست)
**هدف**: تعریف شرایط هر قانون

```sql
CREATE TABLE PolicyConditions (
    Id BIGINT PRIMARY KEY IDENTITY(1,1),
    PolicyRuleId BIGINT NOT NULL,
    AttributeDefinitionId BIGINT NOT NULL,
    ConditionOperatorId BIGINT NOT NULL,
    Value NVARCHAR(500) NOT NULL,
    LogicalOperator NVARCHAR(10) DEFAULT 'AND' CHECK (LogicalOperator IN ('AND', 'OR')),
    Order INT DEFAULT 0,
    CreatedUserId BIGINT,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2
);
```

### 7. RolePolicyRule (تخصیص سیاست به نقش)
**هدف**: ارتباط بین نقش‌های RBAC و قوانین ABAC

```sql
CREATE TABLE RolePolicyRule (
    Id BIGINT PRIMARY KEY IDENTITY(1,1),
    RoleId BIGINT NOT NULL,
    PolicyRuleId BIGINT NOT NULL,
    IsActive BIT DEFAULT 1,
    ValidFrom DATETIME2 DEFAULT GETDATE(),
    ValidTo DATETIME2 NULL,
    CreatedUserId BIGINT,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2
);
```

## ترتیب ورود اطلاعات

برای راه‌اندازی صحیح سیستم ABAC، اطلاعات باید به ترتیب زیر وارد شوند:

### مرحله 1: تعریف ویژگی‌ها
1. **ConditionOperators**: تعریف عملگرهای مقایسه‌ای
2. **AttributeGroups**: ایجاد گروه‌های ویژگی
3. **AttributeDefinitions**: تعریف ویژگی‌ها و تخصیص به گروه‌ها

### مرحله 2: تعریف سیاست‌ها
4. **PolicyRules**: ایجاد قوانین ABAC
5. **PolicyConditions**: تعریف شرایط هر قانون

### مرحله 3: تخصیص مقادیر
6. **AttributeValues**: تخصیص مقادیر ویژگی‌ها به کاربران و منابع

### مرحله 4: فعال‌سازی
7. **RolePolicyRule**: تخصیص سیاست‌ها به نقش‌های موجود

## مثال عملی

### سناریو: کنترل دسترسی به اسناد محرمانه

#### مرحله 1: تعریف ویژگی‌ها
```sql
-- تعریف ویژگی سطح کاربر
INSERT INTO AttributeDefinitions (Name, DisplayName, DataType, Source, IsRequired)
VALUES ('UserLevel', 'سطح کاربر', 'Number', 'User', 1);

-- تعریف ویژگی نوع سند
INSERT INTO AttributeDefinitions (Name, DisplayName, DataType, Source, IsRequired)
VALUES ('DocumentType', 'نوع سند', 'String', 'Resource', 1);

-- تعریف ویژگی سطح محرمانگی
INSERT INTO AttributeDefinitions (Name, DisplayName, DataType, Source, IsRequired)
VALUES ('ConfidentialityLevel', 'سطح محرمانگی', 'Number', 'Resource', 1);
```

#### مرحله 2: تخصیص مقادیر
```sql
-- کاربر احمد دارای سطح 3 است
INSERT INTO AttributeValues (AttributeDefinitionId, UserId, Value)
VALUES (1, 1, '3');

-- سند X از نوع محرمانه با سطح 4 است
INSERT INTO AttributeValues (AttributeDefinitionId, ResourceId, ResourceType, Value)
VALUES (2, 100, 'Document', 'محرمانه');
INSERT INTO AttributeValues (AttributeDefinitionId, ResourceId, ResourceType, Value)
VALUES (3, 100, 'Document', '4');
```

#### مرحله 3: تعریف سیاست
```sql
-- ایجاد قانون "دسترسی به اسناد محرمانه"
INSERT INTO PolicyRules (Name, DisplayName, Description, Effect, Priority)
VALUES ('ConfidentialDocumentAccess', 'دسترسی به اسناد محرمانه', 
        'فقط کاربران با سطح بالا می‌توانند به اسناد محرمانه دسترسی داشته باشند', 
        'Allow', 1);

-- شرط 1: سطح کاربر باید 3 یا بالاتر باشد
INSERT INTO PolicyConditions (PolicyRuleId, AttributeDefinitionId, ConditionOperatorId, Value, Order)
VALUES (1, 1, 4, '3', 1); -- GreaterThan or Equal

-- شرط 2: نوع سند باید محرمانه باشد
INSERT INTO PolicyConditions (PolicyRuleId, AttributeDefinitionId, ConditionOperatorId, Value, LogicalOperator, Order)
VALUES (1, 2, 1, 'محرمانه', 'AND', 2); -- Equals
```

#### مرحله 4: تخصیص به نقش
```sql
-- تخصیص سیاست به نقش "مدیر"
INSERT INTO RolePolicyRule (RoleId, PolicyRuleId)
VALUES (1, 1);
```

### نتیجه
با این تنظیمات:
- ✅ کاربر احمد (سطح 3) می‌تواند به سند X (محرمانه) دسترسی داشته باشد
- ❌ کاربر علی (سطح 1) نمی‌تواند به سند X دسترسی داشته باشد
- ✅ کاربر احمد می‌تواند به اسناد عادی دسترسی داشته باشد

## ترکیب ABAC با RBAC

سیستم ما از یک مدل ترکیبی استفاده می‌کند:

### 1. لایه RBAC (پایه)
- **User**: کاربران سیستم
- **Role**: نقش‌های تعریف شده
- **Permission**: مجوزهای پایه
- **UserRole**: تخصیص نقش به کاربر
- **RolePermission**: تخصیص مجوز به نقش

### 2. لایه ABAC (پیشرفته)
- **PolicyRules**: قوانین پیشرفته
- **RolePolicyRule**: تخصیص قوانین به نقش‌ها
- **AttributeValues**: مقادیر ویژگی‌ها

### فرآیند ارزیابی دسترسی:
1. **بررسی RBAC**: آیا کاربر دارای نقش و مجوز مورد نیاز است؟
2. **بررسی ABAC**: آیا شرایط ویژگی‌ها برآورده می‌شود؟
3. **تصمیم نهایی**: ترکیب نتایج RBAC و ABAC

### مثال ترکیبی:
```csharp
// کاربر احمد دارای نقش "مدیر" است (RBAC)
// و سطح دسترسی 3 دارد (ABAC)
// بنابراین می‌تواند به اسناد محرمانه دسترسی داشته باشد
```

## API Endpoints

### مدیریت ویژگی‌ها
- `GET /api/app/v1/wft/attributedefinitions/List` - لیست ویژگی‌ها
- `POST /api/app/v1/wft/attributedefinitions/Add` - افزودن ویژگی
- `PUT /api/app/v1/wft/attributedefinitions/update` - ویرایش ویژگی

### مدیریت گروه‌های ویژگی
- `GET /api/app/v1/wft/attributegroups/List` - لیست گروه‌ها
- `POST /api/app/v1/wft/attributegroups/Add` - افزودن گروه
- `PUT /api/app/v1/wft/attributegroups/update` - ویرایش گروه

### مدیریت سیاست‌ها
- `GET /api/app/v1/wft/policyrules/List` - لیست سیاست‌ها
- `POST /api/app/v1/wft/policyrules/Add` - افزودن سیاست
- `PUT /api/app/v1/wft/policyrules/update` - ویرایش سیاست

### ارزیابی دسترسی
- `POST /api/app/v1/wft/abac/evaluate` - ارزیابی ساده
- `POST /api/app/v1/wft/abac/evaluate-detailed` - ارزیابی تفصیلی

### تخصیص سیاست به نقش
- `POST /api/app/v1/wft/rolepolicyassignment/assign` - تخصیص سیاست
- `DELETE /api/app/v1/wft/rolepolicyassignment/remove` - حذف تخصیص

---

## نکات مهم پیاده‌سازی

### 1. عملکرد (Performance)
- استفاده از Indexing مناسب برای جداول اصلی
- Caching برای ویژگی‌های پرتکرار
- Lazy Loading برای روابط پیچیده

### 2. امنیت
- Validation کامل ورودی‌ها
- Logging تمام عملیات حساس
- Audit Trail برای تغییرات

### 3. مقیاس‌پذیری
- طراحی ماژولار
- امکان افزودن ویژگی‌های جدید بدون تغییر کد
- پشتیبانی از شرایط پیچیده

### 4. نگهداری
- مستندسازی کامل
- تست‌های واحد و یکپارچگی
- Monitoring و Alerting

---

**نکته**: این سیستم به گونه‌ای طراحی شده که امکان توسعه و گسترش آسان را فراهم می‌آورد. برای افزودن ویژگی‌ها یا قوانین جدید، کافی است داده‌های مربوطه را در جداول مربوطه وارد کنید.
