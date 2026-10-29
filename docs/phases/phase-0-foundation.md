# المرحلة 0 — التحليل والتأسيس المعماري (الأيام 1–3)

**الهدف:** أساس تشغيل موحد بثلاث طبقات، SQL Server، تسجيل دخول وصلاحيات أولية، وهيكل واجهة قابل للتوسع.

## المهام

### تحليل وقرارات
- [x] تثبيت نطاق MVP والأدوار والتبعيات وحدود الأيام؛ تسجيل القرارات غير المحسومة.
- [x] تحديد سياسة المنطقة الزمنية للنادي، حدود اليوم، احتساب الأشهر التقويمية، سياسة تكرار الهاتف، وصيغ التقارير.
- [x] رسم ERD أولي يضم Identity، Members، Plans، Subscriptions، Payments، Attendance، Training، Measurements، Notifications وAuditLogs.

### Backend — ASP.NET Core
- [x] إنشاء Solution ومشاريع API/Presentation وApplication/Business وInfrastructure/Data.
- [x] ضبط اتجاه المراجع API → Application/Infrastructure وInfrastructure → Application.
- [x] إعداد DI، إعدادات البيئة، logging، معالجة استثناءات مركزية، DTOs والتحقق من المدخلات.
- [x] تهيئة ASP.NET Core Identity والأدوار Admin وCoach، تسجيل الدخول والخروج، وسياسة الحساب المعطل. (AUTH-01..06)
- [x] إضافة Swagger/OpenAPI ونسخة المسار `/api/v1`.

### Data — SQL Server وEF Core
- [x] إعداد SQL Server LocalDB وDbContext وEF Core؛ الاتصال قابل للاستبدال بمتغير بيئة.
- [x] إنشاء وتطبيق migration أولي لـIdentity والأدوار على قاعدة `FitManagerDev`.
- [x] تجهيز آلية bootstrap لإنشاء Admin من متغيرات بيئة فقط؛ إنشاء الحساب نفسه ينتظر اختيار بيانات الاعتماد محليًا.
- [x] تحديد موضع seed للأعضاء والبيانات الخيالية بعد بناء جداول المجالات؛ تنفيذها مؤجل للمراحل التي تضيف الجداول.

### Frontend — React/TypeScript
- [x] إنشاء تطبيق React/TypeScript أولي وتخطيط دخول RTL متجاوب ومعالجة حالات التحميل والخطأ.
- [x] بناء شاشة الدخول وربطها بنقطة API باستراتيجية Bearer قصيرة العمر؛ لا يوجد تخزين دائم للرمز.
- [x] إكمال حالة المصادقة داخل الذاكرة، حراسة شاشة العمل، وعرض المحتوى حسب الدور؛ الواجهة ليست بديلًا عن صلاحيات الـBackend.

## معيار الإنجاز

يمكن تشغيل API والواجهة محليًا، الاتصال بـSQL Server وتطبيق migration، وتسجيل دخول Admin/Coach مع رفض الوصول غير المصرح به. توثيق إعداد التشغيل الأولي موجود.

## الحالة الحالية

- [x] Solution ومشاريع API/Application/Infrastructure والاعتماديات باتجاه الطبقات.
- [x] إعداد SQL Server وEF Core وASP.NET Core Identity، migration أولي وجداول الأدوار.
- [x] JWT قصير العمر، تعطيل الحساب وتحديث SecurityStamp لإبطال الرموز؛ login/logout.
- [x] API لعرض وإنشاء وتعديل/تعطيل حسابات Coach ومحمية بدور Admin.
- [x] واجهة دخول RTL أولية وملفات Vite/React/TypeScript.
- [x] README وملف مثال إعداد الواجهة و.gitignore.
- [x] توثيق نطاق MVP وقرارات الزمن والتواريخ والهاتف والتقارير ومخطط ERD الأولي.
- [x] واجهة ما بعد الدخول تعرض مساحة Admin أو Coach بحسب الدور، وتحمل قائمة المدربين للإدارة فقط.
- [ ] تثبيت Node.js وتشغيل واجهة React محليًا؛ Node غير متاح في بيئة العمل الحالية.
- [ ] إنشاء أول حساب Admin ببيانات اعتماد يحددها مالك المشروع والتحقق من تسجيل الدخول.
- [x] تحديد السياسات ومخطط ERD المفاهيمي قبل المرحلة 1؛ تفاصيل الجداول تثبت مع تنفيذ المراحل.

تفاصيل القرارات في [architecture-decisions.md](../architecture-decisions.md). تشغيل الواجهة وإنشاء Admin يتطلبان بيئة محلية وبيانات اعتماد يحددها مالك النظام.

## مراجع BRD

AUTH-01..06، متطلبات Database وTechnical Requirements وSecurity، الشاشات 1 و18، والجدول الأصلي أيام 1–3.
