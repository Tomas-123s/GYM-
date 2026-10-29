import { useState, type FormEvent } from 'react';
import { Activity, ArrowLeft, Eye, EyeOff, LockKeyhole, Mail, ShieldCheck } from 'lucide-react';

const apiBase = import.meta.env.VITE_API_BASE_URL ?? 'https://localhost:7003/api/v1';

export function App() {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [busy, setBusy] = useState(false);
  const [message, setMessage] = useState('');

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault(); setBusy(true); setMessage('');
    try {
      const response = await fetch(`${apiBase}/auth/login`, { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ email, password }) });
      if (!response.ok) throw new Error(response.status === 401 ? 'البريد أو كلمة المرور غير صحيحة.' : 'تعذر الاتصال بالخادم. حاول مرة أخرى.');
      const result = await response.json();
      window.dispatchEvent(new CustomEvent('fitmanager:authenticated', { detail: result }));
      setMessage(`مرحبًا ${result.fullName}`);
    } catch (error) { setMessage(error instanceof Error ? error.message : 'حدث خطأ غير متوقع.'); }
    finally { setBusy(false); }
  }

  return <main className="login-shell">
    <section className="brand-panel" aria-label="FitManager">
      <div className="brand-top"><span className="brand-mark"><Activity size={23}/></span><span>FitManager</span></div>
      <div className="brand-copy"><span className="eyebrow">إدارة أذكى، نتائج أقوى</span><h1>كل تفاصيل ناديك<br/><em>في مكان واحد.</em></h1><p>من متابعة الأعضاء والاشتراكات إلى تنظيم التدريب والتقدم، ابدأ يومك بصورة أوضح.</p></div>
      <div className="brand-footer"><ShieldCheck size={17}/><span>منصة آمنة لإدارة عمليات النادي</span><span className="version">نسخة تجريبية · 01</span></div>
      <div className="orb orb-one"/><div className="orb orb-two"/>
    </section>
    <section className="form-panel"><div className="form-wrap">
      <div className="mobile-brand"><span className="brand-mark"><Activity size={21}/></span>FitManager</div>
      <div className="welcome"><span className="eyebrow">أهلًا بعودتك</span><h2>سجّل الدخول</h2><p>أدخل بيانات حسابك للوصول إلى لوحة الإدارة.</p></div>
      <form onSubmit={submit}>
        <label htmlFor="email">البريد الإلكتروني</label><div className="input-wrap"><Mail size={18}/><input id="email" type="email" autoComplete="username" placeholder="name@gym.com" required value={email} onChange={e=>setEmail(e.target.value)}/></div>
        <div className="password-label"><label htmlFor="password">كلمة المرور</label><span>لإعادة التعيين تواصل مع مدير النظام</span></div>
        <div className="input-wrap"><LockKeyhole size={18}/><input id="password" type={showPassword?'text':'password'} autoComplete="current-password" placeholder="أدخل كلمة المرور" required value={password} onChange={e=>setPassword(e.target.value)}/><button className="reveal" type="button" onClick={()=>setShowPassword(value=>!value)} aria-label={showPassword?'إخفاء كلمة المرور':'إظهار كلمة المرور'}>{showPassword?<EyeOff size={18}/>:<Eye size={18}/>}</button></div>
        <button className="submit" type="submit" disabled={busy}>{busy?'جارٍ التحقق…':<>الدخول إلى حسابك <ArrowLeft size={18}/></>}</button><p className="status" role="status">{message}</p>
      </form>
      <p className="form-foot">بالدخول، أنت توافق على سياسات النادي وإرشادات استخدام النظام.</p>
    </div></section>
  </main>;
}
