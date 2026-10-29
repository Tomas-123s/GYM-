# FitManager

MVP لإدارة نادٍ رياضي باستخدام ASP.NET Core Web API وReact/TypeScript وSQL Server.

## بنية الحل

- `src/FitManager.Api`: طبقة HTTP والمصادقة والتوثيق.
- `src/FitManager.Application`: طبقة حالات الاستخدام وقواعد العمل (حاليًا نقطة انطلاق للمرحلة التالية).
- `src/FitManager.Infrastructure`: EF Core وSQL Server وASP.NET Core Identity.
- `src/FitManager.Web`: واجهة React/TypeScript عربية RTL.
- `docs`: خطة التنفيذ مقسمة حسب المراحل.

الاتجاه المقصود للاعتمادات: API → Application/Infrastructure، وInfrastructure → Application. لا توضع قواعد العمل في Controllers.

## المتطلبات المحلية

- .NET SDK 10.
- SQL Server LocalDB أو SQL Server متاح بعنوان اتصال مضبوط.
- Node.js 20.19+ وnpm لتشغيل الواجهة.

## تشغيل الـAPI محليًا

1. انسخ إعداد اتصال SQL Server عند الحاجة إلى user-secrets أو متغير البيئة `ConnectionStrings__FitManager`.
2. عيّن سر JWT محليًا، ولا تحفظه في Git:

   ```powershell
   dotnet user-secrets init --project src/FitManager.Api
   dotnet user-secrets set "Jwt:SigningKey" "ضع-سرًا-عشوائيًا-محليًا-بطول-32-بايت-على-الأقل" --project src/FitManager.Api
   ```

3. طبّق الـmigration الأولي على قاعدة البيانات:

   ```powershell
   dotnet tool restore
   dotnet ef database update --project src/FitManager.Infrastructure --startup-project src/FitManager.Api
   ```

   المستودع يثبت إصدار EF CLI في `dotnet-tools.json`.

4. أنشئ أول حساب Admin باستخدام متغيرات بيئية محلية. لا توجد بيانات اعتماد افتراضية:

   ```powershell
   $env:Seed__Admin__Enabled = "true"
   $env:Seed__Admin__Email = "admin@gym.local"
   $env:Seed__Admin__FullName = "Gym Administrator"
   $env:Seed__Admin__Password = "ضع-كلمة-مرور-قوية-مؤقتة-هنا"
   dotnet run --project src/FitManager.Api --launch-profile https
   ```

   بعد أول تشغيل، اجعل `Seed__Admin__Enabled` تساوي `false` أو امسح متغيرات seed.

- فحص الصحة: `GET /api/v1/health`.
- توثيق OpenAPI متاح في Development على `/openapi/v1.json`.
- تسجيل الدخول: `POST /api/v1/auth/login` بجسم `{ "email": "...", "password": "..." }`.
- تسجيل المدربين وإدارتهم (Admin فقط): `GET/POST /api/v1/admin/coaches` و`GET/PUT /api/v1/admin/coaches/{id}`. لا يوجد تسجيل ذاتي.

## تشغيل الواجهة

```powershell
cd src/FitManager.Web
Copy-Item .env.example .env.local
npm install
npm run dev
```

اضبط `VITE_API_BASE_URL` حسب منفذ API الفعلي. الواجهة حاليًا شاشة دخول أولية؛ بقية الشاشات ستُبنى بالمراحل اللاحقة.

## ملاحظات أمنية حالية

- JWT قصير العمر (30 دقيقة)؛ لا يُحفظ في localStorage. الواجهة تحتفظ بنتيجة المصادقة في الذاكرة لحين بناء سياق التطبيق.
- endpoint تسجيل الخروج يحدّث SecurityStamp للحساب فيبطل الرموز الصادرة له؛ تعطيل الحساب يبطل رموزه كذلك.
- أول حساب Admin يُنشأ صراحةً بمتغيرات البيئة بعد تطبيق migration، ولا توجد بيانات اعتماد افتراضية بالمستودع.
