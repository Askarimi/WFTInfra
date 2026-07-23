# مستند ورود داده‌ها برای RBAC و ABAC

این سند راهنمای عملی و یکپارچه‌ای برای ورود، مدیریت و نگه‌داری داده‌های RBAC (کنترل دسترسی مبتنی بر نقش) و ABAC (کنترل دسترسی مبتنی بر صفت) در سامانه فعلی است. هدف، ارائه دستورالعمل‌های واضح برای تیم‌های فنی فارسی‌زبان است تا داده‌های امنیتی را با نام‌گذاری یکنواخت، بدون تکرار و با رعایت اصل «انکار به‌صورت پیش‌فرض» ثبت و نگه‌داری کنند.

## 1) مقدمه
- RBAC: دسترسی‌ها بر اساس «نقش»‌های کاربر تعریف می‌شوند. هر نقش مجموعه‌ای از «مجوزها (Permission)» دارد و کاربران با انتساب نقش‌ها، دسترسی‌های لازم را کسب می‌کنند.
- ABAC: علاوه بر نقش، تصمیم دسترسی بر اساس «صفات (Attributes)» کاربر/منبع/محیط و «سیاست‌ها (Policies)» و «شرایط (Conditions)» انجام می‌شود. این لایه امکان ریزدانه‌کردن دسترسی‌ها را می‌دهد (مثلاً «کاربر فقط به اسناد دپارتمان خودش دسترسی داشته باشد»).
- یکپارچه‌سازی: در سامانه فعلی ابتدا RBAC بررسی می‌شود (`HasPermissionAsync`). در صورت عبور، ABAC با ارزیابی سیاست‌ها و شرایط وارد عمل می‌شود (`EvaluateAccessDetailedAsync`). خروجی نهایی می‌تواند همراه با دلیل و جزئیات ارزیابی باشد.

## 2) نمای کلی مدل داده‌ها

### 2.1) RBAC

| مدل | فیلدها (نمونه) | روابط کلیدی | مثال مقدار |
| --- | --- | --- | --- |
| Role | Name, IsActive, Description | UserRoles, RolePermissions, RolePolicyRules | Name=Admin, IsActive=true |
| Permission | Name, DisplayName, IsActive | RolePermissions | Name=CreateUser, DisplayName=ایجاد کاربر |
| RolePermission | RoleId, PermissionId | Role↔Permission | RoleId=1, PermissionId=10 |
| UserRole | UserId, RoleId | User↔Role | UserId=25, RoleId=1 |

- رابطه RBAC: User → UserRole → Role → RolePermission → Permission

### 2.2) ABAC

| مدل | فیلدها (نمونه) | روابط کلیدی | مثال مقدار |
| --- | --- | --- | --- |
| AttributeGroup | Name, DisplayName, Description, IsActive | AttributeDefinitions | Name=UserProfile |
| AttributeDefinition | Name, DataType, Source(User/Resource/Environment/Action), IsActive | AttributeValues, PolicyConditions | Name=DepartmentId, DataType=Number, Source=User |
| AttributeValue | AttributeDefinitionId, UserId?, ResourceId?, ResourceType?, Value, ValidFrom, ValidTo, IsActive | - | Value=42, UserId=25 یا ResourceId=100, ResourceType=Document |
| PolicyRule | Name, DisplayName, Description, Priority, Effect(Allow/Deny), IsActive | PolicyConditions, RolePolicyRules | Name=DepartmentDocumentAccess, Priority=1, Effect=Allow |
| PolicyCondition | PolicyRuleId, AttributeDefinitionId, ConditionOperatorId, Value, LogicalOperator(AND/OR), Order | - | Value=42، LogicalOperator=AND |
| ConditionOperator | Name, DisplayName, Symbol, DataTypes, IsActive | PolicyConditions | Name=Equals, Symbol== |
| RolePolicyRule | RoleId, PolicyRuleId, IsActive, ValidFrom?, ValidTo? | Role↔PolicyRule | RoleId=1, PolicyRuleId=7 |

