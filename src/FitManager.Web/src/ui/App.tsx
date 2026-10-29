import { useEffect, useState, type FormEvent } from 'react';
import { Activity, ArrowLeft, Eye, EyeOff, LockKeyhole, LogOut, Mail, ShieldCheck, Users } from 'lucide-react';

const apiBase = import.meta.env.VITE_API_BASE_URL ?? 'https://localhost:7003/api/v1';
type Session = { accessToken: string; expiresInMinutes: number; fullName: string; email: string; roles: string[] };

export function App() {
  const [session, setSession] = useState<Session | null>(null);
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [busy, setBusy] = useState(false);
  const [message, setMessage] = useState('');
  const [coaches, setCoaches] = useState<{ id: string; fullName: string; email: string; isActive: boolean }[]>([]);

  useEffect(() => {
    if (!session?.roles.includes('Admin')) return;
    const controller = new AbortController();
    fetch(`${apiBase}/admin/coaches`, { headers: { Authorization: `Bearer ${session.accessToken}` }, signal: controller.signal })
      .then(response => response.ok ? response.json() : Promise.reject(new Error('تعذر تحميل قائمة المدربين.')))
      .then(setCoaches).catch(error => { if (!controller.signal.aborted) setMessage(error.message); });
    return () => controller.abort();
  }, [session]);

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault(); setBusy(true); setMessage('');
    try {
      const response = await fetch(`${apiBase}/auth/login`, { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ email, password }) });
      if (!response.ok) throw new Error(response.status === 401 ? 'البريد أو كلمة المرور غير صحيحة.' : 'تعذر الاتصال بالخادم. حاول مرة أخرى.');
      const result: Session = await response.json();
      setSession(result); setPassword('');
    } catch (error) { setMessage(error instanceof Error ? error.message : 'حدث خطأ غير متوقع.'); }
    finally { setBusy(false); }
  }

  async function logout() {
    if (session) await fetch(`${apiBase}/auth/logout`, { method: 'POST', headers: { Authorization: `Bearer ${session.accessToken}` } }).catch(() => undefined);
    setSession(null); setCoaches([]); setMessage('تم تسجيل الخروج.');
  }

  if (session) return <main className="dashboard-shell" dir="rtl">
    <header className="dashboard-header"><div className="brand-top"><span className="brand-mark"><Activity size={23}/></span><span>FitManager</span></div><button className="logout-button" onClick={logout}><LogOut size={17}/> تسجيل الخروج</button></header>
    <section className="dashboard-content"><span className="eyebrow">{session.roles.includes('Admin') ? 'لوحة الإدارة' : 'مساحة المدرب'}</span><h1>مرحبًا، {session.fullName}</h1><p>تم تسجيل الدخول بصلاحية {session.roles.join('، ')}.</p>
      {session.roles.includes('Admin') && <section className="dashboard-card"><div className="card-title"><Users size={19}/><h2>المدربون</h2><span>{coaches.length}</span></div>{coaches.length ? <ul>{coaches.map(coach => <li key={coach.id}><strong>{coach.fullName}</strong><span>{coach.email}</span><small>{coach.isActive ? 'نشط' : 'معطل'}</small></li>)}</ul> : <p>لا يوجد مدربون للعرض حاليًا.</p>}</section>}
      {session.roles.includes('Coach') && <section className="dashboard-card"><ShieldCheck size={19}/><h2>حساب المدرب</h2><p>مساحة الأعضاء والتدريب ستتوفر في المراحل التالية.</p></section>}
      <p className="status" role="status">{message}</p>
    </section>
  </main>;

  return <main className="login-shell">
    <section className="brand-panel" aria-label="FitManager"><div className="brand-top"><span className="brand-mark"><Activity size={23}/></span><span>FitManager</span></div><div className="brand-copy"><span className="eyebrow">إدارة أذكى، نتائج أقوى</span><h1>كل تفاصيل ناديك<br/><em>في مكان واحد.</em></h1><p>من متابعة الأعضاء والاشتراكات إلى تنظيم التدريب والتقدم، ابدأ يومك بصورة أوضح.</p></div><div className="brand-footer"><ShieldCheck size={17}/><span>منصة آمنة لإدارة عمليات النادي</span><span className="version">نسخة تجريبية · 01</span></div><div className="orb orb-one"/><div className="orb orb-two"/></section>
    <section className="form-panel"><div className="form-wrap"><div className="mobile-brand"><span className="brand-mark"><Activity size={21}/></span>FitManager</div><div className="welcome"><span className="eyebrow">أهلًا بعودتك</span><h2>سجّل الدخول</h2><p>أدخل بيانات حسابك للوصول إلى مساحة العمل.</p></div>
      <form onSubmit={submit}><label htmlFor="email">البريد الإلكتروني</label><div className="input-wrap"><Mail size={18}/><input id="email" type="email" autoComplete="username" placeholder="name@gym.com" required value={email} onChange={e=>setEmail(e.target.value)}/></div><div className="password-label"><label htmlFor="password">كلمة المرور</label><span>لإعادة التعيين تواصل مع مدير النظام</span></div><div className="input-wrap"><LockKeyhole size={18}/><input id="password" type={showPassword?'text':'password'} autoComplete="current-password" placeholder="أدخل كلمة المرور" required value={password} onChange={e=>setPassword(e.target.value)}/><button className="reveal" type="button" onClick={()=>setShowPassword(value=>!value)} aria-label={showPassword?'إخفاء كلمة المرور':'إظهار كلمة المرور'}>{showPassword?<EyeOff size={18}/>:<Eye size={18}/>}</button></div><button className="submit" type="submit" disabled={busy}>{busy?'جارٍ التحقق…':<>الدخول إلى حسابك <ArrowLeft size={18}/></>}</button><p className="status" role="status">{message}</p></form>
      <p className="form-foot">بمجرد الدخول، ستظهر لك الأقسام المناسبة لصلاحيات حسابك.</p></div></section>
  </main>;
}