- روابط ABAC:
  - Role → RolePolicyRule → PolicyRule → PolicyCondition → AttributeDefinition
  - AttributeValue به User/Resource متصل می‌شود و در شرایط با اپراتورها استفاده می‌شود.

## 3) فرآیند ورود داده RBAC (گام‌به‌گام)
1. ایجاد Permission:
   - تعیین `Name` فنی یکتا (مثلاً CreateUser، ViewUser، EditUser، DeleteUser).
   - تعیین `DisplayName` قابل‌خواندن برای UI، `IsActive=true`.
2. ایجاد Role:
   - تعیین `Name` یکتا (مثلاً Admin، Operator).
   - تکمیل `Description` و `IsActive=true`.
3. اتصال Role به Permission:
   - ایجاد رکوردهای `RolePermission` برای هر مجوز موردنیاز نقش.
4. انتساب Role به User:
   - ایجاد رکورد `UserRole` برای هر کاربر و نقش‌های مرتبط.

نکته: نام‌گذاری `Permission.Name` باید ثابت و یکتا باشد و در کنترلرها/سرویس‌ها از همین نام‌ها استفاده شود.

## 4) فرآیند ورود داده ABAC (گام‌به‌گام)
1. تعریف گروه صفات (اختیاری):
   - ایجاد `AttributeGroup` برای دسته‌بندی (مثلاً UserProfile).
2. تعریف صفت‌ها:
   - ایجاد `AttributeDefinition` با فیلدهای `Name`, `DataType` (String/Number/Boolean/DateTime)، و `Source` (User/Resource/Environment/Action).
3. مقداردهی صفت‌ها:
   - ایجاد `AttributeValue` برای User یا Resource:
     - برای User: `UserId` مقداردهی شود.
     - برای Resource: `ResourceId` و `ResourceType` مقداردهی شود.
     - `Value` و در صورت نیاز `ValidFrom/ValidTo` تنظیم شوند.
4. تعریف سیاست‌ها:
   - ایجاد `PolicyRule` با `Name`, `Priority`, `Effect` (Allow یا Deny)، `IsActive=true`.
5. افزودن شروط سیاست:
   - ایجاد `PolicyCondition` برای هر شرط با ارجاع به `AttributeDefinition` و `ConditionOperator`، مقدار مقایسه (`Value`)، `LogicalOperator` (AND/OR) و `Order`.
6. اتصال نقش‌ها به سیاست‌ها:
   - ایجاد `RolePolicyRule` برای اتصال هر نقش به سیاست(ها) با `IsActive=true` و در صورت نیاز `ValidFrom/ValidTo`.

## 5) اپراتورهای شرط (Condition Operators)
اپراتورهای پشتیبانی‌شده در کد فعلی (نام‌ها به‌صورت غیرحساس به حروف بزرگ/کوچک پردازش می‌شوند):
- equals: برابری رشته‌ای/بدون حساسیت به حروف
  - مثال (String): actual="Sales" , expected="sales" → true
- contains: شامل بودن رشته‌ای
  - مثال: actual="Sales-West" , expected="west" → true
- startswith / endswith: شروع/پایان رشته
  - مثال: actual="DOC-1001" , expected="DOC-" → true
- greaterthan / lessthan: مقایسه عددی (Decimal)
  - مثال: actual="10" , expected="5" → greaterthan=true
- between: بازه عددی با قالب "min-max"
  - مثال: actual="7" , expected="5-10" → true
- in: عضویت در مجموعه کاما-جدا
  - مثال: actual="HR" , expected="HR,IT,FIN" → true

یادداشت:
- مقایسه‌ها برای رشته‌ها OrdinalIgnoreCase هستند.
- برای عددی‌ها تلاش می‌شود به `decimal` تبدیل شود؛ در غیراین‌صورت false بازمی‌گردد.
- لطفاً `DataTypes` سازگار با اپراتور را در `ConditionOperator` تعریف کنید (مثلاً String/Number).

## 6) اعتبار تاریخ (Date-based Validity)
- RolePolicyRule:
  - در صورت تنظیم `ValidFrom/ValidTo`، سیاست فقط در بازه مشخص اعمال می‌شود.
  - در ورود داده، بازه‌ها را هم‌پوشان نسازید و از تاریخ UTC استفاده کنید (پیشنهاد).
- AttributeValue:
  - با `ValidFrom/ValidTo` می‌توان مقدار صفت را در بازه‌ای فعال کرد.
  - اگر خالی باشند و `IsActive=true` باشد، مقدار فعلی دائماً معتبر تلقی می‌شود.

## 7) نمودار متنی جریان ارزیابی دسترسی
```
درخواست دسترسی (UserId, PermissionName, Resource?, Context?)
  ├─ RBAC: HasPermissionAsync(UserId, PermissionName)
  │    ├─ خیر → نتیجه نهایی: Deny
  │    └─ بله → ادامه
  └─ ABAC: EvaluateAccessDetailedAsync(UserId, PermissionName, Resource?, Context?)
       ├─ جمع‌آوری UserAttributes و (در صورت وجود) ResourceAttributes
       ├─ استخراج PolicyRule های مرتبط با نقش‌های کاربر (RolePolicyRule → PolicyRule)
       ├─ ارزیابی PolicyCondition ها به ترتیب Priority و Order
       │    ├─ اگر Effect = Deny و شرایط برقرار → Deny (با Reason)
       │    └─ اگر Effect = Allow و شرایط برقرار → Allow (با Reason)
       └─ اگر هیچ Allow برقرار نشد → Deny (Default-Deny)
```

## 8) بهترین شیوه‌ها (Best Practices)
- نام‌گذاری یکنواخت:
  - `Permission.Name` کوتاه، یکتا و انگلیسی (PascalCase)؛ `DisplayName` فارسی و خوانا.
  - `AttributeDefinition.Name` یکتا و متناسب با `Source` (مثلاً User.DepartmentId).
- جلوگیری از تکرار:
  - قبل از ایجاد Permission/AttributeDefinition جدید، بررسی وجودی انجام دهید.
- اصل امنیت:
  - Default-Deny را رعایت کنید؛ فقط آنچه لازم است Allow کنید.
  - اپراتورها و انواع داده را متناسب انتخاب کنید.
- تاریخ/زمان:
  - از UTC برای `ValidFrom/ValidTo` استفاده کنید.
  - تناقض و هم‌پوشانی بازه‌ها را کنترل کنید.
- نگه‌داری:
  - سیاست‌ها را با `Priority` مناسب مرتب کنید؛ تغییرات را مستندسازی و تست کنید.

## 9) سناریوهای نمونه ورود داده (ترکیبی RBAC + ABAC)

### سناریوی 1: Allow
- RBAC:
  - Role=DocumentEditor دارای Permission=EditDocument.
  - User=Ali نقش DocumentEditor دارد.
- ABAC:
  - AttributeDefinition: User.DepartmentId (Number, Source=User)
  - AttributeValue: برای Ali مقدار 42.
  - PolicyRule: Name=DepartmentDocEdit, Effect=Allow, Priority=1
  - PolicyCondition: Attribute=User.DepartmentId, Operator=equals, Value=42
  - RolePolicyRule: اتصال Role=DocumentEditor به PolicyRule=DepartmentDocEdit.
- نتیجه:
  - RBAC: Ali مجوز EditDocument دارد.
  - ABAC: شرط DepartmentId==42 برقرار → Allow.

### سناریوی 2: Deny
- RBAC:
  - Role=DocumentViewer دارای Permission=ViewDocument.
  - User=Sara نقش DocumentViewer دارد.
- ABAC:
  - AttributeDefinition: Resource.SensitivityLevel (Number, Source=Resource)
  - AttributeValue: برای سند X مقدار 5.
  - PolicyRule: Name=HighSensitivityBlock, Effect=Deny, Priority=1
  - PolicyCondition: Attribute=Resource.SensitivityLevel, Operator=greaterthan, Value=3
  - RolePolicyRule: اتصال Role=DocumentViewer به HighSensitivityBlock.
- نتیجه:
  - RBAC: Sara مجوز ViewDocument دارد.
  - ABAC: SensitivityLevel>3 برقرار → قانون Deny اعمال و دسترسی رد می‌شود.
